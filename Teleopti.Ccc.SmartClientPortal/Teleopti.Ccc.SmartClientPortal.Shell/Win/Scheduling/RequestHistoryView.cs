using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Scheduling.Requests;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
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
                var ret = Guid.Empty;
                if (comboBoxAdvPersons.SelectedItem != null)
                    ret = (Guid)comboBoxAdvPersons.SelectedValue;
                return ret;
            }
        }

        public int StartRow { get; set; }
        
        public void FillRequestList(ListViewItem[] listViewItems)
        {
            listViewRequests.Items.Clear();
            listViewRequests.Items.AddRange(listViewItems);
        }

        public int TotalCount { set; get; }

        public int PageSize
        {
            get { return 25; }
        }

        public void ShowRequestDetails(string details)
        {
            textBox1.Text = details;
        }

        public void ShowForm()
        {
            ShowDialog();
        }


        public void FillPersonCombo(ICollection<IRequestPerson> persons, Guid preselectedPerson)
        {
	        if (persons == null || persons.Count == 0)
	        {
		        comboBoxAdvPersons.Items.Clear();
		        comboBoxAdvPersons.SelectedValue = null;
		        return;
	        }

            comboBoxAdvPersons.ValueMember = "Id";
            comboBoxAdvPersons.DisplayMember = "Name";
            comboBoxAdvPersons.DataSource = persons;
        	var selectedValue = comboBoxAdvPersons.SelectedValue;
            comboBoxAdvPersons.SelectedValue = preselectedPerson;
			//if preselectedPerson is not present in list SelectedValue will be null, then  reset back
            if (comboBoxAdvPersons.SelectedValue == null && selectedValue != null)
				comboBoxAdvPersons.SelectedValue = selectedValue;
        }

        public void SetNextEnabledState(bool enabled)
        {
            linkNext.Enabled = enabled;
        }

        public void SetPreviousEnabledState(bool enabled)
        {
            linkPrevious.Enabled = enabled;
        }

        public void ShowDataSourceException(DataSourceException dataSourceException)
        {
            using (var view = new SimpleExceptionHandlerView(dataSourceException,
                                                                    Resources.RequestHistory,
                                                                    Resources.ServerUnavailable))
            {
                view.ShowDialog();
            }
        }

        private void ComboBoxAdvPersonsSelectedIndexChanged(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<RequestHistoryPageChanged>().Publish(RequestHistoryPage.First);
        }

        private void listViewRequests_SelectedIndexChanged(object sender, EventArgs e)
        {
            IRequestHistoryLightweight selected = null;
            if (listViewRequests.FocusedItem != null)
                selected = listViewRequests.FocusedItem.Tag as IRequestHistoryLightweight;
            _eventAggregator.GetEvent<RequestHistoryRequestChanged>().Publish(selected);
        }

        private void LinkPreviousLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _eventAggregator.GetEvent<RequestHistoryPageChanged>().Publish(RequestHistoryPage.Previous);
        }

        private void LinkNextLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _eventAggregator.GetEvent<RequestHistoryPageChanged>().Publish(RequestHistoryPage.Next);
        }
    }
}
