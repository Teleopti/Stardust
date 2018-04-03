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
		private readonly INow _now;
		private readonly PersonalSkills _personalSkills = new PersonalSkills();

		public OvertimeRequestOpenPeriodValidator(INow now, ISkillTypeRepository skillTypeRepository)
		{
			_now = now;
			_skillTypeRepository = skillTypeRepository;
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

			var mergedOvertimeRequestOpenPeriods = getMergedOvertimeRequestOpenPeriods(overtimeRequestOpenPeriodSkillTypeGroups,
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

		private List<IOvertimeRequestOpenPeriod> getMergedOvertimeRequestOpenPeriods(
			IEnumerable<IGrouping<ISkillType, IOvertimeRequestOpenPeriod>> overtimeRequestOpenPeriodSkillTypeGroups,
			IPermissionInformation permissionInformation, DateOnlyPeriod dateOnlyPeriod)
		{
			var mergedOvertimeRequestOpenPeriods = new List<IOvertimeRequestOpenPeriod>();
			foreach (var overtimeRequestOpenPeriodSkillTypeGroup in overtimeRequestOpenPeriodSkillTypeGroups)
			{
				var overtimePeriodProjection = new OvertimeRequestPeriodProjection(
					overtimeRequestOpenPeriodSkillTypeGroup.OrderBy(o => o.OrderIndex).ToList(),
					permissionInformation.Culture(),
					permissionInformation.UICulture(),
					new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), permissionInformation.DefaultTimeZone())));
				var openPeriods = overtimePeriodProjection.GetProjectedOvertimeRequestsOpenPeriods(dateOnlyPeriod);
				mergedOvertimeRequestOpenPeriods.Add(new OvertimeRequestOpenPeriodMerger().Merge(openPeriods));
			}
			return mergedOvertimeRequestOpenPeriods;
		}

		private IEnumerable<IGrouping<ISkillType, IOvertimeRequestOpenPeriod>> getOvertimeRequestOpenPeriodSkillTypeGroups(
			IEnumerable<IOvertimeRequestOpenPeriod> overtimeRequestOpenPeriods, IPerson person, DateOnlyPeriod dateOnlyPeriod)
		{
			var personPeriod = person.PersonPeriods(dateOnlyPeriod).ToArray();
			var defaultSkillType = getDefaultSkillType();
			var personSkillTypeDescriptions = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p))
				.Select(p => p.Skill.SkillType.Description).ToList();

			var overtimeRequestOpenPeriodSkillTypeGroups =
				overtimeRequestOpenPeriods
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