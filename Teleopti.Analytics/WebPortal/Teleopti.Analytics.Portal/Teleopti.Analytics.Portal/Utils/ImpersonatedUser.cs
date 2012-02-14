using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using Teleopti.Analytics.Portal.AnalyzerProxy;

namespace Teleopti.Analytics.Portal.Utils
{
    public class ImpersonatedUser : IDisposable
    {

        private string _domain = "";
        private string _username = "";
        private string _password = "";
        private static IntPtr _tokenHandle = new IntPtr(0);
        private static WindowsImpersonationContext _impersonatedUser;

        public ImpersonatedUser()
        {
            _domain = PermissionInformation.AnonymousDomain;
            _username = PermissionInformation.AnonymousUserName;
            _password = PermissionInformation.AnonymousPassword;
            impersonate();
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(string User, string Domain, string Password, int LogonType, int LogonProvider, ref IntPtr Token);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        // If you incorporate this code into a DLL, be sure to demand that it
        // runs with FullTrust.
        /// <summary>
        /// Impersonates another Windows user
        /// </summary>
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        private void impersonate()
        {
            {
                // Use the unmanaged LogonUser function to get the user token for
                // the specified user, domain, and password.
                const int defaultProvider = 0;

                // Passing this parameter causes LogonUser to create a primary token.
                const int interactive = 2;

                _tokenHandle = IntPtr.Zero;

                // ---- Step - 1
                // Call LogonUser to obtain a handle to an access token.
                bool returnValue = LogonUser(_username, _domain, _password, interactive, defaultProvider, ref _tokenHandle);

                if (!returnValue)
                {
                    int ret = Marshal.GetLastWin32Error();
                    throw new System.ComponentModel.Win32Exception(ret);
                }

                // ---- Step - 2
                var newId = new WindowsIdentity(_tokenHandle);

                // ---- Step - 3
                _impersonatedUser = newId.Impersonate();
            }
        }

        // Stops impersonation
        public void Undo()
        {
            _impersonatedUser.Undo();

            // Free the tokens.
            if (_tokenHandle != IntPtr.Zero)
                CloseHandle(_tokenHandle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _domain = null;
                _username = null;
                _password = null;
            }

            if (_impersonatedUser != null)
                Undo();

            _impersonatedUser = null;
        }

        ~ImpersonatedUser()
        {
            Dispose(false);
        }
    }
}