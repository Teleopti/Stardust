using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
	public class Scenario : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IScenario, IDeleteTag, IAggregateRoot_Events
	{
		private Description _description;
		private bool _defaultScenario;
		private bool _enableReporting;
		private bool _isDeleted;
		private bool _restricted;

		public override void NotifyTransactionComplete(DomainUpdateType operation)
		{
			base.NotifyTransactionComplete(operation);

			switch (operation)
			{
				case DomainUpdateType.Insert:
				case DomainUpdateType.Update:
					AddEvent(new ScenarioChangeEvent
					{
						ScenarioId = Id.GetValueOrDefault()
					});
					break;
				case DomainUpdateType.Delete:
					AddEvent(new ScenarioDeleteEvent
					{
						ScenarioId = Id.GetValueOrDefault()
					});
					break;

			}
		}

		public Scenario(string name)
		{
			_description = new Description(name);
		}

		public Scenario() : this("_")
		{
		}

		public virtual Description Description => _description;

		public virtual void ChangeName(string name)
		{
			_description = new Description(name);
		}

		public virtual bool DefaultScenario
		{
			get { return _defaultScenario; }
			//TODO: only one scenario may be default, create special scenariocollection for this
			set { _defaultScenario = value; }
		}

		public virtual bool IsDeleted => _isDeleted;

		public virtual bool EnableReporting
		{
			get { return _enableReporting; }
			set { _enableReporting = value; }
		}

		public virtual int CompareTo(IScenario other)
		{
			return String.Compare(Description.Name, other.Description.Name, StringComparison.CurrentCulture);
		}

		public override string ToString()
		{
			return String.Concat(base.ToString(), " ", Description.ToString());
		}

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}

		public virtual bool Restricted
		{
			get { return _restricted; }
			set { _restricted = value; }
		}
	}
}
