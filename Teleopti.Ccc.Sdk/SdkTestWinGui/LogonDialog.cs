using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SdkTestWinGui
{
    public partial class LogonDialog : Form
    {
        public LogonDialog()
        {
            InitializeComponent();
        }

        public string LogonName
        {
            get
            {
                return textBoxUser.Text;
            }
        }

        public string PassWord
        {
            get
            {
                return textBoxPwd.Text;
            }
        }
    }
}
