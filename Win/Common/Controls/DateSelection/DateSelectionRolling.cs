using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Time;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.DateSelection
{
    public partial class DateSelectionRolling : IDateSelectionControl
    {
        private IList<PeriodScaleUnit> _periodScaleUnitTypes;
        private int _factor = -1;

        public event EventHandler<DateRangeChangedEventArgs> DateRangeChanged;

        public DateSelectionRolling()
        {
            InitializeComponent();
            if (!DesignMode) initializePeriodUnitComboBox();
        }

        /// <summary>
        /// Gets or sets the button apply text.
        /// </summary>
        /// <value>The button apply text.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-15
        /// </remarks>
        [Category("Teleopti Texts"), Localizable(true), Browsable(true)]
        public string ButtonApplyText
        {
            get { return buttonApply.Text; }
            set { buttonApply.Text = value; }
        }

        /// <summary>
        /// Gets or sets the factor to get dates from history or future.
        /// -1 gets history, 1 gets future
        /// </summary>
        /// <value>The factor.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-15
        /// </remarks>
        [Category("Teleopti behavior"), Browsable(true), DefaultValue(-1)]
        public int Factor
        {
            get { return _factor; }
            set { _factor = value; }
        }

        public override bool HasHelp
        {
            get
            {
                return false;
            }
        }

        private void initializePeriodUnitComboBox()
        {
            _periodScaleUnitTypes = new List<PeriodScaleUnit>();
            _periodScaleUnitTypes.Add(new PeriodScaleUnit(PeriodScaleUnitType.Days,UserTexts.Resources.Days));
            _periodScaleUnitTypes.Add(new PeriodScaleUnit(PeriodScaleUnitType.Weeks,UserTexts.Resources.Weeks));
            _periodScaleUnitTypes.Add(new PeriodScaleUnit(PeriodScaleUnitType.Months,UserTexts.Resources.Months));
            _periodScaleUnitTypes.Add(new PeriodScaleUnit(PeriodScaleUnitType.Years,UserTexts.Resources.Years));
            comboBoxPeriodScaleUnit.DataSource = _periodScaleUnitTypes;
            comboBoxPeriodScaleUnit.DisplayMember = "DisplayName";
            comboBoxPeriodScaleUnit.SelectedItem = _periodScaleUnitTypes[1];
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            if (DateRangeChanged == null) return;

            ReadOnlyCollection<DateOnlyPeriod> dates = new ReadOnlyCollection<DateOnlyPeriod>(GetSelectedDates());
            if (dates.Count>0)
            {
                DateRangeChangedEventArgs dateRangeChangedEventArgs = new DateRangeChangedEventArgs(dates);
                DateRangeChanged(this, dateRangeChangedEventArgs);
            }
        }

        #region IDateSelectionControl Members

        /// <summary>
        /// Gets or sets a value indicating whether [show apply button].
        /// </summary>
        /// <value><c>true</c> if [show apply button]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-02
        /// </remarks>
        [Browsable(true), DefaultValue(true), Category("Teleopti Behavior")]
        public bool ShowApplyButton
        {
            get
            {
                return buttonApply.Visible;
            }
            set
            {
                buttonApply.Visible = value;
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
        [Browsable(false)]
        public IList<DateOnlyPeriod> GetSelectedDates()
        {
            string value = maskedTextBoxNumberOf.Text.Trim();
            int valueAsInt;
            IList<DateOnlyPeriod> datesToReturn = new List<DateOnlyPeriod>();
            if (!string.IsNullOrEmpty(value) &&
                int.TryParse(value, out valueAsInt))
            {
                PeriodScaleUnit periodScaleUnit = (PeriodScaleUnit) comboBoxPeriodScaleUnit.SelectedItem;
                DateOnlyPeriod dateTimePeriod = PeriodScaleUnit.GetScaleUnitDateTimePeriod(
                    periodScaleUnit.UnitType,
                    valueAsInt*_factor,
                    DateOnly.Today);

                datesToReturn.Add(dateTimePeriod);
            }

            return datesToReturn;
        }

        #endregion
    }
}