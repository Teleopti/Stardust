using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class BestShiftChooser
	{
		private readonly MatrixListFactory _matrixListFactory;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;

		public BestShiftChooser(MatrixListFactory matrixListFactory, IEffectiveRestrictionCreator effectiveRestrictionCreator)
		{
			_matrixListFactory = matrixListFactory;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
		}

		public IEditableShift PrepareAndChooseBestShift(IScheduleDay schedulePart,
			SchedulingOptions schedulingOptions,
			WorkShiftFinderService finderService)
		{
			if (schedulePart == null)
				throw new ArgumentNullException(nameof(schedulePart));
			if (schedulingOptions == null)
				throw new ArgumentNullException(nameof(schedulingOptions));
			if (finderService == null)
				throw new ArgumentNullException(nameof(finderService));

			var scheduleDateOnlyPerson = schedulePart.DateOnlyAsPeriod.DateOnly;
			IPersonPeriod personPeriod = schedulePart.Person.Period(scheduleDateOnlyPerson);
			if (personPeriod != null)
			{
				//only fixed staff will be scheduled this way
				if (personPeriod.PersonContract.Contract.EmploymentType != EmploymentType.HourlyStaff)
					if (!schedulePart.IsScheduled())
					{
						IWorkShiftCalculationResultHolder cache;
						using (PerformanceOutput.ForOperation("Finding the best shift"))
						{
							IScheduleMatrixPro matrix =
								_matrixListFactory.CreateMatrixListForSelection(schedulePart.Owner, new List<IScheduleDay> {schedulePart}).First();

							var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);
							cache = finderService.FindBestShift(schedulePart, schedulingOptions, matrix, effectiveRestriction);
						}

						return cache?.ShiftProjection.TheMainShift;
					}
			}
			return null;
		}
	}
}