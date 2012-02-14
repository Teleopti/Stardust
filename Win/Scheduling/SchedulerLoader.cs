using System;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Schedule;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class SchedulerLoader : BaseUserControlWithUnitOfWork
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerLoader"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-04
        /// </remarks>
        public SchedulerLoader()
        {
            InitializeComponent();
            if (StateHolder.IsInitialized) BindScenarios();
            if (!DesignMode) SetTexts();
            SetColor();
        }

        private void SetColor()
        {
            this.tableLayoutPanel1.BackColor = ColorHelper.StandardPanelBackground();
            this.BackColor = ColorHelper.StandardTreeBackgroundColor();
        }


        /// <summary>
        /// Binds the scenarios.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-04
        /// </remarks>
        private void BindScenarios()
        {
            lstScenario.ComboBox.DataSource = new ScenarioRepository(UnitOfWork).FindAllSorted();
            lstScenario.ComboBox.DisplayMember = "Description";
            if (lstScenario.Items.Count > 0)
                lstScenario.SelectedIndex = 0;
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            //if (monthCalendar1.SelectedDates.Count == 0)
            //    monthCalendar1.SelectedDates.Add(DateTime.Today);

            //IEnumerable<DateTime> selectedDates = monthCalendar1.SelectedDates.OfType<DateTime>();

            //DateTimePeriod selectedPeriod;
            //selectedPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
            //    selectedDates.Min().Date,
            //    selectedDates.Max().Date.AddDays(1).AddTicks(-1));
            //Scenario scenario = (Scenario)lstScenario.SelectedItem;
            //SchedulingScreen sc = new SchedulingScreen(selectedPeriod,scenario);
            //sc.Show();
        }
    }
}
