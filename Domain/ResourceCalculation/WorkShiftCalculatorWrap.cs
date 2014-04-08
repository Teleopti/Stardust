using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public static class WorkShiftCalculatorExtensions
	{
		public static double CalculateShiftValue(
			this IWorkShiftCalculator instance,
			IVisualLayerCollection mainShiftLayers,
			IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> skillStaffPeriods,
			WorkShiftLengthHintOption lengthFactor,
			bool useMinimumPersons,
			bool useMaximumPersons)
		{
			return instance.CalculateShiftValue(
				new WorkShiftCalculatableVisualLayerCollection(mainShiftLayers), 
				new WorkShiftCalculatorSkillStaffPeriodData(skillStaffPeriods), 
				lengthFactor,
				useMaximumPersons, useMaximumPersons,
				TimeHelper.FitToDefaultResolution);
		}

		public static double CalculateShiftValue(
			this IWorkShiftCalculator instance,
			IVisualLayerCollection mainShiftLayers,
			IWorkShiftCalculatorSkillStaffPeriodData skillStaffPeriodData,
			WorkShiftLengthHintOption lengthFactor,
			bool useMinimumPersons,
			bool useMaximumPersons)
		{
			return instance.CalculateShiftValue(
				new WorkShiftCalculatableVisualLayerCollection(mainShiftLayers),
				skillStaffPeriodData,
				lengthFactor,
				useMaximumPersons, useMaximumPersons,
				TimeHelper.FitToDefaultResolution);
		}


		public static double CalculateDeviationImprovementAfterAssignment(IVisualLayerCollection layerCollection, Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> skillStaffPeriods)
		{
			return WorkShiftCalculator.CalculateDeviationImprovementAfterAssignment(
				new WorkShiftCalculatableVisualLayerCollection(layerCollection),
				new WorkShiftCalculatorSkillStaffPeriodData(skillStaffPeriods));
		}

		public static IEnumerable<IImprovableWorkShiftCalculation> CalculateListForBestImprovementAfterAssignment(IList<IWorkShiftCalculationResultHolder> cashes, Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> skillStaffPeriods)
		{
			return WorkShiftCalculator.CalculateListForBestImprovementAfterAssignment(cashes, new WorkShiftCalculatorSkillStaffPeriodData(skillStaffPeriods));
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

		public IVisualLayerCollection Inner { get; private set; }

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
		private readonly IVisualLayer _layer;

		public WorkShiftCalculatableLayer(IVisualLayer layer)
		{
			_layer = layer;
		}

		public DateTime PeriodStartDateTime { get { return _layer.Period.StartDateTime; } }
		public DateTime PeriodEndDateTime { get { return _layer.Period.EndDateTime; } }
		public IWorkShiftCalculatableActivity Activity { get { return new WorkShiftCalculatableActivity((IActivity)_layer.Payload); } }
	}

	public class WorkShiftCalculatableActivity : IWorkShiftCalculatableActivity
	{
		public IActivity Activity { get; private set; }

		public WorkShiftCalculatableActivity(IActivity activity)
		{
			Activity = activity;
		}

		public bool RequiresSkill { get { return Activity.RequiresSkill; } }
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
			var key = ((WorkShiftCalculatableActivity) activity).Activity;
			return _data.ContainsKey(key) ? new WorkShiftCalculatorStaffPeriodData(_data[key]) : null;
		}

		public IEnumerable<IWorkShiftCalculatorStaffPeriodData> All()
		{
			return from i in _data.Values select new WorkShiftCalculatorStaffPeriodData(i);
		}
	}

	public class WorkShiftCalculatorStaffPeriodData : IWorkShiftCalculatorStaffPeriodData
	{
		private readonly IDictionary<DateTime, ISkillStaffPeriodDataHolder> _data;

		public WorkShiftCalculatorStaffPeriodData(IDictionary<DateTime, ISkillStaffPeriodDataHolder> data)
		{
			_data = data;
		}

		public IWorkShiftCalculatableSkillStaffPeriod ForTime(DateTime dateTime)
		{
			return _data.ContainsKey(dateTime) ? _data[dateTime] : null;
		}

		public IWorkShiftCalculatableSkillStaffPeriod First()
		{
			return _data.Values.FirstOrDefault();
		}

		public IEnumerable<IWorkShiftCalculatableSkillStaffPeriod> All()
		{
			return _data.Values;
		}
	}

}