using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Backlog
{
	public class BacklogTask
	{
		private readonly TimeSpan _incomingDemand;
		private readonly DateOnly _startDate;
		private TimeSpan _serviceLevel;
		private readonly IList<DateOnly> _closedDays = new List<DateOnly>();
		private readonly BacklogProductPlanTask _backlogProductPlanTask;
		private readonly BacklogScheduledTask _backlogScheduledTask;

		public BacklogTask(TimeSpan incomingDemand, DateOnly startDate, TimeSpan serviceLevel)
		{
			_incomingDemand = incomingDemand;
			_startDate = startDate;
			_serviceLevel = serviceLevel;
			_backlogProductPlanTask = new BacklogProductPlanTask(this);
			_backlogScheduledTask = new BacklogScheduledTask(this);
		}

		public DateOnly StartDate
		{
			get { return _startDate; }
		}

		public TimeSpan ServiceLevel
		{
			get { return _serviceLevel; }
		}

		public IEnumerable<DateOnly> ClosedDays
		{
			get { return _closedDays; }
		}

		public TimeSpan IncomingDemand
		{
			get { return _incomingDemand; }
		}

		public BacklogProductPlanTask BacklogProductPlanTask
		{
			get { return _backlogProductPlanTask; }
		}

		public BacklogScheduledTask BacklogScheduledTask
		{
			get { return _backlogScheduledTask; }
		}

		public DateOnlyPeriod SpanningDateOnlyPeriod()
		{
			var endDate = new DateOnly(StartDate.Date.AddTicks(_serviceLevel.Ticks).Date).AddDays(-1);
			return new DateOnlyPeriod(_startDate, endDate);
		}

		public void CloseDate(DateOnly date)
		{
			if (!SpanningDateOnlyPeriod().Contains(date))
				return;

			if(!_closedDays.Contains(date))
				_closedDays.Add(date);
		}
	}
}