using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Reporting
{
    public partial class ReportTimeZoneSelector : BaseUserControl
    {
        public ReportTimeZoneSelector()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!StateHolderReader.IsInitialized || DesignMode) return;

            SetTexts();
            LoadTimeZones();
        }

        public void LoadTimeZones()
        {
            TimeZoneInfo defaultTimeZone = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone;
            IList<TimeZoneInfo> timeZoneList = TimeZoneInfo.GetSystemTimeZones().Select(t => t).ToList();

            comboBoxAdvTimeZone.DisplayMember = "DisplayName";
            comboBoxAdvTimeZone.DataSource = timeZoneList;

             foreach (TimeZoneInfo timeZone in timeZoneList)
            {
                if (timeZone.StandardName == defaultTimeZone.StandardName)
                {
                    comboBoxAdvTimeZone.SelectedItem = timeZone;
                    break;
                }
            }
        }

        public TimeZoneInfo TimeZone()
        {
        	if (comboBoxAdvTimeZone.InvokeRequired)
            {
                return (TimeZoneInfo)comboBoxAdvTimeZone.Invoke(new Func<TimeZoneInfo>(TimeZone));
            }
        	return (TimeZoneInfo)comboBoxAdvTimeZone.SelectedItem;
        }

    	public override bool HasHelp
        {
            get
            {
                return false;
            }
        }
    }
}
