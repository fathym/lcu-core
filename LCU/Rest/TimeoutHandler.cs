using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LCU.Rest
{
	public class TimeoutHandler : DelegatingHandler
	{
		#region Properties
		public virtual TimeSpan DefaultTimeout { get; set; }
		#endregion

		#region Constructors
		public TimeoutHandler()
		{
			DefaultTimeout = TimeSpan.FromSeconds(100);
		}
		#endregion

		#region Helpers
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			try
			{
				using (var cts = getCancellationTokenSource(request, cancellationToken))
				{
					return await base.SendAsync(request, cts?.Token ?? cancellationToken);
				}
			}
			catch (OperationCanceledException)
				when (!cancellationToken.IsCancellationRequested)
			{
				throw new TimeoutException();
			}
		}

		protected virtual CancellationTokenSource getCancellationTokenSource(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var timeout = request.GetTimeout() ?? DefaultTimeout;

			if (timeout == Timeout.InfiniteTimeSpan)
			{
				// No need to create a CTS if there's no timeout
				return null;
			}
			else
			{
				var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

				cts.CancelAfter(timeout);

				return cts;
			}
		}
		#endregion
	}
}
