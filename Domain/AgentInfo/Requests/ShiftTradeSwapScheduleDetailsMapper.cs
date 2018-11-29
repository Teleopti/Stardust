using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class ShiftTradeSwapScheduleDetailsMapper : IShiftTradeSwapScheduleDetailsMapper
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _scenarioRepository;

		public ShiftTradeSwapScheduleDetailsMapper(IScheduleStorage scheduleStorage, ICurrentScenario scenarioRepository)
		{
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
		}

		public void Map(IShiftTradeRequest shiftTradeRequest)
		{
			var scheduleDictionary = loadScheduleDictionary(shiftTradeRequest.Period, new List<IPerson> { shiftTradeRequest.PersonFrom, shiftTradeRequest.PersonTo });

			foreach (var shiftTradeSwapDetail in shiftTradeRequest.ShiftTradeSwapDetails)
			{
				updateShiftTradeSwapScheduleInformation(scheduleDictionary, shiftTradeSwapDetail);
			}
		}
		
		private IScheduleDictionary loadScheduleDictionary(DateTimePeriod period, IList<IPerson> people)
		{
			var longPeriod = new DateOnlyPeriod(new DateOnly(period.StartDateTime.AddDays(-1)), new DateOnly(period.EndDateTime.AddDays(1)));
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				people,
				new ScheduleDictionaryLoadOptions(false, false),
				longPeriod, _scenarioRepository.Current());

			return scheduleDictionary;
		}

		private static void updateShiftTradeSwapScheduleInformation(IScheduleDictionary scheduleDictionary, IShiftTradeSwapDetail shiftTradeSwapDetail)
		{
			var rangeFrom = scheduleDictionary[shiftTradeSwapDetail.PersonFrom];
			var rangeTo = scheduleDictionary[shiftTradeSwapDetail.PersonTo];

			var partFrom = rangeFrom.ScheduledDay(shiftTradeSwapDetail.DateFrom);
			var partTo = rangeTo.ScheduledDay(shiftTradeSwapDetail.DateTo);

			shiftTradeSwapDetail.SchedulePartFrom = partFrom;
			shiftTradeSwapDetail.SchedulePartTo = partTo;
		}

	}
}