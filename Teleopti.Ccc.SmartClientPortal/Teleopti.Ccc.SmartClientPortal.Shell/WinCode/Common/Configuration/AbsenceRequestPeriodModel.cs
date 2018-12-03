using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
    public class AbsenceRequestPeriodModel
    {
        public WorkflowControlSetModel Owner { get; set; }
        private IAbsenceRequestOpenPeriod _absenceRequestOpenPeriod;
        private AbsenceRequestPeriodTypeModel _periodType;

        public AbsenceRequestPeriodModel(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod, WorkflowControlSetModel owner)
        {
            Owner = owner;
            SetDomainEntity(absenceRequestOpenPeriod);
        }

        public IAbsenceRequestOpenPeriod DomainEntity
        {
            get { return _absenceRequestOpenPeriod; }
        }

        public AbsenceRequestPeriodTypeModel PeriodType
        {
            get { return _periodType; }
        }

        public IAbsence Absence
        {
            get { return _absenceRequestOpenPeriod.Absence; }
            set
            {
                _absenceRequestOpenPeriod.Absence = value;
                Owner.IsDirty = true;
            }
        }

        public IAbsenceRequestValidator PersonAccountValidator
        {
            get { return _absenceRequestOpenPeriod.PersonAccountValidator; }
            set
            {
                _absenceRequestOpenPeriod.PersonAccountValidator = value;
                Owner.IsDirty = true;
            }
        }

        public IAbsenceRequestValidator StaffingThresholdValidator
        {
            get { return _absenceRequestOpenPeriod.StaffingThresholdValidator; }
            set
            {
                _absenceRequestOpenPeriod.StaffingThresholdValidator = value;
                Owner.IsDirty = true;
            }
        }

        public IProcessAbsenceRequest AbsenceRequestProcess
        {
            get { return _absenceRequestOpenPeriod.AbsenceRequestProcess; }
            set
            {
                _absenceRequestOpenPeriod.AbsenceRequestProcess = value;
                Owner.IsDirty = true;
            }
        }

        public TypedBindingCollection<IAbsenceRequestValidator> StaffingThresholdValidatorList
        {
            get
            {
                var bindingCollection = new TypedBindingCollection<IAbsenceRequestValidator>();
                _absenceRequestOpenPeriod.StaffingThresholdValidatorList.ForEach(bindingCollection.Add);
                return bindingCollection;
            }
        }

        public IList<IAbsenceRequestValidator> PersonAccountValidatorList
        {
            get
            {
                var bindingCollection = new TypedBindingCollection<IAbsenceRequestValidator>();
                _absenceRequestOpenPeriod.PersonAccountValidatorList.ForEach(bindingCollection.Add);
                return bindingCollection;
            }
        }

        public IList<IProcessAbsenceRequest> AbsenceRequestProcessList
        {
            get
            {
                var bindingCollection = new TypedBindingCollection<IProcessAbsenceRequest>();
                _absenceRequestOpenPeriod.AbsenceRequestProcessList.ForEach(bindingCollection.Add);
                return bindingCollection;
            }
        }

        public int? RollingStart
        {
            get
            {
                var period = _absenceRequestOpenPeriod as AbsenceRequestOpenRollingPeriod;
                if (period == null) return null;
                return period.BetweenDays.Minimum;
            }
            set
            {
                if (value.HasValue)
                {
                    var period = (AbsenceRequestOpenRollingPeriod)_absenceRequestOpenPeriod;
                    var currentEndDay = period.BetweenDays.Maximum;
                    if (currentEndDay < value)
                    {
                        currentEndDay = value.Value;
                    }
                    period.BetweenDays = new MinMax<int>(value.Value, currentEndDay);
                    Owner.IsDirty = true;
                }
            }
        }

        public int? RollingEnd
        {
            get
            {
                var period = _absenceRequestOpenPeriod as AbsenceRequestOpenRollingPeriod;
                if (period == null) return null;
                return period.BetweenDays.Maximum;
            }
            set
            {
                if (value.HasValue)
                {
                    var period = (AbsenceRequestOpenRollingPeriod)_absenceRequestOpenPeriod;
                    var currentStartDay = period.BetweenDays.Minimum;
                    if (currentStartDay > value)
                    {
                        currentStartDay = value.Value;
                    }
                    period.BetweenDays = new MinMax<int>(currentStartDay, value.Value);
                    Owner.IsDirty = true;
                }
            }
        }

        public DateOnly? PeriodStartDate
        {
            get
            {
                var period = _absenceRequestOpenPeriod as AbsenceRequestOpenDatePeriod;
                if (period == null) return null;
                return period.Period.StartDate;
            }
            set
            {
                if (value.HasValue)
                {
                    var period = (AbsenceRequestOpenDatePeriod)_absenceRequestOpenPeriod;
                    var currentEndDate = period.Period.EndDate;
                    if (currentEndDate < value.Value)
                    {
                        currentEndDate = value.Value;
                    }
                    period.Period = new DateOnlyPeriod(value.Value, currentEndDate);
                    Owner.IsDirty = true;
                }
            }
        }

        public DateOnly? PeriodEndDate
        {
            get
            {
                var period = _absenceRequestOpenPeriod as AbsenceRequestOpenDatePeriod;
                if (period == null) return null;
                return period.Period.EndDate;
            }
            set
            {
                if (value.HasValue)
                {
                    var period = (AbsenceRequestOpenDatePeriod)_absenceRequestOpenPeriod;
                    var currentStartDate = period.Period.StartDate;
                    if (currentStartDate > value.Value)
                    {
                        currentStartDate = value.Value;
                    }
                    period.Period = new DateOnlyPeriod(currentStartDate, value.Value);
                    Owner.IsDirty = true;
                }
            }
        }

        public DateOnly OpenStartDate
        {
            get { return _absenceRequestOpenPeriod.OpenForRequestsPeriod.StartDate; }
            set
            {
                value = value.ValidDateOnly();
                var currentEndDate = _absenceRequestOpenPeriod.OpenForRequestsPeriod.EndDate;
                if (currentEndDate < value)
                {
                    currentEndDate = value;
                }
                _absenceRequestOpenPeriod.OpenForRequestsPeriod = new DateOnlyPeriod(value, currentEndDate);
                Owner.IsDirty = true;
            }
        }

        public DateOnly OpenEndDate
        {
            get { return _absenceRequestOpenPeriod.OpenForRequestsPeriod.EndDate; }
            set
            {
                value = value.ValidDateOnly();
                var currentStartDate = _absenceRequestOpenPeriod.OpenForRequestsPeriod.StartDate;
                if (currentStartDate > value)
                {
                    currentStartDate = value;
                }
                _absenceRequestOpenPeriod.OpenForRequestsPeriod = new DateOnlyPeriod(currentStartDate, value);
                Owner.IsDirty = true;
            }
        }

        public void SetDomainEntity(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
        {
            _absenceRequestOpenPeriod = absenceRequestOpenPeriod;
            _periodType = new AbsenceRequestPeriodTypeModel(_absenceRequestOpenPeriod, string.Empty);
            foreach (var periodType in WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters)
            {
                if (_periodType.Equals(periodType))
                    _periodType.DisplayText = periodType.DisplayText;
            }
        }
    }
}