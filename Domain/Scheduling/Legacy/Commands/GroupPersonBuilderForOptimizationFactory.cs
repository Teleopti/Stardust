using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class GroupPersonBuilderForOptimizationFactory : IGroupPersonBuilderForOptimizationFactory
	{
		private readonly IGroupScheduleGroupPageDataProvider _groupScheduleGroupPageDataProvider;
		private readonly Func<IGroupPagePerDateHolder> _groupPagePerDateHolder;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IGroupPageCreator _groupPageCreator;
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;

		public GroupPersonBuilderForOptimizationFactory(IGroupScheduleGroupPageDataProvider groupScheduleGroupPageDataProvider,
			Func<IGroupPagePerDateHolder> groupPagePerDateHolder,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IGroupPageCreator groupPageCreator,
			ICurrentTeleoptiPrincipal currentTeleoptiPrincipal)
		{
			_groupScheduleGroupPageDataProvider = groupScheduleGroupPageDataProvider;
			_groupPagePerDateHolder = groupPagePerDateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_groupPageCreator = groupPageCreator;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
		}

		public void Create(ISchedulingOptions schedulingOptions)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			if (schedulerStateHolder.LoadedPeriod != null)
			{
				IList<DateOnly> dates =
					schedulerStateHolder.LoadedPeriod.Value.ToDateOnlyPeriod(_currentTeleoptiPrincipal.Current().Regional.TimeZone).
						DayCollection();
				_groupPagePerDateHolder().GroupPersonGroupPagePerDate =
					_groupPageCreator.CreateGroupPagePerDate(dates,
						_groupScheduleGroupPageDataProvider,
						schedulingOptions.GroupOnGroupPageForTeamBlockPer,
						true);
			}
		}


	}


	public interface IGroupPersonBuilderWrapper
	{
		void SetSingleAgentTeam();
		IGroupPersonBuilderForOptimization ForOptimization();
		void Reset();
	}

	public class GroupPersonBuilderWrapper : IGroupPersonBuilderWrapper
	{
		private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private GroupPersonBuilderForOptimizationAndSingleAgentTeam _singleAgentTeam;

		public GroupPersonBuilderWrapper(IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
		{
			_groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
		}

		public void SetSingleAgentTeam()
		{
			_singleAgentTeam = new GroupPersonBuilderForOptimizationAndSingleAgentTeam();
		}

		public IGroupPersonBuilderForOptimization ForOptimization()
		{
			return _singleAgentTeam ?? _groupPersonBuilderForOptimization;
		}

		public void Reset()
		{
			_singleAgentTeam = null;
		}
	}
}