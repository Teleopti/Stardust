﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
    public class BudgetDay : AggregateRootWithBusinessUnit, IBudgetDay
    {
        private IBudgetGroup _budgetGroup;
        private readonly IScenario _scenario;
        private readonly DateOnly _day;
        private double _fulltimeEquivalentHours;
        private double? _staffEmployed;
        private Percent _attritionRate;
        private double _recruitment;
        private double _contractors;
        private double _daysOffPerWeek;
        private double _overtimeHours;
        private double _studentHours;
        private double _forecastedHours;
        private readonly IDictionary<Guid, Percent> _customShrinkages = new Dictionary<Guid, Percent>();
        private readonly IDictionary<Guid, Percent> _customEfficiencyShrinkages = new Dictionary<Guid, Percent>();
        private bool _isClosed;
        private Percent _absenceThreshold;
        private double? _absenceExtra;
        private double? _absenceOverride;
        private double _totalAllowance;
        private double _allowance;

        protected BudgetDay()
        {
        }

        public BudgetDay(IBudgetGroup budgetGroup, IScenario scenario, DateOnly day) : this()
        {
            _budgetGroup = budgetGroup;
            _scenario = scenario;
            _day = day;
        }

        public virtual DateOnly Day
        {
            get { return _day; }
        }

        public virtual IScenario Scenario
        {
            get { return _scenario; }
        }

        public virtual IBudgetGroup BudgetGroup
        {
            get { return _budgetGroup; }
            set { _budgetGroup = value; }
        }

        public virtual double FulltimeEquivalentHours
        {
            get { return _fulltimeEquivalentHours; }
            set { _fulltimeEquivalentHours = value; }
        }

        public virtual double? StaffEmployed
        {
            get { return _staffEmployed; }
            set { _staffEmployed = value; }
        }

        public virtual Percent AttritionRate
        {
            get { return _attritionRate; }
            set { _attritionRate = value; }
        }

        public virtual double Recruitment
        {
            get { return _recruitment; }
            set { _recruitment = value; }
        }

        public virtual double Contractors
        {
            get { return _contractors; }
            set { _contractors = value; }
        }

        public virtual double DaysOffPerWeek
        {
            get { return _daysOffPerWeek; }
            set { _daysOffPerWeek = value; }
        }

        public virtual double OvertimeHours
        {
            get { return _overtimeHours; }
            set { _overtimeHours = value; }
        }

        public virtual double StudentHours
        {
            get { return _studentHours; }
            set { _studentHours = value; }
        }

        public virtual double ForecastedHours
        {
            get { return _forecastedHours; }
            set { _forecastedHours = value; }
        }

        public virtual ICustomShrinkageWrapper CustomShrinkages
        {
            get { return new CustomShrinkageWrapper(_budgetGroup, _customShrinkages); }
        }

        public virtual ICustomEfficiencyShrinkageWrapper CustomEfficiencyShrinkages
        {
            get { return new CustomEfficiencyShrinkageWrapper(_budgetGroup, _customEfficiencyShrinkages); }
        }

        public virtual bool IsClosed
        {
            get { return _isClosed; }
            set { _isClosed = value; }
        }

        public virtual Percent AbsenceThreshold
        {
            get { return _absenceThreshold; }
            set { _absenceThreshold = value; }
        }

        public virtual double? AbsenceExtra
        {
            get { return _absenceExtra; }
            set { _absenceExtra = value; }
        }

        public virtual double? AbsenceOverride
        {
            get { return _absenceOverride; }
            set { _absenceOverride = value; }
        }

        public virtual double TotalAllowance
        {
            get { return _totalAllowance; }
            set { _totalAllowance = value; }
        }

        public virtual double Allowance
        {
            get { return _allowance; }
            set { _allowance = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual BudgetCalculationResult Calculate(IBudgetCalculator calculator)
        {
            return calculator.Calculate(this);
        }
    }
}