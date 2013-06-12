using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public abstract class AbsenceRequestOpenPeriod : AggregateEntity, IAbsenceRequestOpenPeriod
    {
        private IAbsence _absence;
        private DateOnlyPeriod _openForRequestsPeriod;

        private static readonly IList<IAbsenceRequestValidator> _personAccountValidatorList =
            new List<IAbsenceRequestValidator> {new AbsenceRequestNoneValidator(), new PersonAccountBalanceValidator()};

        private IAbsenceRequestValidator _currentPersonAccountValidator;
        private IAbsenceRequestValidator _currentStaffingThresholdValidator;
        private IProcessAbsenceRequest _currentAbsenceRequestProcess;

        private int _personAccountValidator;
        private int _staffingThresholdValidator;
        private int _absenceRequestProcess;

        private static readonly IList<IAbsenceRequestValidator> _staffingThresholdValidatorList =
            new List<IAbsenceRequestValidator> { new AbsenceRequestNoneValidator(), new StaffingThresholdValidator(), new BudgetGroupAllowanceValidator(), new BudgetGroupHeadCountValidator()};

        private static readonly IList<IProcessAbsenceRequest> _absenceRequestProcessList =
            new List<IProcessAbsenceRequest> {new PendingAbsenceRequest(), new GrantAbsenceRequest(), new DenyAbsenceRequest()};

        private int _orderIndex;
        
        public virtual IAbsence Absence
        {
            get { return _absence; }
            set { _absence = value; }
        }

        public abstract DateOnlyPeriod GetPeriod(DateOnly viewpointDateOnly);

        public virtual DateOnlyPeriod OpenForRequestsPeriod
        {
            get { return _openForRequestsPeriod; }
            set { _openForRequestsPeriod = value; }
        }

        public virtual IList<IAbsenceRequestValidator> PersonAccountValidatorList
        {
            get { return _personAccountValidatorList; }
        }

        public virtual IList<IAbsenceRequestValidator> StaffingThresholdValidatorList
        {
            get { return _staffingThresholdValidatorList; }
        }

        public virtual IAbsenceRequestValidator StaffingThresholdValidator
        {
            get
            {
                if (_currentStaffingThresholdValidator == null)
                {
                    _currentStaffingThresholdValidator = _staffingThresholdValidatorList[_staffingThresholdValidator].CreateInstance();
                }
                return _currentStaffingThresholdValidator;
            }
            set
            {
                InParameter.NotNull("value", value);
                _staffingThresholdValidator = _staffingThresholdValidatorList.IndexOf(value);
                _currentStaffingThresholdValidator = value.CreateInstance();
            }
        }

        public virtual int OrderIndex
        {
            get
            {
                IWorkflowControlSet owner = Parent as IWorkflowControlSet;
                _orderIndex = owner==null ? -1 : owner.AbsenceRequestOpenPeriods.IndexOf(this);
                return _orderIndex;
            }
            set
            {
                _orderIndex = value;
            }
        }

        public virtual IList<IProcessAbsenceRequest> AbsenceRequestProcessList
        {
            get { return _absenceRequestProcessList; }
        }

        public virtual IProcessAbsenceRequest AbsenceRequestProcess
        {
            get
            {
                if (_currentAbsenceRequestProcess==null)
                {
                    _currentAbsenceRequestProcess = _absenceRequestProcessList[_absenceRequestProcess].CreateInstance();
                }
                return _currentAbsenceRequestProcess;
            }
            set
            {
                InParameter.NotNull("value", value);
                _absenceRequestProcess = _absenceRequestProcessList.IndexOf(value);
                _currentAbsenceRequestProcess = value.CreateInstance();
            }
        }

        public virtual IEnumerable<IAbsenceRequestValidator> GetSelectedValidatorList()
        {
            return new List<IAbsenceRequestValidator>{ PersonAccountValidator, StaffingThresholdValidator};
        }

        public virtual IAbsenceRequestValidator PersonAccountValidator
        {
            get
            {
                if (_currentPersonAccountValidator==null)
                {
                    _currentPersonAccountValidator = _personAccountValidatorList[_personAccountValidator].CreateInstance();
                }
                return _currentPersonAccountValidator;
            }
            set
            {
                InParameter.NotNull("value", value);
                _personAccountValidator = _personAccountValidatorList.IndexOf(value);
                _currentPersonAccountValidator = value.CreateInstance();
            }
        }

        public virtual object Clone()
        {
            return NoneEntityClone();
        }

        public virtual IAbsenceRequestOpenPeriod NoneEntityClone()
        {
            var clone = (AbsenceRequestOpenPeriod) MemberwiseClone();
            clone.SetId(null);
            clone._currentAbsenceRequestProcess = null;
            clone._currentPersonAccountValidator = null;
            clone._currentStaffingThresholdValidator = null;
            return clone;
        }

        public virtual IAbsenceRequestOpenPeriod EntityClone()
        {
            var clone = (AbsenceRequestOpenPeriod)MemberwiseClone();
            clone._currentAbsenceRequestProcess = null;
            clone._currentPersonAccountValidator = null;
            clone._currentStaffingThresholdValidator = null;
            return clone;
        }
    }
}