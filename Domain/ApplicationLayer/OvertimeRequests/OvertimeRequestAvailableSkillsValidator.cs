using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestAvailableSkillsValidator : IOvertimeRequestValidator
	{
		private readonly IOvertimeRequestUnderStaffingSkillProvider _overtimeRequestUnderStaffingSkillProvider;
		private readonly IOvertimeRequestSkillProvider _overtimeRequestSkillProvider;
		private readonly ISkillOpenHourFilter _skillOpenHourFilter;

		public OvertimeRequestAvailableSkillsValidator(IOvertimeRequestUnderStaffingSkillProvider overtimeRequestUnderStaffingSkillProvider, IOvertimeRequestSkillProvider overtimeRequestSkillProvider, ISkillOpenHourFilter skillOpenHourFilter)
		{
			_overtimeRequestUnderStaffingSkillProvider = overtimeRequestUnderStaffingSkillProvider;
			_overtimeRequestSkillProvider = overtimeRequestSkillProvider;
			_skillOpenHourFilter = skillOpenHourFilter;
		}

		public OvertimeRequestValidationResult Validate(IPersonRequest personRequest)
		{
			var period = personRequest.Request.Period;
			var skills = _overtimeRequestSkillProvider.GetAvailableSkills(period).ToList();
			if (!skills.Any())
			{
				return new OvertimeRequestValidationResult
				{
					IsValid = false,
					InvalidReason =Resources.NoAvailableSkillForOvertime
				};
			}

			skills = _skillOpenHourFilter.Filter(period, skills).ToList();
			if (!skills.Any())
			{
				return new OvertimeRequestValidationResult
				{
					IsValid = false,
					InvalidReason = Resources.PeriodIsOutOfSkillOpenHours
				};
			}

			var seriousUnderstaffingSkills = _overtimeRequestUnderStaffingSkillProvider.GetSeriousUnderstaffingSkills(period, skills);
			if (seriousUnderstaffingSkills.Count == 0)
			{
				return new OvertimeRequestValidationResult
				{
					IsValid = false,
					InvalidReason = Resources.NoUnderStaffingSkill
				};
			}

			return new OvertimeRequestValidationResult
			{
				IsValid = true
			};
		}
	}
}