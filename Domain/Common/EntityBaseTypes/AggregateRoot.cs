using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public abstract class AggregateRoot : Entity,
		IAggregateRoot,
		IChangeInfo,
		IVersioned
	{
		private int? _version;
		private IPerson _updatedBy;
		private DateTime? _updatedOn;

		private readonly IList<IEvent> _events = new List<IEvent>();

		public virtual IEnumerable<IEvent> PopAllEvents()
		{
			var allEvents = _events.ToArray();
			_events.Clear();
			return allEvents;
		}

		public virtual IEnumerable<IEvent> AllEvents() { return _events; }

		protected void AddEvent(IEvent @event)
		{
			_events.Add(@event);
		}



		public virtual int? Version
		{
			get { return _version; }
		}

		public virtual void SetVersion(int version)
		{
			_version = version;
		}


		public virtual IPerson UpdatedBy
		{
			get { return _updatedBy; }
		}

		public virtual DateTime? UpdatedOn
		{
			get { return _updatedOn; }
			set { _updatedOn = value; }
		}

		public override void ClearId()
		{
			base.ClearId();
			_updatedBy = null;
			_updatedOn = null;
		}
	}
}
