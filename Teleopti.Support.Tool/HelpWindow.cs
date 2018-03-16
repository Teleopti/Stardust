using System.Windows.Forms;
using Teleopti.Support.Tool.Tool;

namespace Teleopti.Support.Tool
{
	public partial class HelpWindow : Form, ISupportCommand
	{
		public static readonly string HelpText = @"Command line arguments:

		-? or ? or -HELP or HELP, Shows this help
		-MO[MODE] is where to put the config files values: Develop or Deploy (Develop is default)
			-BC Backup config settings for SSO and CSP ( before patching if there is custom settings)
			-RC Restore the config settings for SSO and CSP from the backup, combine with -MODEPLOY (use after patching if there is custom settings)
		-TC[ALL|RC|CUSTOMER] Set ToggleMode to ALL, RC or CUSTOMER.
		-SET [searchFor] [replaceWith] Set a setting value
				Example: Teleopti.Support.Tool.exe -TC""ALL""";

		public HelpWindow()
		{
			InitializeComponent();
		}

		public HelpWindow(string text)
			: this()
		{
			textBox1.Text = text;
			textBox1.SelectionStart = 0;
			textBox1.SelectionLength = 0;
		}

		public void Execute()
		{
			ShowDialog();
		}
	}
}