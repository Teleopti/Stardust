using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using System;
using System.Windows.Forms;
using System.Diagnostics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.WorkloadPages
{
    public partial class WorkloadGeneral : BaseUserControl, IPropertyPage
    {
        public WorkloadGeneral()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            SetColor();
        }

        private void SetColor()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
        }


        #region IPropertyPage Members

        public void Populate(IAggregateRoot aggregateRoot)
        {
            Workload workload = aggregateRoot as Workload;

            textBoxName.Text = workload.Name;
            textBoxDescription.Text = workload.Description;
            textBoxSkill.Text = workload.Skill.Name;
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            Workload workload = aggregateRoot as Workload;
            try
            {
                workload.Name = textBoxName.Text.Trim();
            }
            catch(ArgumentException ex)
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(string.Concat(UserTexts.Resources.WorkloadNameIsInvalid, "  "), "", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, (RightToLeft == System.Windows.Forms.RightToLeft.Yes ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0));
                Trace.WriteLine(ex.Message);
                return false;
            }
            workload.Description = textBoxDescription.Text;

            return true;
        }

        public string PageName
        {
            get { return UserTexts.Resources.General; }
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

        #endregion
    }
}
