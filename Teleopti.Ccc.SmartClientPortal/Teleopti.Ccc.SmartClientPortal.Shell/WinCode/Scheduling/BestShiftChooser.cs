using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class BestShiftChooser
	{
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly Func<IWorkShiftFinderResultHolder> _allResults;

		public BestShiftChooser(IMatrixListFactory matrixListFactory, IEffectiveRestrictionCreator effectiveRestrictionCreator, Func<IWorkShiftFinderResultHolder> allResults)
		{
			_matrixListFactory = matrixListFactory;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_allResults = allResults;
		}

		public IEditableShift PrepareAndChooseBestShift(IScheduleDay schedulePart,
			ISchedulingOptions schedulingOptions,
			IWorkShiftFinderService finderService)
		{
			if (schedulePart == null)
				throw new ArgumentNullException("schedulePart");
			if (schedulingOptions == null)
				throw new ArgumentNullException("schedulingOptions");
			if (finderService == null)
				throw new ArgumentNullException("finderService");

			var scheduleDateOnlyPerson = schedulePart.DateOnlyAsPeriod.DateOnly;
			IPersonPeriod personPeriod = schedulePart.Person.Period(scheduleDateOnlyPerson);
			if (personPeriod != null)
			{
				//only fixed staff will be scheduled this way
				if (personPeriod.PersonContract.Contract.EmploymentType != EmploymentType.HourlyStaff)
					if (!schedulePart.IsScheduled())
					{
						DateTime schedulingTime = DateTime.Now;
						WorkShiftFinderServiceResult cache;
						using (PerformanceOutput.ForOperation("Finding the best shift"))
						{
							IScheduleMatrixPro matrix =
								_matrixListFactory.CreateMatrixListForSelection(schedulePart.Owner, new List<IScheduleDay> {schedulePart})[0];

							var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);
							cache = finderService.FindBestShift(schedulePart, schedulingOptions, matrix, effectiveRestriction);
						}
						var result = cache.FinderResult;
						_allResults().AddResults(new List<IWorkShiftFinderResult> {result}, schedulingTime);

						if (cache.ResultHolder == null)
							return null;

						result.Successful = true;
						return cache.ResultHolder.ShiftProjection.TheMainShift;
					}
			}
			return null;
		}
	}
}