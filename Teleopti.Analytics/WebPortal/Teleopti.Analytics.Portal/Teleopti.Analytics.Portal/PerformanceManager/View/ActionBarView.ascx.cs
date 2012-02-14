﻿using System;

namespace Teleopti.Analytics.Portal.PerformanceManager.View
{
    public partial class ActionBarView : System.Web.UI.UserControl
    {
        public event EventHandler DeleteModeChanged;

        protected void Page_Load(object sender, EventArgs e)
        {
            linkButtonDeleteMode.Click += new EventHandler(linkButtonDeleteMode_Click);
        }

        void linkButton1_Click(object sender, EventArgs e)
        {
            if (Session["DeleteMode"] != null)
            {
                if ((bool)Session["DeleteMode"])
                {
                    Session["DeleteMode"] = false;
                    linkButtonDeleteMode.Text = "Delete mode on";
                    OnDeleteModeChanged(e);
                    return;
                }
            }

            Session["DeleteMode"] = true;
            linkButtonDeleteMode.Text = "Delete mode off";
            OnDeleteModeChanged(e);
        }

        protected void linkButtonDeleteMode_Click(object sender, EventArgs e)
        {
            if (Session["DeleteMode"] != null)
            {
                if ((bool)Session["DeleteMode"])
                {
                    Session["DeleteMode"] = false;
                    linkButtonDeleteMode.Text = "Delete mode on";
                    OnDeleteModeChanged(e);
                    return;
                }
            }

            Session["DeleteMode"] = true;
            linkButtonDeleteMode.Text = "Delete mode off";
            OnDeleteModeChanged(e);
        }

        public bool NewReportEnabled
        {
            set
            {
                aNewReport.Disabled = !value;
            }
        }

        public bool DeleteModeEnabled
        {
            set
            {
                linkButtonDeleteMode.Enabled = value;
            }
        }

        protected void OnDeleteModeChanged(EventArgs e)
        {
            if (DeleteModeChanged != null)
            {
                DeleteModeChanged(this, e);
            }
        }
    }
}