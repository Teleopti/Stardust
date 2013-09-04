using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Matrix
{
    public partial class ReportWindow : Form
    {
        public ReportWindow()
        {
            InitializeComponent();
        }

        public Uri Url
        {
            get { return webBrowser1.Url; }
            set { webBrowser1.Url = value; 
        } 
        }
    }
}
