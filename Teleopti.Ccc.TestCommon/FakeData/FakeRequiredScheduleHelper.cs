using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeRequiredScheduleHelper : IRequiredScheduleHelper
	{
		public void ScheduleSelectedStudents(IList<IScheduleDay> allSelectedSchedules, IBackgroundWorkerWrapper backgroundWorker,
			ISchedulingOptions schedulingOptions)
		{
			throw new NotImplementedException();
		}

		public void RemoveShiftCategoryBackToLegalState(IList<IScheduleMatrixPro> matrixList, IBackgroundWorkerWrapper backgroundWorker,
			IOptimizationPreferences optimizationPreferences, ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod,
			IList<IScheduleMatrixPro> allMatrixes)
		{
			throw new NotImplementedException();
		}

		public void ScheduleSelectedPersonDays(IList<IScheduleDay> allSelectedSchedules, IList<IScheduleMatrixPro> matrixList, IList<IScheduleMatrixPro> matrixListAll,
			IBackgroundWorkerWrapper backgroundWorker, ISchedulingOptions schedulingOptions)
		{
			throw new NotImplementedException();
		}
	}
}  