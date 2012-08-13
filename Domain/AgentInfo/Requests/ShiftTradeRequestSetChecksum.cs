using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
    public class ShiftTradeRequestSetChecksum : IShiftTradeRequestSetChecksum
    {
        private readonly IScenario _defaultScenario;
        private readonly IScheduleRepository _scheduleRepository;
        private IScheduleDictionary _scheduleDictionary;

        public ShiftTradeRequestSetChecksum(IScenario defaultScenario, IScheduleRepository scheduleRepository)
        {
            _defaultScenario = defaultScenario;
            _scheduleRepository = scheduleRepository;
        }

        public void SetChecksum(IRequest request)
        {
            IShiftTradeRequest shiftTradeRequest = request as IShiftTradeRequest;
            if (shiftTradeRequest == null) return;

            ShiftTradeRequestPersonExtractor shiftTradeRequestPersonExtractor = new ShiftTradeRequestPersonExtractor();
            shiftTradeRequestPersonExtractor.ExtractPersons(shiftTradeRequest);
            _scheduleDictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(shiftTradeRequestPersonExtractor.Persons), new ScheduleDictionaryLoadOptions(false,false),
                                                          shiftTradeRequest.Period.ChangeEndTime(
                                                              TimeSpan.FromHours(25)).ChangeStartTime(TimeSpan.FromHours(-25)),
                                                          _defaultScenario);
            SetChecksumToShiftTradeRequest(shiftTradeRequest);
        }

        private void SetChecksumToShiftTradeRequest(IShiftTradeRequest shiftTradeRequest)
        {
            foreach (IShiftTradeSwapDetail shiftTradeSwapDetail in shiftTradeRequest.ShiftTradeSwapDetails)
            {
                IScheduleRange rangeFrom = _scheduleDictionary[shiftTradeSwapDetail.PersonFrom];
                IScheduleRange rangeTo = _scheduleDictionary[shiftTradeSwapDetail.PersonTo];

                IScheduleDay partFrom = rangeFrom.ScheduledDay(shiftTradeSwapDetail.DateFrom);
                IScheduleDay partTo = rangeTo.ScheduledDay(shiftTradeSwapDetail.DateTo);
                long checksumFrom = new ShiftTradeChecksumCalculator(partFrom).CalculateChecksum();
                long checksumTo = new ShiftTradeChecksumCalculator(partTo).CalculateChecksum();

                shiftTradeSwapDetail.SchedulePartFrom = partFrom;
                shiftTradeSwapDetail.SchedulePartTo = partTo;
                shiftTradeSwapDetail.ChecksumFrom = checksumFrom;
                shiftTradeSwapDetail.ChecksumTo = checksumTo;
            }
        }
    }
}