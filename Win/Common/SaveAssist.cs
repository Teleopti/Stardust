using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Common
{
    public partial class SaveAssist : Form
    {
        public SaveAssist()
        {
            InitializeComponent();
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            close();
        }

        private void SaveAssist_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void close()
        {
            timer1.Enabled = false;
            //this.ParentForm.Activate();
            Close();
        }
    }
}
