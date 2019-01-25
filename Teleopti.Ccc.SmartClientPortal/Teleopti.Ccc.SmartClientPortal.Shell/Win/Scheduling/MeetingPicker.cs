using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    public partial class MeetingPicker : BaseDialogForm
    {
        private bool _cancel = true;

        public MeetingPicker(String description, IEnumerable<IPersonMeeting> meetings)
        {
            InitializeComponent();
            if (!DesignMode)
                SetTexts();

			var timeZone = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone;
            foreach (IPersonMeeting meeting in meetings)
            {
                TupleItem comboItem = new TupleItem();
                comboItem.ValueMember = meeting.BelongsToMeeting;
				var period = meeting.Period.TimePeriod(timeZone);
                comboItem.Text = meeting.BelongsToMeeting.GetSubject(new NoFormatting()) + "  " + period.ToShortTimeString();
                comboBoxMeetings.Items.Add(comboItem);
            }

            labelPerson.Text = description;

            comboBoxMeetings.SelectedIndex = 0;
        }

        private void buttonOkClick(object sender, EventArgs e)
        {
            _cancel = false;
            Hide();
        }

        private void buttonCancelClick(object sender, EventArgs e)
        {
            _cancel = true;
            Hide();
        }

        public IMeeting SelectedMeeting()
        {
            if (_cancel)
                return null;

            return (IMeeting) ((TupleItem)comboBoxMeetings.SelectedItem).ValueMember;
        }
    }
}
