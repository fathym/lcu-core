using System.Collections.Generic;

namespace LCU.Hosting.Options
{
    public class LCUStartupIdentityOptions
    {
        public virtual List<string> CustomPolicies { get; set; }

        public virtual LCUMicrosoftIdentityOptions OpenID { get; set; }

        public virtual LCUStartupSessionsOptions Sessions { get; set; }
    }

    public class LCUMicrosoftIdentityOptions
    {
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the Azure Active Directory instance, e.g. "https://login.microsoftonline.com".
        /// </summary>
        public string Instance { get; set; } = null!;

        /// <summary>
        /// Gets or sets the tenant ID.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the domain of the Azure Active Directory tenant, e.g. contoso.onmicrosoft.com.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the edit profile user flow name for B2C, e.g. b2c_1_edit_profile.
        /// </summary>
        public string EditProfilePolicyId { get; set; }

        /// <summary>
        /// Gets or sets the sign up or sign in user flow name for B2C, e.g. b2c_1_susi.
        /// </summary>
        public string SignUpSignInPolicyId { get; set; }

        /// <summary>
        /// Gets or sets the reset password user flow name for B2C, e.g. B2C_1_password_reset.
        /// </summary>
        public string ResetPasswordPolicyId { get; set; }

        /// <summary>
        /// Gets the default user flow (which is signUpsignIn).
        /// </summary>
        public string DefaultUserFlow => SignUpSignInPolicyId;

        /// <summary>
        /// Enables legacy ADAL cache serialization and deserialization.
        /// Performance improvements when working with MSAL only apps.
        /// Set to true if you have a shared cache with ADAL apps.
        /// </summary>
        /// The default is <c>false.</c>
        public bool LegacyCacheCompatibilityEnabled { get; set; }

        /// <summary>
        /// Is considered B2C if the attribute SignUpSignInPolicyId is defined.
        /// </summary>
        internal bool IsB2C
        {
            get => !string.IsNullOrWhiteSpace(DefaultUserFlow);
        }

        /// <summary>
        /// Specifies if the x5c claim (public key of the certificate) should be sent to the STS.
        /// Sending the x5c enables application developers to achieve easy certificate rollover in Azure AD:
        /// this method will send the public certificate to Azure AD along with the token request,
        /// so that Azure AD can use it to validate the subject name based on a trusted issuer policy.
        /// This saves the application admin from the need to explicitly manage the certificate rollover
        /// (either via portal or PowerShell/CLI operation). For details see https://aka.ms/msal-net-sni.
        /// </summary>
        /// The default is <c>false.</c>
        public bool SendX5C { get; set; }

        /// <summary>
        /// Requests an auth code for the frontend (SPA using MSAL.js for instance). 
        /// See https://aka.ms/msal-net/spa-auth-code for details.
        /// </summary>
        /// The default is <c>false.</c>
        public bool WithSpaAuthCode { get; set; }

        /// <summary>
        /// Daemon applications can validate a token based on roles, or using the ACL-based authorization
        /// pattern to control tokens without a roles claim. If using ACL-based authorization,
        /// Microsoft Identity Web will not throw if roles or scopes are not in the Claims.
        /// For details see https://aka.ms/ms-identity-web/daemon-ACL.
        /// </summary>
        /// The default is <c>false.</c>
        public bool AllowWebApiToBeAuthorizedByACL { get; set; }

        /// <summary>
        /// Used, when deployed to Azure, to specify explicitly a user assigned managed identity.
        /// See https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/how-to-manage-ua-identity-portal.
        /// </summary>
        public string UserAssignedManagedIdentityClientId { get; set; }
    }
}
