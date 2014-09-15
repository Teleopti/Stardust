// ---------------------------------------------------------------------------
// Campari Software
//
// InternetInformationServicesDetection.cs
//
// For more information on, look at:
//
// http://technet2.microsoft.com/WindowsServer/en/library/5b36c13b-c72e-4488-8bbe-7e4228911c381033.mspx?mfr=true
// http://geekswithblogs.net/sdorman/archive/2007/03/01/107732.aspx
// http://blogs.iis.net:80/chrisad/archive/2006/09/01/Detecting-if-IIS-is-installed_2E002E002E00_.aspx
//
// ---------------------------------------------------------------------------
// Copyright (C) 2006-2007 Campari Software
// All rights reserved.
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR
// FITNESS FOR A PARTICULAR PURPOSE.
// ---------------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Microsoft.Win32;

namespace CheckPreRequisites.Environment
{

	/// <summary>
	/// Provides support for determining if a specific version of the .NET
	/// Framework runtime is installed and the service pack level for the
	/// runtime version.
	/// </summary>
	public static class InternetInformationServicesDetection
	{
		const string IISRegKeyName = "Software\\Microsoft\\InetStp";
		const string IISRegKeyValue = "MajorVersion";
		const string IISRegKeyMinorVersionValue = "MinorVersion";
		const string IISComponentRegKeyName = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Setup\\Oc Manager\\Subcomponents";
		const string IIS7ComponentRegKeyName = "Software\\Microsoft\\InetStp\\Components";
		const string MsDtcSecurityRegKeyName = "SOFTWARE\\Microsoft\\MSDTC\\Security";
		const string MsDtcGeneralRegKeyName = "SOFTWARE\\Microsoft\\MSDTC";
		const string Netfx20RegKeyName = "Software\\Microsoft\\ASP.NET\\2.0.50727.0";
		const string Netfx40RegKeyName = "Software\\Microsoft\\ASP.NET\\4.0.30319.0";
		const string NetRegKeyValue = "DllFullPath";
		const string AspNetFilterDll = "aspnet_filter.dll";
		const string AspNetIsaapiDll = "aspnet_isapi.dll";
		const string IIS7ApplicationHostConfig = "\\inetsrv\\config\\applicationHost.config";

		private static bool GetRegistryValue<T>(RegistryHive hive, string key, string value, RegistryValueKind kind, out T data)
		{
			bool success = false;
			data = default(T);

			using (RegistryKey baseKey = RegistryKey.OpenRemoteBaseKey(hive, String.Empty))
			{
				if (baseKey != null)
				{
					using (RegistryKey registryKey = baseKey.OpenSubKey(key, RegistryKeyPermissionCheck.ReadSubTree))
					{
						if (registryKey != null)
						{
							try
							{
								// If the key was opened, try to retrieve the value.
								RegistryValueKind kindFound = registryKey.GetValueKind(value);
								if (kindFound == kind)
								{
									object regValue = registryKey.GetValue(value, null);
									if (regValue != null)
									{
										data = (T)Convert.ChangeType(regValue, typeof(T), CultureInfo.InvariantCulture);
										success = true;
									}
								}
							}
							catch (IOException)
							{
								// The registry value doesn't exist. Since the
								// value doesn't exist we have to assume that
								// the component isn't installed and return
								// false and leave the data param as the
								// default value.
							}
						}
					}
				}
			}
			return success;
		}

		private static bool IsIIS51Installed()
		{
			bool found = false;
			int regValue;

			if (GetRegistryValue(RegistryHive.LocalMachine, IISRegKeyName, IISRegKeyValue, RegistryValueKind.DWord, out regValue))
			{
				if (regValue == 5)
				{
					// We know that we have either 5.0 or 5.1, so check the MinorVersion value.
					int minorVersion;
					if (GetRegistryValue(RegistryHive.LocalMachine, IISRegKeyName, IISRegKeyMinorVersionValue, RegistryValueKind.DWord, out minorVersion))
					{
						if (minorVersion == 1)
						{
							found = true;
						}
					}
				}
			}

			return found;
		}

		private static bool IsIIS6Installed()
		{
			bool found = false;
			int regValue = 0;

			if (GetRegistryValue(RegistryHive.LocalMachine, IISRegKeyName, IISRegKeyValue, RegistryValueKind.DWord, out regValue))
			{
				if (regValue == 6)
				{
					found = true;
				}
			}

			return found;
		}

