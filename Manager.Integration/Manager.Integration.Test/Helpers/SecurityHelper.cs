using System.Security.Principal;

namespace Manager.Integration.Test.Helpers
{
    public static class SecurityHelper
    {
        public static string GetLoggedInUser()
        {
            return WindowsIdentity.GetCurrent()
                .Name;
        }
    }
}