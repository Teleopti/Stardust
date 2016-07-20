using System;
using System.Collections.Generic;
using System.Xml;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using System.Drawing;
using System.Linq;

namespace Teleopti.Ccc.Domain.Scheduling
{
    [Serializable]
    public class Activity : Payload, IActivity, IAggregateRootWithEvents
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

	    /// <summary>
        /// Initializes a new instance of the <see cref="Activity"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Activity(string name)
            : base(true)
        {
            _description = new Description(name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Activity"/> class for NHibernate.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        protected Activity()
        {
        }

		public override IEnumerable<IEvent> PopAllEvents(INow now, DomainUpdateType? operation = null)
		{
			var events = base.PopAllEvents(now, operation).ToList();
			if (!operation.HasValue) return events;
			switch (operation)
			{
				case DomainUpdateType.Insert:
				case DomainUpdateType.Update:
				case DomainUpdateType.Delete:
					events.Add(new ActivityChangedEvent
					{
						ActivityId = Id.GetValueOrDefault()
					});
					break;
			}
			return events;
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

    	public virtual bool RequiresSeat
    	{
    		get {
    			return _requiresSeat;
    		}
    		set {
    			if (_requiresSeat != value)
    			{
    				var valueBefore = _requiresSeat;
    				_requiresSeat = value;
    				var valueAfter = _requiresSeat;
    				AddEvent(new ActivityChangedEvent
    					{
    						ActivityId = Id.GetValueOrDefault(),
    						OldValue = XmlConvert.ToString(valueBefore),
    						NewValue = XmlConvert.ToString(valueAfter),
    						Property = "RequiresSeat"
    					});
    			}
    		}
    	}

    	public override string ToString()
        {
            return String.Concat(Description.Name, ", ", base.ToString());
        }

		public virtual Description ConfidentialDescription(IPerson assignedPerson)
		{
			return Description;
		}

		public virtual Color ConfidentialDisplayColor(IPerson assignedPerson)
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
