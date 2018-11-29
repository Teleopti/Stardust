using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Time;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection
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

        [Category("Teleopti Texts"), Localizable(true), Browsable(true)]
        public string ButtonApplyText
        {
            get { return buttonApply.Text; }
            set { buttonApply.Text = value; }
        }

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

        private void buttonApplyClick(object sender, EventArgs e)
        {
            if (DateRangeChanged == null) return;

            var dates = new ReadOnlyCollection<DateOnlyPeriod>(GetSelectedDates());
            if (dates.Count>0)
            {
                var dateRangeChangedEventArgs = new DateRangeChangedEventArgs(dates);
                DateRangeChanged(this, dateRangeChangedEventArgs);
            }
        }

        #region IDateSelectionControl Members

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

        [Browsable(false)]
        public IList<DateOnlyPeriod> GetSelectedDates()
        {
            string value = maskedTextBoxNumberOf.Text.Trim();
            int valueAsInt;
            IList<DateOnlyPeriod> datesToReturn = new List<DateOnlyPeriod>();
            if (!string.IsNullOrEmpty(value) &&
                int.TryParse(value, out valueAsInt))
            {
                var periodScaleUnit = (PeriodScaleUnit) comboBoxPeriodScaleUnit.SelectedItem;
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