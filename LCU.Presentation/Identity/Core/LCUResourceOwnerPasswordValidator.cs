using IdentityServer4.Models;
using IdentityServer4.Validation;
using LCU.Graphs.Registry.Enterprises.Identity;
using System;
using System.Threading.Tasks;

namespace LCU.Presentation.Identity.Core
{
	public class LCUResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
	{
		#region Fields
		protected readonly IdentityGraph identity;
		#endregion

		#region Constructors
		public LCUResourceOwnerPasswordValidator(IdentityGraph identity)
		{
			this.identity = identity;
		}
		#endregion

		#region API Methods
		public virtual async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
		{
			try
			{
				var status = await identity.Validate("", context.UserName, context.Password);

				if (status)
				{
					var userId = status.Metadata["UserID"].ToString();

					var claims = await identity.GetClaims(userId);

					context.Result = new GrantValidationResult(subject: userId, authenticationMethod: "LCU",
						claims: claims);
				}
				else
					context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, status.Message);
			}
			catch (Exception ex)
			{
				context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid username or password");
			}
		}
		#endregion
	}
}
