using System;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.SkillPages
{
	public partial class SkillThresholds : BaseUserControl, IPropertyPage
	{
		public SkillThresholds()
		{
			InitializeComponent();

			var cultureInfo = TeleoptiPrincipal.Current.Regional.Culture;
			percentTextBoxOverstaffing.Setup(cultureInfo);
			percentTextBoxSeriousUnderstaffing.Setup(cultureInfo);
			percentTextBoxUnderstaffing.Setup(cultureInfo);

			if (!DesignMode)
			{
				SetTexts();
				var understaffingRealmEnabled = PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.AbsenceRequests);
				percentTextBoxUnderstaffingFor.Enabled = understaffingRealmEnabled;
				labelFor.Enabled = understaffingRealmEnabled;
				if(percentTextBoxUnderstaffingFor.Enabled)
				{
					percentTextBoxUnderstaffingFor.Setup(cultureInfo);
					percentTextBoxUnderstaffingFor.Minimum = 0;
					percentTextBoxUnderstaffingFor.Maximum = 100;
					percentTextBoxUnderstaffingFor.DefaultValue = 100;
				}
			}
		}

		public void Populate(IAggregateRoot aggregateRoot)
		{
			var skill = aggregateRoot as ISkill;
			if (skill==null) throw new ArgumentNullException("aggregateRoot","The supplied root must be of type: ISkill.");

			percentTextBoxSeriousUnderstaffing.DoubleValue = skill.StaffingThresholds.SeriousUnderstaffing.Value;
			percentTextBoxUnderstaffing.DoubleValue = skill.StaffingThresholds.Understaffing.Value;
			percentTextBoxOverstaffing.DoubleValue = skill.StaffingThresholds.Overstaffing.Value;
			if (percentTextBoxUnderstaffingFor.Enabled)
				percentTextBoxUnderstaffingFor.DoubleValue = skill.StaffingThresholds.UnderstaffingFor.Value;
		}

		public bool Depopulate(IAggregateRoot aggregateRoot)
		{
			var skill = aggregateRoot as ISkill;
			if (skill == null) throw new ArgumentNullException("aggregateRoot", "The supplied root must be of type: ISkill.");

			skill.StaffingThresholds = percentTextBoxUnderstaffingFor.Enabled
										? new StaffingThresholds(
											new Percent(percentTextBoxSeriousUnderstaffing.DoubleValue),
											new Percent(percentTextBoxUnderstaffing.DoubleValue),
											new Percent(percentTextBoxOverstaffing.DoubleValue),
											new Percent(percentTextBoxUnderstaffingFor.DoubleValue))
										: new StaffingThresholds(
											new Percent(percentTextBoxSeriousUnderstaffing.DoubleValue),
											new Percent(percentTextBoxUnderstaffing.DoubleValue),
											new Percent(percentTextBoxOverstaffing.DoubleValue));
			return true;
		}

		public string PageName
		{
			get { return UserTexts.Resources.Thresholds; }
		}

		public void SetEditMode()
		{
		}
	}
}