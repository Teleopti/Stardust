using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling.Requests;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class RequestHistoryView : BaseDialogForm, IRequestHistoryView
    {
        private readonly IEventAggregator _eventAggregator;

        public RequestHistoryView(IEventAggregator eventAggregator)
            : this()
        {
            _eventAggregator = eventAggregator;
        }

        public RequestHistoryView()
        {
            InitializeComponent();
            SetTexts();

        }

        public Guid SelectedPerson
        {
            get 
            { 
                var ret = new Guid();
                if (comboBoxAdvPersons.SelectedItem != null)
                    ret = (Guid)comboBoxAdvPersons.SelectedValue;
                return ret;
            }
        }

        public int StartRow
        {
            get { return 1; }
        }

        public void FillRequestList(ListViewItem[] listViewItems)
        {
            listViewRequests.Items.Clear();
            listViewRequests.Items.AddRange(listViewItems);
        }

        public IRequestHistoryLightWeight SelectedRequest
        {
            get { throw new NotImplementedException(); }
        }

        public void ShowRequestDetails(string details)
        {
            throw new NotImplementedException();
        }

        public void ShowForm()
        {
            ShowDialog();
        }


        public void FillPersonCombo(ICollection<IRequestPerson> persons, Guid preSelectedPerson)
        {
            comboBoxAdvPersons.ValueMember = "Id";
            comboBoxAdvPersons.DisplayMember = "Name";
            comboBoxAdvPersons.DataSource = persons;
            comboBoxAdvPersons.SelectedValue = preSelectedPerson;
        }

        private void ComboBoxAdvPersonsSelectedIndexChanged(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<RequestHistoryPersonChanged>().Publish("");
            
        }
    }
}
