using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Teleopti.Support.Tool.Controls.ConnectionString
{
    
    public partial class Summary : UserControl
    {
        private bool executeOk;
        
        public Summary()
        {
            InitializeComponent();
        }

        public void ClearSettings()
        {
           
            Tsettings.Controls.Clear();
        }

        public void AddSetting(string setting, string value)
        {
            Label l1 = new Label();
            Label l2 = new Label();
            l1.AutoSize = true;
            l2.AutoSize = true;
            l1.Text = setting;
            l2.Text = value;
            Tsettings.Controls.Add(l1);
            Tsettings.Controls.Add(l2);
        }
    
        public void AddJobStep(Control control)
        {
            leftFlowPanel.Controls.Add(control);
        }


        public bool ExecuteOk
        {
            get { return executeOk; }
            set
            {
                executeOk=value;
                if (value)
                {
                    resultImage.Image = global::Teleopti.Support.Tool.Properties.Resources.accept1;

                    resultImage.Refresh();
                }
                else
                {
                    resultImage.Image = global::Teleopti.Support.Tool.Properties.Resources.delete;
                    resultImage.Refresh();
                }
            }
        }

        public string ResultText
        {
            get { return lResult.Text; }
            set { lResult.Text = value; }
        }
        public bool LogVisible
        {
            get { return log.Visible; }
            set { log.Visible = value; }
        }
        

        public LinkLabel ViewLog
        {
            get { return log; }
        }

      

       
    }
}
