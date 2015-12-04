﻿using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Settings
{
	public class StateGroupActivityAlarmModel : EntityContainer<IRtaMap>
	{
		private IRtaMap _containedOriginalEntity;

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

		public IRtaMap ContainedOriginalEntity
		{
			get { return _containedOriginalEntity; }
			set { _containedOriginalEntity = value; }
		}

		public StateGroupActivityAlarmModel(IRtaMap rtaMap)
		{
			setContainedEntity(rtaMap);
		}

		private void setContainedEntity(IRtaMap rtaMap)
		{
			ContainedEntity = rtaMap.EntityClone();
			ContainedOriginalEntity = rtaMap;
		}

		public void UpdateAfterMerge(IRtaMap rtaMap)
		{
			setContainedEntity(rtaMap);
		}

		public void ResetRtaStateGroupState(IRtaMap stateGroup)
		{
			setContainedEntity(stateGroup ?? ContainedOriginalEntity);
		}
	}
}