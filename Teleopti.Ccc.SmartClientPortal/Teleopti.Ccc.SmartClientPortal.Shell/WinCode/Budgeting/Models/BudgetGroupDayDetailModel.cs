using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models
{
	public class BudgetGroupDayDetailModel : BaseModel, IBudgetGroupDayDetailModel
	{
		private double _fulltimeEquivalentHours;
		private DateOnly _date;
		private double? _staffEmployed;
		private Percent _attritionRate;
		private double _recruitment;
		private double _grossStaff;
		private double _contractors;
		private double _daysOffPerWeek;
		private double _netStaff;
		private double _netStaffFcAdj;
		private double _overtimeHours;
		private double _studentsHours;
		private double _budgetedStaff;
		private double _forecastedHours;
		private double _forecastedStaff;
		private double _difference;
		private Percent _differencePercent;
		private bool _isClosed;
		private Percent _absenceThreshold;
		private double? _absenceExtra;
		private double? _absenceOverride;
		private double _budgetedLeave;
		private double _budgetedSurplus;
		private double _shrinkedAllowance;
		private double _fullAllowance;

		public event EventHandler<CustomEventArgs<IBudgetGroupDayDetailModel>> Invalidate;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public BudgetGroupDayDetailModel(IBudgetDay budgetDay)
		{
			_date = budgetDay.Day; //column header

			_fulltimeEquivalentHours = budgetDay.FulltimeEquivalentHours;
			_attritionRate = budgetDay.AttritionRate;
			_staffEmployed = budgetDay.StaffEmployed;
			_recruitment = budgetDay.Recruitment;
			_contractors = budgetDay.Contractors;
			_daysOffPerWeek = budgetDay.DaysOffPerWeek;
			_overtimeHours = budgetDay.OvertimeHours;
			_studentsHours = budgetDay.StudentHours;
			_forecastedHours = budgetDay.ForecastedHours;
			_isClosed = budgetDay.IsClosed;
			_absenceThreshold = budgetDay.AbsenceThreshold;
			_absenceExtra = budgetDay.AbsenceExtra;
			_absenceOverride = budgetDay.AbsenceOverride;
			_fullAllowance = budgetDay.FullAllowance;
			_shrinkedAllowance = budgetDay.ShrinkedAllowance;
			BudgetDay = budgetDay;
		}

		public DateDayModel Date
		{
			get
			{
				return new DateDayModel(_date);
			}
			set
			{
				TriggerNotifyPropertyChanged(nameof(Date));
				if (value != null) _date = value.Date;
			}
		}

		public double FulltimeEquivalentHours
		{
			get { return _fulltimeEquivalentHours; }
			set
			{
				_fulltimeEquivalentHours = value;
				TriggerNotifyPropertyChanged(nameof(FulltimeEquivalentHours));
				TriggerInvalidate();
			}
		}

		public string Week
		{
			get
			{
				var weekNumber = DateHelper.WeekNumber(_date.Date, CultureInfo.CurrentCulture);
				var week = DateHelper.GetWeekPeriod(_date, CultureInfo.CurrentCulture);
				return string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.WeekAbbreviationDot, weekNumber,
					week.StartDate.ToShortDateString());
			}
			set
			{
				TriggerNotifyPropertyChanged(nameof(Week));
				_date = new DateOnly(DateTime.Parse(value, CultureInfo.CurrentCulture));
			}
		}

		private void TriggerInvalidate()
		{
			Invalidate?.Invoke(this, new CustomEventArgs<IBudgetGroupDayDetailModel>(this));
		}

		public double? StaffEmployed
		{
			get { return _staffEmployed; }
			set
			{
				if (_staffEmployed != value)
				{
					_staffEmployed = value;
					TriggerNotifyPropertyChanged(nameof(StaffEmployed));
					TriggerInvalidate();
				}
			}
		}

		public Percent AttritionRate
		{
			get { return _attritionRate; }
			set
			{
				if (_attritionRate != value)
				{
					_attritionRate = value;
					TriggerNotifyPropertyChanged(nameof(AttritionRate));
					TriggerInvalidate();
				}
			}
		}

		public double Recruitment
		{
			get { return _recruitment; }
			set
			{
				if (_recruitment != value)
				{
					_recruitment = value;
					TriggerNotifyPropertyChanged(nameof(Recruitment));
					TriggerInvalidate();
				}
			}
		}

		public double GrossStaff
		{
			get { return _grossStaff; }
			set
			{
				if (_grossStaff != value)
				{
					_grossStaff = value;
					TriggerNotifyPropertyChanged(nameof(GrossStaff));
				}
			}
		}

		public double Contractors
		{
			get { return _contractors; }
			set
			{
				if (_contractors != value)
				{
					_contractors = value;
					TriggerNotifyPropertyChanged(nameof(Contractors));
					TriggerInvalidate();
				}
			}
		}

		public double DaysOffPerWeek
		{
			get { return _daysOffPerWeek; }
			set
			{
				if (_daysOffPerWeek != value)
				{
					_daysOffPerWeek = value;
					TriggerNotifyPropertyChanged(nameof(DaysOffPerWeek));
					TriggerInvalidate();
				}
			}
		}

		public bool IsClosed
		{
			get { return _isClosed; }
			set
			{
				if (_isClosed != value)
				{
					_isClosed = value;
					TriggerNotifyPropertyChanged(nameof(IsClosed));
					TriggerInvalidate();
				}
			}
		}

		public double NetStaff
		{
			get { return _netStaff; }
			set
			{
				if (_netStaff != value)
				{
					_netStaff = value;
					TriggerNotifyPropertyChanged(nameof(NetStaff));
				}
			}
		}

		public double NetStaffFcAdj
		{
			get { return _netStaffFcAdj; }
			set
			{
				if (_netStaffFcAdj != value)
				{
					_netStaffFcAdj = value;
					TriggerNotifyPropertyChanged(nameof(NetStaffFcAdj));
				}
			}
		}

		public double OvertimeHours
		{
			get { return _overtimeHours; }
			set
			{
				if (_overtimeHours != value)
				{
					_overtimeHours = value;
					TriggerNotifyPropertyChanged(nameof(OvertimeHours));
					TriggerInvalidate();
				}
			}
		}

		public double StudentsHours
		{
			get { return _studentsHours; }
			set
			{
				if (_studentsHours != value)
				{
					_studentsHours = value;
					TriggerNotifyPropertyChanged(nameof(StudentsHours));
					TriggerInvalidate();
				}
			}
		}

		public double BudgetedStaff
		{
			get { return _budgetedStaff; }
			set
			{
				if (_budgetedStaff != value)
				{
					_budgetedStaff = value;
					TriggerNotifyPropertyChanged(nameof(BudgetedStaff));
				}
			}
		}

		public double ForecastedHours
		{
			get { return _forecastedHours; }
			set
			{
				if (_forecastedHours != value)
				{
					_forecastedHours = value;
					TriggerNotifyPropertyChanged(nameof(ForecastedHours));
					TriggerInvalidate();
				}
			}
		}

		public double ForecastedStaff
		{
			get { return _forecastedStaff; }
			set
			{
				if (_forecastedStaff != value)
				{
					_forecastedStaff = value;
					TriggerNotifyPropertyChanged(nameof(ForecastedStaff));
				}
			}
		}

		public double Difference
		{
			get { return _difference; }
			set
			{
				if (_difference != value)
				{
					_difference = value;
					TriggerNotifyPropertyChanged(nameof(Difference));
				}
			}
		}

		public Percent DifferencePercent
		{
			get { return _differencePercent; }
			set
			{
				if (_differencePercent != value)
				{
					_differencePercent = value;
					TriggerNotifyPropertyChanged(nameof(DifferencePercent));
				}
			}
		}

		public IBudgetDay BudgetDay { get; set; }

		public Percent AbsenceThreshold
		{
			get { return _absenceThreshold; }
			set
			{
				if (_absenceThreshold != value)
				{
					_absenceThreshold = value;
					TriggerNotifyPropertyChanged(nameof(AbsenceThreshold));
					TriggerInvalidate();
				}
			}
		}

		public double? AbsenceExtra
		{
			get { return _absenceExtra; }
			set
			{
				if (_absenceExtra != value)
				{
					_absenceExtra = value;
					TriggerNotifyPropertyChanged(nameof(AbsenceExtra));
					TriggerInvalidate();
				}
			}
		}

		public double? AbsenceOverride
		{
			get { return _absenceOverride; }
			set
			{
				if (_absenceOverride != value)
				{
					_absenceOverride = value;
					TriggerNotifyPropertyChanged(nameof(AbsenceOverride));
					TriggerInvalidate();
				}
			}
		}

		public double BudgetedLeave
		{
			get { return _budgetedLeave; }
			set
			{
				if (_budgetedLeave != value)
				{
					_budgetedLeave = value;
					TriggerNotifyPropertyChanged(nameof(BudgetedLeave));
				}
			}
		}

		public double BudgetedSurplus
		{
			get { return _budgetedSurplus; }
			set
			{
				if (_budgetedSurplus != value)
				{
					_budgetedSurplus = value;
					TriggerNotifyPropertyChanged(nameof(BudgetedSurplus));
				}
			}
		}

		public double ShrinkedAllowance
		{
			get { return _shrinkedAllowance; }
			set
			{
				if (_shrinkedAllowance != value)
				{
					_shrinkedAllowance = value;
					TriggerNotifyPropertyChanged(nameof(ShrinkedAllowance));
				}
			}
		}

		public double FullAllowance
		{
			get { return _fullAllowance; }
			set
			{
				if (_fullAllowance != value)
				{
					_fullAllowance = value;
					TriggerNotifyPropertyChanged(nameof(FullAllowance));
				}
			}
		}

		public void UpdateBudgetDay()
		{
			BudgetDay.AttritionRate = _attritionRate;
			BudgetDay.FulltimeEquivalentHours = _fulltimeEquivalentHours;
			BudgetDay.StaffEmployed = _staffEmployed;
			BudgetDay.Recruitment = _recruitment;
			BudgetDay.Contractors = _contractors;
			BudgetDay.DaysOffPerWeek = _daysOffPerWeek;
			BudgetDay.OvertimeHours = _overtimeHours;
			BudgetDay.StudentHours = _studentsHours;
			BudgetDay.ForecastedHours = _forecastedHours;
			BudgetDay.IsClosed = _isClosed;
			BudgetDay.AbsenceThreshold = _absenceThreshold;
			BudgetDay.AbsenceOverride = _absenceOverride;
			BudgetDay.AbsenceExtra = _absenceExtra;
			BudgetDay.FullAllowance = _fullAllowance;
			BudgetDay.ShrinkedAllowance = _shrinkedAllowance;
		}

		public void Recalculate(IBudgetCalculator calculator)
		{
			var result = BudgetDay.Calculate(calculator);

			NetStaffFcAdj = result.NetStaffFcAdj;
			NetStaff = result.NetStaff;
			GrossStaff = result.GrossStaff;
			BudgetedStaff = result.BudgetedStaff;
			ForecastedStaff = result.ForecastedStaff;
			Difference = result.Difference;
			DifferencePercent = result.DifferencePercent;
			BudgetedLeave = result.BudgetedLeave;
			BudgetedSurplus = result.BudgetedSurplus;
			FullAllowance = result.FullAllowance;
			ShrinkedAllowance = result.ShrinkedAllowance;
		}

		public void RecalculateWithoutNetStaffForecastAdjustCalculator(IBudgetCalculator calculator, double netStaffFcAdj)
		{
			var result = BudgetDay.CalculateWithoutNetStaffFcAdj(calculator, netStaffFcAdj);

			NetStaff = result.NetStaff;
			GrossStaff = result.GrossStaff;
			BudgetedStaff = result.BudgetedStaff;
			ForecastedStaff = result.ForecastedStaff;
			Difference = result.Difference;
			DifferencePercent = result.DifferencePercent;
			BudgetedLeave = result.BudgetedLeave;
			BudgetedSurplus = result.BudgetedSurplus;
			FullAllowance = result.FullAllowance;
			ShrinkedAllowance = result.ShrinkedAllowance;
		}

		public Percent GetShrinkage(ICustomShrinkage customShrinkage)
		{
			return BudgetDay.CustomShrinkages.GetShrinkage(customShrinkage.Id.GetValueOrDefault(Guid.Empty));
		}

		public void SetShrinkage(ICustomShrinkage customShrinkage, Percent percent)
		{
			BudgetDay.CustomShrinkages.SetShrinkage(customShrinkage.Id.GetValueOrDefault(Guid.Empty), percent);
			TriggerInvalidate();
		}

		public Percent GetEfficiencyShrinkage(ICustomEfficiencyShrinkage customEfficiencyShrinkage)
		{
			return BudgetDay.CustomEfficiencyShrinkages.GetEfficiencyShrinkage(customEfficiencyShrinkage.Id.GetValueOrDefault(Guid.Empty));
		}

		public void SetEfficiencyShrinkage(ICustomEfficiencyShrinkage customEfficiencyShrinkage, Percent percent)
		{
			BudgetDay.CustomEfficiencyShrinkages.SetEfficiencyShrinkage(customEfficiencyShrinkage.Id.GetValueOrDefault(Guid.Empty), percent);
			TriggerInvalidate();
		}

		public void TriggerRecalculation()
		{
			TriggerInvalidate();
		}
	}
}