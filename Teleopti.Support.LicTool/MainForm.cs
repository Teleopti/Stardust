using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Licensing.Agreements;
using Teleopti.Ccc.Secrets.Licensing;

namespace Teleopti.Support.LicTool
{
	public partial class MainForm : Form
	{
		private IList<LicenseAgreement> _allAgreements;

		public MainForm()
		{
			InitializeComponent();
			InstallLocalKeypair();
			comboBoxAgreement.SelectedIndexChanged += comboBoxAgreementSelectedIndexChanged;
			loadAgreements();
			comboBoxAgentsOrSeats.SelectedIndex = 0;
			labelRatio.Enabled = false;
			numericUpDownCountRatio.Value = 1.5M;
			numericUpDownCountRatio.Enabled = false;
		}

		void comboBoxAgreementSelectedIndexChanged(object sender, EventArgs e)
		{
			var item = comboBoxAgreement.SelectedItem as LicenseAgreement;
		}

		private void loadAgreements()
		{
			comboBoxAgreement.DisplayMember = "DisplayName";
			comboBoxAgreement.ValueMember = "ResourceName";
			var provider = new AgreementProvider();
			_allAgreements = provider.GetAllAgreements();
			comboBoxAgreement.DataSource = _allAgreements;
		}

		private static string GetPublicKey()
		{
			try
			{
				var parms = new CspParameters(1)
								{
									Flags = CspProviderFlags.UseMachineKeyStore,
									KeyContainerName = XmlLicense.ContainerName,
									KeyNumber = 2
								}; // PROV_RSA_FULL

				using (var csp = new RSACryptoServiceProvider(parms))
				{
					return csp.ToXmlString(false);
				}
			}
			catch (CryptographicException ex)
			{
				MessageBox.Show("Error: could not extract public key from machine store. Original error: " + ex.Message);
				return "";
			}

   
		}

		//[SecurityPermission(SecurityAction.LinkDemand,Flags = SecurityPermissionFlag.UnmanagedCode)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		private static void InstallLocalKeypair()
		{
			try
			{
					string path = Directory.GetCurrentDirectory();

					using (System.Diagnostics.Process proc = new System.Diagnostics.Process())
					{
						proc.StartInfo.FileName = path + "\\InstallLocalKeypair.bat";
						proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
						proc.StartInfo.CreateNoWindow = false;
						proc.Start();
						proc.WaitForExit();
					}

			}
			catch (InvalidOperationException ex)
			{
				MessageBox.Show("Error: could not install a local keypair. Original error: " + ex.Message);
			}

		}

		static string replaceInvalidFileChars(string testName)        
		   {                
			   var windowsPathBadChars = new Regex("[" + Regex.Escape("<>:\"/\\|?*") + "]");
			   if (windowsPathBadChars.IsMatch(testName)) 
			   { return Regex.Replace(testName,"[" + Regex.Escape("<>:\"/\\|?*") + "]","_");
			   //{ //return false; 
			   };                // other checks for UNC, drive-path format, etc                
			   return testName;
		   }

		private void Initialize_form()
		{
			//     InstallLocalKeypair();

			txtbxCustomerName.Text = null;
			dtpkrExpirationDate.Value = DateTime.Now;
			numExpirationGracePeriodDays.Value = 0;
			ExpirationGracePeriodHours.Value = 0;
			numMaxActiveAgents.Value = 0;
			comboBoxAgentsOrSeats.SelectedIndex = 0;
			numericUpDownCountRatio.Value = 1.5M;
			numMaxActiveAgentsGrace.Value = 0;

			chkAgentScheduleMessenger.Checked = false;
			//chkAgentScheduleMessenger.Enabled = true;
			chkAgentSelfService.Checked = false;
			chkBase.Checked = false;
			chkHolidayPlanner.Checked = false;
			//chkHolidayPlanner.Enabled = true;
			chkPayrollIntegration.Checked = false;
			chkPerformanceManager.Checked = false;
			chkRealtimeAdherence.Checked = false;
			chkShiftTrades.Checked = false;
			//chkShiftTrades.Enabled = true;
			chkMyTimeWeb.Checked = false;
			chkMobileReports.Checked = false;

			chkDeveloper.Checked = false;
			checkBoxVersion8.Checked = false;
		}

