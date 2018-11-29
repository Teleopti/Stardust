using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts.Forecasting
{
    /// <summary>
    /// Represents the Navigation Control for smart parts
    /// </summary>
    /// <remarks>
    /// Created by: Muhamad Risath
    /// Created date: 2008-09-25
    /// </remarks>
    public partial class NavigationControl : UserControl
    {
        private DateOnly _nextDate;
        private DateOnly _previousDate;
        private readonly SmartPartBase forecasterSmartPart;

        public SmartPartBase ForecasterSmartPart
        {
            get 
            { 
                return forecasterSmartPart; 
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationControl"/> class.
        /// </summary>
        /// <param name="forecaster">The forecaster.</param>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 10/8/2008
        /// </remarks>
        public NavigationControl(SmartPartBase forecaster, DateOnlyPeriod period)
        {
            InitializeComponent();
            _nextDate = period.EndDate;
            _previousDate = period.StartDate;
            forecasterSmartPart = forecaster;
        }

        private void NavigationControl_Load(object sender, EventArgs e)
        {

                var culture =
                    TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
                _previousDate = DateOnly.Today;
                int dayOfYear = culture.Calendar.GetDayOfYear(_previousDate.Date);
                _previousDate = new DateOnly(culture.Calendar.AddDays(_previousDate.Date, -dayOfYear + 1));
                _nextDate = new DateOnly(culture.Calendar.AddYears(_previousDate.Date,1));
                _nextDate = new DateOnly(culture.Calendar.AddDays(_nextDate.Date, -1));
                autoLabelYear.Text = culture.Calendar.GetYear(_nextDate.Date).ToString(culture);
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
                var culture =
                    TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
                _nextDate = new DateOnly(culture.Calendar.AddYears(_nextDate.Date,1));
                _previousDate = new DateOnly(culture.Calendar.AddYears(_previousDate.Date, 1));
                autoLabelYear.Text = culture.Calendar.GetYear(_nextDate.Date).ToString(culture);
                RaiseValueChanged();
        }

        private void buttonPrevious_Click(object sender, EventArgs e)
        {
                var culture =
                    TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
                _nextDate = new DateOnly(culture.Calendar.AddYears(_nextDate.Date, -1));
                _previousDate = new DateOnly(culture.Calendar.AddYears(_previousDate.Date, -1));
                autoLabelYear.Text = culture.Calendar.GetYear(_nextDate.Date).ToString(culture);
                RaiseValueChanged();
        }

        private void RaiseValueChanged()
        {
            //smartpart IDs are hard coded at tree_node click event when smart parts are initialliy loaded from ForecasterNavigator 
            int smartpartId = Int32.Parse(ForecasterSmartPart.SmartPartId, CultureInfo.InvariantCulture);

            switch (smartpartId)
            {
                case 1:
                    ValidationSmartPart validation = (ValidationSmartPart)ForecasterSmartPart;
                    validation.ForecastPeriod = new DateOnlyPeriod(_previousDate, _nextDate);
                    RefreshSmartpart();
                    break;

                case 2:
                    DetailedSmartPart detailed = (DetailedSmartPart)ForecasterSmartPart;
                    detailed.ForecastPeriod = new DateOnlyPeriod(_previousDate, _nextDate);
                    RefreshSmartpart();
                    break;

                case 3:
                    BudgetsSmartPart budgeted = (BudgetsSmartPart) ForecasterSmartPart;
                    budgeted.ForecastPeriod = new DateOnlyPeriod(_previousDate, _nextDate);
                    RefreshSmartpart();
                    break;
            }
        }

        private void RefreshSmartpart()
        {
            IList<SmartPartParameter> smartPartParameter = new List<SmartPartParameter>();
            smartPartParameter.Add(ForecasterSmartPart.SmartPartParameters[0]);
            ForecasterSmartPart.RefreshSmartPart(smartPartParameter);
        }
    }
}