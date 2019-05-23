using IdentityServer4.Models;
using IdentityServer4.Services;
using LCU.Graphs.Registry.Enterprises.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LCU.Presentation.Identity.Core
{
	public class LCUProfileService : IProfileService
	{
		#region Fields
		protected readonly IdentityGraph identity;
		#endregion

		#region Constructors
		public LCUProfileService(IdentityGraph identity)
		{
			this.identity = identity;
		}

		//public LCUProfileService()
		//{
		//    this.identity = new IdentityGraph(new Graphs.LCUGraphConfig());
		//}
		#endregion

		#region API Methods
		public virtual async Task GetProfileDataAsync(ProfileDataRequestContext context)
		{
			try
			{
				var userId = context.Subject.Identity.Name ?? context.Subject.Claims.FirstOrDefault(x => x.Type == "user_id" || x.Type == ClaimTypes.NameIdentifier)?.Value;

				var claims = await identity.GetClaims(userId);

				//	TODO:  Couldn't figure out where to set requested claim types, once we do, can remove IsNullOrEmpty check
				context.IssuedClaims = claims.Where(x => context.RequestedClaimTypes.Contains(x.Type)).ToList();
                //context.IssuedClaims = claims.Where(x => context.RequestedClaimTypes.IsNullOrEmpty() || context.RequestedClaimTypes.Contains(x.Type)).ToList();

                if (!userId.IsNullOrEmpty())
                {
                    context.AddRequestedClaims(claims);

                    context.IssuedClaims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
                }
			}
			catch (Exception ex)
			{
				//log your error
			}
		}

		public virtual async Task IsActiveAsync(IsActiveContext context)
		{
			try
			{
				var userId = context.Subject.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;

				if (!userId.IsNullOrEmpty())
				{
					var status = await identity.Exists(userId);

					context.IsActive = status;
				}
			}
			catch (Exception ex)
			{
				//handle error logging
			}
		}
		#endregion
	}
}
