using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Settings
{
	public class StateGroupActivityAlarmModel : EntityContainer<IStateGroupActivityAlarm>
	{
		private IStateGroupActivityAlarm _containedOriginalEntity;

		public IActivity Activity {
			get { return ContainedEntity.Activity; }
		}
		public IRtaStateGroup StateGroup
		{
			get { return ContainedEntity.StateGroup; }
		}
		public IRtaRule RtaRule
		{
			get { return ContainedEntity.RtaRule; }
			set { ContainedEntity.RtaRule = value; }
		}

		public IStateGroupActivityAlarm ContainedOriginalEntity
		{
			get { return _containedOriginalEntity; }
			set { _containedOriginalEntity = value; }
		}

		public StateGroupActivityAlarmModel(IStateGroupActivityAlarm stateGroupActivityAlarm)
		{
			setContainedEntity(stateGroupActivityAlarm);
		}

		private void setContainedEntity(IStateGroupActivityAlarm stateGroupActivityAlarm)
		{
			ContainedEntity = stateGroupActivityAlarm.EntityClone();
			ContainedOriginalEntity = stateGroupActivityAlarm;
		}

		public void UpdateAfterMerge(IStateGroupActivityAlarm stateGroupActivityAlarm)
		{
			setContainedEntity(stateGroupActivityAlarm);
		}

		public void ResetRtaStateGroupState(IStateGroupActivityAlarm stateGroup)
		{
			setContainedEntity(stateGroup ?? ContainedOriginalEntity);
		}
	}
}