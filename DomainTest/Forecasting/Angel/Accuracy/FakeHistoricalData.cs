using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy
{
	public class FakeHistoricalData : IHistoricalData
	{

		public TaskOwnerPeriod Fetch(IWorkload workload, DateOnlyPeriod period)
		{
			var dateOnly1 = period.StartDate;
			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(dateOnly1, workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });

			var validatedVolumeDay1 = new ValidatedVolumeDay(workload, dateOnly1)
			{
				ValidatedAverageAfterTaskTime = TimeSpan.FromSeconds(3),
				ValidatedAverageTaskTime = TimeSpan.FromSeconds(2),
				TaskOwner = workloadDay1,
				ValidatedTasks = 110
			};

			var dateOnly2 = period.EndDate;
			var workloadDay2 = new WorkloadDay();
			workloadDay2.Create(dateOnly2, workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });

			var validatedVolumeDay2 = new ValidatedVolumeDay(workload, dateOnly2)
			{
				ValidatedAverageAfterTaskTime = TimeSpan.FromSeconds(3),
				ValidatedAverageTaskTime = TimeSpan.FromSeconds(2),
				TaskOwner = workloadDay2,
				ValidatedTasks = 110
			};
			var taskOwnerDays = new ITaskOwner[] { validatedVolumeDay1, validatedVolumeDay2 };
			var taskOwnerPeriod = new TaskOwnerPeriod(new DateOnly(), taskOwnerDays, TaskOwnerPeriodType.Other);
			return taskOwnerPeriod;
		}
	}
}