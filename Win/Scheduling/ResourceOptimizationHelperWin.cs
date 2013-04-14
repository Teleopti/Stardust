﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	/// <summary>
	/// HelperClass for calculating resources
	/// </summary>
	/// <remarks>
	/// Created from OptimizerHelper
	/// I think it should be refact. and moved to Domain
	/// </remarks>
	public class ResourceOptimizationHelperWin : ResourceOptimizationHelper, IResourceOptimizationHelperWin
	{
		private readonly ISchedulerStateHolder _stateHolder;

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceOptimizationHelperWin"/> class.
		/// </summary>
		/// <param name="stateHolder">The state holder.</param>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2008-05-27
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ResourceOptimizationHelperWin(ISchedulerStateHolder stateHolder, ISingleSkillDictionary singleSkillDictionary)
			: base(stateHolder.SchedulingResultState, new OccupiedSeatCalculator(new SkillVisualLayerCollectionDictionaryCreator(), new SeatImpactOnPeriodForProjection()), new NonBlendSkillCalculator(new NonBlendSkillImpactOnPeriodForProjection()), singleSkillDictionary, new SingleSkillMaxSeatCalculator())
		{
			_stateHolder = stateHolder;
		}

		public void ResourceCalculateAllDays(DoWorkEventArgs e, BackgroundWorker backgroundWorker, bool useOccupancyAdjustment)
		{
            resourceCalculateDays(e, backgroundWorker, useOccupancyAdjustment, _stateHolder.ConsiderShortBreaks, _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection());
		}

		private void prepareAndCalculateDate(DateOnly date, bool useOccupancyAdjustment, bool considerShortBreaks, ToolStripProgressBar progressBar)
		{

			using (PerformanceOutput.ForOperation("PrepareAndCalculateDate " + date.ToShortDateString(CultureInfo.CurrentCulture)))
			{
				ResourceCalculateDate(date, useOccupancyAdjustment, considerShortBreaks);
			}

			if (progressBar != null)
				progressBar.PerformStep();
		}

		public void ResourceCalculateMarkedDays(DoWorkEventArgs e, BackgroundWorker backgroundWorker, bool considerShortBreaks, bool useOccupancyAdjustment)
		{
			resourceCalculateDays(e, backgroundWorker, useOccupancyAdjustment, considerShortBreaks, _stateHolder.DaysToRecalculate);
			_stateHolder.ClearDaysToRecalculate();
		}

		private void resourceCalculateDays(DoWorkEventArgs e, BackgroundWorker backgroundWorker, bool useOccupancyAdjustment, bool considerShortBreaks, IEnumerable<DateOnly> dates)
		{
			var numberOfDays = dates.Count();
			if (numberOfDays == 0) return;

			for (int index = 0; index < numberOfDays; index++)
			{
				DateOnly date = dates.ElementAt(index);
				prepareAndCalculateDate(date, useOccupancyAdjustment, considerShortBreaks, null);
				if (backgroundWorker != null)
				{
					if (backgroundWorker.CancellationPending)
					{
						e.Cancel = true;
						return;
					}
					backgroundWorker.ReportProgress(1);
				}
			}
		}
	}
}
