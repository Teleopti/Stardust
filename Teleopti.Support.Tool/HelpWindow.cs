using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Teleopti.Support.Tool
{
    public partial class HelpWindow : Form
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

    }
}
