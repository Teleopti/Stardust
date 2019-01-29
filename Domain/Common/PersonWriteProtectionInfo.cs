using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Common
{
    public class PersonWriteProtectionInfo : SimpleAggregateRoot, IPersonWriteProtectionInfo, IChangeInfo
    {
#pragma warning disable 0649
        private IPerson _updatedBy;
        private DateTime? _updatedOn;
        private IPerson _createdBy;
        private DateTime? _createdOn;
#pragma warning restore 0649       
        private IPerson _belongsTo;
        private DateOnly? _personWriteProtectedDate;

        public PersonWriteProtectionInfo(IPerson person)
        {
            _belongsTo = person;
        }

        protected PersonWriteProtectionInfo(){}

        public virtual IPerson BelongsTo
        {
            get { return _belongsTo; }
            set { _belongsTo = value; }
        }

        public virtual IPerson UpdatedBy
        {
            get { return _updatedBy; }
        }

        public virtual DateTime? UpdatedOn
        {
            get { return _updatedOn; }
        }

        public virtual DateOnly? PersonWriteProtectedDate
        {
            get { return _personWriteProtectedDate; }
            set
            {
				if (!PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.SetWriteProtection))
                    throw new PermissionException("You don't have permission to set write protection");
                _personWriteProtectedDate = value;
               }
        }

        public virtual DateOnly? WriteProtectedUntil()
        {
            DateOnly protectionTeam;
            int notWriteProtectedDays = 10000;

            IWorkflowControlSet controlSet = _belongsTo.WorkflowControlSet;
            if (controlSet != null)
            {
                if (controlSet.WriteProtection.HasValue)
                    notWriteProtectedDays = controlSet.WriteProtection.Value;
                protectionTeam = DateOnly.Today.AddDays(-notWriteProtectedDays);
            }
            else
            {
                protectionTeam = DateOnly.Today.AddDays(-notWriteProtectedDays);
            }
            if (!_personWriteProtectedDate.HasValue)
                return protectionTeam;

            var date = protectionTeam;
            if (date < _personWriteProtectedDate.Value)
            {
                date = _personWriteProtectedDate.Value;
            }
            return date;
        }

        public virtual bool IsWriteProtected(DateOnly dateOnly)
        {
            DateOnly? protectedUntil = WriteProtectedUntil();
            if(protectedUntil.HasValue)
            {
                return !(dateOnly > protectedUntil.Value);
            }
            return false;
        }

        public virtual IPerson CreatedBy
        {
            get { return _createdBy; }
        }

        public virtual DateTime? CreatedOn
        {
            get { return _createdOn; }
        }

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ 543;
		}
    }
}
