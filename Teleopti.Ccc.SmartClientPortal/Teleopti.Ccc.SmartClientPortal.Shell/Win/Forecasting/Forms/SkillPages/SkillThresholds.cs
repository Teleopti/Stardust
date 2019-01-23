using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.SkillPages
{
	public partial class SkillThresholds : BaseUserControl, IPropertyPage
	{
		public SkillThresholds()
		{
			InitializeComponent();

			var cultureInfo = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
			percentTextBoxOverstaffing.Setup(cultureInfo);
			percentTextBoxSeriousUnderstaffing.Setup(cultureInfo);
			percentTextBoxUnderstaffing.Setup(cultureInfo);

			if (!DesignMode)
			{
				SetTexts();
				var understaffingRealmEnabled = PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.AbsenceRequests);
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