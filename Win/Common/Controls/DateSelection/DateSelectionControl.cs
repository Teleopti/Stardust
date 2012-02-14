using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.DateSelection
{
    public partial class DateSelectionControl
    {
        private readonly IList<IDateSelectionControl> _dateSelectionControls = new List<IDateSelectionControl>();
        private bool _showAddButtons = true;
        private bool _showTabArea= true;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateSelectionControl"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-02
        /// </remarks>
        public DateSelectionControl()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            SetColors();
            _dateSelectionControls.Add(dateSelectionFromTo1);
            _dateSelectionControls.Add(dateSelectionRolling1);
            _dateSelectionControls.Add(dateSelectionCalendar1);
            dateSelectionFromTo1.SetCulture(CultureInfo.CurrentCulture);
            dateSelectionFromTo1.ValueChanged += dateSelectionFromTo1_DateRangeValueChanged;
        }

        private void SetColors()
        {
            BackColor = ColorHelper.TabBackColor();
        }

        public void AllowValueChangedEvent(bool allow)
        {
            if (!allow) dateSelectionFromTo1.ValueChanged -= dateSelectionFromTo1_DateRangeValueChanged;
        }

        /// <summary>
        /// Gets or sets the color of the tab panel back.
        /// </summary>
        /// <value>The color of the tab panel back.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2009-02-26
        /// </remarks>
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

        /// <summary>
        /// Gets or sets the tab panel border style.
        /// </summary>
        /// <value>The tab panel border style.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2009-02-26
        /// </remarks>
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

        /// <summary>
        /// Gets the selected dates.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-02
        /// </remarks>
        public IList<DateOnlyPeriod> GetCurrentlySelectedDates()
        {
            var dateSelection =
                tabControlAdvDateSelection.SelectedTab.Controls[0] as IDateSelectionControl;
            if (dateSelection == null) return new List<DateOnlyPeriod>();

            return dateSelection.GetSelectedDates();
        }

        /// <summary>
        /// Occurs when [date range changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-02
        /// </remarks>
        public event EventHandler<DateRangeChangedEventArgs> DateRangeChanged;

        private void dateSelectionCalendar1_DateRangeChanged(object sender, DateRangeChangedEventArgs e)
        {
            OnDateRangeChanged(e.SelectedDates);
        }

        private void dateSelectionFromTo1_DateRangeChanged(object sender, DateRangeChangedEventArgs e)
        {
            OnDateRangeChanged(e.SelectedDates);
        }

        private void dateSelectionFromTo1_DateRangeValueChanged(object sender, EventArgs e)
        {
            if (sender.GetType() != typeof (DateSelectionFromTo)) return;
            var dates = GetCurrentlySelectedDates();
            var dateCollection = new ReadOnlyCollection<DateOnlyPeriod>(dates);
            OnDateRangeChanged(dateCollection);
        }

        private void dateSelectionRolling1_DateRangeChanged(object sender, DateRangeChangedEventArgs e)
        {
            OnDateRangeChanged(e.SelectedDates);
        }

        private void OnDateRangeChanged(ReadOnlyCollection<DateOnlyPeriod> dates)
        {
        	var handler = DateRangeChanged;
            if (handler!= null)
            {
                handler.Invoke(this,new DateRangeChangedEventArgs(dates));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show date selection rolling].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show date selection rolling]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-17
        /// </remarks>
        [DefaultValue(true), Browsable(true), Category("Teleopti Appearance")]
        public bool ShowDateSelectionRolling
        {
            get { return tabPageAdvRolling.TabVisible; }
            set { tabPageAdvRolling.TabVisible = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show date selection from to].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show date selection from to]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-17
        /// </remarks>
        [DefaultValue(true), Browsable(true), Category("Teleopti Appearance")]
        public bool ShowDateSelectionFromTo
        {
            get { return tabPageAdvFromTo.TabVisible; }
            set { tabPageAdvFromTo.TabVisible = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show date selection calendar].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show date selection calendar]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-17
        /// </remarks>
        [DefaultValue(true),Browsable(true),Category("Teleopti Appearance")]
        public bool ShowDateSelectionCalendar
        {
            get { return tabPageAdvCalendar.TabVisible; }
            set { tabPageAdvCalendar.TabVisible = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show add buttons].
        /// </summary>
        /// <value><c>true</c> if [show add buttons]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-02
        /// </remarks>
        [Browsable(true),Category("Teleopti Appearance")]
        public bool ShowAddButtons
        {
            get { return _showAddButtons; }
            set
            {
                _showAddButtons = value;
                SetApplyButtonsVisible();
            }
        }

        private void SetApplyButtonsVisible()
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

        /// <summary>
        /// Gets or sets a value indicating whether [use future].
        /// </summary>
        /// <value><c>true</c> if [use future]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-02
        /// </remarks>
        [DefaultValue(false), Browsable(true), Category("Teleopti Appearance")]
        public bool UseFuture
        {
            get { return (dateSelectionRolling1.Factor == 1); }
            set { dateSelectionRolling1.Factor = (value) ? 1 : -1; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has help information.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has help information; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-01
        /// </remarks>
        public override bool HasHelp
        {
            get
            {
                return false;
            }
        }
    }
}
