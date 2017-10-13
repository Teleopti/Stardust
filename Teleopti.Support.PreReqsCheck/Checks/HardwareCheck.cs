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

		public void RunHardWareChecks(int numberOfAgent, CheckType checkType)
		{
			CheckOS();
			CheckComputerName();
			CheckArchitecture();

			if (checkType == CheckType.Db)
			{
				var dbLimits = new DbLimits();
				CheckMemory(numberOfAgent, dbLimits);
				CheckCpuCores(numberOfAgent, dbLimits);
			}else if (checkType == CheckType.Web)
			{
				var webLimits = new WebLimits();
				CheckMemory(numberOfAgent, webLimits);
				CheckCpuCores(numberOfAgent, webLimits);
			}else if (checkType == CheckType.Worker)
			{
				var workerLimits= new WorkerLimits();
				CheckMemory(numberOfAgent, workerLimits);
				CheckCpuCores(numberOfAgent, workerLimits);
			}
		}

		private void CheckOS()
		{
			var lineNumber = _form1.printNewFeature("Operating System", "OS Version", "Windows Server 2008 R2 or later",
			                       SystemInfo.Version.ToString());
			
			switch (SystemInfo.MajorVersion)
			{
				case 6:
					if (SystemInfo.Version == WindowsVersion.WindowsServer2008R2 || SystemInfo.Version == WindowsVersion.WindowsServer2012
					    || SystemInfo.Version == WindowsVersion.WindowsServer2012R2)
						_form1.printFeatureStatus(true, "", lineNumber);
					else
						_form1.printFeatureStatus(false, "", lineNumber);
					break;
				default:
					_form1.printFeatureStatus(false, "", lineNumber);
					break;
			}
		}

		private void CheckComputerName()
		{
			var computerName = SystemInformation.ComputerName;
			var hostName = Dns.GetHostName();
			var lineNumber = _form1.printNewFeature("Operating system", "Computer name", "Allowed characters for computer is [A-Z,a-z,0-9], and maximum length is 15",
								   hostName);
			_form1.printFeatureStatus(computerName == Regex.Replace(computerName, "[^a-zA-Z0-9_]", "") &&
									  String.Equals(computerName, hostName, StringComparison.CurrentCultureIgnoreCase), "", lineNumber);
		}

		private void CheckArchitecture()
		{
			using (var componentsKey =
				Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", false))
			{
				if (componentsKey == null)
					return;

				var architecture = (string) componentsKey.GetValue("PROCESSOR_ARCHITECTURE", "N/A");
				var lineNumber = _form1.printNewFeature("Operating System", "Architecture", "x64 required", architecture);
				_form1.printFeatureStatus(architecture.Contains("64"), "", lineNumber);
			}
		}
		

		private void CheckMemory(int numberOfAgent, Limits limits)
		{
			const int oneGb = 1073741824;

			try
			{
				var search = new ManagementObjectSearcher("Select * From Win32_ComputerSystem");
				foreach (var mobject in search.Get())
				{
					var ramBytes = (Convert.ToDouble(mobject["TotalPhysicalMemory"]));
					

					var isOk = false;
					var message = "";
					foreach (var limit in limits.All)
					{
						if (numberOfAgent <= limit.MaxAgents)
						{
							if (Math.Round(ramBytes/oneGb, 1) >= limit.Memory)
							{
								isOk = true;
							}
							message = limit.Memory.ToString();
							if (limit.NumberOfServers > 1)
							{
								message = $"{limit.Memory} per server. High Availability is mandatory with {limit.NumberOfServers} servers.";
							}
							
							break;
						}
					}
					var lineNumber = _form1.printNewFeature("Hardware", "RAM Size in Giga Bytes", message,
										   Math.Round(ramBytes / oneGb, 1).ToString(CultureInfo.InvariantCulture));
					_form1.printFeatureStatus(isOk, "", lineNumber);
				}
			}
			catch (Exception ex)
			{
				var lineNumber = _form1.printNewFeature("Hardware", "RAM Size in Giga Bytes", "", "Error when checking " + ex.Message);
				_form1.printFeatureStatus(false, "", lineNumber);
			}
		}

		private void CheckCpuCores(int numberOfAgent, Limits limits)
		{
			using (var componentsKey = Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor", false))
			{
				if (componentsKey == null)
					return;

				var processors = componentsKey.SubKeyCount;

				var isOk = false;
				var message = "";

				foreach (var limit in limits.All)
				{
					if (numberOfAgent <= limit.MaxAgents)
					{
						if (processors >= limit.Processor)
						{
							isOk = true;
						}
						message = limit.Processor.ToString();
						if (limit.NumberOfServers > 1)
						{
							message = $"{limit.Processor} per server. High Availability is mandatory with {limit.NumberOfServers} servers.";
						}
						break;
					}
				}
				var lineNumber = _form1.printNewFeature("Hardware", "Processor CPU's ", message, processors.ToString(CultureInfo.InvariantCulture));
				_form1.printFeatureStatus(isOk, "", lineNumber);
			}
		}
	}

}
