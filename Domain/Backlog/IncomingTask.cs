using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Backlog
{
	public class IncomingTask
	{
		private readonly DateOnlyPeriod _spanningPeriod;
		private readonly TimeSpan _serviceTime;
		private readonly double _totalWorkItems;
		private readonly TimeSpan _averageWorkTimePerItem;
		private readonly IDictionary<DateOnly, TaskDay> _taskDays = new Dictionary<DateOnly, TaskDay>();
		private double _incomingBacklogWork;

		public IncomingTask(DateOnlyPeriod spanningPeriod)
		{
			_spanningPeriod = spanningPeriod;
			foreach (var dateOnly in spanningPeriod.DayCollection())
			{
				_taskDays.Add(dateOnly, new TaskDay());
			}
		}

		public IncomingTask(DateOnlyPeriod spanningPeriod, TimeSpan serviceTime, double totalWorkItems, TimeSpan averageWorkTimePerItem)
		{
			_spanningPeriod = spanningPeriod;
			_serviceTime = serviceTime;
			_totalWorkItems = totalWorkItems;
			_averageWorkTimePerItem = averageWorkTimePerItem;
			foreach (var dateOnly in spanningPeriod.DayCollection())
			{
				_taskDays.Add(dateOnly, new TaskDay());
			}
		}

		public DateOnlyPeriod SpanningPeriod
		{
			get { return _spanningPeriod; }
		}

		public TimeSpan ServiceTime
		{
			get { return _serviceTime; }
		}

		public double TotalWorkItems
		{
			get { return _totalWorkItems + _incomingBacklogWork; }
		}

		public TimeSpan AverageWorkTimePerItem
		{
			get { return _averageWorkTimePerItem; }
		}

		public void SetIncomingBackLogWork(double workItems)
		{
			_incomingBacklogWork = workItems;
		}
	}
}