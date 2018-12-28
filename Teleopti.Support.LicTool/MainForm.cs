using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
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
			loadAgreements();
			comboBoxAgentsOrSeats.SelectedIndex = 0;
			labelRatio.Enabled = false;
			numericUpDownCountRatio.Value = 1.5M;
			numericUpDownCountRatio.Enabled = false;
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
				var path = Directory.GetCurrentDirectory();

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
			   return windowsPathBadChars.IsMatch(testName) 
				   ? Regex.Replace(testName,"[" + Regex.Escape("<>:\"/\\|?*") + "]","_") 
				   : testName;
		   }

		private void Initialize_form()
		{
			txtbxCustomerName.Text = null;
			dtpkrExpirationDate.Value = DateTime.Now;
			chkPerpetual.Checked = false;
			numExpirationGracePeriodDays.Value = 0;
			ExpirationGracePeriodHours.Value = 0;
			numMaxActiveAgents.Value = 0;
			comboBoxAgentsOrSeats.SelectedIndex = 0;
			numericUpDownCountRatio.Value = 1.5M;
			numMaxActiveAgentsGrace.Value = 0;
			comboBoxMajor.SelectedItem = "8";

			chkBase.Checked = false;
			chkLifestyle.Checked = false;
			chkShiftTrader.Checked = false;
			chkVacationPlanner.Checked = false;
			chkOvertimeAvailability.Checked = false;
			chkNotify.Checked = false;
			chkRealtimeAdherence.Checked = false;
			chkAgentScheduleMessenger.Checked = false;
			chkSMSLink.Checked = false;
			chkCalendarLink.Checked = false;
			chkPerformanceManager.Checked = false;
			chkPayrollIntegration.Checked = false;
			chkMyTeam.Checked = false;
			chkVNextPilot.Checked = false;
			chkOutbound.Checked = false;
			chkSeatPlanner.Checked = false;
			chkOvertimeRequests.Checked = false;
			chkBPOExchange.Checked = false;
			chkChatBot.Checked = false;
			chkInsights.Checked = false;
		}

		private void EnterDemoSettings()
		{
			txtbxCustomerName.Text = "Demo product activation key! For internal use only!";
			numExpirationGracePeriodDays.Value = 5;
			numMaxActiveAgents.Value = 100;
			numMaxActiveAgentsGrace.Value = 10;
			comboBoxMajor.SelectedItem = "8";

			chkBase.Checked = true;
			chkLifestyle.Checked = true;
			chkShiftTrader.Checked = true;
			chkVacationPlanner.Checked = true;
			chkOvertimeAvailability.Checked = true;
			chkNotify.Checked = true;
			chkRealtimeAdherence.Checked = true;
			chkAgentScheduleMessenger.Checked = true;
			chkPerformanceManager.Checked = true;
			chkPayrollIntegration.Checked = true;
		}

		private void EnterFreemiumSettings()
		{
			txtbxCustomerName.Text = "Freemium product activation key";
			numExpirationGracePeriodDays.Value = 30;
			numMaxActiveAgents.Value = 1;
			numMaxActiveAgentsGrace.Value = 10;
			comboBoxAgreement.Text = "TeleoptiLic_En_Sw_Forecasts.txt";
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
		private void ProductActivationKeyFileLoad()
		{
			using (var openFileDialog = new OpenFileDialog())
			{
				openFileDialog.InitialDirectory = "\\\\a380\\T-Files\\BU Teleopti CCC\\LICENSE KEYS V8";
				openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
				openFileDialog.FilterIndex = 2;
				openFileDialog.RestoreDirectory = true;

				var pubkey = GetPublicKey();
				if (openFileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(pubkey))
				{
					Initialize_form();
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
						var xlic = new XmlLicense(xdoc, pubkey,true);

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
						chkPerpetual.Checked = xlic.Perpetual;

						numericUpDownCountRatio.Value = xlic.Ratio;
						comboBoxAgentsOrSeats.SelectedIndex = xlic.AgentsOrSeats;

						numExpirationGracePeriodDays.Value = xlic.ExpirationGracePeriod.Days;
						ExpirationGracePeriodHours.Value = xlic.ExpirationGracePeriod.Hours;

						comboBoxMajor.SelectedItem = xlic.MajorVersion;

						HashSet<string> options = xlic.SchemaOptions;

						//Standard
						if (options.Contains("Base")) chkBase.Checked = true;
						if (options.Contains("Lifestyle")) chkLifestyle.Checked = true;
						if (options.Contains("ShiftTrader")) chkShiftTrader.Checked = true;
						if (options.Contains("VacationPlanner")) chkVacationPlanner.Checked = true;
						if (options.Contains("OvertimeAvailability")) chkOvertimeAvailability.Checked = true;
						if (options.Contains("Notify")) chkNotify.Checked = true;
						if (options.Contains("RealtimeAdherence")) chkRealtimeAdherence.Checked = true;
						if (options.Contains("AgentScheduleMessenger")) chkAgentScheduleMessenger.Checked = true;
						if (options.Contains("SMSLink")) chkSMSLink.Checked = true;
						if (options.Contains("CalendarLink")) chkCalendarLink.Checked = true;
						if (options.Contains("PerformanceManager")) chkPerformanceManager.Checked = true;
						if (options.Contains("PayrollIntegration")) chkPayrollIntegration.Checked = true;
						if (options.Contains("MyTeam")) chkMyTeam.Checked = true;
						if (options.Contains("VNext")) chkVNextPilot.Checked = true;
						if (options.Contains("Outbound")) chkOutbound.Checked = true;
						if (options.Contains("OvertimeRequests")) chkOvertimeRequests.Checked = true;
						if (options.Contains("SeatPlanner")) chkSeatPlanner.Checked = true;
						if (options.Contains("BPOExchange")) chkBPOExchange.Checked = true;
						if (options.Contains("Grant")) chkChatBot.Checked = true;
						if (options.Contains("Insights")) chkInsights.Checked = true;
						
						//Freemium
						if (options.Contains("Forecasts")) chkFreemium.Checked = true;

						if (xlic.SchemaName != "TeleoptiCCC" && xlic.SchemaName != "TeleoptiCCCFreemium" && xlic.SchemaName != "TeleoptiWFM")
						{
							MessageBox.Show("Sorry, this tool can only handle Teleopti WFM product activation keys.\nPlease choose another!",
								"Not a Teleopti WFM product activation key", MessageBoxButtons.OK);
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

		private void LoadProductActivationKeyFile_Click(object sender, EventArgs e)
		{
			ProductActivationKeyFileLoad();
		}

		private void ProductActivationKeyFileSaveAndClose()
		{
			if (string.IsNullOrEmpty(comboBoxMajor.SelectedItem.ToString()))
			{
				MessageBox.Show(this, "You must set a Major Version!", "Major Version missing");
				return;
			} 
			IFormatProvider invariant = CultureInfo.InvariantCulture;
			var xdoc = new XmlDocument();
			XmlDeclaration xmlDeclaration = xdoc.CreateXmlDeclaration("1.0", "utf-8", null);
			XmlElement rootNode = xdoc.CreateElement("License");
			xdoc.InsertBefore(xmlDeclaration, xdoc.DocumentElement);
			xdoc.AppendChild(rootNode);
			rootNode.AppendChild(xdoc.CreateElement("CustomerName")).AppendChild(xdoc.CreateTextNode(txtbxCustomerName.Text));
			if (chkPerpetual.Checked)
				rootNode.AppendChild(xdoc.CreateElement("Perpetual")).AppendChild(xdoc.CreateTextNode("true"));
			rootNode.AppendChild(xdoc.CreateElement("ExpirationDate")).AppendChild(xdoc.CreateTextNode(dtpkrExpirationDate.Value.ToString("s", invariant)));
			rootNode.AppendChild(xdoc.CreateElement("ExpirationGracePeriod")).AppendChild(xdoc.CreateTextNode("P" + numExpirationGracePeriodDays.Value.ToString(invariant) + "D"));
			rootNode.AppendChild(xdoc.CreateElement("MaxActiveAgents")).AppendChild(xdoc.CreateTextNode(numMaxActiveAgents.Value.ToString(invariant)));
			rootNode.AppendChild(xdoc.CreateElement("MaxActiveAgentsGrace")).AppendChild(xdoc.CreateTextNode(numMaxActiveAgentsGrace.Value.ToString(invariant)));
			rootNode.AppendChild(xdoc.CreateElement("MajorVersion")).AppendChild(xdoc.CreateTextNode(comboBoxMajor.SelectedItem.ToString()));

			//Standard Product Activation Key
			if (!chkFreemium.Checked)
			{
				rootNode.AppendChild(xdoc.CreateElement("AgentsOrSeats")).AppendChild(xdoc.CreateTextNode(comboBoxAgentsOrSeats.SelectedIndex.ToString(invariant)));
				rootNode.AppendChild(xdoc.CreateElement("MaxSeats")).AppendChild(xdoc.CreateTextNode(numMaxActiveAgents.Value.ToString(invariant)));
				rootNode.AppendChild(xdoc.CreateElement("SeatRatio")).AppendChild(xdoc.CreateTextNode(numericUpDownCountRatio.Value.ToString(invariant)));

				XmlElement elmOptions = xdoc.CreateElement("TeleoptiWFM");
				if (chkBase.Checked)
					elmOptions.AppendChild(xdoc.CreateElement("Base")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkLifestyle.Checked)
					elmOptions.AppendChild(xdoc.CreateElement("Lifestyle")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkShiftTrader.Checked)
					elmOptions.AppendChild(xdoc.CreateElement("ShiftTrader")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkVacationPlanner.Checked)
					elmOptions.AppendChild(xdoc.CreateElement("VacationPlanner")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkOvertimeAvailability.Checked)
					elmOptions.AppendChild(xdoc.CreateElement("OvertimeAvailability")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkNotify.Checked)
					elmOptions.AppendChild(xdoc.CreateElement("Notify")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkRealtimeAdherence.Checked)
					elmOptions.AppendChild(xdoc.CreateElement("RealtimeAdherence")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkAgentScheduleMessenger.Checked) 
					elmOptions.AppendChild(xdoc.CreateElement("AgentScheduleMessenger")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkSMSLink.Checked) 
					elmOptions.AppendChild(xdoc.CreateElement("SMSLink")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkCalendarLink.Checked) 
					elmOptions.AppendChild(xdoc.CreateElement("CalendarLink")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkPerformanceManager.Checked) 
					elmOptions.AppendChild(xdoc.CreateElement("PerformanceManager")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkPayrollIntegration.Checked) 
					elmOptions.AppendChild(xdoc.CreateElement("PayrollIntegration")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkMyTeam.Checked)
					elmOptions.AppendChild(xdoc.CreateElement("MyTeam")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkVNextPilot.Checked)
					elmOptions.AppendChild(xdoc.CreateElement("VNext")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkOutbound.Checked)
					elmOptions.AppendChild(xdoc.CreateElement("Outbound")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkOvertimeRequests.Checked)
					elmOptions.AppendChild(xdoc.CreateElement("OvertimeRequests")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkSeatPlanner.Checked)
					elmOptions.AppendChild(xdoc.CreateElement("SeatPlanner")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkBPOExchange.Checked)
					elmOptions.AppendChild(xdoc.CreateElement("BPOExchange")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkChatBot.Checked)
					elmOptions.AppendChild(xdoc.CreateElement("Grant")).AppendChild(xdoc.CreateTextNode("true"));
				if (chkInsights.Checked)
					elmOptions.AppendChild(xdoc.CreateElement("Insights")).AppendChild(xdoc.CreateTextNode("true"));

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
				MessageBox.Show("Oops! There was an error when signing the product activation key. Exception is: " + ex.Message, "Signing error", MessageBoxButtons.OK);
			}


			using (var sfd = new SaveFileDialog())
			{
				sfd.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
				sfd.Title = "Save the product activation key file";
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
			ProductActivationKeyFileSaveAndClose();
		}
			
		private void btnAdd3Mon_Click(object sender, EventArgs e)
		{
			dtpkrExpirationDate.Value = LicenseExpirationDate.GetLicenseExpirationDate(dtpkrExpirationDate.Value,  90);
		}

		private void btnAdd2Yrs_Click(object sender, EventArgs e)
		{
			dtpkrExpirationDate.Value = LicenseExpirationDate.GetLicenseExpirationDate(dtpkrExpirationDate.Value,  2 * 365);
		}

		private void chkLifestyle_CheckedChanged(object sender, EventArgs e)
		{
			if (chkLifestyle.Checked)
			{
				chkShiftTrader.Checked = true;
				chkShiftTrader.Enabled = false;

				chkVacationPlanner.Checked = true;
				chkVacationPlanner.Enabled = false;

				chkOvertimeAvailability.Checked = true;
				chkOvertimeAvailability.Enabled = false;

			}
			else
			{
				chkShiftTrader.Checked = false;
				chkShiftTrader.Enabled = true;

				chkVacationPlanner.Checked = false;
				chkVacationPlanner.Enabled = true;

				chkOvertimeAvailability.Checked = false;
				chkOvertimeAvailability.Enabled = true;
			}
		}


		private void chkNotify_CheckedChanged(object sender, EventArgs e)
		{
			if (chkNotify.Checked)
			{
				chkRealtimeAdherence.Checked = true;
				chkRealtimeAdherence.Enabled = false;

				chkAgentScheduleMessenger.Checked = true;
				chkAgentScheduleMessenger.Enabled = false;

				chkSMSLink.Checked = true;
				chkSMSLink.Enabled = false;

				chkCalendarLink.Checked = true;
				chkCalendarLink.Enabled = false;

			}
			else
			{
				chkRealtimeAdherence.Checked = false;
				chkRealtimeAdherence.Enabled = true;

				chkAgentScheduleMessenger.Checked = false;
				chkAgentScheduleMessenger.Enabled = true;

				chkSMSLink.Checked = false;
				chkSMSLink.Enabled = true;

				chkCalendarLink.Checked = false;
				chkCalendarLink.Enabled = true;
			}
		}

		private void loadProductActivationKeyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ProductActivationKeyFileLoad();

		}

		private void createAndSaveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ProductActivationKeyFileSaveAndClose();
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
			grpBoxModules.Visible = !chkFreemium.Checked;

			EnterFreemiumSettings();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			Initialize_form();
		}
	}
}
