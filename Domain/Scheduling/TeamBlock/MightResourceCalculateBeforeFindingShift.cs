using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class MightResourceCalculateBeforeFindingShift : IResourceCalculateDelayer
	{
		private readonly ResourceCalculateDelayer _resourceCalculateDelayer;
		private readonly SchedulingResourceCalculationLimiter _schedulingResourceCalculationLimiter;
		private readonly SkillGroups _skillGroups;
		private lastSuccessful _lastSuccessful;
		private readonly Random _random = new Random();

		public MightResourceCalculateBeforeFindingShift(ResourceCalculateDelayer resourceCalculateDelayer, 
									SchedulingResourceCalculationLimiter schedulingResourceCalculationLimiter,
									SkillGroups skillGroups)
		{
			_resourceCalculateDelayer = resourceCalculateDelayer;
			_schedulingResourceCalculationLimiter = schedulingResourceCalculationLimiter;
			_skillGroups = skillGroups;
		}

		public void Execute(IPerson person)
		{
			if (_lastSuccessful == null)
				return;

			if (person==null || _schedulingResourceCalculationLimiter.Limit(_skillGroups.NumberOfAgentsInSameSkillGroup(person)).Value > _random.NextDouble())
			{
				_resourceCalculateDelayer.CalculateIfNeeded(_lastSuccessful.ScheduleDateOnly, _lastSuccessful.WorkShiftProjectionPeriod, _lastSuccessful.DoIntraIntervalCalculation);
				_lastSuccessful = null;
			}
		}

		public void CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod,bool doIntraIntervalCalculation)
		{
			_lastSuccessful = new lastSuccessful(scheduleDateOnly, workShiftProjectionPeriod, doIntraIntervalCalculation);
		}

		private class lastSuccessful
		{
			public DateTimePeriod? WorkShiftProjectionPeriod { get; }
			public bool DoIntraIntervalCalculation { get; }
			public DateOnly ScheduleDateOnly { get; }

			public lastSuccessful(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod, bool doIntraIntervalCalculation)
			{
				WorkShiftProjectionPeriod = workShiftProjectionPeriod;
				DoIntraIntervalCalculation = doIntraIntervalCalculation;
				ScheduleDateOnly = scheduleDateOnly;
			}
		}
	}
}