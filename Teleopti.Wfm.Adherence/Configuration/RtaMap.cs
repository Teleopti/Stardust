using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Configuration.Events;

namespace Teleopti.Wfm.Adherence.Configuration
{
	public class RtaMap : AggregateRoot_Events_Versioned_BusinessUnitId, IRtaMap
	{
		private Guid? _activity;
		private IRtaStateGroup _stateGroup;
		private IRtaRule _rtaRule;

		public RtaMap()
		{
		}
		
		public override void NotifyTransactionComplete(DomainUpdateType operation)
		{
			base.NotifyTransactionComplete(operation);
			AddEvent(new RtaMapChangedEvent());
		}

		public virtual Guid? Activity
		{
			get { return _activity; }
			set { _activity = value; }
		}

		public virtual IRtaStateGroup StateGroup
		{
			get { return _stateGroup; }
			set { _stateGroup = value; }
		}

		public virtual IRtaRule RtaRule
		{
			get { return _rtaRule; }
			set { _rtaRule = value; }
		}

		public virtual IRtaMap EntityClone()
		{
			return (IRtaMap) MemberwiseClone();
		}
	}
}