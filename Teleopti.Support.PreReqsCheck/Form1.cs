using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CheckPreRequisites.Checks;
using Microsoft.Win32;

namespace CheckPreRequisites
{
	public partial class Form1 : Form
	{
		private readonly HardwareCheck _hardwareCheck;
		private readonly WebCheck _webCheck;
		private readonly V6Check _v6Check;
		private readonly DatabaseCheck _databaseCheck;

		public Form1()
		{
			InitializeComponent();
			_hardwareCheck = new HardwareCheck(this);
			_databaseCheck = new DatabaseCheck(this);
			_webCheck = new WebCheck(this);
			_v6Check = new V6Check(this, _databaseCheck);
		}
		
		// ReSharper disable InconsistentNaming
		private void button1_Click(object sender, EventArgs e)
		{
			ClearListView();

			if (comboBoxServerSetup.SelectedItem.ToString().Contains("Web"))
			{
				AgentLevel();
				_hardwareCheck.RunHardWareChecks((int) numericUpDownAgents.Value);
				_webCheck.RunWebChecks();
			}

			if (comboBoxServerSetup.SelectedItem.ToString().Contains("DB"))
			{
				AgentLevel();
				_hardwareCheck.RunHardWareChecks((int) numericUpDownAgents.Value);
				_databaseCheck.RunDbChecks(comboBoxSQLInstance.SelectedItem.ToString().Trim());
			}

			if (comboBoxServerSetup.SelectedItem.ToString().Contains("Version6"))
				_v6Check.RunV6Checks(textBoxDBName.Text);
		}

		private void CheckDbConnection_Click(object sender, EventArgs e)
		{
			_databaseCheck.CheckDbInternals(textBoxDBName.Text);
		}
		
		public string ConnStringGet(string dbName)
		{
			string connStr;

			if (radioButtonWinAuth.Checked)
			{
				connStr =
					string.Format(
						"Application Name=TeleoptiPreReqCheck;Data Source={0};Persist Security Info=True;Integrated Security=SSPI;Initial Catalog=" +
						dbName,
						textBoxSQLServerName.Text);
			}
			else
			{
				connStr =
					string.Format(
						"Application Name=TeleoptiPreReqCheck;Data Source={0};Persist Security Info=True;User Id={1};Password={2};Initial Catalog=" +
						dbName,
						textBoxSQLServerName.Text, SQLLogin.Text, SQLPwd.Text);
			}
			return connStr;
		}

		#region PrintStuff

		public void printInfo(string info)
		{
			labelInfo.Text = info;
		}

		public void printNewFeature(string checkWhat, string feature, string minValue, string featureValue)
		{
			listView1.Items.Add(checkWhat);
			listView1.Items[listView1.Items.Count - 1].SubItems.Add(feature);
			listView1.Items[listView1.Items.Count - 1].SubItems.Add(featureValue);
			listView1.Items[listView1.Items.Count - 1].SubItems.Add(minValue);
		}

		public void printFeatureStatus(bool featureStatus, string toolTip = "")
		{
			if (featureStatus)
			{
				listView1.Items[listView1.Items.Count - 1].SubItems.Add("OK");
				listView1.Items[listView1.Items.Count - 1].ForeColor = Color.Green;
			}
			else
			{
				listView1.Items[listView1.Items.Count - 1].SubItems.Add("not OK!");
				listView1.Items[listView1.Items.Count - 1].ForeColor = Color.Red;
			}
			listView1.Items[listView1.Items.Count - 1].ToolTipText = toolTip;
		}

		public void printFeatureStatus(string status)
		{
			if (status == "Warning")
			{
				listView1.Items[listView1.Items.Count - 1].SubItems.Add("Warning");
				listView1.Items[listView1.Items.Count - 1].ForeColor = Color.Orange;
			}
			else
			{
				listView1.Items[listView1.Items.Count - 1].SubItems.Add(status);
				listView1.Items[listView1.Items.Count - 1].ForeColor = Color.Black;
			}
		}

		#endregion

