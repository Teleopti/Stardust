using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

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

        #region Properties

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

        public virtual string Name
        {
            get { return Description.Name; }
        }

   
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

        #endregion

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
            get { return _description; }
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

        public virtual Description ConfidentialDescription(IPerson assignedPerson)
        {
            return isPermittedToSeePayloadInfo(assignedPerson) 
                    ? Description : ConfidentialPayloadValues.Description;
        }

		public virtual Color ConfidentialDisplayColor(IPerson assignedPerson)
        {
            return isPermittedToSeePayloadInfo(assignedPerson)
                       ? DisplayColor : ConfidentialPayloadValues.DisplayColor;
        }

        private bool isPermittedToSeePayloadInfo(IPerson assignedPerson)
        {
            var principal = TeleoptiPrincipal.CurrentPrincipal;
	        var dateOnly = DateOnly.Today;

			if (assignedPerson != null && assignedPerson.IsTerminated() && assignedPerson.PersonPeriodCollection.Count > 0)
				dateOnly = assignedPerson.PersonPeriodCollection.Last().EndDate();

             return !Confidential ||
                   principal.Organisation.IsUser(assignedPerson) ||
                   PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewConfidential, dateOnly, assignedPerson);

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

        public override IPayload UnderlyingPayload
        {
            get { return this; }
        }
    }
}