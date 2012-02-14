using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortalCode.Common.Controls;
using Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade;

namespace Teleopti.Ccc.AgentPortal.Common.Controls
{
    public partial class DateSelectorView : BaseUserControl, IDateSelectorView
    {
        private readonly DateSelectorPresenter _presenter;
        private BindingList<DateTime> _deletedList;
        private readonly ShiftTradeModel _model;

        public DateSelectorView(ShiftTradeModel model)
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            _model = model;
            _presenter = new DateSelectorPresenter(_model, this);
        }

        private void DateSelectorView_Load(object sender, EventArgs e)
        {
            _presenter.Initialize();
            setCulture();
        }

        private void setCulture()
        {
            dateTimePickerAdvCurrentSelectedDate.Culture = CultureInfo.CurrentCulture;
            dateTimePickerFromDate.Culture = CultureInfo.CurrentCulture;
            dateTimePickerToDate.Culture = CultureInfo.CurrentCulture;
        }

        private void buttonAdvAddDate_Click(object sender, EventArgs e)
        {
            AddDate();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public BindingList<DateTime> DateList
        {
            get
            {
                if (DesignMode)
                    return new BindingList<DateTime>();
                return (BindingList<DateTime>)listBoxDates.DataSource;
            }
            set
            {
                listBoxDates.DisplayMember = "Date";
                listBoxDates.DataSource = value ;
            }
        }

        public DateTime CurrentDate
        {
            get
            {
                if (DesignMode)
                    return new DateTime(2009,4,4);
                return dateTimePickerAdvCurrentSelectedDate.Value;
            }
            set { dateTimePickerAdvCurrentSelectedDate.Value = value; }
        }

        public DateTime CurrentDateFrom
        {
            get
            {
                if (DesignMode)
                    return new DateTime(2009, 4, 4);
                return dateTimePickerFromDate.Value;
            }
            set { dateTimePickerFromDate.Value = value; }
        }

        public DateTime CurrentDateTo
        {
            get
            {
                if (DesignMode)
                    return new DateTime(2009, 4, 4);
                return dateTimePickerToDate.Value;
            }
            set { dateTimePickerToDate.Value = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public BindingList<DateTime> SelectedDeleteDates
        {
            get { return _presenter.GetSelectedItems(listBoxDates.SelectedItems, _deletedList); }
            set
            {
                _deletedList = value;
                updateListBox();
            }
        }

        private void updateListBox()
        {
            listBoxDates.SelectedItems.Clear();
            foreach (DateTime dateTime in _deletedList)
            {
                listBoxDates.SelectedItems.Add(dateTime);
            }
        }

        public DateTime InitialDate
        {
            set
            {
                dateTimePickerAdvCurrentSelectedDate.Value = value;
                dateTimePickerFromDate.Value = value;
                dateTimePickerToDate.Value = value;
            }
        }

        public void AddDate()
        {
            _presenter.AddDate();           
        }

        public void AddDateRange()
        {
            _presenter.AddDateRange();
        }

        public void DeleteDates()
        {
            _presenter.DeleteDates();
        }

        private void buttonAdvAddDates_Click(object sender, EventArgs e)
        {
            AddDateRange();
        }

        private void buttonAdvDelete_Click(object sender, EventArgs e)
        {
            DeleteDates();
        }

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();
            dateTimePickerAdvCurrentSelectedDate.Calendar.TodayButton.Text = UserTexts.Resources.Today;
            dateTimePickerAdvCurrentSelectedDate.Calendar.NoneButton.Text = UserTexts.Resources.None;
            dateTimePickerFromDate.Calendar.TodayButton.Text = UserTexts.Resources.Today;
            dateTimePickerFromDate.Calendar.NoneButton.Text = UserTexts.Resources.None;
            dateTimePickerToDate.Calendar.TodayButton.Text = UserTexts.Resources.Today;
            dateTimePickerToDate.Calendar.NoneButton.Text = UserTexts.Resources.None;
        }

        public void EnableContent(bool enabled)
        {
            groupBoxMultipleDates.Enabled = enabled;
            groupBoxSingelDateSelection.Enabled = enabled;
            labelSelectDate.Enabled = enabled;
            buttonAdvDelete.Enabled = enabled;
            listBoxDates.SelectionMode = enabled ? SelectionMode.MultiSimple : SelectionMode.None;
        }
    }
}
