using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace ClickOnceSign
{
    public class Sign
    {
        private static String mage = "mage.exe";
        private static StreamWriter logFile;

        public static bool CoSign(
            String appDir, String application, String manifest, 
            String providerUrl, String certFile, String password)
        {
            String appUrl = providerUrl;

            if (!appUrl.EndsWith(application))
            {
                if (!appUrl.EndsWith("/"))
                    appUrl += "/";
                appUrl += application;
            }

            try
            {
                DateTime now = DateTime.Now;
                logFile = new StreamWriter("sign" + now.Hour + now.Minute + now.Second + now.Millisecond + ".log");

                logFile.WriteLine("ClickOnceSign logfile");
                logFile.WriteLine("appDir: " + appDir);
                logFile.WriteLine("application: " + application);
                logFile.WriteLine("manifest: " + manifest);
                logFile.WriteLine("providerUrl: " + providerUrl);
                logFile.WriteLine("certFile: " + certFile);
                
                CheckPreReqs(appDir, application, manifest, certFile);
                String tempDir = GetTempDirectory();
                CopyDir(appDir, tempDir);
                String cd = ChangeDirectory(tempDir);
                mage = cd + "\\mage.exe";
                UpdateManifest(manifest);
                SignFile(manifest, certFile, password);
                UpdateProvider(application, appUrl);
                UpdateAppManifestRef(application, manifest);
                SignFile(application, certFile, password);
                ReinstallApplication(appDir, tempDir, application, manifest);
                ChangeDirectory(cd);
                DeleteTempDir(tempDir);
            }
            catch (Exception e)
            {
                logFile.WriteLine("EXCEPTION: " + e);
                MessageBox.Show(e.Message, "Failed");
                logFile.Close();
                return false;
            }

            logFile.Close();
            // MessageBox.Show("Successfully signed ClickOnce deployment files in directory: " + appDir, "Signed");
            return true;
        }

        private static void UpdateManifest(String manifest)
        {
            RunExe(mage, "-u " + manifest, "Failed to update manifest: " + manifest);
        }

        private static void DeleteTempDir(String tempDir)
        {
            try
            {
                Directory.Delete(tempDir, true);
            }
            catch (Exception)
            {
                throw new Exception("Failed to delete temporary directory: " + tempDir);
            }
        }

        private static void ReinstallApplication(String appDir, String tempDir, String application, String manifest)
        {
            try
            {
                File.Copy(Path.Combine(tempDir, application), Path.Combine(appDir, application), true);
                File.Copy(Path.Combine(tempDir, manifest), Path.Combine(appDir, manifest), true);
            }
            catch (Exception)
            {
                throw new Exception("Failed to install application or manifest at: " + appDir);
            }
        }

        private static void SignFile(String file, String certFile, String password) 
        {
            String signString = " -CertFile " + "\"" + certFile + "\"";
            if (password != null && password.Length > 0)            
                signString += " -Password " + "\"" + password + "\"";
            RunExe(mage, "-Sign " + file + signString, "Failed to sign file '" + file + "' with certificate file '" + certFile + "'");
        }

        private static void UpdateAppManifestRef(String application, String manifest)
        {
            //TODO: Check why publisher is needed, the file is correct at first, but publisher is replaced with Hewlet Packard

            RunExe(mage, "-u " + application + " -AppManifest " + manifest + " -pub \"Teleopti\""
                , "Failed to update application '" + application + "' with new manifest ref");
        }

        private static void UpdateProvider(String application, String appUrl)
        {
            RunExe(mage, "-u " + application + " -ProviderUrl " + appUrl, "Failed to update provider url in file '" + application + "'");
        }

        private static void RunExe(String exe, String arg, String errorMessage)
        {
            try 
            {
                RunProgram(exe, arg);
            }
            catch (Exception) 
            {
                throw new Exception(errorMessage);
            }
        }
        
        private static void CheckPreReqs(String appDir, String application, String manifest, String certFile)
        {
            if (!File.Exists(Path.Combine(appDir, application)))
                throw new Exception("Application file do not exists: " + Path.Combine(appDir, application));

            if (!File.Exists(Path.Combine(appDir, manifest)))
                throw new Exception("Manifest file do not exists: " + Path.Combine(appDir, manifest));
    
            if (!File.Exists(certFile))            
                throw new Exception("Certificate file do not exists: " + certFile);         
        }

        private static String GetTempDirectory()
        {
            string path = "";
            try
            {
                path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                logFile.WriteLine("Creating directory:" + path);

                Directory.CreateDirectory(path);                
            }
            catch (Exception e)
            {
                throw new Exception("Failed to create temporary direcory " + path + "\n(" + e.Message + ")");
            }
            return path;
        }

        private static String ChangeDirectory(String newCd)
        {
            try
            {
                logFile.WriteLine("Change Directory to: " + newCd);
                String oldCd = System.Environment.CurrentDirectory;
                System.Environment.CurrentDirectory = newCd;
                return oldCd;
            }
            catch (Exception)
            {
                throw new Exception("Failed to change to directory: " + newCd);
            }
        }

        private static void RunProgram(String path, String args)
        {
            //TODO: Check for spaces in arguments
           // MessageBox.Show("RUN: " + path + " " + args + "(" + System.Environment.CurrentDirectory + ")");

            logFile.WriteLine("Executing: " + path + " " + args);

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
               //  MessageBox.Show("Failed! " + p.ExitCode);
                logFile.WriteLine("Execute failed");
                throw new Exception("Process error code: " + p.ExitCode + " (" + path + " " + args + ")");
            }
        }

        private static void CopyDir(String origDir, String destDir)
        {
            // TODO: Check dir exists
            // TODO: Check file copy stuff (check replace)

            logFile.WriteLine("Copying dir: " + origDir + " -> " + destDir);

            if (origDir.EndsWith("\\"))
                origDir = origDir.Substring(0, origDir.Length - 1);
            
            try
            {              
                foreach (String file in Directory.GetFiles(origDir))
                {
                    String newfile = file.Replace(origDir, destDir);
                    if (newfile.EndsWith(".deploy"))
                    {
                        newfile = newfile.Substring(0, newfile.Length - 7);
                    }

                    File.Copy(file, newfile, true);
                }

                foreach (String dir in Directory.GetDirectories(origDir))
                {
                    String newDir = Path.Combine(destDir, Path.GetFileName(dir));
                    Directory.CreateDirectory(newDir);
                    CopyDir(dir, newDir);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to copy from '" + origDir
                    + "' to temporary directory '" + destDir + "'" + "\n( + " + e.Message + ")");
            }
        }
    }
}
