﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.WinCode.Budgeting.Presenters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Models
{
    public class BudgetGroupDistributedDetailModel : BaseModel, ILockable
    {
		private bool _shouldRecalculate;
		private bool _isLocked;
        private readonly IList<IBudgetGroupDayDetailModel> _budgetDays;
    	private readonly IBudgetDayProvider _budgetDayProvider;
        private readonly IBudgetPermissionService _budgetPermissionService;
        private double _forecastedHours;
		private double _fulltimeEquivalentHours;
		private double? _staffEmployed;
		private Percent _attritionRate;
		private double _recruitment;
		private double _grossStaff;
		private double _contractors;
		private double _daysOffPerWeek;
		private double _netStaff;
		private double _overtimeHours;
		private double _studentsHours;
		private double _forecastedStaff;
		private double _difference;
		private Percent _differencePercent;
		private double _netNetStaff;
		private int _count;
        private int _openDaysCount;
        private double _budgetedLeave;
        private double _budgetedSurplus;
        private double _allowance;
        private double? _absenceOverride;
        private Percent _absenceThreshold;
        private double? _absenceExtra;
        private double _totalAllowance;
        
        public BudgetGroupDistributedDetailModel(IList<IBudgetGroupDayDetailModel> budgetDays, IBudgetDayProvider budgetDayProvider, IBudgetPermissionService budgetPermissionService)
        {
        	_budgetDays = budgetDays;
        	_budgetDayProvider = budgetDayProvider;
            _budgetPermissionService = budgetPermissionService;
            foreach (var budgetDay in _budgetDays)
            {
				budgetDay.Invalidate += budgetDay_Invalidate;
				budgetDay.PropertyChanged += budgetDay_PropertyChanged;
            }
            _count = _budgetDays.Count();
            _openDaysCount = _budgetDays.Count(day => !day.IsClosed);

            RecalculateAndSetMonthTotal();
        }

    	public IEnumerable<IBudgetGroupDayDetailModel> BudgetDays
    	{
			get { return _budgetDays; }
    	}

        public bool IsLocked
        {
            get { return _isLocked; }
        }

    	private void budgetDay_PropertyChanged(object sender, PropertyChangedEventArgs e)
    	{
			CheckIfLockedOrRecalculate();
    	}

    	private void budgetDay_Invalidate(object sender, EventArgs e)
    	{
    		CheckIfLockedOrRecalculate();
    	}

    	private void CheckIfLockedOrRecalculate()
    	{
    		if (_isLocked)
    		{
    			_shouldRecalculate = true;
    			return;
    		}
    		RecalculateAndSetMonthTotal();
    	}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private void RecalculateAndSetMonthTotal()
        {
			using (PerformanceOutput.ForOperation("RecalculateAndSetMonthTotal"))
			{

			    var openBudgetDays = _budgetDays.Where(day => !day.IsClosed).ToList();
			    _openDaysCount = openBudgetDays.Count;

			    //Sum
                _forecastedHours = openBudgetDays.Sum(b => b.ForecastedHours);
                _overtimeHours = openBudgetDays.Sum(b => b.OvertimeHours);
                _contractors = openBudgetDays.Sum(b => b.Contractors);
                _studentsHours = openBudgetDays.Sum(b => b.StudentsHours);
                _recruitment = openBudgetDays.Sum(b => b.Recruitment);

			    //Average
			    calculateWithClosedDays(openBudgetDays);

                _staffEmployed = _budgetDays.First().StaffEmployed;
			    _forecastedStaff = 0d;
                
                if (_fulltimeEquivalentHours != 0d)
                {
                    if (_openDaysCount != 0)
                    {
                        _forecastedStaff = _forecastedHours / _fulltimeEquivalentHours / _openDaysCount;
                    }
                }


			    _difference = _netNetStaff - _forecastedStaff;

                if (_forecastedStaff != 0d)
                    _differencePercent = new Percent(_difference / _forecastedStaff);
                else
                    _differencePercent = new Percent(0d);

                if (_budgetPermissionService.IsAllowancePermitted)
                {
                    _budgetedLeave = _openDaysCount > 0 ? openBudgetDays.Average(b => b.BudgetedLeave) : 0d;
                    _budgetedSurplus = _openDaysCount > 0 ? openBudgetDays.Average(b => b.BudgetedSurplus) : 0d;
                    _absenceThreshold = _openDaysCount > 0
                                            ? new Percent(openBudgetDays.Average(b => b.AbsenceThreshold.Value))
                                            : new Percent(0d);
                    _absenceExtra = _openDaysCount > 0 ? openBudgetDays.Average(b => b.AbsenceExtra) : 0d;
                    _absenceOverride = _openDaysCount > 0 ? openBudgetDays.Average(b => b.AbsenceOverride) : 0d;
                    _totalAllowance = _openDaysCount > 0 ? openBudgetDays.Average(b => b.TotalAllowance) : 0d;
                    _allowance = _openDaysCount > 0 ? openBudgetDays.Average(b => b.Allowance) : 0d;

                    TriggerNotifyPropertyChanged("BudgetedLeave");
                    TriggerNotifyPropertyChanged("BudgetedSurplus");
                    TriggerNotifyPropertyChanged("AbsenceThreshold");
                    TriggerNotifyPropertyChanged("AbsenceExtra");
                    TriggerNotifyPropertyChanged("AbsenceOverride");
                    TriggerNotifyPropertyChanged("TotalAllowance");
                    TriggerNotifyPropertyChanged("Allowance");
                }

                TriggerNotifyPropertyChanged("NetStaff");
                TriggerNotifyPropertyChanged("Recruitment");
                TriggerNotifyPropertyChanged("DifferencePercent");
                TriggerNotifyPropertyChanged("Difference");
                TriggerNotifyPropertyChanged("GrossStaff");
                TriggerNotifyPropertyChanged("StudentsHours");
                TriggerNotifyPropertyChanged("Contractors");
                TriggerNotifyPropertyChanged("StaffEmployed");
                TriggerNotifyPropertyChanged("FulltimeEquivalentHours");
                TriggerNotifyPropertyChanged("OvertimeHours");
                TriggerNotifyPropertyChanged("DaysOfPerWeek");
                TriggerNotifyPropertyChanged("AttritionRate");
                TriggerNotifyPropertyChanged("NetNetStaff");
                TriggerNotifyPropertyChanged("ForecastedHours");
                TriggerNotifyPropertyChanged("ForecastedStaff");
                
			}
        }

        private void calculateWithClosedDays(List<IBudgetGroupDayDetailModel> budgetDays)
        {
            if (_openDaysCount == 0)
            {
                _netNetStaff = 0;
                _forecastedStaff = 0;
                _netStaff = 0;
                _attritionRate = new Percent(0d);
                _daysOffPerWeek = 0d;
                _grossStaff = 0d;
                _fulltimeEquivalentHours = 0d;
            }
            else
            {
                _netStaff = budgetDays.Average(b => b.NetStaff);
                _netNetStaff = budgetDays.Average(b => b.BudgetedStaff);
                _attritionRate = new Percent(budgetDays.Average(b => b.AttritionRate.Value));
                _daysOffPerWeek = budgetDays.Average(b => b.DaysOffPerWeek);
                _grossStaff = budgetDays.Average(b => b.GrossStaff);
                _fulltimeEquivalentHours = budgetDays.Average(b => b.FulltimeEquivalentHours);
            }
        }

        public string Month
        {
            get
            {
                return CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(
					CultureInfo.CurrentCulture.Calendar.GetMonth(_budgetDays.First().BudgetDay.Day.Date));
            }
        }

        public double FulltimeEquivalentHours
        {
            get { return _fulltimeEquivalentHours; }
            set
            {
				using (_budgetDayProvider.BatchUpdater())
                foreach (var budgetDay in _budgetDays)
                {
                    budgetDay.FulltimeEquivalentHours = value;
                }
                _fulltimeEquivalentHours = value;
                TriggerNotifyPropertyChanged("FulltimeEquivalentHours");

            }
        }



        public double? StaffEmployed
        {
            get { return _staffEmployed; }
            set
            {
				using (_budgetDayProvider.BatchUpdater())
					_budgetDays.First().StaffEmployed = value;
                _staffEmployed = value;
                TriggerNotifyPropertyChanged("StaffEmployed");
            }
        }

        public Percent AttritionRate
        {
            get { return _attritionRate; }
            set
            {
				using (_budgetDayProvider.BatchUpdater())
                foreach (var budgetDay in _budgetDays)
                {
                    budgetDay.AttritionRate = value;
                }
                _attritionRate = value;
                TriggerNotifyPropertyChanged("AttritionRate");
            }
        }

        public double Recruitment
        {
            get { return _recruitment; }
            set
            {
				using (_budgetDayProvider.BatchUpdater())
					_budgetDays.First().Recruitment = value;
            	_recruitment = value;
                TriggerNotifyPropertyChanged("Recruitment");
            }
        }

        public double GrossStaff
        {
            get { return _grossStaff; }
            set
            {
                _grossStaff = value;
                TriggerNotifyPropertyChanged("GrossStaff");
            }
        }

        public double Contractors
        {
            get { return _contractors; }
            set
            {
                var distributedContractors = value / _openDaysCount;
				using (_budgetDayProvider.BatchUpdater())
                foreach (var budgetDay in _budgetDays)
                {
	                budgetDay.Contractors = budgetDay.IsClosed ? 0.0 : distributedContractors;
                }
                _contractors = value;
                TriggerNotifyPropertyChanged("Contractors");
            }
        }

        public double DaysOffPerWeek
        {
            get { return _daysOffPerWeek; }
            set
            {
				using (_budgetDayProvider.BatchUpdater())
                foreach (var budgetDay in _budgetDays)
                {
                    budgetDay.DaysOffPerWeek = value;
                }
                _daysOffPerWeek = value;
                TriggerNotifyPropertyChanged("DaysOffPerWeek");
            }
        }

        public double NetStaff
        {
            get { return _netStaff; }
            set
            {
                _netStaff = value;
                TriggerNotifyPropertyChanged("NetStaff");
            }
        }

        public double OvertimeHours
        {
            get { return _overtimeHours; }
            set
            {
                var distributedOvertime = value / _openDaysCount;
				using (_budgetDayProvider.BatchUpdater())
                foreach (var budgetDay in _budgetDays)
                {
	                budgetDay.OvertimeHours = budgetDay.IsClosed ? 0.0 : distributedOvertime;
                }

                _overtimeHours = value;
                TriggerNotifyPropertyChanged("OvertimeHours");
            }
        }

        public double StudentsHours
        {
            get { return _studentsHours; }
            set
            {
                var distributedStudents = value / _openDaysCount;
				using (_budgetDayProvider.BatchUpdater())
                foreach (var budgetDay in _budgetDays)
                {
	                budgetDay.StudentsHours = budgetDay.IsClosed ? 0.0 : distributedStudents;
                }
                _studentsHours = value;
                TriggerNotifyPropertyChanged("StudentsHours");
            }
        }

        public double ForecastedHours
        {
            get { return _forecastedHours; }
            set
            {
                var distributedForecastedStaff = value / _count;
				using (PerformanceOutput.ForOperation("Batch updating forecasted hours for all budget days"))
				{
					using (_budgetDayProvider.BatchUpdater())
					{
						foreach (var budgetGroupDayDetailModel in _budgetDays)
						{
							using (PerformanceOutput.ForOperation("Setting forecasted hours for one budget day"))
							{
								budgetGroupDayDetailModel.ForecastedHours = distributedForecastedStaff;
							}
						}
					}
				}

            	_forecastedHours = value;
				using (PerformanceOutput.ForOperation("Notify property changed for ForecastedHours"))
				{
					TriggerNotifyPropertyChanged("ForecastedHours");
				}
            }
        }

        public double ForecastedStaff
        {
            get { return _forecastedStaff; }
            set
            {
                _forecastedStaff = value;
                TriggerNotifyPropertyChanged("ForecastedStaff");
            }
        }

        public double Difference
        {
            get { return _difference; }
            set
            {
                _difference = value;
                TriggerNotifyPropertyChanged("Difference");
            }
        }

        public Percent DifferencePercent
        {
            get { return _differencePercent; }
            set
            {
                _differencePercent = value;
                TriggerNotifyPropertyChanged("DifferencePercent");
            }
        }

        public double NetNetStaff
        {
            get { return _netNetStaff; }
            set
            {
                _netNetStaff = value;
                TriggerNotifyPropertyChanged("NetNetStaff");
            }
        }
        public Percent AbsenceThreshold
        {
            get { return _absenceThreshold; }
            set
            {
                using (_budgetDayProvider.BatchUpdater())
                {
                    foreach (var budgetDay in _budgetDays)
                        budgetDay.AbsenceThreshold = value;
                }
                _absenceThreshold = value;
                TriggerNotifyPropertyChanged("AbsenceThreshold");
            }
        }

        public double? AbsenceExtra
        {
            get { return _absenceExtra; }
            set
            {
                using (_budgetDayProvider.BatchUpdater())
                {
                    foreach (var budgetDay in _budgetDays)
                        budgetDay.AbsenceExtra = value;
                }
                _absenceExtra = value;
                TriggerNotifyPropertyChanged("AbsenceExtra");
            }
        }

        public double? AbsenceOverride
        {
            get { return _absenceOverride; }
            set
            {
                using (_budgetDayProvider.BatchUpdater())
                {
                    foreach (var budgetDay in _budgetDays)
                        budgetDay.AbsenceOverride = value;
                }
                _absenceOverride = value;
                TriggerNotifyPropertyChanged("AbsenceOverride");
            }
        }

        public double BudgetedLeave
        {
            get { return _budgetedLeave; }
            set
            {
                _budgetedLeave = value;
                TriggerNotifyPropertyChanged("BudgetedLeave");
            }
        }

        public double BudgetedSurplus
        {
            get { return _budgetedSurplus; }
            set
            {
                _budgetedSurplus = value;
                TriggerNotifyPropertyChanged("BudgetedSurplus");
            }
        }

        public double TotalAllowance
        {
            get { return _totalAllowance; }
            set
            {
                _totalAllowance = value;
                TriggerNotifyPropertyChanged("TotalAllowance");
            }
        }

        public double Allowance
        {
            get { return _allowance; }
            set
            {
                _allowance = value;
                TriggerNotifyPropertyChanged("Allowance");
            }
        }

        public Percent GetCustomShrinkage(ICustomShrinkage shrinkage)
        {
            var openBudgetDays = _budgetDays.Where(b => !b.IsClosed).ToList();
            var acc = openBudgetDays.Count > 0
                          ? openBudgetDays.Average(budgetDay => budgetDay.GetShrinkage(shrinkage).Value)
                          : 0d;
            return new Percent(acc);
        }

        public Percent GetEfficiencyShrinkage(ICustomEfficiencyShrinkage shrinkage)
        {
            var openBudgetDays = _budgetDays.Where(b => !b.IsClosed).ToList();
            var acc = openBudgetDays.Count > 0
                          ? openBudgetDays.Average(budgetDay => budgetDay.GetEfficiencyShrinkage(shrinkage).Value)
                          : 0d;
            return new Percent(acc);
        }


        public void Lock()
        {
            _shouldRecalculate = false;
            _isLocked = true;
        }

        public void Release()
        {
            if (_shouldRecalculate)
            {
                RecalculateAndSetMonthTotal();
            }
            _isLocked = false;
            _shouldRecalculate = false;
        }


        public void SetCustomShrinkage(ICustomShrinkage customShrinkage, Percent percent)
		{
			using (_budgetDayProvider.BatchUpdater())
            foreach (var budgetDay in _budgetDays)
            {
                budgetDay.SetShrinkage(customShrinkage, percent);
            }
        }

        public void SetCustomEfficiencyShrinkage(ICustomEfficiencyShrinkage customEfficiencyShrinkage, Percent percent)
		{
			using (_budgetDayProvider.BatchUpdater())
            foreach (var budgetDay in _budgetDays)
            {
                budgetDay.SetEfficiencyShrinkage(customEfficiencyShrinkage, percent);
            }
        }
    }
}
