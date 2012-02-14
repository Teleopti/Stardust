using System;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.SkillPages
{
    //It is spelled correctly in British english according to Andreas
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Optimisation")]
    public partial class SkillOptimisation : BaseUserControl, IPropertyPage
    {
        public SkillOptimisation()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public void Populate(IAggregateRoot aggregateRoot)
        {
            var skill = aggregateRoot as ISkill;
            if (skill == null) throw new ArgumentNullException("aggregateRoot", "The supplied root must be of type: ISkill.");

            trackBarExPriority.Value = skill.Priority;
            trackBarExOverStaffingFactor.Value = (int)(100 * skill.OverstaffingFactor.Value);
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            var skill = aggregateRoot as ISkill;
            if (skill == null) throw new ArgumentNullException("aggregateRoot", "The supplied root must be of type: ISkill.");

            skill.Priority = trackBarExPriority.Value;
            skill.OverstaffingFactor = new Percent((double)trackBarExOverStaffingFactor.Value/100);

            return true;
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
            get { return UserTexts.Resources.Optimisation; }
        }

		//reset to default values (in the middle)
		private void trackBarExPriorityDoubleClick(object sender, EventArgs e)
		{
			trackBarExPriority.Value = 4;
		}

		private void trackBarExOverStaffingFactorDoubleClick(object sender, EventArgs e)
		{
			trackBarExOverStaffingFactor.Value = 50;
		}
    }
}
