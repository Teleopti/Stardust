using System;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class AlarmConfigurable : IDataSetup
	{
		public string Activity { get; set; }
		public string PhoneState { get; set; }
		public string Name { get; set; }
		public int StaffingEffect { get; set; }
		public string AlarmColor { get; set; }
		public string BusinessUnit { get; set; }
		public string Adherence { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var color = string.IsNullOrEmpty(AlarmColor) ? Color.Red : Color.FromName(AlarmColor);
			var alarmType = new AlarmType(new Description(Name), color, TimeSpan.Zero, StaffingEffect);

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
					stateGroup.AddState(PhoneState, PhoneState, Guid.Empty);
					stateGroupRepository.Add(stateGroup);
				}
			}

			IActivity activity = null;
			if (Activity != null)
				activity = activityRepository.LoadAll().First(a => a.Name == Activity);

			var stateGroupActivityAlarm = new StateGroupActivityAlarm(stateGroup, activity) { AlarmType = alarmType };

			if (!string.IsNullOrEmpty(BusinessUnit))
			{
				var businessUnit = new BusinessUnitRepository(currentUnitOfWork).LoadAll().Single(b => b.Name == BusinessUnit);
				if (stateGroup != null)
					stateGroup.SetBusinessUnit(businessUnit);
				stateGroupActivityAlarm.SetBusinessUnit(businessUnit);
			}

			if (!string.IsNullOrWhiteSpace(Adherence))
			{
				Adherence adherence;
				if(Enum.TryParse(Adherence, true, out adherence))
				alarmType.Adherence = adherence;
			}
			
			var alarmTypeRepository = new AlarmTypeRepository(currentUnitOfWork);
			alarmTypeRepository.Add(alarmType);

			var alarmRepository = new StateGroupActivityAlarmRepository(currentUnitOfWork);
			alarmRepository.Add(stateGroupActivityAlarm);
		}
	}
}