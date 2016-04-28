﻿using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CheckPreRequisites.Checks
{
    public class WebCheck
    {
        private readonly Form1 _form1;

        public WebCheck(Form1 form1)
        {
            _form1 = form1;
        }

        private void checkFeatures()
        {
            if (getFileToRun() == "")
            {
                _form1.printInfo("Can not find a file with features that match the operating system");
                return;
            }

            using (Stream stream = GetType().Assembly.GetManifestResourceStream(getFileToRun()))
            {
                var file = new StreamReader(stream, Encoding.UTF8);
                _form1.printInfo("Checking features and enabling them. If a feature needs downloading it can take some minutes to run this.");
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line))
                        line = line.Trim();
                    if (!string.IsNullOrEmpty(line) && !line.StartsWith("#"))
                    {
                        _form1.Cursor = Cursors.WaitCursor;
                        var strings = line.Split(',');
                        _form1.printNewFeature("IIS", "Windows feature", "Enabled", strings[1]);
                        var result = FeatureChecker.CheckAndEnable(strings[1], strings[0]);
                        if (result.NotInElevatedMood)
                        {
                            MessageBox.Show("Elevated permissions are required to run this.", "Restart application in elevated mood to complete these tasks.");
                            return;
                        }

                        _form1.printFeatureStatus(result.Enabled, result.ToolTip);
                        Application.DoEvents();
                        _form1.Cursor = Cursors.Default;
                    }

                }
            }
            _form1.printInfo("Ready checking features");

        }

        private string getFileToRun()
        {
            return "CheckPreRequisites." + SystemInfo.Version + ".txt";

            //			[2]: "CheckPreRequisites.WindowsServer2008R2.txt"
            //[3]: "CheckPreRequisites.WindowsServer2012.txt"
            //[4]: "CheckPreRequisites.WindowsServer2012R2.txt"
            //[5]: "CheckPreRequisites.Windows7.txt"
            //[6]: "CheckPreRequisites.Windows81.txt"

        }

        // ReSharper disable InconsistentNaming
        public void RunWebChecks()
        {
            Get461FromRegistry();
            checkFeatures();
            //CheckNetFx();
            //CheckIIS();
        }
        private void Get461FromRegistry()
        {
            _form1.printNewFeature(".NET Framework", "System", "Installed", "4.6.1");
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    var installed = (int)ndpKey.GetValue("Release");
                    if (installed == 394271 || installed == 394254)
                        _form1.printFeatureStatus(true, ".Net framework 4.6.1 is installed");
                    else
                        _form1.printFeatureStatus(false, ".Net framework 4.6.1 is not installed");
                }
                else
                {
                    _form1.printFeatureStatus(false, ".Net framework 4.6.1 is not installed");
                }
            }
        }
        //private void CheckNetFx()
        //{
        //	_form1.printNewFeature(".NET framework", ".NET Framework", "version 4.0 required",
        //								  FrameworkVersionDetection.GetExactVersion(FrameworkVersion.Fx40).ToString());
        //	_form1.printFeatureStatus(FrameworkVersionDetection.IsInstalled(FrameworkVersion.Fx40));
        //}

        //private void CheckIIS()
        //{
        //	using (var componentsKey =
        //		Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\InetStp", false))
        //	{
        //		if (componentsKey != null)
        //		{
        //			var majorVersion = (int) componentsKey.GetValue("MajorVersion", -1);
        //			var minorVersion = (int) componentsKey.GetValue("MinorVersion", -1);
        //			if (majorVersion != -1 && minorVersion != -1)
        //			{
        //				_form1.printNewFeature("IIS installed", "IIS version", "IIS 5.1 or later", majorVersion + "." + minorVersion);
        //				_form1.printFeatureStatus(true);
        //			}
        //			if (majorVersion == 7 || majorVersion == 8)
        //				CheckIIS7SubComponents();

        //			else //assume 6 or 5.1
        //				CheckIIS6SubComponents();
        //		}
        //		else
        //		{
        //			_form1.printNewFeature("IIS installed", "IIS web server is not installed", "IIS 5.1 or later", "");
        //			_form1.printFeatureStatus(false);
        //		}
        //	}
        //}

        //private void CheckIIS7SubComponents()
        //{
        //	using (var componentsKey =
        //		Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\InetStp\Components", false))
        //	{
        //		if (componentsKey != null)
        //		{
        //			_form1.printNewFeature("IIS Subcomp", "IIS Management Console", "", "");
        //			_form1.printFeatureStatus(
        //				InternetInformationServicesDetection.IsInstalled(
        //					InternetInformationServices7Component.ManagementConsole));

        //			_form1.printNewFeature("IIS Subcomp", "ASP.NET registered", "", "");
        //			_form1.printFeatureStatus(
        //				InternetInformationServicesDetection.IsInstalled(InternetInformationServices7Component.ASPNET));

        //			_form1.printNewFeature("IIS Subcomp", ".NET Extensibility", "", "");
        //			_form1.printFeatureStatus(
        //				InternetInformationServicesDetection.IsInstalled(
        //					InternetInformationServices7Component.NetFxExtensibility));

        //			_form1.printNewFeature("IIS Subcomp", "Static Content", "", "");
        //			_form1.printFeatureStatus(
        //				InternetInformationServicesDetection.IsInstalled(InternetInformationServices7Component.StaticContent));

        //			_form1.printNewFeature("IIS Subcomp", "Basic authentication", "", "");
        //			_form1.printFeatureStatus(
        //				InternetInformationServicesDetection.IsInstalled(
        //					InternetInformationServices7Component.BasicAuthentication));

        //			_form1.printNewFeature("IIS Subcomp", "Windows authentication", "", "");
        //			_form1.printFeatureStatus(
        //				InternetInformationServicesDetection.IsInstalled(
        //					InternetInformationServices7Component.WindowsAuthentication));

        //			try
        //			{
        //				_form1.printNewFeature("IIS Subcomp", ".NET 4.0 registered", "", "");
        //				_form1.printFeatureStatus(InternetInformationServicesDetection.IsAspNetRegistered(FrameworkVersion.Fx40));
        //			}
        //			catch (Exception ex)
        //			{
        //				_form1.printNewFeature("IIS Subcomp", ".NET 4.0 registered", "", "Error when checking" + ex.Message);
        //				_form1.printFeatureStatus(false);
        //			}

        //			try
        //			{
        //				_form1.printNewFeature("IIS .WCF", "WCF registered", "", "");
        //				if (FrameworkVersionDetection.IsInstalled(WindowsFoundationLibrary.WCF))
        //					_form1.printFeatureStatus("Check Manually");
        //			}
        //			catch (Exception ex)
        //			{
        //				_form1.printNewFeature("IIS .WCF", "WCF registered", "", "Error when checking" + ex.Message);
        //				_form1.printFeatureStatus(false);
        //			}
        //		}
        //	}

        //	try
        //	{
        //		using (var de = new DirectoryEntry("IIS://localhost/w3svc"))
        //		{
        //			foreach (string ext in de.Properties["WebSvcExtRestrictionList"])
        //				if (ext.IndexOf("ASP.NET v2.0", StringComparison.Ordinal) != -1)
        //					_form1.printFeatureStatus(ext.StartsWith("1,"));
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		_form1.printNewFeature("IIS .Net", "ASP.NET Enabled", "", "Error when checking" + ex.Message);
        //		_form1.printFeatureStatus(false);
        //	}

        //}

        //private void CheckIIS6SubComponents()
        //{
        //	using (var componentsKey =
        //		Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Setup\Oc Manager\Subcomponents", false)
        //		)
        //	{
        //		if (componentsKey != null)
        //		{
        //			_form1.printNewFeature("IIS", "IIS Management Console", "", "");
        //			_form1.printFeatureStatus(
        //				InternetInformationServicesDetection.IsInstalled(InternetInformationServicesComponent.InetMgr));
        //		}
        //	}

        //	// Case Sensitive
        //	const string webServerSchema = "IIsWebServer";
        //	const string serverName = "LocalHost";
        //	var w3Svc = new DirectoryEntry("IIS://" + serverName + "/w3svc/1/ROOT");
        //	foreach (DirectoryEntry site in w3Svc.Children)
        //	{
        //		if (site.SchemaClassName != webServerSchema) continue;

        //		_form1.printNewFeature("IIS", "ASP.NET Registered", "", "");
        //		try
        //		{
        //			_form1.printFeatureStatus(site.Properties["HttpCustomHeaders"].Value.ToString() == "X-Powered-By: ASP.NET");
        //		}
        //		catch
        //		{
        //			//We will assume that the feature is not installed.
        //			_form1.printFeatureStatus(false);
        //		}
        //		break;
        //	}

        //	using (var de = new DirectoryEntry("IIS://localhost/W3SVC"))
        //	{
        //		_form1.printNewFeature("IIS", "ASP.NET Enabled", "", "");
        //		foreach (string ext in de.Properties["WebSvcExtRestrictionList"])
        //		{
        //			if (ext.IndexOf("ASP.NET v2.0", StringComparison.Ordinal) == -1)
        //				continue;
        //			_form1.printFeatureStatus(ext.StartsWith("1,"));
        //		}
        //	}

        //}

        // ReSharper restore InconsistentNaming
    }
}
