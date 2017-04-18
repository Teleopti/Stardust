using System.Windows.Forms;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WorkloadPages
{
    public partial class WorkloadOpenHours : BaseUserControl, IPropertyPage
    {

        public WorkloadOpenHours()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            SetColors();

        }

        private void SetColors()
        {
           BackColor = ColorHelper.WizardBackgroundColor();
            weekOpenHoursGridWorkload.BackColor = ColorHelper.WizardBackgroundColor();
            weekOpenHoursGridWorkload.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
        }


        public void Populate(IAggregateRoot aggregateRoot)
        {
            Workload workload = aggregateRoot as Workload;
            weekOpenHoursGridWorkload.LoadDays(workload);
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            Workload workload = aggregateRoot as Workload;
            weekOpenHoursGridWorkload.AddTimesToWorkload(workload);
            return true;
        }

        public string PageName
        {
            get { return UserTexts.Resources.OpenHours; }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-19
        /// </remarks>
        public void SetEditMode()
        {
        }

        private void weekOpenHoursGridWorkload_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true)
            {
                switch (e.KeyCode)
                {
                    case Keys.X:
                        e.Handled = true;
                        break;
                }
            }
        }

        #region grid events
        //protected override void OnMouseDown(MouseEventArgs e)
        //{
        //    base.OnMouseDown(e);
        //    gridOpenHours.Refresh();
        //}


        #endregion

        #region Handle OpenHours

        #endregion


        
    }
}
