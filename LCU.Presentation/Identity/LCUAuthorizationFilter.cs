using Fathym;
using LCU.Presentation.Enterprises;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Linq;
using System.Security.Claims;

namespace LCU.Presentation.Identity
{
    public class LCUAuthorizationFilter : FilterAttribute, IAsyncAuthorizationFilter
    {
        public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var appCtx = context.HttpContext.ResolveContext<ApplicationContext>(ApplicationContext.CreateLookup(context.HttpContext));

            var lcuAuthCtxt = context.HttpContext.ResolveContext<LCUAuthorizationContext>(LCUAuthorizationContext.Lookup);
            
            var isIdentityPath = context.HttpContext.Request.PathBase.Value.ToLower().StartsWith("/.identity");

            if (!isIdentityPath && appCtx.LookupConfig.IsPrivate && !context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

                var properties = new AuthenticationProperties();

                if (!lcuAuthCtxt.Schemes.IsNullOrEmpty())
                    foreach (var scheme in lcuAuthCtxt.Schemes)
                        await context.HttpContext.ChallengeAsync(scheme, properties);
                else
                    await context.HttpContext.ChallengeAsync(properties);
            }
        }
    }
}