		private void EnterDemoSettings()
		{
			txtbxCustomerName.Text = "Demo license! For internal use only!";
			//dtpkrExpirationDate.Value = LicenseExpirationDate.GetLicenseExpirationDate(DateTime.Today, 94);
			//ExpirationDate.Value = DateTime.Today.Add(new TimeSpan(90,12,0,0,0));
			numExpirationGracePeriodDays.Value = 30;
			numMaxActiveAgents.Value = 100;
			numMaxActiveAgentsGrace.Value = 10;

			chkAgentScheduleMessenger.Checked = true;
			chkAgentSelfService.Checked = true;
			chkBase.Checked = true;
			chkHolidayPlanner.Checked = true;
			chkPayrollIntegration.Checked = true;
			chkPerformanceManager.Checked = true;
			chkRealtimeAdherence.Checked = true;
			chkShiftTrades.Checked = true;
			chkMyTimeWeb.Checked = true;
			chkMobileReports.Checked = true;

		}

		private void EnterFreemiumSettings()
		{
			txtbxCustomerName.Text = "Freemium license";
			numExpirationGracePeriodDays.Value = 30;
			numMaxActiveAgents.Value = 1;
			numMaxActiveAgentsGrace.Value = 10;
			comboBoxAgreement.Text = "TeleoptiLic_En_Sw_Forecasts.txt";

			//chkAgentScheduleMessenger.Checked = false;
			//chkAgentSelfService.Checked = false;
			//chkBase.Checked = false;
			//chkHolidayPlanner.Checked = false;
			//chkPayrollIntegration.Checked = false;
			//chkPerformanceManager.Checked = false;
			//chkRealtimeAdherence.Checked = false;
			//chkShiftTrades.Checked = false;
			//chkMyTimeWeb.Checked = false;

		}

		private void DemoSettings_Click(object sender, EventArgs e)
		{
			EnterDemoSettings();   
	  
		}