		private static bool IsIIS7Installed()
		{
			bool found = false;
			int regValue = 0;

			if (GetRegistryValue(RegistryHive.LocalMachine, IISRegKeyName, IISRegKeyValue, RegistryValueKind.DWord, out regValue))
			{
				if (regValue == 7)
				{
					found = true;
				}
			}

			return found;
		}

		private static bool IsIISComponentInstalled(string subcomponent)
		{
			bool found = false;
			int regValue = 0;

			if (GetRegistryValue(RegistryHive.LocalMachine, IISComponentRegKeyName, subcomponent, RegistryValueKind.DWord, out regValue))
			{
				if (regValue == 1)
				{
					found = true;
				}
			}

			return found;
		}

		private static bool IsIIS7ComponentInstalled(string subcomponent)
		{
			bool found = false;
			int regValue = 0;

			if (GetRegistryValue(RegistryHive.LocalMachine, IIS7ComponentRegKeyName, subcomponent, RegistryValueKind.DWord, out regValue))
			{
				if (regValue == 1)
				{
					found = true;
				}
			}

			return found;
		}


		private static bool MsDtcSecurityInstalled(string subcomponent)
		{
			bool found = false;
			int regValue = 0;

			if (GetRegistryValue(RegistryHive.LocalMachine, MsDtcSecurityRegKeyName, subcomponent, RegistryValueKind.DWord, out regValue))
			{
				if (regValue == 1)
				{
					found = true;
				}
			}

			return found;
		}

		private static bool MsDtcGeneralInstalled(string subcomponent)
		{
			bool found = false;
			int regValue = 0;

			if (GetRegistryValue(RegistryHive.LocalMachine, MsDtcGeneralRegKeyName, subcomponent, RegistryValueKind.DWord, out regValue))
			{
				if (regValue == 1)
				{
					found = true;
				}
			}

			return found;
		}

		private static bool IsAspNet40Registered()
		{
			bool found = false;
			string regValue;

			//Check1 -do we have the ASP.NET reg key at all?
			if ((GetRegistryValue(RegistryHive.LocalMachine, Netfx40RegKeyName, NetRegKeyValue, RegistryValueKind.String, out regValue)))
			{
				//Check2 - parse the IIS7ApplicationHostConfig file and try to find aspnet_filter.dll for .net 4.0
				string findInFile = regValue.Replace(AspNetIsaapiDll, AspNetFilterDll);
				string iiSconfigFile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.System) + IIS7ApplicationHostConfig;

				var doc = new XmlDocument();
				doc.Load(iiSconfigFile);
				XmlElement root = doc.DocumentElement;
				if (root == null) return false;
				var filtersNode = root.SelectSingleNode("//isapiFilters");

				//if we do find it set true
				if (filtersNode == null) return false;
				foreach (XmlNode node in filtersNode.ChildNodes)
				{
					string filter = node.OuterXml;
					if (filter.Contains(findInFile))
					{
						found = true;
					}
				}
			}
			return found;
		}

		private static bool IsAspNet20Registered()
		{
			string regValue;
			return (GetRegistryValue(RegistryHive.LocalMachine, Netfx20RegKeyName, NetRegKeyValue, RegistryValueKind.String, out regValue));
		}

		public static bool IsInstalled(InternetInformationServicesVersion iisVersion)
		{
			bool ret = false;

			switch (iisVersion)
			{
				case InternetInformationServicesVersion.IIS51:
					ret = IsIIS51Installed();
					break;

				case InternetInformationServicesVersion.IIS6:
					ret = IsIIS6Installed();
					break;

				case InternetInformationServicesVersion.IIS7:
					ret = IsIIS7Installed();
					break;
			}

			return ret;
		}

