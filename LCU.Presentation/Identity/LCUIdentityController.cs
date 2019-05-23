using Fathym;
using Fathym.Presentation.MVC;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using LCU.Identity;
using LCU.Presentation.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace LCU.Presentation.Identity
{
	public class LCUIdentityController : FathymController
	{
		#region Fields
		protected readonly IClientStore clientStore;

		protected readonly IDeviceFlowInteractionService deviceIntSvc;

		protected readonly IEventService events;

		protected readonly IIdentityServerInteractionService idIntSvc;

		protected readonly IResourceStore resourceStore;

		protected readonly IAuthenticationSchemeProvider schemeProvider;

		protected readonly IUserStore userStore;
		#endregion

		#region Properties
		public virtual bool AllowLocalLogin { get; set; }

		public virtual bool AllowRememberLogin { get; set; }

		public virtual bool AutomaticRedirectAfterSignOut { get; set; }

		public virtual bool EnableOfflineAccess { get; set; }

		public virtual bool IncludeWindowsGroups { get; set; }

		public virtual TimeSpan RememberMeLoginDuration { get; set; }

		public virtual bool ShowLogoutPrompt { get; set; }

		public virtual string WindowsAuthenticationSchemeName { get; set; }
		#endregion

		#region Constructors
		public LCUIdentityController(IIdentityServerInteractionService idIntSvc, IDeviceFlowInteractionService deviceIntSvc,
			IEventService events, IClientStore clientStore, IAuthenticationSchemeProvider schemeProvider,
			IResourceStore resourceStore, IUserStore userStore)
		{
			this.clientStore = clientStore;

			this.deviceIntSvc = deviceIntSvc;

			this.events = events;

			this.idIntSvc = idIntSvc;

			this.resourceStore = resourceStore;

			this.schemeProvider = schemeProvider;

			this.userStore = userStore;

			AllowLocalLogin = true;

			AllowRememberLogin = true;

			AutomaticRedirectAfterSignOut = false;

			EnableOfflineAccess = true;

			IncludeWindowsGroups = false;

			RememberMeLoginDuration = TimeSpan.FromDays(30);

			ShowLogoutPrompt = true;

			WindowsAuthenticationSchemeName = Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme;
		}
		#endregion

		#region API Methods
		public virtual async Task<IActionResult> Diagnostic()
		{
			var localAddresses = new string[] { "127.0.0.1", "::1", HttpContext.Connection.LocalIpAddress.ToString() };

			var status = Status.Initialized;

			if (!localAddresses.Contains(HttpContext.Connection.RemoteIpAddress.ToString()))
				return Json(new { Status = status });
			else
			{
				var result = await HttpContext.AuthenticateAsync();

				dynamic diag = new { Status = Status.Success, AuthResult = result, Clients = new string[] { } };

				if (result.Properties.Items.ContainsKey("client_list"))
				{
					var encoded = result.Properties.Items["client_list"];

					var bytes = Base64Url.Decode(encoded);

					var value = Encoding.UTF8.GetString(bytes);

					diag.Clients = value.FromJSON<string[]>();
				}

				return Json(diag);
			}
		}

		/// <summary>
		/// initiate roundtrip to external authentication provider
		/// </summary>
		[HttpGet]
		public virtual async Task<IActionResult> ExternalChallenge(string provider, string returnUrl)
		{
			if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

			// validate returnUrl - either it is a valid OIDC URL or back to a local page
			if (Url.IsLocalUrl(returnUrl) == false && idIntSvc.IsValidReturnUrl(returnUrl) == false)
			{
				// user might have clicked on a malicious link - should be logged
				throw new Exception("invalid return URL");
			}

			if (WindowsAuthenticationSchemeName == provider)
			{
				// windows authentication needs special handling
				return await processWindowsLoginAsync(returnUrl);
			}
			else
			{
				// start challenge and roundtrip the return URL and scheme 
				var props = new AuthenticationProperties
				{
					RedirectUri = Url.Action(nameof(ExternalCallback)),
					Items =
					{
						{ "returnUrl", returnUrl },
						{ "scheme", provider },
					}
				};

				return Challenge(props, provider);
			}
		}

		/// <summary>
		/// Post processing of external authentication
		/// </summary>
		[HttpGet]
		public virtual async Task<IActionResult> ExternalCallback()
		{
			// read external identity from the temporary cookie
			var result = await HttpContext.AuthenticateAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);
			if (result?.Succeeded != true)
			{
				throw new Exception("External authentication error");
			}

			// lookup our user and external provider info
			var (user, provider, providerUserId, claims) = await FindUserFromExternalProvider(result);
			if (user == null)
			{
				// this might be where you might initiate a custom workflow for user registration
				// in this sample we don't show how that would be done, as our sample implementation
				// simply auto-provisions new external user
				user = await autoProvisionUser(provider, providerUserId, claims);
			}

			// this allows us to collect any additonal claims or properties
			// for the specific prtotocols used and store them in the local auth cookie.
			// this is typically used to store data needed for signout from those protocols.
			var additionalLocalClaims = new List<Claim>();
			var localSignInProps = new AuthenticationProperties();
			processLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);
			processLoginCallbackForWsFed(result, additionalLocalClaims, localSignInProps);
			processLoginCallbackForSaml2p(result, additionalLocalClaims, localSignInProps);

			// issue authentication cookie for user
			await events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.SubjectId, user.Username));

			await HttpContext.SignInAsync(user.SubjectId, user.Username, provider, localSignInProps, additionalLocalClaims.ToArray());

			// delete temporary cookie used during external authentication
			await HttpContext.SignOutAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);

			// retrieve return URL
			var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

			// check if external login is in the context of an OIDC request
			var context = await idIntSvc.GetAuthorizationContextAsync(returnUrl);
			if (context != null)
			{
				if (await clientStore.IsPkceClientAsync(context.ClientId))
				{
					// if the client is PKCE then we assume it's native, so this change in how to
					// return the response is for better UX for the end user.

					return Json(new
					{
						Status = Status.Success,
						RedirectURL = returnUrl
					});
				}
			}

			return Redirect(returnUrl);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> HandleDeviceCode(DeviceAuthorizationInputModel model)
		{
			if (model == null)
				throw new ArgumentNullException(nameof(model));

			var result = await processConsent(model);

			return Json(new
			{
				Consent = result,
				Status = Status.Success
			});
		}

		[AllowAnonymous]
		//[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Login()
		{
			return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, "oidc");
		}

		//	TODO:	Remove Allow Anon attribute
		[AllowAnonymous]
		public virtual async Task<IActionResult> LastError(string errorId)
		{
			var message = await idIntSvc.GetErrorContextAsync(errorId);

			return Json(message);
		}

		[AllowAnonymous]
		public virtual async Task<IActionResult> Logout()
		{
			return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, "oidc");
		}

		[HttpPost]
		//[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> ProcessConsent(ConsentInputModel model)
		{
			var result = await processConsent(model);

			if (result.IsRedirect)
			{
				if (await clientStore.IsPkceClientAsync(result.ClientId))
				{
					// if the client is PKCE then we assume it's native, so this change in how to
					// return the response is for better UX for the end user.
					//return View("Redirect", new RedirectViewModel { RedirectUrl = result.RedirectUri });

					return Json(new
					{
						Status = Status.Success,
						RedirectURL = result.RedirectUri
					});
				}

				return Redirect(result.RedirectUri);
			}

			if (result.HasValidationError)
			{
				ModelState.AddModelError(string.Empty, result.ValidationError);
			}

			if (result.ShowView)
			{
				return View("Index", result.ViewModel);
			}

			return View("Error");
		}

		[HttpPost]
		//[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> RevokeGrant(string clientId)
		{
			await idIntSvc.RevokeUserConsentAsync(clientId);

			await events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), clientId));

			return RedirectToAction("Index");
		}
		#endregion

		#region Helpers
		protected virtual ScopeViewModel createScopeViewModel(IdentityResource identity, bool check)
		{
			return new ScopeViewModel
			{
				Name = identity.Name,
				DisplayName = identity.DisplayName,
				Description = identity.Description,
				Emphasize = identity.Emphasize,
				Required = identity.Required,
				Checked = check || identity.Required
			};
		}

		protected virtual ScopeViewModel createScopeViewModel(Scope scope, bool check)
		{
			return new ScopeViewModel
			{
				Name = scope.Name,
				DisplayName = scope.DisplayName,
				Description = scope.Description,
				Emphasize = scope.Emphasize,
				Required = scope.Required,
				Checked = check || scope.Required
			};
		}

		protected virtual ScopeViewModel getOfflineAccessScope(bool check)
		{
			return new ScopeViewModel
			{
				Name = IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess,
				DisplayName = "Offline Access",
				Description = "Access to your applications and resources, even when you are offline",
				Emphasize = true,
				Checked = check
			};
		}

		#region Account Helpers

		#endregion

		#region Consent Helpers
		protected virtual async Task<ConsentViewModel> buildConsentViewModel(string returnUrl, ConsentInputModel model = null)
		{
			var request = await idIntSvc.GetAuthorizationContextAsync(returnUrl);
			if (request != null)
			{
				var client = await clientStore.FindEnabledClientByIdAsync(request.ClientId);
				if (client != null)
				{
					var resources = await resourceStore.FindEnabledResourcesByScopeAsync(request.ScopesRequested);

					if (resources != null && (resources.IdentityResources.Any() || resources.ApiResources.Any()))
					{
						return createConsentViewModel(model, returnUrl, request, client, resources);
					}
					else
					{
						//_logger.LogError("No scopes matching: {0}", request.ScopesRequested.Aggregate((x, y) => x + ", " + y));
					}
				}
				else
				{
					//_logger.LogError("Invalid client id: {0}", request.ClientId);
				}
			}
			else
			{
				//_logger.LogError("No consent request matching request: {0}", returnUrl);
			}

			return null;
		}

		protected virtual ConsentViewModel createConsentViewModel(ConsentInputModel model, string returnUrl,
			AuthorizationRequest request, Client client, Resources resources)
		{
			var vm = new ConsentViewModel
			{
				RememberConsent = model?.RememberConsent ?? true,
				ScopesConsented = model?.ScopesConsented ?? Enumerable.Empty<string>(),
				ReturnURL = returnUrl,
				ClientName = client.ClientName ?? client.ClientId,
				ClientUrl = client.ClientUri,
				ClientLogoUrl = client.LogoUri,
				AllowRememberConsent = client.AllowRememberConsent
			};

			vm.IdentityScopes = resources.IdentityResources.Select(x => createScopeViewModel(x, vm.ScopesConsented.Contains(x.Name) || model == null)).ToArray();

			vm.ResourceScopes = resources.ApiResources.SelectMany(x => x.Scopes).Select(x => createScopeViewModel(x, vm.ScopesConsented.Contains(x.Name) || model == null)).ToArray();

			if (EnableOfflineAccess && resources.OfflineAccess)
			{
				vm.ResourceScopes = vm.ResourceScopes.Union(new ScopeViewModel[] {
					getOfflineAccessScope(vm.ScopesConsented.Contains(IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess) || model == null)
				});
			}

			return vm;
		}

		protected virtual async Task<ProcessConsentResult> processConsent(ConsentInputModel model)
		{
			var result = new ProcessConsentResult();

			// validate return url is still valid
			var request = await idIntSvc.GetAuthorizationContextAsync(model.ReturnURL);

			if (request == null) return result;

			ConsentResponse grantedConsent = null;

			// user clicked 'no' - send back the standard 'access_denied' response
			if (model?.Button == "no")
			{
				grantedConsent = ConsentResponse.Denied;

				// emit event
				await events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), request.ClientId, request.ScopesRequested));
			}
			// user clicked 'yes' - validate the data
			else if (model?.Button == "yes")
			{
				// if the user consented to some scope, build the response model
				if (model.ScopesConsented != null && model.ScopesConsented.Any())
				{
					var scopes = model.ScopesConsented;
					if (EnableOfflineAccess == false)
					{
						scopes = scopes.Where(x => x != IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess);
					}

					grantedConsent = new ConsentResponse
					{
						RememberConsent = model.RememberConsent,
						ScopesConsented = scopes.ToArray()
					};

					// emit event
					await events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.ClientId, request.ScopesRequested, grantedConsent.ScopesConsented, grantedConsent.RememberConsent));
				}
				else
				{
					result.ValidationError = "You must pick at least one permission";
				}
			}
			else
			{
				result.ValidationError = "Invalid selection";
			}

			if (grantedConsent != null)
			{
				// communicate outcome of consent back to identityserver
				await idIntSvc.GrantConsentAsync(request, grantedConsent);

				// indicate that's it ok to redirect back to authorization endpoint
				result.RedirectUri = model.ReturnURL;

				result.ClientId = request.ClientId;
			}
			else
			{
				// we need to redisplay the consent UI
				result.ViewModel = await buildConsentViewModel(model.ReturnURL, model);
			}

			return result;
		}
		#endregion

		#region Device Helpers
		protected virtual async Task<DeviceAuthorizationViewModel> buildDevicesViewModelAsync(string userCode, DeviceAuthorizationInputModel model = null)
		{
			var request = await deviceIntSvc.GetAuthorizationContextAsync(userCode);
			if (request != null)
			{
				var client = await clientStore.FindEnabledClientByIdAsync(request.ClientId);
				if (client != null)
				{
					var resources = await resourceStore.FindEnabledResourcesByScopeAsync(request.ScopesRequested);

					if (resources != null && (resources.IdentityResources.Any() || resources.ApiResources.Any()))
					{
						return createDevicesConsentViewModel(userCode, model, client, resources);
					}
					else
					{
						//_logger.LogError("No scopes matching: {0}", request.ScopesRequested.Aggregate((x, y) => x + ", " + y));
					}
				}
				else
				{
					//_logger.LogError("Invalid client id: {0}", request.ClientId);
				}
			}

			return null;
		}

		protected virtual DeviceAuthorizationViewModel createDevicesConsentViewModel(string userCode, DeviceAuthorizationInputModel model, Client client, Resources resources)
		{
			var vm = new DeviceAuthorizationViewModel
			{
				UserCode = userCode,

				RememberConsent = model?.RememberConsent ?? true,
				ScopesConsented = model?.ScopesConsented ?? Enumerable.Empty<string>(),

				ClientName = client.ClientName ?? client.ClientId,
				ClientUrl = client.ClientUri,
				ClientLogoUrl = client.LogoUri,
				AllowRememberConsent = client.AllowRememberConsent
			};

			vm.IdentityScopes = resources.IdentityResources.Select(x => createScopeViewModel(x, vm.ScopesConsented.Contains(x.Name) || model == null)).ToArray();

			vm.ResourceScopes = resources.ApiResources.SelectMany(x => x.Scopes).Select(x => createScopeViewModel(x, vm.ScopesConsented.Contains(x.Name) || model == null)).ToArray();

			if (EnableOfflineAccess && resources.OfflineAccess)
			{
				vm.ResourceScopes = vm.ResourceScopes.Union(new[]
				{
					getOfflineAccessScope(vm.ScopesConsented.Contains(IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess) || model == null)
				});
			}

			return vm;
		}

		protected virtual async Task<ProcessConsentResult> processConsent(DeviceAuthorizationInputModel model)
		{
			var result = new ProcessConsentResult();

			var request = await deviceIntSvc.GetAuthorizationContextAsync(model.UserCode);

			if (request == null) return result;

			ConsentResponse grantedConsent = null;

			// user clicked 'no' - send back the standard 'access_denied' response
			if (model.Button == "no")
			{
				grantedConsent = ConsentResponse.Denied;

				// emit event
				await events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), request.ClientId, request.ScopesRequested));
			}
			// user clicked 'yes' - validate the data
			else if (model.Button == "yes")
			{
				// if the user consented to some scope, build the response model
				if (model.ScopesConsented != null && model.ScopesConsented.Any())
				{
					var scopes = model.ScopesConsented;
					if (EnableOfflineAccess == false)
					{
						scopes = scopes.Where(x => x != IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess);
					}

					grantedConsent = new ConsentResponse
					{
						RememberConsent = model.RememberConsent,
						ScopesConsented = scopes.ToArray()
					};

					// emit event
					await events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.ClientId, request.ScopesRequested, grantedConsent.ScopesConsented, grantedConsent.RememberConsent));
				}
				else
				{
					result.ValidationError = "You must pick at least one permission";
				}
			}
			else
			{
				result.ValidationError = "Invalid selection";
			}

			if (grantedConsent != null)
			{
				// communicate outcome of consent back to identityserver
				await deviceIntSvc.HandleRequestAsync(model.UserCode, grantedConsent);

				// indicate that's it ok to redirect back to authorization endpoint
				result.RedirectUri = model.ReturnURL;
				result.ClientId = request.ClientId;
			}
			else
			{
				// we need to redisplay the consent UI
				result.ViewModel = await buildDevicesViewModelAsync(model.UserCode, model);
			}

			return result;
		}
		#endregion

		#region External Helpers

		protected virtual async Task<IActionResult> processWindowsLoginAsync(string returnUrl)
		{
			// see if windows auth has already been requested and succeeded
			var result = await HttpContext.AuthenticateAsync(WindowsAuthenticationSchemeName);
			if (result?.Principal is WindowsPrincipal wp)
			{
				// we will issue the external cookie and then redirect the
				// user back to the external callback, in essence, treating windows
				// auth the same as any other external authentication mechanism
				var props = new AuthenticationProperties()
				{
					RedirectUri = Url.Action("Callback"),
					Items =
					{
						{ "returnUrl", returnUrl },
						{ "scheme", WindowsAuthenticationSchemeName },
					}
				};

				var id = new ClaimsIdentity(WindowsAuthenticationSchemeName);
				id.AddClaim(new Claim(JwtClaimTypes.Subject, wp.Identity.Name));
				id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));

				// add the groups as claims -- be careful if the number of groups is too large
				if (IncludeWindowsGroups)
				{
					var wi = wp.Identity as WindowsIdentity;
					var groups = wi.Groups.Translate(typeof(NTAccount));
					var roles = groups.Select(x => new Claim(JwtClaimTypes.Role, x.Value));
					id.AddClaims(roles);
				}

				await HttpContext.SignInAsync(
					IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme,
					new ClaimsPrincipal(id),
					props);
				return Redirect(props.RedirectUri);
			}
			else
			{
				// trigger windows auth
				// since windows auth don't support the redirect uri,
				// this URL is re-triggered when we call challenge
				return Challenge(WindowsAuthenticationSchemeName);
			}
		}

		protected virtual async Task<(LCUUser user, string provider, string providerUserId, IEnumerable<Claim> claims)> FindUserFromExternalProvider(AuthenticateResult result)
		{
			var externalUser = result.Principal;

			// try to determine the unique id of the external user (issued by the provider)
			// the most common claim type for that are the sub claim and the NameIdentifier
			// depending on the external provider, some other claim type might be used
			var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
							  externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
							  throw new Exception("Unknown userid");

			// remove the user id claim so we don't include it as an extra claim if/when we provision the user
			var claims = externalUser.Claims.ToList();
			claims.Remove(userIdClaim);

			var provider = result.Properties.Items["scheme"];
			var providerUserId = userIdClaim.Value;

			// find external user
			var user = await userStore.FindByExternalProvider(provider, providerUserId);

			return (user, provider, providerUserId, claims);
		}

		protected virtual async Task<LCUUser> autoProvisionUser(string provider, string providerUserId, IEnumerable<Claim> claims)
		{
			var user = await userStore.AutoProvisionUser(provider, providerUserId, claims.ToList());
			return user;
		}

		protected virtual void processLoginCallbackForOidc(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
		{
			// if the external system sent a session id claim, copy it over
			// so we can use it for single sign-out
			var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
			if (sid != null)
			{
				localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
			}

			// if the external provider issued an id_token, we'll keep it for signout
			var id_token = externalResult.Properties.GetTokenValue("id_token");
			if (id_token != null)
			{
				localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });
			}
		}

		protected virtual void processLoginCallbackForWsFed(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
		{
		}

		protected virtual void processLoginCallbackForSaml2p(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
		{
		}
		#endregion
		#endregion
	}
}
