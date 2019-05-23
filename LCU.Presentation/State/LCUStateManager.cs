using LCU.Graphs.Registry.Enterprises.State;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Presentation.State
{
	public class LCUStateManager
	{
		#region Fields
		protected readonly CloudBlobClient blobClient;

		protected readonly CloudBlobContainer container;
		#endregion

		#region Constructors
		public LCUStateManager(CloudStorageAccount storageAccount)
		{
			blobClient = storageAccount.CreateCloudBlobClient();

			container = blobClient.GetContainerReference("state");
		}
		#endregion

		#region API Methods
		public virtual CloudBlockBlob LoadStateConfigRef(string apiKey, string state)
		{
			return state.IsNullOrEmpty() ? null : LoadStateRef(apiKey, state, "__config.lcu", null);
		}

		public virtual CloudBlockBlob LoadStateRef(string apiKey, string state, string key, string username)
		{
			var dirPath = $"{apiKey}/{state}";

			if (!username.IsNullOrEmpty())
				dirPath = $"{dirPath}/{username}";

			var dir = container.GetDirectoryReference(dirPath);

			return dir.GetBlockBlobReference(key);
		}

		public virtual async Task<LCUStateConfiguration> LoadStateConfig(string apiKey, string state)
		{
			var cfgRef = LoadStateConfigRef(apiKey, state);

			return await LoadState<LCUStateConfiguration>(cfgRef);
		}

		public virtual async Task<T> LoadState<T>(CloudBlockBlob stateRef)
		{
			if (await stateRef.ExistsAsync())
			{
				var stateStr = await stateRef.DownloadTextAsync();

				return stateStr.FromJSON<T>();
			}
			else
				return default(T);
		}

		public virtual async Task<List<string>> ListStateContainers(string apiKey)
		{
			var dir = container.GetDirectoryReference(apiKey);

			var continuation = new BlobContinuationToken();

			var states = new List<string>();

			do
			{
				var blobSegs = await dir.ListBlobsSegmentedAsync(new BlobContinuationToken());

				states.AddRange(blobSegs.Results.Select(bs => ((CloudBlobDirectory)bs)?.Uri.Segments.Last().Trim('/')));

				continuation = blobSegs.ContinuationToken;
			}
			while (continuation != null);

			return states;
		}

		public virtual async Task SaveState<T>(CloudBlockBlob stateRef, T state)
		{
			await stateRef.UploadTextAsync(state.ToJSON());
		}
		#endregion
	}
}
