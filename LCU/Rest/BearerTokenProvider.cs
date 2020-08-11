using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LCU.Rest
{
	public interface IBearerTokenProvider
	{
		Task<string> Load();

		Task Set(string token);
	}

	public abstract class BaseBearerTokenProvider : IBearerTokenProvider
	{
		#region Constructors
		public BaseBearerTokenProvider()
		{ }
		#endregion

		#region API Methods
		public virtual async Task<string> Load()
		{
			var token = await loadBearerToken();

			return token ?? "";
		}

		public virtual async Task Set(string token)
		{
			if (token.IsNullOrEmpty())
				throw new ArgumentNullException("token");

			await setBearerToken(token);
		}
		#endregion

		#region Helpers
		protected abstract Task<string> loadBearerToken();

		protected abstract Task setBearerToken(string token);
		#endregion
	}

	public class SessionBearerTokenProvider : BaseBearerTokenProvider
	{
		#region Fields
		protected readonly IHttpContextAccessor httpContextAccessor;

		protected readonly string sessionKey;
		#endregion

		#region Constructors
		public SessionBearerTokenProvider(IHttpContextAccessor httpContextAccessor, string sessionKey = null)
			: base()
		{
			this.httpContextAccessor = httpContextAccessor;

			this.sessionKey = sessionKey ?? "LCU-SESSION-BEARER-TOKEN";
		}
		#endregion

		#region Helpers
		protected override async Task<string> loadBearerToken()
		{
			return httpContextAccessor?.HttpContext?.Session.GetString(sessionKey);
		}

		protected override async Task setBearerToken(string token)
		{
			httpContextAccessor.HttpContext.Session.SetString(sessionKey, token);
		}
		#endregion
	}

	public class StaticBearerTokenProvider : BaseBearerTokenProvider
	{
		#region Fields
		protected string token;
		#endregion

		#region Constructors
		public StaticBearerTokenProvider(string token)
			: base()
		{
			this.token = token;
		}
		#endregion

		#region Helpers
		protected override async Task<string> loadBearerToken()
		{
			return token;
		}

		protected override async Task setBearerToken(string token)
		{
			this.token = token;
		}
		#endregion
	}
}
