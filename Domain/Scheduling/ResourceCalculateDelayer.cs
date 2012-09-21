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
			if (_counter % _calculationFrequency == 0 || scheduleDateOnly != _lastDate.Value)
			{
				_resourceOptimizationHelper.ResourceCalculateDate(_lastDate.Value, _useOccupancyAdjustment, _considerShortBreaks);
				if (_calculationFrequency > 1)
				{
					_resourceOptimizationHelper.ResourceCalculateDate(_lastDate.Value.AddDays(1), _useOccupancyAdjustment, _considerShortBreaks);
				}
				else
				{
					DateTimePeriod? dateTimePeriod = workShiftProjectionPeriod;
					if (dateTimePeriod.HasValue)
					{
						DateTimePeriod period = dateTimePeriod.Value;
						if (period.StartDateTime.Date != period.EndDateTime.Date)
							_resourceOptimizationHelper.ResourceCalculateDate(_lastDate.Value.AddDays(1), _useOccupancyAdjustment, _considerShortBreaks);
					}
				}
				_lastDate = scheduleDateOnly;
				_counter = 1;

				return true;
			}

			_counter++;
			return false;
		}

		public bool CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod, IList<IScheduleDay> addedSchedules)
		{
			//Move this to another place
			IList<IVisualLayerCollection> addedVisualLayerCollections = new List<IVisualLayerCollection>();
			foreach (var addedSchedule in addedSchedules)
			{
				IVisualLayerCollection collection = addedSchedule.AssignmentHighZOrder().ProjectionService().CreateProjection();
				addedVisualLayerCollections.Add(collection);
			}
			_resourceOptimizationHelper.ResourceCalculateDate(scheduleDateOnly, _useOccupancyAdjustment, _considerShortBreaks, new List<IVisualLayerCollection>(), addedVisualLayerCollections);
			DateTimePeriod? dateTimePeriod = workShiftProjectionPeriod;
			if (dateTimePeriod.HasValue)
			{
				DateTimePeriod period = dateTimePeriod.Value;
				if (period.StartDateTime.Date != period.EndDateTime.Date)
					_resourceOptimizationHelper.ResourceCalculateDate(scheduleDateOnly.AddDays(1), _useOccupancyAdjustment, _considerShortBreaks, new List<IVisualLayerCollection>(), addedVisualLayerCollections);
			}
			return true;
		}
	}
}