		public static bool IsInstalled(InternetInformationServicesComponent subcomponent)
		{
			bool ret = false;

			switch (subcomponent)
			{
				case InternetInformationServicesComponent.AspNet:
					ret = IsIISComponentInstalled("aspnet");
					break;

				case InternetInformationServicesComponent.Common:
					ret = IsIISComponentInstalled("iis_common");
					break;

				case InternetInformationServicesComponent.ASP:
					ret = IsIISComponentInstalled("iis_asp");
					break;

				case InternetInformationServicesComponent.FTP:
					ret = IsIISComponentInstalled("iis_ftp");
					break;

				case InternetInformationServicesComponent.InetMgr:
					ret = IsIISComponentInstalled("iis_inetmgr");
					break;

				case InternetInformationServicesComponent.InternetDataConnector:
					ret = IsIISComponentInstalled("iis_internetdataconnector");
					break;

				case InternetInformationServicesComponent.NNTP:
					ret = IsIISComponentInstalled("iis_nntp");
					break;

				case InternetInformationServicesComponent.ServerSideIncludes:
					ret = IsIISComponentInstalled("iis_serversideincludes");
					break;

				case InternetInformationServicesComponent.SMTP:
					ret = IsIISComponentInstalled("iis_smtp");
					break;

				case InternetInformationServicesComponent.WebDAV:
					ret = IsIISComponentInstalled("iis_webdav");
					break;

				case InternetInformationServicesComponent.WWW:
					ret = IsIISComponentInstalled("iis_www");
					break;

				case InternetInformationServicesComponent.RemoteAdmin:
					ret = IsIISComponentInstalled("sakit_web");
					break;

				case InternetInformationServicesComponent.BitsISAPI:
					ret = IsIISComponentInstalled("BitsServerExtensionsISAPI");
					break;

				case InternetInformationServicesComponent.Bits:
					ret = IsIISComponentInstalled("BitsServerExtensionsManager");
					break;

				case InternetInformationServicesComponent.FrontPageExtensions:
					ret = IsIISComponentInstalled("fp_extensions");
					break;

				case InternetInformationServicesComponent.InternetPrinting:
					ret = IsIISComponentInstalled("inetprint ");
					break;

				case InternetInformationServicesComponent.TSWebClient:
					ret = IsIISComponentInstalled("TSWebClient ");
					break;
			}


			return ret;
		}

		public static bool IsInstalled(MsDtcSecurity subcomponent)
		{
			bool ret = false;

			switch (subcomponent)
			{
				case MsDtcSecurity.NetworkDtcAccess:
					ret = MsDtcSecurityInstalled("NetworkDtcAccess");
					break;

				case MsDtcSecurity.LuTransactions:
					ret = MsDtcSecurityInstalled("LuTransactions");
					break;

				case MsDtcSecurity.NetworkDtcAccessAdmin:
					ret = MsDtcSecurityInstalled("NetworkDtcAccessAdmin");
					break;

				case MsDtcSecurity.NetworkDtcAccessClients:
					ret = MsDtcSecurityInstalled("NetworkDtcAccessClients");
					break;

				case MsDtcSecurity.NetworkDtcAccessInbound:
					ret = MsDtcSecurityInstalled("NetworkDtcAccessInbound");
					break;

				case MsDtcSecurity.NetworkDtcAccessOutbound:
					ret = MsDtcSecurityInstalled("NetworkDtcAccessOutbound");
					break;

				case MsDtcSecurity.NetworkDtcAccessTip:
					ret = MsDtcSecurityInstalled("NetworkDtcAccessTip");
					break;

				case MsDtcSecurity.NetworkDtcAccessTransactions:
					ret = MsDtcSecurityInstalled("NetworkDtcAccessTransactions");
					break;

				case MsDtcSecurity.XaTransactions:
					ret = MsDtcSecurityInstalled("XaTransactions");
					break;
			}

			return ret;
		}

		public static bool IsInstalled(InternetInformationServices7Component subcomponent)
		{
			bool ret = false;

			switch (subcomponent)
			{
				case InternetInformationServices7Component.ASPNET:
					ret = IsIIS7ComponentInstalled("ASPNET");
					break;
				case InternetInformationServices7Component.ManagementConsole:
					ret = IsIIS7ComponentInstalled("ManagementConsole");
					break;
				case InternetInformationServices7Component.NetFxExtensibility:
					ret = IsIIS7ComponentInstalled("NetFxExtensibility");
					break;
				case InternetInformationServices7Component.BasicAuthentication:
					ret = IsIIS7ComponentInstalled("BasicAuthentication");
					break;
				case InternetInformationServices7Component.WindowsAuthentication:
					ret = IsIIS7ComponentInstalled("WindowsAuthentication");
					break;
				case InternetInformationServices7Component.StaticContent:
					ret = IsIIS7ComponentInstalled("StaticContent");
					break;
			}


			return ret;
		}

		public static bool IsAspRegistered()
		{
			return IsInstalled(InternetInformationServicesComponent.ASP);
		}

		public static bool IsAspNetRegistered(FrameworkVersion frameworkVersion)
		{
			bool ret = false;

			switch (frameworkVersion)
			{
				case FrameworkVersion.Fx20:
				case FrameworkVersion.Fx30:
				case FrameworkVersion.Fx35:
					ret = IsAspNet20Registered();
					break;
				case FrameworkVersion.Fx40:
					ret = IsAspNet40Registered();
					break;
			}
			return ret;
		}
	}
}
