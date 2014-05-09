using System;
using System.Windows.Forms;
using Teleopti.Support.Code.Tool;

namespace Teleopti.Support.Tool
{
    public partial class HelpWindow : Form, ISupportCommand
    {
        public HelpWindow()
        {
            InitializeComponent();
        }

        public HelpWindow(string text):this()
        {
            textBox1.Text = text;
            textBox1.SelectionStart = 0;
            textBox1.SelectionLength = 0;
        }

	    public void Execute(ModeFile modeFile)
	    {
		    ShowDialog();
	    }
    }
}
