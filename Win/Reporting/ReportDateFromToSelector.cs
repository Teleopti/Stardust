﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.Win.Common;


namespace Teleopti.Ccc.Win.Reporting
{
    public partial class ReportDateFromToSelector : BaseUserControl
    {
        public event EventHandler<EventArgs> PeriodChanged;

        public ReportDateFromToSelector()
        {
            InitializeComponent();

            if (!DesignMode && StateHolderReader.IsInitialized)
            {
                DateOnlyPeriod minPeriod = new DateOnlyPeriod(new DateOnly(DateHelper.MinSmallDateTime),
                                                          new DateOnly(DateHelper.MaxSmallDateTime));
                dateTimePickerAdvWorkAStartDate.SetAvailableTimeSpan(minPeriod);
                dateTimePickerAdvWorkEndPeriod.SetAvailableTimeSpan(minPeriod);

                dateTimePickerAdvWorkAStartDate.Value = DateTime.Today;
                dateTimePickerAdvWorkEndPeriod.Value = DateTime.Today;

                SetTexts();

                dateTimePickerAdvWorkAStartDate.Calendar.NoneButton.Text = UserTexts.Resources.None;
                dateTimePickerAdvWorkEndPeriod.Calendar.NoneButton.Text = UserTexts.Resources.None;

                dateTimePickerAdvWorkAStartDate.Calendar.TodayButton.Text = UserTexts.Resources.Today;
                dateTimePickerAdvWorkEndPeriod.Calendar.TodayButton.Text = UserTexts.Resources.Today;

                dateTimePickerAdvWorkAStartDate.UseCurrentCulture =
                    dateTimePickerAdvWorkEndPeriod.UseCurrentCulture = false;
                dateTimePickerAdvWorkEndPeriod.SetCultureInfoSafe(TeleoptiPrincipal.Current.Regional.Culture);
                dateTimePickerAdvWorkAStartDate.SetCultureInfoSafe(TeleoptiPrincipal.Current.Regional.Culture);
            }
        }

       
         /// <summary>
        /// Gets a value indicating whether this instance is work period valid.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is work period valid; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-31
        /// </remarks>
        [Browsable(false)]
        public bool IsWorkPeriodValid
        {
            get { return (WorkPeriodStart <= WorkPeriodEnd); }
        }

        /// <summary>
        /// Gets or sets the work period start.
        /// </summary>
        /// <value>The work period start.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-31
        /// </remarks>
        [Browsable(false)]
        public DateOnly WorkPeriodStart
        {
            get { return new DateOnly(dateTimePickerAdvWorkAStartDate.Value.Date); }
            set
            {
                dateTimePickerAdvWorkAStartDate.Value = value.Date;
                dateTimePickerAdvWorkAStartDate.RefreshFields();
            }
        }

        /// <summary>
        /// Gets or sets the work period end.
        /// </summary>
        /// <value>The work period end.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-31
        /// </remarks>
        [Browsable(false)]
        public DateOnly WorkPeriodEnd
        {
            get { return new DateOnly(dateTimePickerAdvWorkEndPeriod.Value.Date); }
            set { dateTimePickerAdvWorkEndPeriod.Value = value.Date; }
        }


        /// <summary>
        /// Gets or sets the null string.
        /// </summary>
        /// <value>The null string.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-22
        /// </remarks>
        [Browsable(true), Category("Teleopti Texts"), Localizable(true)]
        public string NullString
        {
            get { return dateTimePickerAdvWorkAStartDate.NullString; }
            set
            {
                dateTimePickerAdvWorkAStartDate.NullString = value;
                dateTimePickerAdvWorkEndPeriod.NullString = value;
            }
        }

     
        public void ShowWarning()
        {
            if (IsWorkPeriodValid == false)
            {
                TimedWarningDialog warning = new TimedWarningDialog();
                warning.WarningShownInSeconds = 2;
                warning.WarningMessageShown = "WARNING! \n the startdate is after enddate!";
                warning.WarningShownNearThisControl = labelTargetPeriodTo;
                warning.ShowDialog(this);
            }
        }

       
        /// <summary>
        /// Handles the ValueChanged event of the dateTimePickerAdvWorkEndPeriod control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2008-03-31
        /// </remarks>
        private void dateTimePickerAdvWorkEndPeriod_ValueChanged(object sender, EventArgs e)
        {
            if (dateTimePickerAdvWorkAStartDate.Value > dateTimePickerAdvWorkEndPeriod.Value)
                dateTimePickerAdvWorkAStartDate.Value = dateTimePickerAdvWorkEndPeriod.Value;

        	var handler = PeriodChanged;
            if (handler != null)
            {
            	handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles the ValueChanged event of the dateTimePickerAdvWorkStartDate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2008-03-31
        /// </remarks>
        private void dateTimePickerAdvWorkStartDate_ValueChanged(object sender, EventArgs e)
        {
            if (dateTimePickerAdvWorkEndPeriod.Value < dateTimePickerAdvWorkAStartDate.Value)
                dateTimePickerAdvWorkEndPeriod.Value = dateTimePickerAdvWorkAStartDate.Value;

        	var handler = PeriodChanged;
            if (handler!= null)
            {
            	handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-15
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode || !StateHolderReader.IsInitialized) return;
            
            base.OnLoad(e);

            WorkPeriodStart = DateOnly.Today;
            WorkPeriodEnd = DateOnly.Today;
            
            dateTimePickerAdvWorkAStartDate.ValueChanged += dateTimePickerAdvWorkStartDate_ValueChanged;
            dateTimePickerAdvWorkEndPeriod.ValueChanged += dateTimePickerAdvWorkEndPeriod_ValueChanged;
        }

       
        /// <summary>
        /// Gets the selected dates.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-02
        /// </remarks>
        [Browsable(false)]
        public IList<DateOnlyPeriod> GetSelectedDates
        {
            get
            {
                IList<DateOnlyPeriod> selectedDates = new List<DateOnlyPeriod>();
                if (IsWorkPeriodValid)
                {
                    selectedDates.Add(new DateOnlyPeriod(WorkPeriodStart, WorkPeriodEnd));
                    return selectedDates;
                }

                return selectedDates;
            }
        }

        public override bool HasHelp
        {
            get
            {
                return false;
            }
        }

        public void SetDateFromLabelText(string text)
        {
            labelTargetPeriodFrom.Text = text;
        }

        public void SetDateToLabelText(string text)
        {
            labelTargetPeriodTo.Text = text;
        }
    }
}
