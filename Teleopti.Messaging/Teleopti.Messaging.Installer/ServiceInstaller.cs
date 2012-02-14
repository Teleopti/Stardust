using System;
using System.Configuration.Install;
using System.Globalization;

namespace Teleopti.Messaging.Installer
{
    /// <summary>
    /// Summary description for ServiceInstaller.
    /// </summary>
    public class ServiceInstaller
    {

        #region Private Variables

        private string _servicePath;
        private string _serviceName;
        private string _serviceDisplayName;

        #endregion Private Variables

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string svcPath = args[0]; //@"C:\build\service\Debug\Service.exe";
            Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Path is {0} ...", svcPath));
            string svcName = args[1]; // "Service Name";
            Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Service name is {0} ...", svcName));
            string svcDispName = args[2]; // "Service Display Name";
            Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Display name is {0} ...", svcDispName));
            string svcInstall = args[3];
            Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "You are {0}ing ...", svcInstall));

            ServiceInstaller c = new ServiceInstaller();
            try
            {
                if (svcInstall.ToLower() == "install")
                {
                    Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Installing ..."));
                    c.InstallService(svcPath);
                }
                else
                {
                    Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Uninstalling ..."));
                    c.UnInstallService(svcPath);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
            Console.WriteLine("Press any key to exit ...");
            Console.Read();
        }

        /// <summary>
        /// This method installs and runs the service in the service control manager.
        /// </summary>
        /// <param name="svcPath">The SVC path.</param>
        /// <returns>
        /// True if the process went through successfully. False if there was any error.
        /// </returns>
        public bool InstallService(string svcPath)
        {
            SelfInstaller.InstallMe(svcPath);
            return true;
        }

        /// <summary>
        /// This method uninstalls the service  the service conrol manager.
        /// </summary>
        /// <param name="svcPath">The SVC path.</param>
        /// <returns></returns>
        public bool UnInstallService(string svcPath)
        {
            SelfInstaller.UninstallMe(svcPath);
            return true;
        }

        public string ServicePath
        {
            get { return _servicePath; }
            set { _servicePath = value; }
        }

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }

        public string ServiceDisplayName
        {
            get { return _serviceDisplayName; }
            set { _serviceDisplayName = value; }
        }

    }
    public static class SelfInstaller
    {

        public static bool InstallMe(string svcPath)
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { svcPath });
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool UninstallMe(string svcPath)
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { "/u", svcPath });
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}

