using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace LCU.Testing
{
	public class GenericTests
	{
		#region Fields
		protected IConfiguration config;
		#endregion

		#region Constructors
		public GenericTests()
		{
			setupConfiguration();
		}
		#endregion

		#region Helpers
		protected virtual ILogger<T> createLogger<T>()
		{
			return new LoggerFactory().CreateLogger<T>();
		}

		protected virtual DirectoryInfo getDirectory(string path)
		{
			var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

			var dirPath = Path.Combine(localAppData, path);

			return new DirectoryInfo(path);
		}

		protected virtual FileInfo getFile(string path)
		{
			var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

			var dirPath = Path.Combine(localAppData, path);

			return new FileInfo(path);
		}

		protected virtual void setupConfiguration()
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile("test.settings.json")
				.AddUserSecrets(GetType().Assembly)
				.AddEnvironmentVariables()
				.Build();

			this.config = config;
		}
		#endregion
	}
}
