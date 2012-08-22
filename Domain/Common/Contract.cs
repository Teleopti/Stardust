using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
    public class Contract : AggregateRootWithBusinessUnit, IContract, IDeleteTag
    {
        private Description _description;
        private EmploymentType _employmentType;
        private WorkTimeDirective _workTimeDirective;
        private WorkTime _workTime;
        private readonly IList<IMultiplicatorDefinitionSet> _multiplicatorDefinitionSetCollection = new List<IMultiplicatorDefinitionSet>();
        private bool _isDeleted;
        private TimeSpan _positivePeriodWorkTimeTolerance;
        private TimeSpan _negativePeriodWorkTimeTolerance;
        private int _positiveDayOffTolerance;
        private int _negativeDayOffTolerance;
        private TimeSpan _minTimeSchedulePeriod;
        private TimeSpan _planningTimeBankMin;
        private TimeSpan _planningTimeBankMax;
        private bool _adjustTimeBankWithSeasonality;
        private bool _adjustTimeBankWithPartTimePercentage;
        private bool _isWorkTimeFromContract;
        private bool _isWorkTimeFromSchedulePeriod;

        /// <summary>
        /// Creates a new instance of Contract
        /// </summary>
        /// <param name="name">Name of Contract</param>
        public Contract(string name)
        {
            _employmentType = EmploymentType.FixedStaffNormalWorkTime;
            _description = new Description(name);
            _isWorkTimeFromSchedulePeriod = true;
        }

        /// <summary>
        /// Constructor for NHibernate
        /// </summary>
        protected Contract()
        {
            //
        }

        /// <summary>
        /// Name of Contract
        /// </summary>
        public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Type of employees at contract
        /// </summary>
        public virtual EmploymentType EmploymentType
        {
            get { return _employmentType; }
            set { _employmentType = value; }
        }

        /// <summary>
        /// Work time directive informaton of contract
        /// </summary>
        public virtual WorkTimeDirective WorkTimeDirective
        {
            get
            {
                if (_workTimeDirective == new WorkTimeDirective())
                    return WorkTimeDirective.DefaultWorkTimeDirective;
                return _workTimeDirective;
            }
            set { _workTimeDirective = value; }
        }

        /// <summary>
        /// Work time informaton of contract
        /// </summary>
        public virtual WorkTime WorkTime
        {
            get
            {
                if (_workTime == new WorkTime())
                    return WorkTime.DefaultWorkTime;
                return _workTime;
            }
            set { _workTime = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is choosable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is choosable; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-09
        /// </remarks>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-09
        /// </remarks>
        public virtual bool IsChoosable
        {
            get { return !IsDeleted; }
        }

        public virtual ReadOnlyCollection<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSetCollection
        {
            get { return new ReadOnlyCollection<IMultiplicatorDefinitionSet>(_multiplicatorDefinitionSetCollection); }
        }

        public virtual TimeSpan PlanningTimeBankMax
        {
            get { return _planningTimeBankMax; }
            set
            {
                if (value < TimeSpan.FromHours(0) || value >= TimeSpan.FromHours(100))
                    return;
                _planningTimeBankMax = value;
            }
        }

        public virtual TimeSpan PlanningTimeBankMin
        {
            get { return _planningTimeBankMin; }
            set
            {
                if (value > TimeSpan.FromHours(0) || value <= TimeSpan.FromHours(-100))
                    return;
                _planningTimeBankMin = value;
            }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual TimeSpan PositivePeriodWorkTimeTolerance
        {
            get { return _positivePeriodWorkTimeTolerance; }
            set { _positivePeriodWorkTimeTolerance = value; }
        }

        public virtual TimeSpan NegativePeriodWorkTimeTolerance
        {
            get { return _negativePeriodWorkTimeTolerance; }
            set { _negativePeriodWorkTimeTolerance = value; }
        }

        public virtual void AddMultiplicatorDefinitionSetCollection(IMultiplicatorDefinitionSet definitionSet)
        {
            InParameter.NotNull("definitionSet", definitionSet);

            if (!_multiplicatorDefinitionSetCollection.Contains(definitionSet))
                _multiplicatorDefinitionSetCollection.Add(definitionSet);
        }

        public virtual void RemoveMultiplicatorDefinitionSetCollection(IMultiplicatorDefinitionSet definitionSet)
        {
            _multiplicatorDefinitionSetCollection.Remove(definitionSet);
        }

        public virtual bool AdjustTimeBankWithSeasonality
        {
            get { return _adjustTimeBankWithSeasonality; }
            set { _adjustTimeBankWithSeasonality = value; }
        }

        public virtual bool AdjustTimeBankWithPartTimePercentage
        {
            get { return _adjustTimeBankWithPartTimePercentage; }
            set { _adjustTimeBankWithPartTimePercentage = value; }
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

        public virtual TimeSpan MinTimeSchedulePeriod
        {
            get{ return _minTimeSchedulePeriod; }
            set{ _minTimeSchedulePeriod = value; }
        }

        public virtual int PositiveDayOffTolerance
        {
            get { return _positiveDayOffTolerance; }
            set { _positiveDayOffTolerance = value; }
        }

        public virtual int NegativeDayOffTolerance
        {
            get { return _negativeDayOffTolerance; }
            set { _negativeDayOffTolerance = value; }
        }

        public virtual bool IsWorkTimeFromContract
        {
            get { return _isWorkTimeFromContract; }
            set { _isWorkTimeFromContract = value; }
        }

        public virtual bool IsWorkTimeFromSchedulePeriod
        {
            get { return _isWorkTimeFromSchedulePeriod; }
            set { _isWorkTimeFromSchedulePeriod = value; }
        }
    }
}