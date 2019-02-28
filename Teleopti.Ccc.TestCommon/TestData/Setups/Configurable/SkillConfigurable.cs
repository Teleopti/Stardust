using System;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class SkillConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string Activity { get; set; }
		public string TimeZone { get; set; }
		public ISkill Skill { get; set; }
		public int Resolution { get; set; }
		public int? CascadingIndex { get; set; }
		public double? SeriousUnderstaffingThreshold { get; set; }
		public string SkillType { get; set; } = "SkillTypeInboundTelephony";
		public bool ShowAbandonRate { get; set; } = true;
		public bool ShowReforecastedAgents { get; set; } = true;

		public SkillConfigurable()
		{
			Resolution = 15;
			TimeZone = TimeZoneInfo.Utc.Id;
			CascadingIndex = null;
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var skillTypeRepository = SkillTypeRepository.DONT_USE_CTOR(currentUnitOfWork);
			var skillType = skillTypeRepository.LoadAll().FirstOrDefault(x => x.Description.Name == SkillType);
			if (skillType == null)
			{
				skillType = new SkillTypePhone(new Description(SkillType), ForecastSource.InboundTelephony);
				skillTypeRepository.Add(skillType);
			}
			
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
			Skill = SkillFactory.CreateSkill(Name, skillType, Resolution, timeZone, TimeSpan.Zero);
			if(CascadingIndex.HasValue)
				Skill.SetCascadingIndex(CascadingIndex.Value);
			if (SeriousUnderstaffingThreshold.HasValue)
				Skill.StaffingThresholds = new StaffingThresholds(new Percent(SeriousUnderstaffingThreshold.Value), Skill.StaffingThresholds.Understaffing, Skill.StaffingThresholds.Overstaffing);
			var activityRepository = ActivityRepository.DONT_USE_CTOR(currentUnitOfWork, null, null);
			Skill.Activity = activityRepository.LoadAll().Single(b => b.Description.Name == Activity);

			var skillRepository = SkillRepository.DONT_USE_CTOR(currentUnitOfWork);
			skillRepository.Add(Skill);
		}
	}
}