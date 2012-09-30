﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ResourceCalculateDelayer : IResourceCalculateDelayer
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly int _calculationFrequency;
		private readonly bool _useOccupancyAdjustment;
		private readonly bool _considerShortBreaks;
		private DateOnly? _lastDate;
		private int _counter = 1;

		public ResourceCalculateDelayer(
			IResourceOptimizationHelper resourceOptimizationHelper, 
			int calculationFrequency, 
			bool useOccupancyAdjustment, 
			bool considerShortBreaks)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_calculationFrequency = calculationFrequency;
			_useOccupancyAdjustment = useOccupancyAdjustment;
			_considerShortBreaks = considerShortBreaks;
		}

		public bool CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod)
		{
			if (!_lastDate.HasValue)
				_lastDate = scheduleDateOnly;

			if(_calculationFrequency == 1)
			{
				CalculateIfNeeded(_lastDate.Value, workShiftProjectionPeriod, new List<IScheduleDay>());
				return true;
			}
			if (_counter % _calculationFrequency == 0 || scheduleDateOnly != _lastDate.Value)
			{
				CalculateIfNeeded(_lastDate.Value, null, new List<IScheduleDay>(), new List<IScheduleDay>());
				CalculateIfNeeded(_lastDate.Value.AddDays(1), null, new List<IScheduleDay>(), new List<IScheduleDay>());
				_lastDate = scheduleDateOnly;
				_counter = 1;

				return true;
			}

			_counter++;
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public bool CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod, IList<IScheduleDay> addedSchedules)
		{
			return CalculateIfNeeded(scheduleDateOnly, workShiftProjectionPeriod, addedSchedules, new List<IScheduleDay>());
		}

		public bool CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod, IList<IScheduleDay> addedSchedules, IList<IScheduleDay> removedSchedules)
		{
			_resourceOptimizationHelper.ResourceCalculateDate(scheduleDateOnly, _useOccupancyAdjustment, _considerShortBreaks, removedSchedules, addedSchedules);
			DateTimePeriod? dateTimePeriod = workShiftProjectionPeriod;
			if (dateTimePeriod.HasValue)
			{
				DateTimePeriod period = dateTimePeriod.Value;
				if (period.StartDateTime.Date != period.EndDateTime.Date)
					_resourceOptimizationHelper.ResourceCalculateDate(scheduleDateOnly.AddDays(1), _useOccupancyAdjustment, _considerShortBreaks, removedSchedules, addedSchedules);
			}
			return true;
		}
	}
}