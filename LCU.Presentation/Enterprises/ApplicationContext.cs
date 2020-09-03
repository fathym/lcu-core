using Fathym;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using LCU.Graphs.Registry.Enterprises;

namespace LCU.Presentation.Enterprises
{
	[Serializable]
	public class ApplicationContext
	{
		protected const string Lookup = "<DAF:Application>";

		//public virtual APIProxy API { get; set; }

		//public virtual Guid BuildingForID { get; set; }

		public virtual Guid EnterpriseID { get; set; }

		public virtual string EnterpriseLookup { get; set; }

		public virtual Guid ID { get; set; }

		public virtual ApplicationLookupConfiguration LookupConfig { get; set; }

		public virtual string Name { get; set; }

		public virtual int Priority { get; set; }

		//public virtual ProxyConnection Proxy { get; set; }

		//public virtual SecurityContext Security { get; set; }

		//public virtual ViewStartupConfiguration View { get; set; }

		#region API Methods
		public static string CreateLookup(HttpContext context)
		{
			if (context.Request.Headers.ContainsKey("f-daf-application-lookup"))
				return context.Request.Headers["f-daf-application-lookup"].ToString();

			var path = context.Request.Path;

			var queryString = context.Request.QueryString.Value;

			var userAgent = context.Request.GetUserAgent();

			var id = context.User?.Identity?.As<ClaimsIdentity>();

			var claims = id?.Claims?.Select(c => $"{c.Type}|{c.Value}").ToJSON() ?? String.Empty;

			var appLookup = $"{path}|{queryString}|{userAgent}|{claims}".ToMD5Hash();

			return $"{Lookup}|{appLookup}";
		}
		#endregion
	}
}
