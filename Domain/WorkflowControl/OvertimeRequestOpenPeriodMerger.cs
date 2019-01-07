using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class OvertimeRequestOpenPeriodMerger : IOvertimeRequestOpenPeriodMerger
	{
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly INow _now;

		public OvertimeRequestOpenPeriodMerger(ISkillTypeRepository skillTypeRepository, INow now)
		{
			_skillTypeRepository = skillTypeRepository;
			_now = now;
		}

		public List<OvertimeRequestSkillTypeFlatOpenPeriod> GetMergedOvertimeRequestOpenPeriods(IEnumerable<IOvertimeRequestOpenPeriod> overtimeRequestOpenPeriods,
			IPermissionInformation permissionInformation, DateOnlyPeriod period)
		{
			var phoneSkillType = _skillTypeRepository.LoadAll()
				.FirstOrDefault(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));

			var overtimeRequestOpenPeriodGroups =
				new SkillTypeFlatOvertimeOpenPeriodMapper().Map(overtimeRequestOpenPeriods, phoneSkillType)
					.GroupBy(o => o.SkillType ?? phoneSkillType);

			var viewpointDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), permissionInformation.DefaultTimeZone()));
			var mergedOvertimeRequestOpenPeriods = new List<OvertimeRequestSkillTypeFlatOpenPeriod>();
			foreach (var overtimeRequestOpenPeriodSkillTypeGroup in overtimeRequestOpenPeriodGroups)
			{
				var overtimePeriodProjection = new OvertimeRequestPeriodProjection(
					overtimeRequestOpenPeriodSkillTypeGroup.OrderBy(o => o.OrderIndex).ToList(),
					permissionInformation.Culture(),
					permissionInformation.UICulture(),
					viewpointDate);
				var openPeriods = overtimePeriodProjection.GetProjectedOvertimeRequestsOpenPeriods(period);

				mergedOvertimeRequestOpenPeriods.Add(openPeriods.OrderBy(o => o.OrderIndex).Last());
			}
			return mergedOvertimeRequestOpenPeriods;
		}
	}
}