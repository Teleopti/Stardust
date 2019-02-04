using System;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling
{
    /// <summary>
    /// Represents when an agent is not present due to illnes or vacation e.g
    /// </summary>
    public class Absence : Payload, IAbsence
	{
        private byte _priority = 100;
        private bool _requestable;
        private string _payrollCode;
        private Description _description;
        private Color _displayColor;
        private bool _confidential;

		[NonSerialized]
		private readonly DeletedDescription _deletedDescription = new DeletedDescription();

		/// <summary>
		/// Gets or sets the priority.
		/// Used if a person has multiple PersonAbsences
		/// </summary>
		/// <value>The priority.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-02-05
		/// </remarks>
		public virtual byte Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        public virtual string Name => Description.Name;
		
		public virtual bool Requestable
        {
            get { return _requestable; }
            set { _requestable = value; }
        }

        public virtual string PayrollCode
        {
            get { return _payrollCode; }
            set { _payrollCode = value; }
        }

		public override void NotifyTransactionComplete(DomainUpdateType operation)
	    {
			base.NotifyTransactionComplete(operation);
			switch (operation)
			{
				case DomainUpdateType.Insert:
				case DomainUpdateType.Update:
					AddEvent(new AbsenceChangedEvent
					{
						AbsenceId = Id.GetValueOrDefault()
					});
					break;
				case DomainUpdateType.Delete:
					AddEvent(new AbsenceDeletedEvent
					{
						AbsenceId = Id.GetValueOrDefault()
					});
					break;
			}
		}
		
		public Absence()
            : base(false)
        {
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        public virtual Description Description
        {
			get { return IsDeleted ? _deletedDescription.AppendDeleted(_description) : _description; }
			set { _description = value; }
        }

        /// <summary>
        /// Gets the display color of the Payload.
        /// </summary>
        /// <value>The color of the display.</value>
        public virtual Color DisplayColor
        {
            get { return _displayColor; }
            set { _displayColor = value; }
        }

        public virtual bool Confidential
        {
            get 
            {
                return _confidential;
            }
            set 
            {
                _confidential = value;
            }
        }

        public virtual Description ConfidentialDescription(IPerson assignedPerson, ICurrentAuthorization authorization, ILoggedOnUserIsPerson loggedOnUserIsPerson)
        {
            return isPermittedToSeePayloadInfo(assignedPerson, authorization, loggedOnUserIsPerson) 
                    ? Description : ConfidentialPayloadValues.Description;
        }

		public virtual Color ConfidentialDisplayColor(IPerson assignedPerson, ICurrentAuthorization authorization, ILoggedOnUserIsPerson loggedOnUserIsPerson)
        {
            return isPermittedToSeePayloadInfo(assignedPerson, authorization, loggedOnUserIsPerson)
                       ? DisplayColor : ConfidentialPayloadValues.DisplayColor;
        }

		private bool isPermittedToSeePayloadInfo(IPerson assignedPerson, ICurrentAuthorization authorization, ILoggedOnUserIsPerson loggedOnUserIsPerson)
		{
			if (!Confidential)
				return true;
			if (loggedOnUserIsPerson.IsPerson(assignedPerson))
				return true;

			var dateOnly = DateOnly.Today;
			if (assignedPerson != null && assignedPerson.IsTerminated() && assignedPerson.PersonPeriodCollection.Count > 0)
				dateOnly = assignedPerson.PersonPeriodCollection.Last().EndDate();
			return authorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewConfidential, dateOnly, assignedPerson);
		}

		public virtual object Clone()
        {
            return NoneEntityClone();
        }

        public virtual IAbsence NoneEntityClone()
        {
            IAbsence clone = (IAbsence) MemberwiseClone();
            clone.SetId(null);
            return clone;
        }

        public virtual IAbsence EntityClone()
        {
            return (IAbsence) MemberwiseClone();
        }

        public override IPayload UnderlyingPayload => this;
	}
}