namespace Manager.Integration.Test.Helpers
{
    public static class SecurityHelper
    {
        public static string GetLoggedInUser()
        {
            return System.Security.Principal.WindowsIdentity.GetCurrent()
                .Name;
        }
    }
}