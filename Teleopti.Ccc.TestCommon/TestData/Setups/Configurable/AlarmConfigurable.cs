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

		public void Apply(IUnitOfWork uow)
		{

			var alarmType = new AlarmType(new Description(Name),
				string.IsNullOrEmpty(AlarmColor) ? Color.Red : Color.FromName(AlarmColor), TimeSpan.Zero, AlarmTypeMode.UserDefined,
				StaffingEffect);

			var activityRepository = new ActivityRepository(uow);
			var activity = activityRepository.LoadAll().First(a => a.Name == Activity);
			var stateGroup = new RtaStateGroup(PhoneState, false, true);
			stateGroup.AddState(PhoneState, PhoneState, Guid.Empty);

			var stateGroupRepository = new RtaStateGroupRepository(uow);
			stateGroupRepository.Add(stateGroup);

			var alarmTypeRepository = new AlarmTypeRepository(uow);
			alarmTypeRepository.Add(alarmType);

			var alarmSituation = new StateGroupActivityAlarm(stateGroup, activity) {AlarmType = alarmType};
			var alarmRepository = new StateGroupActivityAlarmRepository(uow);
			alarmRepository.Add(alarmSituation);
		}
	}
}