		//enumerate all local SQL instances
		private void SQLInstanceEnum()
		{
			comboBoxSQLInstance.Items.Clear();

			var rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server");
			if (rk != null)
			{
				var instances = (String[]) rk.GetValue("InstalledInstances");
				if (instances != null)
				{
					if (instances.Length > 0)
					{
						foreach (var element in instances)
						{
							if (element == "MSSQLSERVER")
								comboBoxSQLInstance.Items.Add(System.Environment.MachineName);
							else
								comboBoxSQLInstance.Items.Add(System.Environment.MachineName + @"\" + element);
						}
					}
				}
			}
			else
				comboBoxSQLInstance.Items.Add("NoInstanceDetected");
		}

		//If shared web+Db server then get a higher number of agents
		private int AgentLevel()
		{
			var numberOfAgent = (int) numericUpDownAgents.Value;

			//Check if the server is intended to be both web AND db
			if (comboBoxServerSetup.SelectedItem.ToString().Contains("Web")
			    && comboBoxServerSetup.SelectedItem.ToString().Contains("DB"))
			{
				if (numberOfAgent < 500)
					numberOfAgent = numberOfAgent + 1500;
				else if (numberOfAgent <= 4000)
					numberOfAgent = numberOfAgent + 4000;
				else if (numberOfAgent > 4000)
					// ReSharper disable LocalizableElement
					MessageBox.Show("Warning! Running more then 4000 agents on a single server is not supported");
				// ReSharper restore LocalizableElement
			}
			return numberOfAgent;
		}

		private static void CopyListViewToClipboard(ListView lv)
		{
			var buffer = new StringBuilder();

			foreach (ColumnHeader header in lv.Columns)
			{
				buffer.Append(header.Text.Trim());
				buffer.Append('\t');
			}
			buffer.Append(System.Environment.NewLine);

			foreach (ListViewItem item in lv.Items)
			{
				foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
				{
					buffer.Append(subItem.Text);
					buffer.Append('\t');
				}
				buffer.Append(System.Environment.NewLine);
			}

			Clipboard.SetText(buffer.ToString().Trim());
		}

		private void CopyAllListViewToClipboard()
		{
			var buffer = new StringBuilder();

			foreach (ColumnHeader header in listView1.Columns)
			{
				buffer.Append(header.Text.Trim());
				buffer.Append('\t');
			}

			buffer.Append(System.Environment.NewLine);

			foreach (ListViewItem item in listView1.Items)
			{
				foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
				{
					buffer.Append(subItem.Text);
					buffer.Append('\t');
				}
				buffer.Append(System.Environment.NewLine);
			}

			buffer.Append(System.Environment.NewLine);
			buffer.Append(System.Environment.NewLine);

			foreach (ColumnHeader header in listView2.Columns)
			{
				buffer.Append(header.Text.Trim());
				buffer.Append('\t');
			}
			buffer.Append(System.Environment.NewLine);

			foreach (ListViewItem item in listView2.Items)
			{
				foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
				{
					buffer.Append(subItem.Text);
					buffer.Append('\t');
				}
				buffer.Append(System.Environment.NewLine);
			}

			buffer.Append(System.Environment.NewLine);
			buffer.Append(System.Environment.NewLine);

			foreach (ColumnHeader header in listView3.Columns)
			{
				buffer.Append(header.Text.Trim());
				buffer.Append('\t');
			}
			buffer.Append(System.Environment.NewLine);

			foreach (ListViewItem item in listView3.Items)
			{
				foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
				{
					buffer.Append(subItem.Text);
					buffer.Append('\t');
				}
				buffer.Append(System.Environment.NewLine);
			}


			Clipboard.SetText(buffer.ToString().Trim());
		}

		private void contextMenuStrip1_Click(object sender, EventArgs e)
		{
			if (comboBoxServerSetup.SelectedItem.ToString().Contains("Version6"))
				CopyAllListViewToClipboard();
			else
				CopyListViewToClipboard(listView1);
		}

		private void radioButton2_CheckedChanged(object sender, EventArgs e)
		{
			SQLLogin.Enabled = true;
			SQLPwd.Enabled = true;
			SQLLogin.Text = "";
		}

		private void radioButtonWniAuth(object sender, EventArgs e)
		{
			SQLLogin.Enabled = false;
			SQLPwd.Enabled = false;
			var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
			if (windowsIdentity != null)
				SQLLogin.Text = windowsIdentity.Name;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			comboBoxServerSetup.SelectedIndex = 0;
			groupBoxSQLInstance.Hide();
			var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
			if (windowsIdentity != null)
				SQLLogin.Text = windowsIdentity.Name;
		}

		private void comboBoxServerSetup_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxServerSetup.SelectedItem.ToString().Contains("DB"))
			{
				groupBoxSQLInstance.Show();
				SQLInstanceEnum();
				comboBoxSQLInstance.SelectedIndex = 0;
				groupBoxDatabase.Hide();
			}
			else if (comboBoxServerSetup.SelectedItem.ToString().Contains("Version6"))
			{
				groupBoxDatabase.Show();
				groupBoxSQLInstance.Hide();
			}
			else
			{
				comboBoxSQLInstance.Items.Clear();
				groupBoxSQLInstance.Hide();
				groupBoxDatabase.Hide();
			}
		}

		private void comboBoxSQLInstance_SelectedIndexChanged(object sender, EventArgs e)
		{
			textBoxSQLServerName.Text = comboBoxSQLInstance.SelectedItem.ToString();
		}

		private void ClearListView()
		{
			listView1.Items.Clear();
		}

		private void listView2_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		// ReSharper restore InconsistentNaming
	}
}