		private void ClearSettings_Click(object sender, EventArgs e)
		{

			Initialize_form();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		private void LicenseFileLoad()
		{
			using (var openFileDialog = new OpenFileDialog())
			{
				openFileDialog.InitialDirectory = "\\\\a380\\T-Files\\BU Teleopti CCC\\LICENSE KEYS V7";
				openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
				openFileDialog.FilterIndex = 2;
				openFileDialog.RestoreDirectory = true;


				string pubkey = GetPublicKey();
				if (openFileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(pubkey))
				{
					try
					{
						var xmlstr = string.Empty;
						using (var myStream = openFileDialog.OpenFile())
						{
							var reader = new StreamReader(myStream);	
							xmlstr = reader.ReadToEnd();
							reader.Close();
						}
						XDocument xdoc = XDocument.Parse(xmlstr);
						var xlic = new XmlLicense(xdoc, pubkey);

						txtbxCustomerName.Text = xlic.CustomerName;
						comboBoxAgreement.SelectedIndex = 0;
						if (!string.IsNullOrEmpty(xlic.Agreement))
							foreach (LicenseAgreement agreement in _allAgreements)
							{
								if (agreement.ResourceName.Equals(xlic.Agreement))
									comboBoxAgreement.SelectedIndex = _allAgreements.IndexOf(agreement);
							}

						numMaxActiveAgents.Value = xlic.MaxActiveAgents;
						long grace = Convert.ToInt64(xlic.MaxActiveAgentGrace * 100);
						numMaxActiveAgentsGrace.Value = grace; // value in file is a percentage
						dtpkrExpirationDate.Value = xlic.ExpirationDate;

						numericUpDownCountRatio.Value = xlic.Ratio;
						comboBoxAgentsOrSeats.SelectedIndex = xlic.AgentsOrSeats;

						numExpirationGracePeriodDays.Value = xlic.ExpirationGracePeriod.Days;
						ExpirationGracePeriodHours.Value = xlic.ExpirationGracePeriod.Hours;

						HashSet<string> options = xlic.SchemaOptions;

						//Standard
						if (options.Contains("Base")) chkBase.Checked = true;
						if (options.Contains("AgentScheduleMessenger")) chkAgentScheduleMessenger.Checked = true;
						if (options.Contains("AgentSelfService")) chkAgentSelfService.Checked = true;
						if (options.Contains("HolidayPlanner")) chkHolidayPlanner.Checked = true;
						if (options.Contains("PayrollIntegration")) chkPayrollIntegration.Checked = true;
						if (options.Contains("PerformanceManager")) chkPerformanceManager.Checked = true;
						if (options.Contains("RealtimeAdherence")) chkRealtimeAdherence.Checked = true;
						if (options.Contains("ShiftTrades")) chkShiftTrades.Checked = true;
						if (options.Contains("Developer")) chkDeveloper.Checked = true;
						if (options.Contains("MyTimeWeb")) chkMyTimeWeb.Checked = true;
						if (options.Contains("MobileReports")) chkMobileReports.Checked = true;
						if (options.Contains("SMSLink")) checkBoxSMS.Checked = true;
						if (options.Contains("CalendarLink")) checkBoxCalendar.Checked = true;
						if (options.Contains("Version8")) checkBoxVersion8.Checked = true;

						//Freemium
						if (options.Contains("Forecasts")) chkFreemium.Checked = true;

						if (xlic.SchemaName != "TeleoptiCCC" & xlic.SchemaName != "TeleoptiCCCFreemium")
						{
							MessageBox.Show("Sorry, this tool can only handle TeleoptiCCC licenses.\nPlease choose another!", "Not a TeleoptiCCC license", MessageBoxButtons.OK);
							Initialize_form();
						}
					}
					catch (IOException ex)
					{
						MessageBox.Show("Error: could not read file from disk. Original error: " + ex.Message);
					}
				}
				else
				{
					MessageBox.Show("There was an error opening the file, or the public key was not found", "Error opening or checking file", MessageBoxButtons.OK);
				}
			}
		}


		private void LoadLicenseFile_Click(object sender, EventArgs e)
		{
			LicenseFileLoad();
		}

		private void LicenseFileSaveAndClose()
		{
			if (chkDeveloper.Checked == true)
			{
				MessageBox.Show("The Developer option is selected. Please make sure that this license is for internal use only.", "Developer option is for internal use only", MessageBoxButtons.OK);
			}
			IFormatProvider invariant = CultureInfo.InvariantCulture;
			var xdoc = new XmlDocument();
			XmlDeclaration xmlDeclaration = xdoc.CreateXmlDeclaration("1.0", "utf-8", null);
			XmlElement rootNode = xdoc.CreateElement("License");
			xdoc.InsertBefore(xmlDeclaration, xdoc.DocumentElement);
			xdoc.AppendChild(rootNode);
			rootNode.AppendChild(xdoc.CreateElement("CustomerName")).AppendChild(xdoc.CreateTextNode(txtbxCustomerName.Text));
			rootNode.AppendChild(xdoc.CreateElement("ExpirationDate")).AppendChild(xdoc.CreateTextNode(dtpkrExpirationDate.Value.ToString("s", invariant)));
			rootNode.AppendChild(xdoc.CreateElement("ExpirationGracePeriod")).AppendChild(xdoc.CreateTextNode("P" + numExpirationGracePeriodDays.Value.ToString(invariant) + "D"));
			rootNode.AppendChild(xdoc.CreateElement("MaxActiveAgents")).AppendChild(xdoc.CreateTextNode(numMaxActiveAgents.Value.ToString(invariant)));
			rootNode.AppendChild(xdoc.CreateElement("MaxActiveAgentsGrace")).AppendChild(xdoc.CreateTextNode(numMaxActiveAgentsGrace.Value.ToString(invariant)));


			//Standard License
			if (!chkFreemium.Checked)
			{
				rootNode.AppendChild(xdoc.CreateElement("AgentsOrSeats")).AppendChild(xdoc.CreateTextNode(comboBoxAgentsOrSeats.SelectedIndex.ToString(invariant)));
				rootNode.AppendChild(xdoc.CreateElement("MaxSeats")).AppendChild(xdoc.CreateTextNode(numMaxActiveAgents.Value.ToString(invariant)));
				rootNode.AppendChild(xdoc.CreateElement("SeatRatio")).AppendChild(xdoc.CreateTextNode(numericUpDownCountRatio.Value.ToString(invariant)));

				XmlElement elmOptions = xdoc.CreateElement("TeleoptiCCC");
				if (chkBase.Checked) elmOptions.AppendChild(xdoc.CreateElement("Base")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkAgentScheduleMessenger.Checked) elmOptions.AppendChild(xdoc.CreateElement("AgentScheduleMessenger")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkAgentSelfService.Checked) elmOptions.AppendChild(xdoc.CreateElement("AgentSelfService")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkDeveloper.Checked) elmOptions.AppendChild(xdoc.CreateElement("Developer")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkHolidayPlanner.Checked) elmOptions.AppendChild(xdoc.CreateElement("HolidayPlanner")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkPayrollIntegration.Checked) elmOptions.AppendChild(xdoc.CreateElement("PayrollIntegration")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkPerformanceManager.Checked) elmOptions.AppendChild(xdoc.CreateElement("PerformanceManager")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkRealtimeAdherence.Checked) elmOptions.AppendChild(xdoc.CreateElement("RealtimeAdherence")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkShiftTrades.Checked) elmOptions.AppendChild(xdoc.CreateElement("ShiftTrades")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkMyTimeWeb.Checked) elmOptions.AppendChild(xdoc.CreateElement("MyTimeWeb")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkMobileReports.Checked) elmOptions.AppendChild(xdoc.CreateElement("MobileReports")).AppendChild(xdoc.CreateTextNode("true"));
				if (checkBoxSMS.Checked) elmOptions.AppendChild(xdoc.CreateElement("SMSLink")).AppendChild(xdoc.CreateTextNode("true"));
				if (checkBoxCalendar.Checked) elmOptions.AppendChild(xdoc.CreateElement("CalendarLink")).AppendChild(xdoc.CreateTextNode("true"));
				if (checkBoxVersion8.Checked) elmOptions.AppendChild(xdoc.CreateElement("Version8")).AppendChild(xdoc.CreateTextNode("true"));

				rootNode.AppendChild(elmOptions);
			}
			else
			{
				XmlElement elmOptions = xdoc.CreateElement("TeleoptiCCCFreemium");
				elmOptions.AppendChild(xdoc.CreateElement("Forecasts")).AppendChild(xdoc.CreateTextNode("true"));
				rootNode.AppendChild(elmOptions);
			}

			var agreement = (LicenseAgreement)comboBoxAgreement.SelectedItem;
			XmlElement agreementNode = xdoc.CreateElement("Agreement");
			agreementNode.AppendChild(xdoc.CreateTextNode(agreement.ResourceName));

			rootNode.AppendChild(agreementNode);

			try
			{
				XmlLicense.Sign(xdoc, new CryptoSettingsFromMachineStore(XmlLicense.ContainerName));
			}
			catch (CryptographicException ex)
			{
				MessageBox.Show("Oops! There was an error when signing the license. Exception is: " + ex.Message, "Signing error", MessageBoxButtons.OK);
			}


			using (var sfd = new SaveFileDialog())
			{
				sfd.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
				sfd.Title = "Save the license file";
				//Console.Write(dtpkrExpirationDate.Value.ToString("yyyy-MM-dd"));
				sfd.RestoreDirectory = true;
				sfd.FileName = replaceInvalidFileChars(dtpkrExpirationDate.Value.ToString("yyyyMMdd", invariant) + "_" + txtbxCustomerName.Text + ".xml");

				if (sfd.ShowDialog() == DialogResult.OK)
				{
					Stream fs;
					if ((fs = sfd.OpenFile()) != null)
					{
						xdoc.Save(fs);
						fs.Dispose();
					}
				}
			}
		}


		private void CreateAndSave_Click(object sender, EventArgs e)
		{
			LicenseFileSaveAndClose();
		}
			
			
		private void btnAdd3Mon_Click(object sender, EventArgs e)
		{
			dtpkrExpirationDate.Value = LicenseExpirationDate.GetLicenseExpirationDate(dtpkrExpirationDate.Value,  90);
		}

		private void btnAdd2Yrs_Click(object sender, EventArgs e)
		{
			dtpkrExpirationDate.Value = LicenseExpirationDate.GetLicenseExpirationDate(dtpkrExpirationDate.Value,  2 * 365);
		}

		private void chkAgentSelfService_CheckedChanged(object sender, EventArgs e)
		{
			if (chkAgentSelfService.Checked == true)
			{
				chkShiftTrades.Checked = true;
				chkShiftTrades.Enabled = false;

				chkAgentScheduleMessenger.Checked = true;
				chkAgentScheduleMessenger.Enabled = false;

				chkHolidayPlanner.Checked = true;
				chkHolidayPlanner.Enabled = false;

			}
			if (chkAgentSelfService.Checked == false)
			{
				chkShiftTrades.Checked = false ;
				chkShiftTrades.Enabled = true;

				chkAgentScheduleMessenger.Checked = false;
				chkAgentScheduleMessenger.Enabled = true;

				chkHolidayPlanner.Checked = false;
				chkHolidayPlanner.Enabled = true;
			}
		}

		private void loadLicenseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LicenseFileLoad();

		}

		private void createAndSaveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LicenseFileSaveAndClose();
		}

		private void quitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();

		}

		private void clearAllFieldsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Initialize_form();
		}

		private void demoSettingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EnterDemoSettings();

		}

		
	

		
		private void comboBoxAgentsOrSeats_SelectedIndexChanged(object sender, EventArgs e)
		{
							var seat = comboBoxAgentsOrSeats.SelectedIndex == 1;
				numericUpDownCountRatio.Enabled = seat;
				labelRatio.Enabled = seat;
			
		}

		private void chkFreemium_CheckedChanged(object sender, EventArgs e)
		{
				if (chkFreemium.Checked)
					grpBoxModules.Visible = false;
				else
					grpBoxModules.Visible = true;

				EnterFreemiumSettings();

		}

	 
	}
}
