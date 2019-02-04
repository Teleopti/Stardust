using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using System.Drawing;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling
{
	[Serializable]
	public class Activity : Payload, IActivity, IAggregateRoot_Events
	{
		private Description _description;
		private Color _displayColor;
		private bool _requiresSkill;
		private bool _inReadyTime;
		private string _payrollCode;
		
		private ReportLevelDetail _reportLevelDetail;
		private bool _requiresSeat;

		[NonSerialized]
		private readonly DeletedDescription _deletedDescription = new DeletedDescription();

		private bool _allowOverwrite;
		private bool _isOutboundActivity;

		public Activity(string name) : base(true)
		{
			_description = new Description(name);
		}

		public Activity() : base(true)
		{
			_description = new Description();
		}

		public virtual IList<IActivity> ActivityCollection => new[]{this};

		public override void NotifyTransactionComplete(DomainUpdateType operation)
		{
		    base.NotifyTransactionComplete(operation);
		    AddEvent(new ActivityChangedEvent
		    {
			    ActivityId = Id.GetValueOrDefault()
		    });
	    }

	    public virtual Description Description
		{
			get
			{
				if (IsDeleted)
				{
					return _deletedDescription.AppendDeleted(_description);
				}
				return _description;
			}
			set { _description = value; }
		}

		public virtual Color DisplayColor
		{
			get { return _displayColor; }
			set { _displayColor = value; }
		}

		public virtual bool InReadyTime
		{
			get { return _inReadyTime; }
			set { _inReadyTime = value; }
		}

		public virtual bool RequiresSkill
		{
			get { return _requiresSkill; }
			set { _requiresSkill = value; }
		}
		public virtual string Name
		{
			get { return Description.Name; }
		}

		public virtual bool AllowOverwrite
		{
			get { return _allowOverwrite; }
			set { _allowOverwrite = value; }
		}

		public virtual ReportLevelDetail ReportLevelDetail
		{
			get { return _reportLevelDetail; }
			set { _reportLevelDetail = value; }
		}

		public virtual string PayrollCode
		{
			get{ return _payrollCode; }
			set{ _payrollCode = value; }
		}

		public virtual bool RequiresSeat { get { return _requiresSeat; } set { _requiresSeat = value; } }

		public override string ToString()
		{
			return String.Concat(Description.Name, ", ", base.ToString());
		}

		public virtual Description ConfidentialDescription(IPerson assignedPerson, ICurrentAuthorization authorization, ILoggedOnUserIsPerson loggedOnUserIsPerson)
		{
			return Description;
		}

		public virtual Color ConfidentialDisplayColor(IPerson assignedPerson, ICurrentAuthorization authorization, ILoggedOnUserIsPerson loggedOnUserIsPerson)
		{
			return DisplayColor;
		}

		public override IPayload UnderlyingPayload
		{
			get { return this; }
		}

		public virtual bool IsOutboundActivity
		{
			get { return _isOutboundActivity; }
			set { _isOutboundActivity = value; }
		}
	}
}
