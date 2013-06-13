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

#pragma warning disable 0649
		private IPerson _createdBy;
		private DateTime? _createdOn;
		private int? _version;
		private IPerson _updatedBy;
		private DateTime? _updatedOn;
		private readonly LocalizedUpdateInfo _localizedUpdateInfo = new LocalizedUpdateInfo();
#pragma warning restore 0649


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


		public virtual IPerson CreatedBy
		{
			get { return _createdBy; }
		}

		public virtual DateTime? CreatedOn
		{
			get { return _createdOn; }
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
			_createdBy = null;
			_createdOn = null;
			_updatedBy = null;
			_updatedOn = null;
		}

		public virtual string CreatedTimeInUserPerspective
		{
			get { return _localizedUpdateInfo.CreatedTimeInUserPerspective(this); }
		}
		public virtual string UpdatedTimeInUserPerspective
		{
			get { return _localizedUpdateInfo.UpdatedTimeInUserPerspective(this); }
		}
	}
}
