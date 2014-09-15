using System;
using System.DirectoryServices;
using Campari.Software;
using CheckPreRequisites.Environment;
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

		// ReSharper disable InconsistentNaming
		public void RunWebChecks()
		{
			CheckNetFx();
			CheckIIS();
		}

		private void CheckNetFx()
		{
			_form1.printNewFeature(".NET framework", ".NET Framework", "version 4.0 required",
			                       FrameworkVersionDetection.GetExactVersion(FrameworkVersion.Fx40).ToString());
			_form1.printFeatureStatus(FrameworkVersionDetection.IsInstalled(FrameworkVersion.Fx40));
		}

		private void CheckIIS()
		{
			using (var componentsKey =
				Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\InetStp", false))
			{
				if (componentsKey != null)
				{
					var majorVersion = (int) componentsKey.GetValue("MajorVersion", -1);
					var minorVersion = (int) componentsKey.GetValue("MinorVersion", -1);
					if (majorVersion != -1 && minorVersion != -1)
					{
						_form1.printNewFeature("IIS installed", "IIS version", "IIS 5.1 or later", majorVersion + "." + minorVersion);
						_form1.printFeatureStatus(true);
					}
					if (majorVersion == 7 || majorVersion == 8)
						CheckIIS7SubComponents();

					else //assume 6 or 5.1
						CheckIIS6SubComponents();
				}
				else
				{
					_form1.printNewFeature("IIS installed", "IIS web server is not installed", "IIS 5.1 or later", "");
					_form1.printFeatureStatus(false);
				}
			}
		}

		private void CheckIIS7SubComponents()
		{
			using (var componentsKey =
				Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\InetStp\Components", false))
			{
				if (componentsKey != null)
				{
					_form1.printNewFeature("IIS Subcomp", "IIS Management Console", "", "");
					_form1.printFeatureStatus(
						InternetInformationServicesDetection.IsInstalled(
							InternetInformationServices7Component.ManagementConsole));

					_form1.printNewFeature("IIS Subcomp", "ASP.NET registered", "", "");
					_form1.printFeatureStatus(
						InternetInformationServicesDetection.IsInstalled(InternetInformationServices7Component.ASPNET));

					_form1.printNewFeature("IIS Subcomp", ".NET Extensibility", "", "");
					_form1.printFeatureStatus(
						InternetInformationServicesDetection.IsInstalled(
							InternetInformationServices7Component.NetFxExtensibility));

					_form1.printNewFeature("IIS Subcomp", "Static Content", "", "");
					_form1.printFeatureStatus(
						InternetInformationServicesDetection.IsInstalled(InternetInformationServices7Component.StaticContent));

					_form1.printNewFeature("IIS Subcomp", "Basic authentication", "", "");
					_form1.printFeatureStatus(
						InternetInformationServicesDetection.IsInstalled(
							InternetInformationServices7Component.BasicAuthentication));

					_form1.printNewFeature("IIS Subcomp", "Windows authentication", "", "");
					_form1.printFeatureStatus(
						InternetInformationServicesDetection.IsInstalled(
							InternetInformationServices7Component.WindowsAuthentication));

					try
					{
						_form1.printNewFeature("IIS Subcomp", ".NET 4.0 registered", "", "");
						_form1.printFeatureStatus(InternetInformationServicesDetection.IsAspNetRegistered(FrameworkVersion.Fx40));
					}
					catch (Exception ex)
					{
						_form1.printNewFeature("IIS Subcomp", ".NET 4.0 registered", "", "Error when checking" + ex.Message);
						_form1.printFeatureStatus(false);
					}

					try
					{
						_form1.printNewFeature("IIS .WCF", "WCF registered", "", "");
						if (FrameworkVersionDetection.IsInstalled(WindowsFoundationLibrary.WCF))
							_form1.printFeatureStatus("Check Manually");
					}
					catch (Exception ex)
					{
						_form1.printNewFeature("IIS .WCF", "WCF registered", "", "Error when checking" + ex.Message);
						_form1.printFeatureStatus(false);
					}
				}
			}

			try
			{
				using (var de = new DirectoryEntry("IIS://localhost/w3svc"))
				{
					foreach (string ext in de.Properties["WebSvcExtRestrictionList"])
						if (ext.IndexOf("ASP.NET v2.0", StringComparison.Ordinal) != -1)
							_form1.printFeatureStatus(ext.StartsWith("1,"));
				}
			}
			catch (Exception ex)
			{
				_form1.printNewFeature("IIS .Net", "ASP.NET Enabled", "", "Error when checking" + ex.Message);
				_form1.printFeatureStatus(false);
			}

		}

		private void CheckIIS6SubComponents()
		{
			using (var componentsKey =
				Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Setup\Oc Manager\Subcomponents", false)
				)
			{
				if (componentsKey != null)
				{
					_form1.printNewFeature("IIS", "IIS Management Console", "", "");
					_form1.printFeatureStatus(
						InternetInformationServicesDetection.IsInstalled(InternetInformationServicesComponent.InetMgr));
				}
			}

			// Case Sensitive
			const string webServerSchema = "IIsWebServer";
			const string serverName = "LocalHost";
			var w3Svc = new DirectoryEntry("IIS://" + serverName + "/w3svc/1/ROOT");
			foreach (DirectoryEntry site in w3Svc.Children)
			{
				if (site.SchemaClassName != webServerSchema) continue;

				_form1.printNewFeature("IIS", "ASP.NET Registered", "", "");
				try
				{
					_form1.printFeatureStatus(site.Properties["HttpCustomHeaders"].Value.ToString() == "X-Powered-By: ASP.NET");
				}
				catch
				{
					//We will assume that the feature is not installed.
					_form1.printFeatureStatus(false);
				}
				break;
			}

			using (var de = new DirectoryEntry("IIS://localhost/W3SVC"))
			{
				_form1.printNewFeature("IIS", "ASP.NET Enabled", "", "");
				foreach (string ext in de.Properties["WebSvcExtRestrictionList"])
				{
					if (ext.IndexOf("ASP.NET v2.0", StringComparison.Ordinal) == -1)
						continue;
					_form1.printFeatureStatus(ext.StartsWith("1,"));
				}
			}

		}

		// ReSharper restore InconsistentNaming
	}
}
