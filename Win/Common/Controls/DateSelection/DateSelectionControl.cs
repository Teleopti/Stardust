using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.DateSelection
{
    public partial class DateSelectionControl
    {
        private readonly IList<IDateSelectionControl> _dateSelectionControls = new List<IDateSelectionControl>();
        private bool _showAddButtons = true;
        private bool _showTabArea= true;

        public DateSelectionControl()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            _dateSelectionControls.Add(dateSelectionFromTo1);
            _dateSelectionControls.Add(dateSelectionRolling1);
            _dateSelectionControls.Add(dateSelectionCalendar1);
            dateSelectionFromTo1.SetCulture(CultureInfo.CurrentCulture);
            dateSelectionFromTo1.ValueChanged += dateSelectionFromTo1DateRangeValueChanged;
        }

        public void AllowValueChangedEvent(bool allow)
        {
            if (!allow) dateSelectionFromTo1.ValueChanged -= dateSelectionFromTo1DateRangeValueChanged;
        }

        public Color TabPanelBackColor
        {
            get
            {
                return tabControlAdvDateSelection.TabPanelBackColor;
            }
            set
            {
                tabControlAdvDateSelection.TabPanelBackColor = value;
            }
        }

        public BorderStyle TabPanelBorderStyle
        {
            get
            {
                return tabControlAdvDateSelection.BorderStyle;
            }
            set
            {
                tabControlAdvDateSelection.BorderStyle = value;
            }
        }

        public IList<DateOnlyPeriod> GetCurrentlySelectedDates()
        {
            var dateSelection =
                tabControlAdvDateSelection.SelectedTab.Controls[0] as IDateSelectionControl;
            if (dateSelection == null) return new List<DateOnlyPeriod>();

            return dateSelection.GetSelectedDates();
        }

        public event EventHandler<DateRangeChangedEventArgs> DateRangeChanged;

        private void dateSelectionCalendar1DateRangeChanged(object sender, DateRangeChangedEventArgs e)
        {
            onDateRangeChanged(e.SelectedDates);
        }

        private void dateSelectionFromTo1DateRangeChanged(object sender, DateRangeChangedEventArgs e)
        {
            onDateRangeChanged(e.SelectedDates);
        }

        private void dateSelectionFromTo1DateRangeValueChanged(object sender, EventArgs e)
        {
            if (sender.GetType() != typeof (DateSelectionFromTo)) return;
            var dates = GetCurrentlySelectedDates();
            var dateCollection = new ReadOnlyCollection<DateOnlyPeriod>(dates);
            onDateRangeChanged(dateCollection);
        }

        private void dateSelectionRolling1DateRangeChanged(object sender, DateRangeChangedEventArgs e)
        {
            onDateRangeChanged(e.SelectedDates);
        }

        private void onDateRangeChanged(ReadOnlyCollection<DateOnlyPeriod> dates)
        {
        	var handler = DateRangeChanged;
            if (handler!= null)
            {
                handler.Invoke(this,new DateRangeChangedEventArgs(dates));
            }
        }

        [DefaultValue(true), Browsable(true), Category("Teleopti Appearance")]
        public bool ShowDateSelectionRolling
        {
            get { return tabPageAdvRolling.TabVisible; }
            set { tabPageAdvRolling.TabVisible = value; }
        }

        [DefaultValue(true), Browsable(true), Category("Teleopti Appearance")]
        public bool ShowDateSelectionFromTo
        {
            get { return tabPageAdvFromTo.TabVisible; }
            set { tabPageAdvFromTo.TabVisible = value; }
        }

        [DefaultValue(true),Browsable(true),Category("Teleopti Appearance")]
        public bool ShowDateSelectionCalendar
        {
            get { return tabPageAdvCalendar.TabVisible; }
            set { tabPageAdvCalendar.TabVisible = value; }
        }

        [Browsable(true),Category("Teleopti Appearance")]
        public bool ShowAddButtons
        {
            get { return _showAddButtons; }
            set
            {
                _showAddButtons = value;
                setApplyButtonsVisible();
            }
        }

        private void setApplyButtonsVisible()
        {
            foreach (var d in _dateSelectionControls)
            {
                d.ShowApplyButton = _showAddButtons;
            }
        }

        [Browsable(true), Category("Teleopti Appearance")]
        public bool ShowTabArea
        {
            get { return _showTabArea; }
            set
            {
                _showTabArea = value;
                if (!_showTabArea) tabControlAdvDateSelection.ItemSize = new Size(1, 1);
            }
        }

        public void SetErrorOnDateSelectionFromTo(string error)
        {
            dateSelectionFromTo1.SetErrorOnEndTime(error);
        }

        [Browsable(false)]
        public void SetInitialDates(DateOnlyPeriod dateTime)
        {
            dateSelectionFromTo1.WorkPeriodStart = dateTime.StartDate;
            dateSelectionFromTo1.WorkPeriodEnd = dateTime.EndDate;
            dateSelectionFromTo1.Refresh();

            dateSelectionCalendar1.SetCurrentDate(dateTime.StartDate);
        }

        [DefaultValue(false), Browsable(true), Category("Teleopti Appearance")]
        public bool UseFuture
        {
            get { return (dateSelectionRolling1.Factor == 1); }
            set { dateSelectionRolling1.Factor = (value) ? 1 : -1; }
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
