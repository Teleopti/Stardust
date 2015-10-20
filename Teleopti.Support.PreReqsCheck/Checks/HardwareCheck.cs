using System;
using System.Globalization;
using System.Management;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CheckPreRequisites.Checks
{
	public class HardwareCheck
	{
		// ReSharper disable InconsistentNaming
		private readonly Form1 _form1;

		public HardwareCheck(Form1 form1)
		{
			_form1 = form1;
		}

		public void RunHardWareChecks(int numberOfAgent)
		{
			CheckOS();
			CheckComputerName();
			CheckArchitecture();

			CheckMemory(numberOfAgent);
			CheckCpuCores(numberOfAgent);
		}

		private void CheckOS()
		{
			_form1.printNewFeature("Operating System", "OS Version", "Windows Server 2008 R2 or later",
			                       SystemInfo.Version.ToString());
			
			switch (SystemInfo.MajorVersion)
			{
				case 6:
					if (SystemInfo.Version == WindowsVersion.WindowsServer2008R2 || SystemInfo.Version == WindowsVersion.WindowsServer2012
					    || SystemInfo.Version == WindowsVersion.WindowsServer2012R2)
						_form1.printFeatureStatus(true);
					else
						_form1.printFeatureStatus(false);
					break;
				default:
					_form1.printFeatureStatus(false);
					break;
			}
		}

		private void CheckComputerName()
		{
			var computerName = SystemInformation.ComputerName;
			var hostName = Dns.GetHostName();
			_form1.printNewFeature("Operating system", "Computer name", "Allowed characters for computer is [A-Z,a-z,0-9], and maximum length is 15",
								   hostName);
			_form1.printFeatureStatus(computerName == Regex.Replace(computerName, "[^a-zA-Z0-9_]", "") &&
			                          String.Equals(computerName, hostName, StringComparison.CurrentCultureIgnoreCase));
		}

		private void CheckArchitecture()
		{
			using (var componentsKey =
				Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", false))
			{
				if (componentsKey == null)
					return;

				var architecture = (string) componentsKey.GetValue("PROCESSOR_ARCHITECTURE", "N/A");
				_form1.printNewFeature("Operating System", "Architecture", "x64 required", architecture);
				_form1.printFeatureStatus(architecture.Contains("64"));
			}
		}

		private void CheckMemory(int numberOfAgent)
		{
			const int oneGb = 1073741824;

			try
			{
				var search = new ManagementObjectSearcher("Select * From Win32_ComputerSystem");
				foreach (var mobject in search.Get())
				{
					var ramBytes = (Convert.ToDouble(mobject["TotalPhysicalMemory"]));
					_form1.printNewFeature("Hardware", "RAM Size in Giga Bytes", "",
					                       Math.Round(ramBytes/oneGb, 1).ToString(CultureInfo.InvariantCulture));

					if (numberOfAgent < 400 && Math.Round(ramBytes/oneGb, 1) >= 8)
						_form1.printFeatureStatus(true);
					else if (numberOfAgent <= 1500 && Math.Round(ramBytes/oneGb, 1) >= 16)
						_form1.printFeatureStatus(true);
					else if (numberOfAgent <= 4000 && Math.Round(ramBytes/oneGb, 1) >= 32)
						_form1.printFeatureStatus(true);
					else if (numberOfAgent <= 10000 && Math.Round(ramBytes/oneGb, 1) >= 64)
						_form1.printFeatureStatus(true);
					else if (numberOfAgent > 10000 && Math.Round(ramBytes / oneGb, 1) >= 128)
						_form1.printFeatureStatus(true);
					else
						_form1.printFeatureStatus(false);
				}
			}
			catch (Exception ex)
			{
				_form1.printNewFeature("Hardware", "RAM Size in Giga Bytes", "", "Error when checking " + ex.Message);
				_form1.printFeatureStatus(false);
			}
		}

		private void CheckCpuCores(int numberOfAgent)
		{
			using (var componentsKey =
				Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor", false))
			{
				if (componentsKey == null)
					return;

				var processors = componentsKey.SubKeyCount;
				_form1.printNewFeature("Hardware", "Processor CPU's ", "", processors.ToString(CultureInfo.InvariantCulture));
				if (numberOfAgent < 400 && processors >= 2)
					_form1.printFeatureStatus(true);
				else if (numberOfAgent <= 1500 && processors >= 4)
					_form1.printFeatureStatus(true);
				else if (numberOfAgent <= 4000 && processors >= 8)
					_form1.printFeatureStatus(true);
				else if (numberOfAgent <= 10000 && processors >= 16)
					_form1.printFeatureStatus(true);
				else if (numberOfAgent > 10000 && processors >= 32)
					_form1.printFeatureStatus(true);
				else
					_form1.printFeatureStatus(false);
			}
		}

		// ReSharper restore InconsistentNaming
	}
}
