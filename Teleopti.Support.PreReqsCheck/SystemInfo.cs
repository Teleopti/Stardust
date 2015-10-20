using System;
using System.Runtime.InteropServices;

namespace CheckPreRequisites
{
   /// <summary>
   /// Provides information about the running operating system.
   /// </summary>
   public static class SystemInfo
   {
      public static int MajorVersion
      {
         get { return System.Environment.OSVersion.Version.Major; }
      }

      public static int MinorVersion
      {
         get { return System.Environment.OSVersion.Version.Minor; }
      }

      public static int MajorRevision
      {
         get { return System.Environment.OSVersion.Version.MajorRevision; }
      }

      public static int MinorRevision
      {
         get { return System.Environment.OSVersion.Version.MinorRevision; }
      }

      public static int Revision
      {
         get { return System.Environment.OSVersion.Version.Revision; }
      }

      public static int Build
      {
         get { return System.Environment.OSVersion.Version.Build; }
      }

      public static string VersionString
      {
         get { return System.Environment.OSVersion.Version.ToString(); }
      }

      public static string ServicePack
      {
         get
         {
            string servicePack = String.Empty;
            Win32Api.OSVERSIONINFOEX osVersionInfo = new Win32Api.OSVERSIONINFOEX();

            osVersionInfo.dwOSVersionInfoSize = (uint)Marshal.SizeOf(typeof(Win32Api.OSVERSIONINFOEX));

            if (Win32Api.GetVersionEx(ref osVersionInfo))
            {
               servicePack = osVersionInfo.szCSDVersion;
            }

            return servicePack;
         }
      }

      public static WindowsVersion Version
      {
         get
         {
            WindowsVersion version = WindowsVersion.Unknown;

            Win32Api.OSVERSIONINFOEX osVersionInfo = new Win32Api.OSVERSIONINFOEX();
            osVersionInfo.dwOSVersionInfoSize = (uint)Marshal.SizeOf(typeof(Win32Api.OSVERSIONINFOEX));

            Win32Api.SYSTEM_INFO systemInfo = new Win32Api.SYSTEM_INFO();
            Win32Api.GetSystemInfo(ref systemInfo);

            if (Win32Api.GetVersionEx(ref osVersionInfo))
            {
               switch (System.Environment.OSVersion.Platform)
               {
                  case PlatformID.Win32Windows:
                     {
                        switch (osVersionInfo.dwMajorVersion)
                        {
                           case 4:
                              {
                                 switch (osVersionInfo.dwMinorVersion)
                                 {
                                    case 0:
                                       if (osVersionInfo.szCSDVersion == "B" ||
                                           osVersionInfo.szCSDVersion == "C")
                                          version = WindowsVersion.Windows95OSR2;
                                       else
                                          version = WindowsVersion.Windows95;
                                       break;
                                    case 10:
                                       if (osVersionInfo.szCSDVersion == "A")
                                          version = WindowsVersion.Windows98SE;
                                       else
                                          version = WindowsVersion.Windows98;
                                       break;
                                    case 90:
                                       version = WindowsVersion.WindowsMillennium;
                                       break;
                                 }
                              }
                              break;
                        }
                     }
                     break;

                  case PlatformID.Win32NT:
                     {
                        switch (osVersionInfo.dwMajorVersion)
                        {
                           case 3:
                              version = WindowsVersion.WindowsNT351;
                              break;

                           case 4:
                              switch (osVersionInfo.wProductType)
                              {
                                 case 1:
                                    version = WindowsVersion.WindowsNT40;
                                    break;
                                 case 3:
                                    version = WindowsVersion.WindowsNT40Server;
                                    break;
                              }
                              break;

                           case 5:
                              {
                                 switch (osVersionInfo.dwMinorVersion)
                                 {
                                    case 0:
                                       version = WindowsVersion.Windows2000;
                                       break;
                                    case 1:
                                       version = WindowsVersion.WindowsXP;
                                       break;
                                    case 2:
                                       {
                                          if (osVersionInfo.wSuiteMask == Win32Api.VER_SUITE_WH_SERVER)
                                          {
                                             version = WindowsVersion.WindowsHomeServer;
                                          }
                                          else if (osVersionInfo.wProductType == Win32Api.VER_NT_WORKSTATION &&
                                                  systemInfo.wProcessorArchitecture == Win32Api.PROCESSOR_ARCHITECTURE_AMD64)
                                          {
                                             version = WindowsVersion.WindowsXPProfessionalx64;
                                          }
                                          else
                                          {
                                             version = Win32Api.GetSystemMetrics(Win32Api.SM_SERVERR2) == 0 ?
                                                WindowsVersion.WindowsServer2003 :
                                                WindowsVersion.WindowsServer2003R2;
                                          }
                                       }
                                       break;
                                 }

                              }
                              break;

                           case 6:
                              {
                                 switch (osVersionInfo.dwMinorVersion)
                                 {
                                    case 0:
                                       {
                                          version = osVersionInfo.wProductType == Win32Api.VER_NT_WORKSTATION ?
                                             WindowsVersion.WindowsVista :
                                             WindowsVersion.WindowsServer2008;
                                       }
                                       break;

                                    case 1:
                                       {
                                          version = osVersionInfo.wProductType == Win32Api.VER_NT_WORKSTATION ?
                                             WindowsVersion.Windows7 :
                                             WindowsVersion.WindowsServer2008R2;

                                       }
                                       break;

                                    case 2:
                                       {
                                           version = osVersionInfo.wProductType == Win32Api.VER_NT_WORKSTATION ?
                                              WindowsVersion.Windows8 :
                                              WindowsVersion.WindowsServer2012;

                                       }
                                       break;
												case 3:
													{
														version = osVersionInfo.wProductType == Win32Api.VER_NT_WORKSTATION ?
															WindowsVersion.Windows81 :
															WindowsVersion.WindowsServer2012R2;

													}
													break;
                                 }
                              }
									
		                        break;
									case 10:
										{
											version = osVersionInfo.wProductType == Win32Api.VER_NT_WORKSTATION ?
															WindowsVersion.Windows10 :
															WindowsVersion.WindowsServer2016;
										}
										break;
								}
                     }
                     break;
               }
            }

            return version;
         }
      }

