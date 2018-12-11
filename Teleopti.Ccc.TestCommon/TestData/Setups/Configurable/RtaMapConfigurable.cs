using System;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.Configuration.Repositories;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class RtaMapConfigurable : IDataSetup
	{
		public string Activity { get; set; }
		public string PhoneState { get; set; }
		public string Name { get; set; }
		public int StaffingEffect { get; set; }
		public string AlarmColor { get; set; }
		public string DisplayColor { get; set; }
		public string BusinessUnit { get; set; }
		public string Adherence { get; set; }
		public string IsAlarm { get; set; }
		public string AlarmThreshold { get; set; }
		public bool IsLogOutState { get; set; }

		public IRtaStateGroup RtaStateGroup { get; private set; }


		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var alarmColor = string.IsNullOrEmpty(AlarmColor) ? Color.Red : Color.FromName(AlarmColor);
			var displayColor = string.IsNullOrEmpty(DisplayColor) ? alarmColor : Color.FromName(DisplayColor);

			var ruleRepository = new RtaRuleRepository(currentUnitOfWork);

			IRtaRule rule = null;
			if (Name != null)
				rule = ruleRepository.LoadAll().FirstOrDefault(x => x.Description.Name == Name);

			if (rule == null)
			{
				rule = new RtaRule(new Description(Name ?? RandomName.Make()), displayColor, 0, StaffingEffect)
				{
					AlarmColor = alarmColor
				};
				if (!string.IsNullOrWhiteSpace(Adherence))
					rule.Adherence = (Adherence)Enum.Parse(typeof(Adherence), Adherence);
				if (!string.IsNullOrWhiteSpace(IsAlarm))
					rule.IsAlarm = bool.Parse(IsAlarm);
				if (!string.IsNullOrWhiteSpace(AlarmThreshold))
					rule.ThresholdTime = (int) TimeSpan.Parse(AlarmThreshold).TotalSeconds;
				ruleRepository.Add(rule);
			}

			var activityRepository = new ActivityRepository(currentUnitOfWork);

			IRtaStateGroup stateGroup = null;
			if (PhoneState != null)
			{
				var stateGroupRepository = new RtaStateGroupRepository(currentUnitOfWork);
				stateGroup = (from g in stateGroupRepository.LoadAll()
					from s in g.StateCollection
					where s.StateCode == PhoneState
					select g).SingleOrDefault();
				if (stateGroup == null)
				{
					stateGroup = new RtaStateGroup(PhoneState, false, true);
					stateGroup.IsLogOutState = IsLogOutState;
					stateGroup.AddState(PhoneState, PhoneState);
					stateGroupRepository.Add(stateGroup);
				}
				RtaStateGroup = stateGroup;
			}

			IActivity activity = null;
			if (!Activity.IsNullOrEmpty())
				activity = activityRepository.LoadAll().First(a => a.Name == Activity);

			var rtaMap = new RtaMap(stateGroup, activity) { RtaRule = rule };

			if (!string.IsNullOrEmpty(BusinessUnit))
			{
				var businessUnit = new BusinessUnitRepository(currentUnitOfWork).LoadAll().Single(b => b.Name == BusinessUnit);
				stateGroup?.SetBusinessUnit(businessUnit);
				rtaMap.SetBusinessUnit(businessUnit);
			}

			var rtaMapRepository = new RtaMapRepository(currentUnitOfWork);
			rtaMapRepository.Add(rtaMap);
		}
	}
}