using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{

	public abstract class VersionedAggregateRootWithBusinessUnitWithoutChangeInfo : Entity,
		IVersioned,
		IAggregateRoot,
		IBelongsToBusinessUnit
	{
		private IBusinessUnit _businessUnit;

		public virtual IBusinessUnit BusinessUnit
		{
			get { return _businessUnit ?? (_businessUnit = CurrentBusinessUnit.Instance.Current()); }
			protected set { _businessUnit = value; }
		}

		private int? _version;

		public virtual int? Version
		{
			get { return _version; }
		}

		public virtual void SetVersion(int version)
		{
			_version = version;
		}
	}

	public abstract class VersionedAggregateRootWithBusinessUnit : VersionedAggregateRoot,
													  IBelongsToBusinessUnit
	{
		private IBusinessUnit _businessUnit;

		public virtual IBusinessUnit BusinessUnit
		{
			get { return _businessUnit ?? (_businessUnit = CurrentBusinessUnit.Instance.Current()); }
			protected set { _businessUnit = value; }
		}
	}

	public abstract class NonversionedAggregateRootWithBusinessUnit : AggregateRoot,
														  IBelongsToBusinessUnit
	{
		private IBusinessUnit _businessUnit;

		public virtual IBusinessUnit BusinessUnit
		{
			get { return _businessUnit ?? (_businessUnit = CurrentBusinessUnit.Instance.Current()); }
			protected set { _businessUnit = value; }
		}
	}

	public abstract class VersionedAggregateRoot : AggregateRoot,
		IVersioned
	{
		private int? _version;

		public virtual int? Version
		{
			get { return _version; }
		}

		public virtual void SetVersion(int version)
		{
			_version = version;
		}
	}

	public abstract class AggregateRoot : Entity,
		IAggregateRoot,
		IChangeInfo
	{
		private IPerson _updatedBy;
		private DateTime? _updatedOn;
		private readonly LocalizedUpdateInfo _localizedUpdateInfo = new LocalizedUpdateInfo();
		private readonly IList<Func<IEvent>> _events = new List<Func<IEvent>>();

		public virtual IEnumerable<IEvent> PopAllEvents()
		{
			var allEvents = _events.Select(e => e.Invoke()).ToArray();
			_events.Clear();
			return allEvents;
		}

		protected void AddEvent(Func<IEvent> @event)
		{
			_events.Add(@event);
		}

		protected void AddEvent(IEvent @event)
		{
			_events.Add(() => @event);
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

		public virtual string UpdatedTimeInUserPerspective
		{
			get
			{
				return _localizedUpdateInfo.UpdatedTimeInUserPerspective(this);
			}
		}

	}
}
