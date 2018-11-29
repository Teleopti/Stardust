using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public abstract class AbsenceRequestOpenPeriod : AggregateEntity, IAbsenceRequestOpenPeriod
	{
		private IAbsence _absence;
		private DateOnlyPeriod _openForRequestsPeriod;

		private IAbsenceRequestValidator _currentPersonAccountValidator;
		private IAbsenceRequestValidator _currentStaffingThresholdValidator;
		private IProcessAbsenceRequest _currentAbsenceRequestProcess;

		private int _personAccountValidator;
		private int _staffingThresholdValidator;
		private int _absenceRequestProcess;

		private static readonly IList<IAbsenceRequestValidator> _personAccountValidatorList =
			new List<IAbsenceRequestValidator>
			{
				new AbsenceRequestNoneValidator(),
				new PersonAccountBalanceValidator()
			};

		private static readonly IList<IAbsenceRequestValidator> _staffingThresholdValidatorList =
			new List<IAbsenceRequestValidator>
			{
				 new AbsenceRequestNoneValidator(),
				 new StaffingThresholdValidator(),
				 new BudgetGroupAllowanceValidator(),
				 new BudgetGroupHeadCountValidator(),
				 new StaffingThresholdWithShrinkageValidator()
			};

		private static readonly IList<IProcessAbsenceRequest> _absenceRequestProcessList =
			new List<IProcessAbsenceRequest>
			{
				new PendingAbsenceRequest(),
				new GrantAbsenceRequest(),
				new DenyAbsenceRequest()
			};

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

		public virtual IList<IAbsenceRequestValidator> PersonAccountValidatorList => _personAccountValidatorList;

		public virtual IList<IAbsenceRequestValidator> StaffingThresholdValidatorList => _staffingThresholdValidatorList;

		public virtual IAbsenceRequestValidator StaffingThresholdValidator
		{
			get {
				return _currentStaffingThresholdValidator ??
					   (_currentStaffingThresholdValidator =
						   _staffingThresholdValidatorList[_staffingThresholdValidator].CreateInstance());
			}
			set
			{
				InParameter.NotNull(nameof(StaffingThresholdValidator), value);
				_staffingThresholdValidator = _staffingThresholdValidatorList.IndexOf(value);
				//otherwise it will find no 1, because it inherits from that
				if (value is StaffingThresholdWithShrinkageValidator)
					_staffingThresholdValidator = 4;
				_currentStaffingThresholdValidator = value.CreateInstance();
			}
		}

		public virtual int OrderIndex
		{
			get
			{
				var owner = Parent as IWorkflowControlSet;
				_orderIndex = owner?.AbsenceRequestOpenPeriods.IndexOf(this) ?? -1;
				return _orderIndex;
			}
			set
			{
				_orderIndex = value;
			}
		}

		public virtual IList<IProcessAbsenceRequest> AbsenceRequestProcessList => _absenceRequestProcessList;

		public virtual IProcessAbsenceRequest AbsenceRequestProcess
		{
			get
			{
				if (_currentAbsenceRequestProcess == null)
				{
					_currentAbsenceRequestProcess = _absenceRequestProcessList[_absenceRequestProcess].CreateInstance();
				}
				return _currentAbsenceRequestProcess;
			}
			set
			{
				InParameter.NotNull(nameof(AbsenceRequestProcess), value);
				_absenceRequestProcess = _absenceRequestProcessList.IndexOf(value);
				_currentAbsenceRequestProcess = value.CreateInstance();
			}
		}

		public virtual IEnumerable<IAbsenceRequestValidator> GetSelectedValidatorList()
		{
			return new List<IAbsenceRequestValidator> { PersonAccountValidator, StaffingThresholdValidator };
		}

		public virtual IAbsenceRequestValidator PersonAccountValidator
		{
			get
			{
				if (_currentPersonAccountValidator == null)
				{
					_currentPersonAccountValidator = _personAccountValidatorList[_personAccountValidator].CreateInstance();
				}
				return _currentPersonAccountValidator;
			}
			set
			{
				InParameter.NotNull(nameof(PersonAccountValidator), value);
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
			var clone = (AbsenceRequestOpenPeriod)MemberwiseClone();
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