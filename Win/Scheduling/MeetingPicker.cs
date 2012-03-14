﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class MeetingPicker : BaseRibbonForm
    {
        private bool _cancel = true;

        public MeetingPicker(String description, IEnumerable<IPersonMeeting> meetings)
        {
            InitializeComponent();
            if (!DesignMode)
                SetTexts();

            foreach (IPersonMeeting meeting in meetings)
            {
                TupleItem comboItem = new TupleItem();
                comboItem.ValueMember = meeting.BelongsToMeeting;
                comboItem.Text = meeting.BelongsToMeeting.Subject + "  " + meeting.Period.LocalStartDateTime.ToShortTimeString() +
                    " - " + meeting.Period.LocalEndDateTime.ToShortTimeString();
                comboBoxMeetings.Items.Add(comboItem);
            }

            labelPerson.Text = description;

            comboBoxMeetings.SelectedIndex = 0;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            _cancel = false;
            Hide();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
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
