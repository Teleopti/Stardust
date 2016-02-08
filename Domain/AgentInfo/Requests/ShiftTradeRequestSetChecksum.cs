using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
    public class ShiftTradeRequestSetChecksum : IShiftTradeRequestSetChecksum
    {
	    private readonly ICurrentScenario _scenarioRepository;
	    private readonly IScheduleStorage _scheduleStorage;
        private IScheduleDictionary _scheduleDictionary;

        public ShiftTradeRequestSetChecksum(ICurrentScenario scenarioRepository, IScheduleStorage scheduleStorage)
        {
	        _scenarioRepository = scenarioRepository;
	        _scheduleStorage = scheduleStorage;
        }

        public void SetChecksum(IRequest request)
        {
            var defaultScenario = _scenarioRepository.Current();
            var shiftTradeRequest = request as IShiftTradeRequest;
            if (shiftTradeRequest == null) 
                return;

            var shiftTradeRequestPersonExtractor = new ShiftTradeRequestPersonExtractor();
            shiftTradeRequestPersonExtractor.ExtractPersons(shiftTradeRequest);
            
            var period =
                shiftTradeRequest.Period.ToDateOnlyPeriod(
                    shiftTradeRequest.PersonFrom.PermissionInformation.DefaultTimeZone());
	        var longPeriod = period.Inflate(1);
            _scheduleDictionary =
                _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
                    shiftTradeRequestPersonExtractor.Persons,
                    new ScheduleDictionaryLoadOptions(false, false),
                    longPeriod,
                    defaultScenario);
            SetChecksumToShiftTradeRequest(shiftTradeRequest);
        }

        private void SetChecksumToShiftTradeRequest(IShiftTradeRequest shiftTradeRequest)
        {
            foreach (var shiftTradeSwapDetail in shiftTradeRequest.ShiftTradeSwapDetails)
            {
                var rangeFrom = _scheduleDictionary[shiftTradeSwapDetail.PersonFrom];
                var rangeTo = _scheduleDictionary[shiftTradeSwapDetail.PersonTo];

                var partFrom = rangeFrom.ScheduledDay(shiftTradeSwapDetail.DateFrom);
                var partTo = rangeTo.ScheduledDay(shiftTradeSwapDetail.DateTo);
                var checksumFrom = new ShiftTradeChecksumCalculator(partFrom).CalculateChecksum();
                var checksumTo = new ShiftTradeChecksumCalculator(partTo).CalculateChecksum();

                shiftTradeSwapDetail.SchedulePartFrom = partFrom;
                shiftTradeSwapDetail.SchedulePartTo = partTo;
                shiftTradeSwapDetail.ChecksumFrom = checksumFrom;
                shiftTradeSwapDetail.ChecksumTo = checksumTo;
            }
        }
    }
}