using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Teleopti.Support.Tool.Controls.General
{

    public partial class ProgressItem : UserControl
    {
        private bool _running;
        private bool _error;
        private bool _finished;
        private bool _waiting;
        public ProgressItem()
        {
            InitializeComponent();

        }

        public bool ExecuteStep
        {
            set { runStep.Checked = value; }
            get { return runStep.Checked; }
        }

          public string ItemText
        {
            get { return titleLabel.Text; }
            set { titleLabel.Text = value; }
        }
        public string Subtext
        {
            get { return subtextLabel.Text; }
            set { subtextLabel.Text = value; }
        }
       
        public bool Finished
        {
            get { return _finished; }
            set
            {
                _finished=value;
                if (value)
                {
                 
                    pictureBox1.Image = global::Teleopti.Support.Tool.Properties.Resources.accept;
                    pictureBox1.Refresh();
                }
            }
        }
        public bool Running
        {
            get { return _running; }
            set
            {
             
                _running = value;
                if (value)
                {
                    pictureBox1.Image = global::Teleopti.Support.Tool.Properties.Resources.ajax_loader;
                    pictureBox1.Refresh();
                }
               
            }
        }
        public bool Waiting
        {
            get { return _waiting; }
            set
            {
                _waiting=value;
                if (value)
                {
                    pictureBox1.Image = global::Teleopti.Support.Tool.Properties.Resources.warning;
                    pictureBox1.Refresh();
                }
            }
        }
        public bool Error
        {
            get { return _error; }
            set
            {
                _error = value;
                if (value)
                {

                    pictureBox1.Image = global::Teleopti.Support.Tool.Properties.Resources.delete;
                    pictureBox1.Refresh();
                }
            }
        }

    
    }
}
