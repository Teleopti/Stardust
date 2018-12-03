using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public static class WorkShiftCalculatorExtensions
	{
		public static IWorkShiftCalculationResultHolder CalculateShiftValue(
			this IWorkShiftCalculator instance,
			IEnumerable<IWorkShiftCalculatableLayer> mainShiftLayers,
			IWorkShiftCalculatorSkillStaffPeriodData skillStaffPeriodData,
			WorkShiftLengthHintOption lengthFactor,
			bool useMinimumPersons,
			bool useMaximumPersons)
		{
			return instance.CalculateShiftValue(
				mainShiftLayers,
				skillStaffPeriodData,
				lengthFactor,
				useMaximumPersons,
				useMaximumPersons,
				TimeHelper.FitToDefaultResolutionRoundDown);
		}
	}

	public class WorkShiftCalculatableVisualLayerCollection : IEnumerable<IWorkShiftCalculatableLayer>
	{
		private readonly IEnumerable<IWorkShiftCalculatableLayer> _wrap;

		public WorkShiftCalculatableVisualLayerCollection(IVisualLayerCollection inner)
		{
			Inner = inner;
			_wrap = Inner.Select(l => new WorkShiftCalculatableLayer(l)).ToArray();
		}

		public IVisualLayerCollection Inner { get; }

		public IEnumerator<IWorkShiftCalculatableLayer> GetEnumerator()
		{
			return _wrap.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public class WorkShiftCalculatableLayer : IWorkShiftCalculatableLayer
	{
		public WorkShiftCalculatableLayer(IVisualLayer layer)
		{
			PeriodStartDateTime = layer.Period.StartDateTime;
			PeriodEndDateTime = layer.Period.EndDateTime;
			Activity = new WorkShiftCalculatableActivity((IActivity) layer.Payload);
		}

		public DateTime PeriodStartDateTime { get; }
		public DateTime PeriodEndDateTime { get; }
		public IWorkShiftCalculatableActivity Activity { get; }
	}

	public class WorkShiftCalculatableActivity : IWorkShiftCalculatableActivity
	{
		public WorkShiftCalculatableActivity(IActivity activity)
		{
			Activity = activity;
			RequiresSkill = Activity.RequiresSkill;
		}

		public IActivity Activity { get; }

		public bool RequiresSkill { get; }
	}

	public class WorkShiftCalculatorSkillStaffPeriodData : IWorkShiftCalculatorSkillStaffPeriodData
	{
		private readonly IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> _data;

		public WorkShiftCalculatorSkillStaffPeriodData(IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> data)
		{
			_data = data;
		}

		public bool Empty()
		{
			return _data.Count == 0;
		}

		public IWorkShiftCalculatorStaffPeriodData ForActivity(IWorkShiftCalculatableActivity activity)
		{
			var key = ((WorkShiftCalculatableActivity)activity).Activity;
			IDictionary<DateTime, ISkillStaffPeriodDataHolder> content;
			if (_data.TryGetValue(key, out content))
			{
				return new WorkShiftCalculatorStaffPeriodData(content);
			}
			return null;
		}

		public IEnumerable<IWorkShiftCalculatorStaffPeriodData> All()
		{
			return from i in _data.Values select new WorkShiftCalculatorStaffPeriodData(i);
		}
	}

	public class WorkShiftCalculatorStaffPeriodData : IWorkShiftCalculatorStaffPeriodData
	{
		private readonly IDictionary<DateTime, ISkillStaffPeriodDataHolder> _data;
		private int? _resolution;

		public WorkShiftCalculatorStaffPeriodData(IDictionary<DateTime, ISkillStaffPeriodDataHolder> data)
		{
			_data = data;
		}

		public IDictionary<DateTime, ISkillStaffPeriodDataHolder> Data => _data;

		public IWorkShiftCalculatableSkillStaffPeriod ForTime(DateTime dateTime)
		{
			ISkillStaffPeriodDataHolder result;
			_data.TryGetValue(dateTime, out result);
			return result;
		}

		public int Resolution()
		{
			if (_resolution.HasValue) return _resolution.Value;

			_resolution = 15;

			var firstPeriod = _data.Values.FirstOrDefault();
			if (firstPeriod != null)
			{
				_resolution = (int)firstPeriod.PeriodEndDateTime.Subtract(firstPeriod.PeriodStartDateTime).TotalMinutes;
			}
			return _resolution.Value;
		}
		
		public IEnumerable<IWorkShiftCalculatableSkillStaffPeriod> All()
		{
			return _data.Values;
		}
	}
}