using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WorkloadPages
{
    public partial class WorkloadQueueAdjustment : BaseUserControl, IPropertyPage
    {
        public WorkloadQueueAdjustment()
        {
            InitializeComponent();
            if (DesignMode) return;

            SetTexts();
            SetColors();
            CultureInfo cultureInfo =
                TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
            percentTextBoxOffered.Setup(cultureInfo);
            percentTextBoxOverflowIn.Setup(cultureInfo);
            percentTextBoxOverflowOut.Setup(cultureInfo);
            percentTextBoxAbandoned.Setup(cultureInfo);
            percentTextBoxAbandonedShort.Setup(cultureInfo);
            percentTextBoxAbandonedWithinServiceLevel.Setup(cultureInfo);
            percentTextBoxAbandonedAfterServiceLevel.Setup(cultureInfo);
        }

        private void SetColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
        }

        public void Populate(IAggregateRoot aggregateRoot)
        {
            IWorkload workload = aggregateRoot as IWorkload;
            if (workload==null) throw new ArgumentNullException("aggregateRoot","The supplied root must be of type: IWorkload.");

            QueueAdjustment queueAdjustment = workload.QueueAdjustments;
            percentTextBoxOffered.DoubleValue = queueAdjustment.OfferedTasks.Value;
            percentTextBoxOverflowIn.DoubleValue = queueAdjustment.OverflowIn.Value;
            percentTextBoxOverflowOut.DoubleValue = queueAdjustment.OverflowOut.Value;
            percentTextBoxAbandoned.DoubleValue = queueAdjustment.Abandoned.Value;
            percentTextBoxAbandonedShort.DoubleValue = queueAdjustment.AbandonedShort.Value;
            percentTextBoxAbandonedWithinServiceLevel.DoubleValue = queueAdjustment.AbandonedWithinServiceLevel.Value;
            percentTextBoxAbandonedAfterServiceLevel.DoubleValue = queueAdjustment.AbandonedAfterServiceLevel.Value;

			if (workload.Skill == null) return;

        	var textManager = new TextManager(workload.Skill.SkillType);
			labelCalculatedCalls.Text = textManager.WordDictionary["TotalStatisticCalculatedTasks"];
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            IWorkload workload = aggregateRoot as IWorkload;
            if (workload == null) throw new ArgumentNullException("aggregateRoot", "The supplied root must be of type: IWorkload.");

            workload.QueueAdjustments = new QueueAdjustment
                                            {
                                                OfferedTasks = new Percent(percentTextBoxOffered.DoubleValue),
                                                OverflowIn = new Percent(percentTextBoxOverflowIn.DoubleValue),
                                                OverflowOut = new Percent(percentTextBoxOverflowOut.DoubleValue),
                                                Abandoned = new Percent(percentTextBoxAbandoned.DoubleValue),
                                                AbandonedShort = new Percent(percentTextBoxAbandonedShort.DoubleValue),
                                                AbandonedWithinServiceLevel =
                                                    new Percent(percentTextBoxAbandonedWithinServiceLevel.DoubleValue),
                                                AbandonedAfterServiceLevel =
                                                    new Percent(percentTextBoxAbandonedAfterServiceLevel.DoubleValue)
                                            };

            return true;
        }

        #region IPropertyPage Members

        public string PageName
        {
            get { return UserTexts.Resources.Calculations; }
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