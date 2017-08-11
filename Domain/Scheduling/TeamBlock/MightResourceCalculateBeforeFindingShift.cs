using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class MightResourceCalculateBeforeFindingShift : IResourceCalculateDelayer
	{
		private readonly ResourceCalculateDelayer _resourceCalculateDelayer;
		private lastSuccessful _lastSuccessful;

		public MightResourceCalculateBeforeFindingShift(ResourceCalculateDelayer resourceCalculateDelayer)
		{
			_resourceCalculateDelayer = resourceCalculateDelayer;
		}

		public void Execute(IPerson person)
		{
			if (_lastSuccessful == null)
				return;

			_resourceCalculateDelayer.CalculateIfNeeded(_lastSuccessful.ScheduleDateOnly, _lastSuccessful.WorkShiftProjectionPeriod, _lastSuccessful.DoIntraIntervalCalculation);
			_lastSuccessful = null;
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