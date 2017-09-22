using log4net;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ClickOnceSign
{
    static class Program
    {
        private static ILog logFile = LogManager.GetLogger(typeof(Program));

        private static String application = "";
        private static String manifest = "";
        private static String providerUrl = "";
        private static String certFile = "";
        private static String password = "";
        private static String appDir = "";
        private static bool silent = false;
        private static bool fastClose = false;
        private static bool disabledTextboxes = false;

        [STAThread]
        static void Main(String[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            if (!ParseCommandLine(args))
                Usage();

            CheckProviderUrl();

            if (silent)
            {
                bool r = Sign.CoSign(appDir, application, manifest, providerUrl, certFile, password);
                Environment.Exit(r ? 0 : 1);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 form = new Form1();

            form.SetApplication(application);
            form.SetManifest(manifest);
            form.SetProviderUrl(providerUrl);
            form.SetCertFile(certFile);
            form.SetPassword(password);
            form.SetAppDir(appDir);
            form.SetFastClose(fastClose);
            
            if (disabledTextboxes)
                form.DisableTextboxes();

            if (appDir.Length == 0)
                form.SetAppDir(System.Environment.CurrentDirectory);

            Application.Run(form);
        }

        private static void Usage()
        {
            String usage = "Usage: ClickOnceSign.exe [-f] [-d] [-s] [-a application] [-m manifest] [-u providerurl]"
                + "[-c certfile] [-p password] [-dir appdir]";
            logFile.Warn(usage);
            Environment.Exit(1);
        }

        private static void CheckProviderUrl()
        {
            // TODO: Check how to handle this correctly

            if (providerUrl == null)
                return;

            //if (providerUrl.StartsWith("http://localhost"))
            //{
            //    providerUrl = providerUrl.Replace("http://localhost", "http://" + System.Net.Dns.GetHostName());
            //}
            //if (providerUrl.StartsWith("http://*"))
            //{
            //    providerUrl = providerUrl.Replace("http://*", "http://" + System.Net.Dns.GetHostName());
            //}
        }

        private static bool getSingleArg(List<String> list, String arg)
        {
            bool r = list.Contains(arg);
            if (r)
                list.Remove(arg);
            return r;                
        }

        private static String getDoubleArg(List<String> list, String arg)
        {
            if (list.Contains(arg))
            {
                int i = list.IndexOf(arg);
                if (i + 1 < list.Count)
                {
                    String r = list[i + 1];
                    list.RemoveAt(i + 1);
                    list.RemoveAt(i);
                    return r;
                }
                else
                    throw new Exception("Invalid Command Line");
            }

            return "";
        }

        private static bool ParseCommandLine(String[] args)
        {         
            List<String> a = new List<String>(args);

            try
            {
                fastClose = getSingleArg(a, "-f");
                disabledTextboxes = getSingleArg(a, "-d");
                silent = getSingleArg(a, "-s");

                application = getDoubleArg(a, "-a");
                manifest = getDoubleArg(a, "-m");
                providerUrl = getDoubleArg(a, "-u");
                certFile = getDoubleArg(a, "-c");
                password = getDoubleArg(a, "-p");
                appDir = getDoubleArg(a, "-dir");
            }
            catch (Exception)
            {
                return false;
            }

            return a.Count == 0;
        }
    }
}