      public static WindowsEdition Edition
      {
         get
         {
            WindowsEdition edition = WindowsEdition.Unknown;

            Win32Api.OSVERSIONINFOEX osVersionInfo = new Win32Api.OSVERSIONINFOEX();
            osVersionInfo.dwOSVersionInfoSize = (uint)Marshal.SizeOf(typeof(Win32Api.OSVERSIONINFOEX));

            if (Win32Api.GetVersionEx(ref osVersionInfo))
            {
               switch(osVersionInfo.dwMajorVersion)
               {
                  case 4:
                     {
                        switch(osVersionInfo.wProductType)
                        {
                           case Win32Api.VER_NT_WORKSTATION:
                              edition = WindowsEdition.Workstation;
                              break;

                           case Win32Api.VER_NT_SERVER:
                              edition = (osVersionInfo.wSuiteMask & Win32Api.VER_SUITE_ENTERPRISE) != 0 ? 
                                 WindowsEdition.EnterpriseServer :
                                 WindowsEdition.StandardServer;
                              break;
                        }
                     }
                     break;

                  case 5:
                     {
                        switch (osVersionInfo.wProductType)
                        {
                           case Win32Api.VER_NT_WORKSTATION:
                              {
                                 edition = (osVersionInfo.wSuiteMask & Win32Api.VER_SUITE_PERSONAL) != 0 ?
                                    WindowsEdition.Home :
                                    WindowsEdition.Professional;
                              }
                              break;

                           case Win32Api.VER_NT_SERVER:
                              {
                                 switch(osVersionInfo.dwMinorVersion)
                                 {
                                    case 0:
                                       {
                                          if ((osVersionInfo.wSuiteMask & Win32Api.VER_SUITE_DATACENTER) != 0)
                                          {
                                             edition = WindowsEdition.DatacenterServer;
                                          }
                                          else if ((osVersionInfo.wSuiteMask & Win32Api.VER_SUITE_ENTERPRISE) != 0)
                                          {
                                             edition = WindowsEdition.AdvancedServer;
                                          }
                                          else
                                          {
                                             edition = WindowsEdition.Server;
                                          }
                                       }
                                       break;

                                    default:
                                       {
                                          if ((osVersionInfo.wSuiteMask & Win32Api.VER_SUITE_DATACENTER) != 0)
                                          {
                                             edition = WindowsEdition.DatacenterServer;
                                          }
                                          else if ((osVersionInfo.wSuiteMask & Win32Api.VER_SUITE_ENTERPRISE) != 0)
                                          {
                                             edition = WindowsEdition.EnterpriseServer;
                                          }
                                          else if ((osVersionInfo.wSuiteMask & Win32Api.VER_SUITE_BLADE) != 0)
                                          {
                                             edition = WindowsEdition.WebEdition;
                                          }
                                          else
                                          {
                                             edition = WindowsEdition.StandardServer;
                                          }

                                       }
                                       break;
                                 }
                              }
                              break;
                        }
                     }
                     break;

                  case 6:
                     {
                        uint ed;
                        if (Win32Api.GetProductInfo(
                           osVersionInfo.dwMajorVersion, 
                           osVersionInfo.dwMinorVersion,
                           osVersionInfo.wServicePackMajor, 
                           osVersionInfo.wServicePackMinor,
                           out ed))
                        {
                           switch (ed)
                           {
                              case Win32Api.PRODUCT_BUSINESS:
                                 edition = WindowsEdition.Business;
                                 break;
                              case Win32Api.PRODUCT_BUSINESS_N:
                                 edition = WindowsEdition.Business_N;
                                 break;
                              case Win32Api.PRODUCT_CLUSTER_SERVER:
                                 edition = WindowsEdition.HPCEdition;
                                 break;
                              case Win32Api.PRODUCT_DATACENTER_SERVER:
                                 edition = WindowsEdition.DatacenterServer;
                                 break;
                              case Win32Api.PRODUCT_DATACENTER_SERVER_CORE:
                                 edition = WindowsEdition.DatacenterServer_CoreInstallation;
                                 break;
                              case Win32Api.PRODUCT_ENTERPRISE:
                                 edition = WindowsEdition.Enterprise;
                                 break;
                              case Win32Api.PRODUCT_ENTERPRISE_N:
                                 edition = WindowsEdition.Enterprise_N;
                                 break;
                              case Win32Api.PRODUCT_ENTERPRISE_SERVER:
                                 edition = WindowsEdition.EnterpriseServer;
                                 break;
                              case Win32Api.PRODUCT_ENTERPRISE_SERVER_CORE:
                                 edition = WindowsEdition.EnterpriseServer_CoreInstallation;
                                 break;
                              case Win32Api.PRODUCT_ENTERPRISE_SERVER_CORE_V:
                                 edition = WindowsEdition.EnterpriseServer_WithoutHyperV_CoreInstallation;
                                 break;
                              case Win32Api.PRODUCT_ENTERPRISE_SERVER_IA64:
                                 edition = WindowsEdition.EnterpriseServer_ForItaniumBasedSystems;
                                 break;
                              case Win32Api.PRODUCT_ENTERPRISE_SERVER_V:
                                 edition = WindowsEdition.EnterpriseServer_WithoutHyperV;
                                 break;
                              case Win32Api.PRODUCT_HOME_BASIC:
                                 edition = WindowsEdition.HomeBasic;
                                 break;
                              case Win32Api.PRODUCT_HOME_BASIC_N:
                                 edition = WindowsEdition.HomeBasic_N;
                                 break;
                              case Win32Api.PRODUCT_HOME_PREMIUM:
                                 edition = WindowsEdition.HomePremium;
                                 break;
                              case Win32Api.PRODUCT_HOME_PREMIUM_N:
                                 edition = WindowsEdition.HomePremium_N;
                                 break;
                              case Win32Api.PRODUCT_HYPERV:
                                 edition = WindowsEdition.HyperVServer;
                                 break;
                              case Win32Api.PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT:
                                 edition = WindowsEdition.EssentialBusinessManagementServer;
                                 break;
                              case Win32Api.PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING:
                                 edition = WindowsEdition.EssentialBusinessMessagingServer;
                                 break;
                              case Win32Api.PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY:
                                 edition = WindowsEdition.EssentialBusinessSecurityServer;
                                 break;
                              case Win32Api.PRODUCT_SERVER_FOR_SMALLBUSINESS:
                                 edition = WindowsEdition.EssentialServerSolutions;
                                 break;
                              case Win32Api.PRODUCT_SERVER_FOR_SMALLBUSINESS_V:
                                 edition = WindowsEdition.EssentialServerSolutions_WithoutHyperV;
                                 break;
                              case Win32Api.PRODUCT_SMALLBUSINESS_SERVER:
                                 edition = WindowsEdition.SmallBusinessServer;
                                 break;
                              case Win32Api.PRODUCT_STANDARD_SERVER:
                                 edition = WindowsEdition.StandardServer;
                                 break;
                              case Win32Api.PRODUCT_STANDARD_SERVER_CORE:
                                 edition = WindowsEdition.StandardServer_CoreInstallation;
                                 break;
                              case Win32Api.PRODUCT_STANDARD_SERVER_CORE_V:
                                 edition = WindowsEdition.StandardServer_WithoutHyperV_CoreInstallation;
                                 break;
                              case Win32Api.PRODUCT_STANDARD_SERVER_V:
                                 edition = WindowsEdition.StandardServer_WithoutHyperV;
                                 break;
                              case Win32Api.PRODUCT_STARTER:
                                 edition = WindowsEdition.Starter;
                                 break;
                              case Win32Api.PRODUCT_STORAGE_ENTERPRISE_SERVER:
                                 edition = WindowsEdition.EnterpriseStorageServer;
                                 break;
                              case Win32Api.PRODUCT_STORAGE_EXPRESS_SERVER:
                                 edition = WindowsEdition.ExpressStorageServer;
                                 break;
                              case Win32Api.PRODUCT_STORAGE_STANDARD_SERVER:
                                 edition = WindowsEdition.StandardStorageServer;
                                 break;
                              case Win32Api.PRODUCT_STORAGE_WORKGROUP_SERVER:
                                 edition = WindowsEdition.WorkgroupStorageServer;
                                 break;
                              case Win32Api.PRODUCT_UNDEFINED:
                                 edition = WindowsEdition.Unknown;
                                 break;
                              case Win32Api.PRODUCT_ULTIMATE:
                                 edition = WindowsEdition.Ultimate;
                                 break;
                              case Win32Api.PRODUCT_ULTIMATE_N:
                                 edition = WindowsEdition.Ultimate_N;
                                 break;
                              case Win32Api.PRODUCT_WEB_SERVER:
                                 edition = WindowsEdition.WebServer;
                                 break;
                              case Win32Api.PRODUCT_WEB_SERVER_CORE:
                                 edition = WindowsEdition.WebServer_CoreInstallation;
                                 break;
                           }
                        }
                     }
                     break;
               }
            }

            return edition;
         }
      }
   }
}
