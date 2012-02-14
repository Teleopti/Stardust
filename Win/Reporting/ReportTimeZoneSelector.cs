using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.Win.Reporting
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
            ICccTimeZoneInfo defaultTimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
            IList<ICccTimeZoneInfo> timeZoneList = TimeZoneInfo.GetSystemTimeZones().Select(t => (ICccTimeZoneInfo)new CccTimeZoneInfo(t)).ToList();

            comboBoxAdvTimeZone.DisplayMember = "DisplayName";
            comboBoxAdvTimeZone.DataSource = timeZoneList;

             foreach (ICccTimeZoneInfo timeZone in timeZoneList)
            {
                if (timeZone.StandardName == defaultTimeZone.StandardName)
                {
                    comboBoxAdvTimeZone.SelectedItem = timeZone;
                    break;
                }
            }
        }

        public ICccTimeZoneInfo TimeZone()
        {
            if (comboBoxAdvTimeZone.InvokeRequired)
            {
                return (ICccTimeZoneInfo)comboBoxAdvTimeZone.Invoke(new Func<ICccTimeZoneInfo>(TimeZone));
            }
            else
            {
                return (ICccTimeZoneInfo)comboBoxAdvTimeZone.SelectedItem;
            }
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
