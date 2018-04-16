using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestOpenPeriodValidator : IOvertimeRequestValidator
	{
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly IOvertimeRequestOpenPeriodMerger _overtimeRequestOpenPeriodMerger;
		private readonly PersonalSkills _personalSkills = new PersonalSkills();

		public OvertimeRequestOpenPeriodValidator(ISkillTypeRepository skillTypeRepository, IOvertimeRequestOpenPeriodMerger overtimeRequestOpenPeriodMerger)
		{
			_skillTypeRepository = skillTypeRepository;
			_overtimeRequestOpenPeriodMerger = overtimeRequestOpenPeriodMerger;
		}

		public OvertimeRequestValidationResult Validate(OvertimeRequestValidationContext context)
		{
			var person = context.PersonRequest.Person;
			var permissionInformation = person.PermissionInformation;
			var dateOnlyPeriod = context.PersonRequest.Request.Period.ToDateOnlyPeriod(permissionInformation.DefaultTimeZone());

			var overtimeRequestOpenPeriods = person.WorkflowControlSet.OvertimeRequestOpenPeriods;
			if (!overtimeRequestOpenPeriods.Any())
				return new OvertimeRequestValidationResult
				{
					IsValid = false,
					InvalidReasons = new[] {Resources.OvertimeRequestDenyReasonClosedPeriod}
				};

			var overtimeRequestOpenPeriodSkillTypeGroups =
				getOvertimeRequestOpenPeriodSkillTypeGroups(overtimeRequestOpenPeriods, person, dateOnlyPeriod).ToArray();
			if (!overtimeRequestOpenPeriodSkillTypeGroups.Any())
				return new OvertimeRequestValidationResult
				{
					IsValid = false,
					InvalidReasons = new[] {Resources.ThereIsNoAvailableSkillForOvertime}
				};

			var mergedOvertimeRequestOpenPeriods = _overtimeRequestOpenPeriodMerger.GetMergedOvertimeRequestOpenPeriods(overtimeRequestOpenPeriods,
				permissionInformation, dateOnlyPeriod);
			if (mergedOvertimeRequestOpenPeriods.All(o => o.AutoGrantType == OvertimeRequestAutoGrantType.Deny))
			{
				return new OvertimeRequestValidationResult
				{
					IsValid = false,
					InvalidReasons = new[] {mergedOvertimeRequestOpenPeriods.FirstOrDefault()?.DenyReason}
				};
			}

			return new OvertimeRequestValidationResult
			{
				IsValid = true
			};
		}

		private IEnumerable<IGrouping<ISkillType, OvertimeRequestSkillTypeFlatOpenPeriod>>
			getOvertimeRequestOpenPeriodSkillTypeGroups(
				IEnumerable<IOvertimeRequestOpenPeriod> overtimeRequestOpenPeriods, IPerson person, DateOnlyPeriod dateOnlyPeriod)
		{
			var personPeriod = person.PersonPeriods(dateOnlyPeriod).ToArray();
			var defaultSkillType = getDefaultSkillType();
			var personSkillTypeDescriptions = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p))
				.Select(p => p.Skill.SkillType.Description).ToList();

			var overtimeRequestOpenPeriodSkillTypeGroups =
				new SkillTypeFlatOvertimeOpenPeriodMapper().Map(overtimeRequestOpenPeriods, defaultSkillType)
					.Where(x => personSkillTypeDescriptions.Contains((x.SkillType ?? defaultSkillType).Description))
					.GroupBy(o => o.SkillType ?? defaultSkillType);
			return overtimeRequestOpenPeriodSkillTypeGroups;
		}

		private ISkillType getDefaultSkillType()
		{
			var phoneSkillType = _skillTypeRepository.LoadAll()
				.FirstOrDefault(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));
			return phoneSkillType;
		}
	}
}