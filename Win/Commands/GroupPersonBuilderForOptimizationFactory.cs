

using System.Collections.Generic;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Commands
{
	public interface IGroupPersonBuilderForOptimizationFactory
	{
		IGroupPersonBuilderForOptimization Create(ISchedulingOptions schedulingOptions);
	}

	public class GroupPersonBuilderForOptimizationFactory : IGroupPersonBuilderForOptimizationFactory
	{
		private readonly IGroupScheduleGroupPageDataProvider _groupScheduleGroupPageDataProvider;
		private readonly IGroupPagePerDateHolder _groupPagePerDateHolder;
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private readonly IGroupPageCreator _groupPageCreator;

		public GroupPersonBuilderForOptimizationFactory(IGroupScheduleGroupPageDataProvider groupScheduleGroupPageDataProvider,
			IGroupPagePerDateHolder groupPagePerDateHolder,
			ISchedulerStateHolder schedulerStateHolder,
			IGroupPageCreator groupPageCreator)
		{
			_groupScheduleGroupPageDataProvider = groupScheduleGroupPageDataProvider;
			_groupPagePerDateHolder = groupPagePerDateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_groupPageCreator = groupPageCreator;
		}

		public IGroupPersonBuilderForOptimization Create(ISchedulingOptions schedulingOptions)
		{

			if (_schedulerStateHolder.LoadedPeriod != null)
			{
				IList<DateOnly> dates =
					_schedulerStateHolder.LoadedPeriod.Value.ToDateOnlyPeriod(TeleoptiPrincipal.Current.Regional.TimeZone).
						DayCollection();
				_groupPagePerDateHolder.GroupPersonGroupPagePerDate =
					_groupPageCreator.CreateGroupPagePerDate(dates,
					                                         _groupScheduleGroupPageDataProvider,
					                                         schedulingOptions.GroupOnGroupPageForTeamBlockPer,
					                                         true);
			}

			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization =
				new GroupPersonBuilderForOptimization(_schedulerStateHolder.SchedulingResultState,
													  _groupPagePerDateHolder, new GroupCreator());
			return groupPersonBuilderForOptimization;
		}
	}
}