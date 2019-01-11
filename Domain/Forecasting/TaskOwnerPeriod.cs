using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class TaskOwnerPeriod : ITaskOwnerPeriod
	{
		private TimeSpan _averageAfterTaskTime;
		private TimeSpan _averageTaskTime;
		private Percent _campaignAfterTaskTime;
		private Percent _campaignTasks;
		private Percent _campaignTaskTime;
		private bool _initialized;
		private bool _isDirty;
		private readonly IList<ITaskOwner> _parents = new List<ITaskOwner>();
		private readonly IList<ITaskOwner> _taskOwnerDayCollection;
		private double _tasks;
		private TimeSpan _totalAverageAfterTaskTime;
		private TimeSpan _totalAverageTaskTime;
		private double _totalStatisticAbandonedTasks;
		private double _totalStatisticAnsweredTasks;
		private TimeSpan _totalStatisticAverageAfterTaskTime;
		private TimeSpan _totalStatisticAverageTaskTime;
		private double _totalStatisticCalculatedTasks;
		private double _totalTasks;
		private bool _turnOffInternalRecalc;

		public TaskOwnerPeriod(DateOnly currentDate, IEnumerable<ITaskOwner> taskOwnerDays, TaskOwnerPeriodType taskOwnerPeriodType)
		{
			CurrentDate = currentDate;
			TypeOfTaskOwnerPeriod = taskOwnerPeriodType;
			_taskOwnerDayCollection = new List<ITaskOwner>(taskOwnerDays?.Count() ?? 0);
			AddRange(taskOwnerDays);
		}
		
		public DateOnly EndDate => GetEndDate();
		
		public bool IsLoaded => (_taskOwnerDayCollection?.Count > 0);
		
		public DateOnly StartDate => GetStartDate();
		
		public IList<ITaskOwner> TaskOwnerDayCollection => _taskOwnerDayCollection.ToArray();
		
		public virtual double TotalTasks
		{
			get
			{
				if (!_initialized) Initialize();
				return _totalTasks;
			}
		}
		
		public TaskOwnerPeriodType TypeOfTaskOwnerPeriod { get; set; }
		
		public void Add(ITaskOwner taskOwnerDay)
		{
			taskOwnerDay.AddParent(this);
			_taskOwnerDayCollection.Add(taskOwnerDay);

			Initialize();
		}
		
		public void AddRange(IEnumerable<ITaskOwner> taskOwnerDays)
		{
			if (taskOwnerDays == null) return;
			Lock();
			taskOwnerDays.ForEach(Add);
			Release();
		}
		
		public void Clear()
		{
			var temporaryList = _taskOwnerDayCollection.ToArray();
			Lock();
			temporaryList.ForEach(Remove);
			Release();
		}
		
		private void OnAverageTaskTimeChanged()
		{
			if (!_turnOffInternalRecalc)
			{
				_isDirty = false;
				foreach (ITaskOwner parent in _parents)
				{
					parent.RecalculateDailyTasks();
					parent.RecalculateDailyAverageTimes();
					parent.RecalculateDailyCampaignTasks();
					parent.RecalculateDailyAverageCampaignTimes();
				}
			}
			else
			{
				foreach (ITaskOwner taskOwner in _parents)
				{
					taskOwner.SetDirty();
				}
				_isDirty = true;
			}
		}
		
		private void OnCampaignAverageTimesChanged()
		{
			if (!_turnOffInternalRecalc)
			{
				_isDirty = false;
				foreach (ITaskOwner parent in _parents)
				{
					parent.RecalculateDailyTasks();
					parent.RecalculateDailyAverageTimes();
					parent.RecalculateDailyCampaignTasks();
					parent.RecalculateDailyAverageCampaignTimes();
				}
			}
			else
			{
				foreach (ITaskOwner taskOwner in _parents)
				{
					taskOwner.SetDirty();
				}
				_isDirty = true;
			}
		}
		
		private void OnCampaignTasksChanged()
		{
			if (!_turnOffInternalRecalc)
			{
				_isDirty = false;
				foreach (ITaskOwner parent in _parents)
				{
					parent.RecalculateDailyTasks();
					parent.RecalculateDailyAverageTimes();
					parent.RecalculateDailyCampaignTasks();
					parent.RecalculateDailyAverageCampaignTimes();
				}
			}
			else
			{
				foreach (ITaskOwner taskOwner in _parents)
				{
					taskOwner.SetDirty();
				}
				_isDirty = true;
			}
		}
		
		private void OnTasksChanged()
		{
			if (!_turnOffInternalRecalc)
			{
				_isDirty = false;
				foreach (ITaskOwner parent in _parents)
				{
					parent.RecalculateDailyTasks();
					parent.RecalculateDailyAverageTimes();
					parent.RecalculateDailyCampaignTasks();
					parent.RecalculateDailyAverageCampaignTimes();
				}
			}
			else
			{
				foreach (ITaskOwner taskOwner in _parents)
				{
					taskOwner.SetDirty();
				}
				_isDirty = true;
			}
		}
		
		public void RecalculateDailyAverageTimes()
		{
			if (!_turnOffInternalRecalc)
			{
				_isDirty = false;
				_turnOffInternalRecalc = true;

				IList<ITaskOwner> taskOwnerDays = TaskOwnerDayOpenCollection();
				if (_totalTasks > 0d)
				{
					_averageTaskTime = TimeSpan.FromTicks((long)
						 (taskOwnerDays.Sum(t => t.AverageTaskTime.Ticks * t.Tasks) / _tasks));
					_averageAfterTaskTime = TimeSpan.FromTicks((long)
						 (taskOwnerDays.Sum(t => t.AverageAfterTaskTime.Ticks * t.Tasks) / _tasks));
					_totalAverageTaskTime = TimeSpan.FromTicks((long)
						 (taskOwnerDays.Sum(t => t.TotalAverageTaskTime.Ticks * t.TotalTasks) / _totalTasks));
					_totalAverageAfterTaskTime = TimeSpan.FromTicks((long)
						 (taskOwnerDays.Sum(t => t.TotalAverageAfterTaskTime.Ticks * t.TotalTasks) / _totalTasks));
				}
				else if (taskOwnerDays.Count > 0)
				{
					_averageTaskTime = TimeSpan.FromTicks((long)
						 (taskOwnerDays.Average(t => t.AverageTaskTime.Ticks)));
					_averageAfterTaskTime = TimeSpan.FromTicks((long)
						 (taskOwnerDays.Average(t => t.AverageAfterTaskTime.Ticks)));
					_totalAverageTaskTime = TimeSpan.FromTicks((long)
						 (taskOwnerDays.Average(t => t.TotalAverageTaskTime.Ticks)));
					_totalAverageAfterTaskTime = TimeSpan.FromTicks((long)
						 (taskOwnerDays.Average(t => t.TotalAverageAfterTaskTime.Ticks)));
				}
				else
				{
					_averageTaskTime = TimeSpan.FromSeconds(0);
					_averageAfterTaskTime = TimeSpan.FromSeconds(0);
					_totalAverageTaskTime = TimeSpan.FromSeconds(0);
					_totalAverageAfterTaskTime = TimeSpan.FromSeconds(0);
				}

				_turnOffInternalRecalc = false;
			}
			else
			{
				_isDirty = true;
			}
		}
		
		public void RecalculateDailyTasks()
		{
			if (!_turnOffInternalRecalc)
			{
				_isDirty = false;
				_turnOffInternalRecalc = true;

				var taskOwnerDays = TaskOwnerDayOpenCollection();
				if (taskOwnerDays.Count > 0)
				{
					_totalTasks = taskOwnerDays.Sum(t => t.TotalTasks);
					_tasks = taskOwnerDays.Sum(t => t.Tasks);
				}
				else
				{
					_tasks = 0;
					_totalTasks = 0;
				}

				_turnOffInternalRecalc = false;
			}
			else
			{
				_isDirty = true;
			}
		}
		
		public virtual void Remove(ITaskOwner taskOwnerDay)
		{
			if (!_taskOwnerDayCollection.Contains(taskOwnerDay)) return;
			_taskOwnerDayCollection.Remove(taskOwnerDay);

			Initialize();

			taskOwnerDay.RemoveParent(this);
		}
		
		private DateOnly GetEndDate()
		{
			var returnDate = CurrentDate;

			switch (TypeOfTaskOwnerPeriod)
			{
				case TaskOwnerPeriodType.Month:
					returnDate = new DateOnly(DateHelper.GetLastDateInMonth(CurrentDate.Date, CultureInfo.CurrentCulture));
					break;
				case TaskOwnerPeriodType.Other:
					if (_taskOwnerDayCollection.Count > 0)
						returnDate = _taskOwnerDayCollection.Max(wd => wd.CurrentDate);
					break;
				case TaskOwnerPeriodType.Week:
					returnDate = new DateOnly(DateHelper.GetLastDateInWeek(CurrentDate.Date, CultureInfo.CurrentCulture));
					break;
			}

			return returnDate;
		}
		
		private DateOnly GetStartDate()
		{
			var returnDate = CurrentDate;

			switch (TypeOfTaskOwnerPeriod)
			{
				case TaskOwnerPeriodType.Month:
					returnDate = new DateOnly(DateHelper.GetFirstDateInMonth(CurrentDate.Date, CultureInfo.CurrentCulture));
					break;
				case TaskOwnerPeriodType.Other:
					if (_taskOwnerDayCollection.Count > 0)
						returnDate = _taskOwnerDayCollection.Min(wd => wd.CurrentDate);
					break;
				case TaskOwnerPeriodType.Week:
					returnDate = DateHelper.GetFirstDateInWeek(CurrentDate, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
					break;
			}

			return returnDate;
		}

		private void Initialize()
		{
			RecalculateDailyTasks();
			RecalculateDailyAverageTimes();
			RecalculateDailyCampaignTasks();
			RecalculateDailyAverageCampaignTimes();
			RecalculateDailyStatisticTasks();
			RecalculateDailyAverageStatisticTimes();

			_initialized = true;
		}
		
		private IList<ITaskOwner> TaskOwnerDayOpenCollection()
		{
			return _taskOwnerDayCollection.Where(wd => wd.OpenForWork.IsOpenForIncomingWork).ToArray();
		}
		
		public TimeSpan AverageAfterTaskTime
		{
			get
			{
				if (!_initialized) Initialize();
				return _averageAfterTaskTime;
			}
			set
			{
				checkIsLoaded();

				long averageAfterTaskTimeTicks = AverageAfterTaskTime.Ticks;
				if (averageAfterTaskTimeTicks == 0) averageAfterTaskTimeTicks = TimeSpan.FromSeconds(1).Ticks;
				ValueDistributor.DistributeAverageAfterTaskTime(
					((double)value.Ticks / averageAfterTaskTimeTicks),
					value,
					TaskOwnerDayOpenCollection(),
					DistributionType.ByPercent);

				_averageAfterTaskTime = value;

				RecalculateDailyAverageTimes();
				RecalculateDailyAverageCampaignTimes();

				OnAverageTaskTimeChanged();
			}
		}

		private void checkIsLoaded()
		{
			if (!IsLoaded)
				throw new InvalidOperationException("The workload period must contain workload days before using this operation.");
		}
		
		public TimeSpan AverageTaskTime
		{
			get
			{
				if (!_initialized) Initialize();
				return _averageTaskTime;
			}
			set
			{
				checkIsLoaded();

				long averageTaskTimeTicks = AverageTaskTime.Ticks;
				if (averageTaskTimeTicks == 0) averageTaskTimeTicks = TimeSpan.FromSeconds(1).Ticks;
				ValueDistributor.DistributeAverageTaskTime(
					 ((double)value.Ticks / averageTaskTimeTicks),
			  value,
					 TaskOwnerDayOpenCollection(),
			  DistributionType.ByPercent);

				_averageTaskTime = value;

				RecalculateDailyAverageTimes();
				RecalculateDailyAverageCampaignTimes();

				OnAverageTaskTimeChanged();
			}
		}
		
		public void Lock()
		{
			_turnOffInternalRecalc = true;
			foreach (ITaskOwner parent in _parents)
			{
				parent.Lock();
			}
		}
		
		public void Release()
		{
			if (_taskOwnerDayCollection.Any(w => w.IsLocked)) return;

			_turnOffInternalRecalc = false;
			if (_isDirty)
			{
				Initialize();
				_isDirty = false;
			}
			foreach (ITaskOwner parent in _parents)
			{
				parent.Release();
			}
		}
		
		public void SetDirty()
		{
			_isDirty = true;
		}

		public virtual OpenForWork OpenForWork
		{
			get
			{
				return new OpenForWork(_taskOwnerDayCollection.Any(wd => wd.OpenForWork.IsOpen),
											  _taskOwnerDayCollection.Any(wd => wd.OpenForWork.IsOpenForIncomingWork));
			}
		}

		public virtual bool IsLocked => _turnOffInternalRecalc;

		public void ClearTemplateName()
		{
			throw new NotImplementedException();
		}

		public virtual void RemoveParent(ITaskOwner parent)
		{
				_parents.Remove(parent);
		}

		public virtual void ClearParents()
		{
			_parents.Clear();
		}

		public void AddParent(ITaskOwner parent)
		{
			if (!_parents.Contains(parent))
				_parents.Add(parent);
		}

		public DateOnly CurrentDate { get; set; }

		public double TotalStatisticCalculatedTasks
		{
			get
			{
				if (!_initialized) Initialize();
				return _totalStatisticCalculatedTasks;
			}
		}

		public double TotalStatisticAnsweredTasks
		{
			get
			{
				if (!_initialized) Initialize();
				return _totalStatisticAnsweredTasks;
			}
		}

		public double TotalStatisticAbandonedTasks
		{
			get
			{
				if (!_initialized) Initialize();
				return _totalStatisticAbandonedTasks;
			}
		}

		public TimeSpan TotalStatisticAverageTaskTime
		{
			get
			{
				if (!_initialized) Initialize();
				return _totalStatisticAverageTaskTime;
			}
		}

		public TimeSpan TotalStatisticAverageAfterTaskTime
		{
			get
			{
				if (!_initialized) Initialize();
				return _totalStatisticAverageAfterTaskTime;
			}
		}

		public void RecalculateDailyAverageStatisticTimes()
		{
			if (!_turnOffInternalRecalc)
			{
				double sumTasks = _taskOwnerDayCollection.Sum(t => t.TotalStatisticCalculatedTasks);
				if (sumTasks > 0d)
				{
					_totalStatisticAverageTaskTime = TimeSpan.FromTicks((long)
							  (_taskOwnerDayCollection.Sum(t => t.TotalStatisticAverageTaskTime.Ticks * t.TotalStatisticCalculatedTasks) / sumTasks));
					_totalStatisticAverageAfterTaskTime = TimeSpan.FromTicks((long)
						 (_taskOwnerDayCollection.Sum(t => t.TotalStatisticAverageAfterTaskTime.Ticks * t.TotalStatisticCalculatedTasks) / sumTasks));
				}
				else
				{
					if (_taskOwnerDayCollection.Count > 0)
					{
						_totalStatisticAverageTaskTime = TimeSpan.FromTicks((long)
								  (_taskOwnerDayCollection.Average(t => t.TotalStatisticAverageTaskTime.Ticks)));
						_totalStatisticAverageAfterTaskTime = TimeSpan.FromTicks((long)
							 (_taskOwnerDayCollection.Average(t => t.TotalStatisticAverageAfterTaskTime.Ticks)));
					}
					else
					{
						_totalStatisticAverageTaskTime = TimeSpan.FromSeconds(0);
						_totalStatisticAverageAfterTaskTime = TimeSpan.FromSeconds(0);
					}
				}
			}
			else
			{
				_isDirty = true;
			}
		}

		public void RecalculateDailyStatisticTasks()
		{
			if (!_turnOffInternalRecalc)
			{
				_totalStatisticCalculatedTasks = _taskOwnerDayCollection.Sum(t => t.TotalStatisticCalculatedTasks);
				_totalStatisticAnsweredTasks = _taskOwnerDayCollection.Sum(t => t.TotalStatisticAnsweredTasks);
				_totalStatisticAbandonedTasks = _taskOwnerDayCollection.Sum(t => t.TotalStatisticAbandonedTasks);
			}
			else
			{
				_isDirty = true;
			}
		}

		public TimeSpan TotalAverageAfterTaskTime
		{
			get
			{
				if (!_initialized) Initialize();
				return _totalAverageAfterTaskTime;
			}
		}

		public TimeSpan TotalAverageTaskTime
		{
			get
			{
				if (!_initialized) Initialize();
				return _totalAverageTaskTime;
			}
		}

		public double Tasks
		{
			get
			{
				if (!_initialized) Initialize();
				return _tasks;
			}
			set
			{
				if (!IsLoaded) throw new InvalidOperationException("The workload period must contain workload days before using this operation.");

				ValueDistributor.Distribute(value, TaskOwnerDayOpenCollection(), DistributionType.ByPercent);
				_tasks = value;

				RecalculateDailyTasks();
				RecalculateDailyCampaignTasks();

				OnTasksChanged();
			}
		}

		public Percent CampaignTasks
		{
			get
			{
				if (!_initialized) Initialize();
				return _campaignTasks;
			}
			set
			{
				checkIfIsClosed();

				bool currentState = _turnOffInternalRecalc;
				_turnOffInternalRecalc = true;
				TaskOwnerDayOpenCollection().ForEach(t => t.CampaignTasks = value);
				_turnOffInternalRecalc = currentState;

				_campaignTasks = value;

				RecalculateDailyTasks();
				RecalculateDailyAverageTimes();
				RecalculateDailyAverageCampaignTimes();

				OnCampaignTasksChanged();
			}
		}

		private void checkIfIsClosed()
		{
			if (!OpenForWork.IsOpen)
				throw new InvalidOperationException("Workload day must be open.");
		}

		public Percent CampaignTaskTime
		{
			get
			{
				if (!_initialized) Initialize();
				return _campaignTaskTime;
			}
			set
			{
				checkIfIsClosed();

				bool currentState = _turnOffInternalRecalc;
				_turnOffInternalRecalc = true;
				TaskOwnerDayOpenCollection().ForEach(t => t.CampaignTaskTime = value);
				_turnOffInternalRecalc = currentState;

				_campaignTaskTime = value;

				RecalculateDailyAverageTimes();

				OnCampaignAverageTimesChanged();
			}
		}

		public Percent CampaignAfterTaskTime
		{
			get
			{
				if (!_initialized) Initialize();
				return _campaignAfterTaskTime;
			}
			set
			{
				checkIfIsClosed();

				bool currentState = _turnOffInternalRecalc;
				_turnOffInternalRecalc = true;
				TaskOwnerDayOpenCollection().ForEach(t => t.CampaignAfterTaskTime = value);
				_turnOffInternalRecalc = currentState;

				_campaignAfterTaskTime = value;

				RecalculateDailyAverageTimes();

				OnCampaignAverageTimesChanged();
			}
		}

		public TimeSpan ForecastedIncomingDemand
		{
			get
			{
				var skillDayList = _taskOwnerDayCollection.OfType<SkillDay>();
				return TimeSpan.FromMinutes(skillDayList.Sum(s => s.ForecastedIncomingDemand.TotalMinutes));
			}
		}

		public TimeSpan ForecastedIncomingDemandWithShrinkage
		{
			get
			{
				var skillDayList = _taskOwnerDayCollection.OfType<SkillDay>();
				return TimeSpan.FromMinutes(skillDayList.Sum(s => s.ForecastedIncomingDemandWithShrinkage.TotalMinutes));
			}
		}

		public void RecalculateDailyCampaignTasks()
		{
			if (!_turnOffInternalRecalc)
			{
				_turnOffInternalRecalc = true;

				var taskOwnerDays = TaskOwnerDayOpenCollection();
				if (taskOwnerDays.Count > 0 && _tasks > 0)
				{
					double totalCamapaignTasks = taskOwnerDays.Sum(t => t.Tasks * t.CampaignTasks.Value);
					_campaignTasks = new Percent(totalCamapaignTasks / _tasks);
				}
				_turnOffInternalRecalc = false;

				OnCampaignTasksChanged();
			}
			else
			{
				_isDirty = true;
			}
		}

		public void RecalculateDailyAverageCampaignTimes()
		{
			if (!_turnOffInternalRecalc)
			{
				_turnOffInternalRecalc = true;

				var taskOwnerDays = TaskOwnerDayOpenCollection();
				if (_averageTaskTime != TimeSpan.Zero)
				{
					double totalCampaignTaskTime = taskOwnerDays.Sum(t => t.AverageTaskTime.Ticks * (1 + t.CampaignTaskTime.Value) * t.Tasks);
					double sumOfTaskTime = _tasks * _averageTaskTime.Ticks;
					_campaignTaskTime = Math.Abs(sumOfTaskTime) < 0.000001 ? new Percent(0) : new Percent((totalCampaignTaskTime / sumOfTaskTime) - 1d);
				}
				if (_averageAfterTaskTime != TimeSpan.Zero)
				{
					double totalCampaignAfterTaskTime = taskOwnerDays.Sum(t => t.AverageAfterTaskTime.Ticks * (1 + t.CampaignAfterTaskTime.Value) * t.Tasks);
					double sumOfAfterTaskTime = _tasks * _averageAfterTaskTime.Ticks;
					_campaignAfterTaskTime = Math.Abs(sumOfAfterTaskTime) < 0.000001 ? new Percent(0) : new Percent((totalCampaignAfterTaskTime / sumOfAfterTaskTime) - 1d);
				}
				_turnOffInternalRecalc = false;
				
				OnCampaignAverageTimesChanged();
			}
			else
			{
				_isDirty = true;
			}
		}

		public void ResetTaskOwner()
		{
			Tasks = 0;
			AverageAfterTaskTime = TimeSpan.Zero;
			AverageTaskTime = TimeSpan.Zero;
			CampaignAfterTaskTime = new Percent(0);
			CampaignTasks = new Percent(0);
			CampaignTaskTime = new Percent(0);
		}
	}
}
