using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestAvailableSkillsValidator : IOvertimeRequestAvailableSkillsValidator
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

		public OvertimeRequestAvailableSkillsValidationResult Validate(IPersonRequest personRequest)
		{
			var period = personRequest.Request.Period;
			var skills = _overtimeRequestSkillProvider.GetAvailableSkills(personRequest.Person, period).ToList();
			if (!skills.Any())
			{
				return new OvertimeRequestAvailableSkillsValidationResult
				{
					IsValid = false,
					InvalidReason =Resources.NoAvailableSkillForOvertime
				};
			}

			skills = _skillOpenHourFilter.Filter(period, skills).ToList();
			if (!skills.Any())
			{
				return new OvertimeRequestAvailableSkillsValidationResult
				{
					IsValid = false,
					InvalidReason = Resources.PeriodIsOutOfSkillOpenHours
				};
			}

			var seriousUnderstaffingSkills = _overtimeRequestUnderStaffingSkillProvider.GetSeriousUnderstaffingSkills(period, skills);
			if (seriousUnderstaffingSkills.Count == 0)
			{
				return new OvertimeRequestAvailableSkillsValidationResult
				{
					IsValid = false,
					InvalidReason = Resources.NoUnderStaffingSkill
				};
			}

			return new OvertimeRequestAvailableSkillsValidationResult
			{
				IsValid = true,
				Skills = seriousUnderstaffingSkills.ToArray()
			};
		}
	}
}