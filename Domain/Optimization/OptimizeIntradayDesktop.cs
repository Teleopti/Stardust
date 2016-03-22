﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IOptimizeIntradayDesktop
	{
		//TODO - remove unneeded params when toggle is gone
		void Optimize(IEnumerable<IScheduleDay> scheduleDays, IOptimizationPreferences optimizerPreferences,
			DateOnlyPeriod selectedPeriod, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			ISchedulingProgress backgroundWorker);	
	}

	public class OptimizeIntradayIslandsDesktop : IOptimizeIntradayDesktop
	{
		private readonly IIntradayOptimizationCommandHandler _intradayOptimizationCommandHandler;
		private readonly DesktopOptimizationContext _fillSchedulerStateHolder;
		private readonly Func<ISchedulerStateHolder> _currentSchedulerStateHolder;

		public OptimizeIntradayIslandsDesktop(IIntradayOptimizationCommandHandler intradayOptimizationCommandHandler, 
																		DesktopOptimizationContext fillSchedulerStateHolder,
																		Func<ISchedulerStateHolder> currentSchedulerStateHolder)
		{
			_intradayOptimizationCommandHandler = intradayOptimizationCommandHandler;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_currentSchedulerStateHolder = currentSchedulerStateHolder;
		}

		public void Optimize(IEnumerable<IScheduleDay> scheduleDays, IOptimizationPreferences optimizerPreferences, DateOnlyPeriod selectedPeriod,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, ISchedulingProgress backgroundWorker)
		{
			using (_fillSchedulerStateHolder.Add(_currentSchedulerStateHolder(), optimizerPreferences))
			{
				_intradayOptimizationCommandHandler.Execute(new IntradayOptimizationCommand
				{
					Period = selectedPeriod
				});
			}
		}	
	}

	public class OptimizeIntradayDesktop : IOptimizeIntradayDesktop
	{
		private readonly IntradayOptimizationContext _intradayOptimizationContext;
		private readonly IntradayOptimizationCallbackContext _intradayOptimizationCallbackContext;
		private readonly IntradayOptimizer2Creator _intradayOptimizer2Creator;
		private readonly IIntradayOptimizerContainer _intradayOptimizerContainer;


		public OptimizeIntradayDesktop(IntradayOptimizationContext intradayOptimizationContext,
										IntradayOptimizationCallbackContext intradayOptimizationCallbackContext,
										IntradayOptimizer2Creator intradayOptimizer2Creator,
										IIntradayOptimizerContainer intradayOptimizerContainer)
		{
			_intradayOptimizationContext = intradayOptimizationContext;
			_intradayOptimizationCallbackContext = intradayOptimizationCallbackContext;
			_intradayOptimizer2Creator = intradayOptimizer2Creator;
			_intradayOptimizerContainer = intradayOptimizerContainer;
		}

		public void Optimize(IEnumerable<IScheduleDay> scheduleDays, IOptimizationPreferences optimizerPreferences,
							DateOnlyPeriod selectedPeriod, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
							ISchedulingProgress backgroundWorker)
		{
			var optimizers = _intradayOptimizer2Creator
				.Create(selectedPeriod, scheduleDays, optimizerPreferences, dayOffOptimizationPreferenceProvider);

			using (_intradayOptimizationContext.Create(selectedPeriod))
			{
				using (_intradayOptimizationCallbackContext.Create(new intradayOptimizationCallback(backgroundWorker)))
				{
					_intradayOptimizerContainer.Execute(optimizers);
				}
			}
		}

		private class intradayOptimizationCallback : IIntradayOptimizationCallback
		{
			private readonly ISchedulingProgress _backgroundWorker;
			private int _counter;
			private readonly string output = Resources.OptimizingIntraday + Resources.Colon + "({0}){1} {2} {3}";

			public intradayOptimizationCallback(ISchedulingProgress backgroundWorker)
			{
				_backgroundWorker = backgroundWorker;
			}

			public void Optimizing(IntradayOptimizationCallbackInfo callbackInfo)
			{
				var e = new ResourceOptimizerProgressEventArgs(0, 0,
					string.Format(output, callbackInfo.NumberOfOptimizers, _counter++,
						callbackInfo.Agent.Name.ToString(NameOrderOption.FirstNameLastName),
						callbackInfo.WasSuccessful ? Resources.wasSuccessful : Resources.wasNotSuccessful));

				_backgroundWorker.ReportProgress(1, e);
			}

			public bool IsCancelled()
			{
				return _backgroundWorker.CancellationPending;
			}
		}
	}
}
