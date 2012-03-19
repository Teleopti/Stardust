using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
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

        public int StartRow { get; set; }
        
        public void FillRequestList(ListViewItem[] listViewItems)
        {
            listViewRequests.Items.Clear();
            listViewRequests.Items.AddRange(listViewItems);
        }

        public int TotalCount { set; get; }

        public int PageSize
        {
            get { return 50; }
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
            comboBoxAdvPersons.ValueMember = "Id";
            comboBoxAdvPersons.DisplayMember = "Name";
            comboBoxAdvPersons.DataSource = persons;
            comboBoxAdvPersons.SelectedValue = preselectedPerson;
        }

        public void SetNextEnabledState(bool enabled)
        {
            buttonAdvNext.Enabled = enabled;
        }

        public void SetPreviousEnabledState(bool enabled)
        {
            buttonAdvPrevious.Enabled = enabled;
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

        private void ButtonAdvNextClick(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<RequestHistoryPageChanged>().Publish(RequestHistoryPage.Next);
        }

        private void ButtonAdvPreviousClick(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<RequestHistoryPageChanged>().Publish(RequestHistoryPage.Previous);
        }

        private void listViewRequests_SelectedIndexChanged(object sender, EventArgs e)
        {
            IRequestHistoryLightweight selected = null;
            if (listViewRequests.FocusedItem != null)
                selected = listViewRequests.FocusedItem.Tag as IRequestHistoryLightweight;
            _eventAggregator.GetEvent<RequestHistoryRequestChanged>().Publish(selected);
        }
    }
}
