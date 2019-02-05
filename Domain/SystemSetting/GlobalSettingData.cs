using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Domain.SystemSetting
{
	public class GlobalSettingData :	SettingData,
										IFilterOnBusinessUnit,
										IChangeInfo,
										IVersioned,
										IPublishEvents
	{
#pragma warning disable 0649
		private IPerson _createdBy;
		private DateTime? _createdOn;
		private int? _version;
		private IPerson _updatedBy;
		private DateTime? _updatedOn;
#pragma warning restore 0649
		private IBusinessUnit _businessUnit;
		private readonly Events _events = new Events();

		protected GlobalSettingData() { }

		public GlobalSettingData(string name) : base(name) { }

		public virtual IBusinessUnit BusinessUnit
		{
			get { return _businessUnit ?? (_businessUnit = ServiceLocator_DONTUSE.CurrentBusinessUnit.Current()); }
			protected set { _businessUnit = value; }
		}

		public virtual IPerson CreatedBy => _createdBy;

		public virtual DateTime? CreatedOn => _createdOn;

		public virtual IPerson UpdatedBy => _updatedBy;

		public virtual DateTime? UpdatedOn => _updatedOn;

		public virtual int? Version => _version;

		public virtual void SetVersion(int version)
		{
			_version = version;
		}

		public virtual void NotifyCommandId(Guid commandId)
		{
			_events.NotifyCommandId(commandId);
		}

		public virtual void NotifyTransactionComplete(DomainUpdateType operation)
		{
			if (Key == CommonNameDescriptionSetting.Key)
			{
				_events.AddEvent(new CommonNameDescriptionChangedEvent());
			}
		}

		public virtual void NotifyDelete()
		{
		}

		public virtual IEnumerable<IEvent> PopAllEvents(IPopEventsContext context)
		{
			return _events.PopAllEvents(context);
		}

		public virtual bool HasEvents()
		{
			return _events.HasEvents();
		}
	}
}
