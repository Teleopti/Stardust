using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IBlockProvider
	{
		IList<IIntradayBlock> Provide(IList<IScheduleMatrixPro> selectedPersonMatrixList);
	}

	public class BlockProvider : IBlockProvider
	{
		private readonly ISchedulingOptions _schedulingOptions;
		private readonly IDynamicBlockFinder _dynamicBlockFinder;
		private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

		public BlockProvider(ISchedulingOptions schedulingOptions,
		                     IDynamicBlockFinder dynamicBlockFinder,
		                     IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
		{
			_schedulingOptions = schedulingOptions;
			_dynamicBlockFinder = dynamicBlockFinder;
			_groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
		}

		public IList<IIntradayBlock> Provide(IList<IScheduleMatrixPro> selectedPersonMatrixList)
		{
			var blocks = new List<IIntradayBlock>();
			List<DateOnly> dayOff, effectiveDays, unLockedDays;

			var startDate = retrieveStartDate(_schedulingOptions.BlockFinderTypeForAdvanceScheduling, selectedPersonMatrixList,
			                                  out dayOff, out effectiveDays, out unLockedDays);

			var selectedPerson = selectedPersonMatrixList.Select(scheduleMatrixPro => scheduleMatrixPro.Person).Distinct().ToList();

			while (startDate != DateOnly.MinValue)
			{
				var allGroupPersonListOnStartDate = new HashSet<IGroupPerson>();

				foreach (var person in selectedPerson)
				{
					allGroupPersonListOnStartDate.Add(_groupPersonBuilderForOptimization.BuildGroupPerson(person, startDate));
				}

				foreach (var fullGroupPerson in allGroupPersonListOnStartDate.GetRandom(allGroupPersonListOnStartDate.Count, true))
				{
					var dateOnlyList = _dynamicBlockFinder.ExtractBlockDays(startDate, fullGroupPerson);
					if (dateOnlyList.Count == 0) continue;
					if (blocks.Any(x => x.BlockDays.Equals(dateOnlyList))) continue;
					blocks.Add(new IntradayBlock {BlockDays = dateOnlyList});
				}
				startDate = getNextDate(startDate, effectiveDays);
			}
			return blocks;
		}

		private static DateOnly getNextDate(DateOnly dateOnly, List<DateOnly> effectiveDays)
		{
			dateOnly = dateOnly.AddDays(1);
			return effectiveDays.Contains(dateOnly) ? dateOnly : DateOnly.MinValue;
		}

		private static DateOnly retrieveStartDate(BlockFinderType blockType, IList<IScheduleMatrixPro> matrixList, out  List<DateOnly> dayOff, out  List<DateOnly> effectiveDays, out  List<DateOnly> unLockedDays)
		{
			var startDate = DateOnly.MinValue;
			dayOff = new List<DateOnly>();
			effectiveDays = new List<DateOnly>();
			unLockedDays = new List<DateOnly>();

			if (matrixList != null)
			{
				for (var i = 0; i < matrixList.Count; i++)
				{
					int i1 = i;
					var openMatrixList = matrixList.Where(x => x.Person.Equals(matrixList[i1].Person));
					foreach (var scheduleMatrixPro in openMatrixList)
					{
						foreach (var scheduleDayPro in scheduleMatrixPro.EffectivePeriodDays.OrderBy(x => x.Day))
						{
							var daySignificantPart = scheduleDayPro.DaySchedulePart().SignificantPart();

							if (startDate == DateOnly.MinValue &&
							    (daySignificantPart != SchedulePartView.DayOff &&
							     daySignificantPart != SchedulePartView.ContractDayOff &&
							     daySignificantPart != SchedulePartView.FullDayAbsence))
							{
								startDate = scheduleDayPro.Day;
							}

							if (daySignificantPart == SchedulePartView.DayOff)
								dayOff.Add(scheduleDayPro.Day);

							effectiveDays.Add(scheduleDayPro.Day);

							if (scheduleMatrixPro.UnlockedDays.Contains(scheduleDayPro))
								unLockedDays.Add(scheduleDayPro.Day);
						}
					}
				}
			}

			if (blockType == BlockFinderType.SingleDay)
				return unLockedDays.FirstOrDefault();

			return startDate;
		}
	}
}