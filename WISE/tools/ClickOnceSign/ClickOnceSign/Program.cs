using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ClickOnceSign
{
    static class Program
    {
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
            // Console.WriteLine("HEJSAN");

            if (! ParseCommandLine(args))
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
            MessageBox.Show(usage, "Invalid command line");
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


/*

      static void Main(string[] args) 
        {
            if (args.Length < 4)
            {
                Console.Out.WriteLine("Usage: co_sign.exe application manifest provider_url hostname [certfile [password]]");
                System.Environment.Exit(1);
            }

            String application = args[0];
            String manifest = args[1];
            String provider_url = args[2];
            String hostname = args[3];
            String certfile = "TemporaryKey.pfx";
            
            if (args.Length > 4 && args[4].Length > 0) 
            {
                certfile = args[4];
            }

            String signString = " -CertFile " + certfile;

            if (args.Length > 5 && args[5].Length > 0)
            {
                signString += " -Password " + args[5];
            }
            
            if (provider_url.StartsWith("http://*"))
            {
                provider_url = "http://" + hostname + provider_url.Substring(8);
            }

            if (provider_url.StartsWith("http://localhost"))
            {
                provider_url = "http://" + hostname + provider_url.Substring(16);
            }
         
            try 
            {                //TODO: Make the copy independent of subdir names
                RunProgram("cmd.exe", "/C copy /Y *.deploy *.");
                RunProgram("cmd.exe", "/C copy /Y ar\\*.deploy ar\\*.");
                RunProgram("cmd.exe", "/C copy /Y no\\*.deploy no\\*.");
                RunProgram("cmd.exe", "/C copy /Y sv\\*.deploy sv\\*.");
                RunProgram("cmd.exe", "/C copy /Y zh-chs\\*.deploy zh-chs\\*.");

                RunProgram("mage.exe", "-u " + manifest);
                RunProgram("mage.exe", "-Sign " + manifest + signString);
                RunProgram("mage.exe", "-u " + application + " -ProviderUrl " + provider_url);
                RunProgram("mage.exe", "-u " + application + " -AppManifest " + manifest);
                RunProgram("mage.exe", "-Sign " + application + signString);                  
            }
            catch (Exception e) 
            {
                Console.Out.WriteLine(e);
                System.Environment.Exit(1);
            }

            System.Environment.Exit(0);

            // TODO: Print message if failed to rerun manually
        }

        private static void RunProgram(String path, String args)
        {
            Console.Out.Write(".");
            //Console.Out.WriteLine(path + " " + args);
            ProcessStartInfo ps = new ProcessStartInfo();
            ps.CreateNoWindow = true;
            ps.WindowStyle = ProcessWindowStyle.Hidden;
            ps.FileName = path; 
            ps.Arguments = args;
            //ps.UseShellExecute = false;
            Process p = Process.Start(ps);
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                throw new Exception("Process error code: " + p.ExitCode + " (" + path + " " + args + ")");
            }
        }
    }*/
