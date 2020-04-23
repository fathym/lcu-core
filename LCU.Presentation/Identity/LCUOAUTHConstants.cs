using System;
using System.Collections.Generic;
using System.Text;

namespace LCU.Presentation.Identity
{
    public class LCUOAUTHConstants
    {
        public const string SignUpPolicy = "B2C_1_LCU_SignUp";
        public const string SignInPolicy = "B2C_1_LCU_SignIn";
        public const string SignUpSignInPolicy = "B2C_1A_SignUp_SignIn";
        public const string EditProfilePolicy = "B2C_1_LCU_Edit_Profile";
        public const string OAUTHPolicySessionKey = "OAUTH:Process";
        public const string OAUTHTokenSessionKey = "OAUTH:Token";
        public const string OAUTHGraphTokenKey = "AD-B2C-ACCESS-TOKEN";
    }
}
