using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class ShiftTradeRequestStatusChecker : IBatchShiftTradeRequestStatusChecker
    {
        private readonly ICurrentScenario _scenarioRepository;
        private readonly IScheduleStorage _scheduleStorage;
        private readonly IPersonRequestCheckAuthorization _authorization;
        private IList<IPerson> _persons;
        private DateTimePeriod? _period;
        private IScheduleDictionary _scheduleDictionary;
        private bool _isInBatchMode;

		  public ShiftTradeRequestStatusChecker(ICurrentScenario scenarioRepository, IScheduleStorage scheduleStorage, IPersonRequestCheckAuthorization authorization)
        {
            _scenarioRepository = scenarioRepository;
            _scheduleStorage = scheduleStorage;
            _authorization = authorization;
        }

        public void StartBatch(IEnumerable<IPersonRequest> personRequests)
        {
            IList<IShiftTradeRequest> shiftTradeRequests = ExtractPeriodAndPersons(personRequests);
            if (shiftTradeRequests.Count == 0 || !_period.HasValue || _persons.Count == 0) return;

            LoadScheduleDictionary(_period.Value);
            _isInBatchMode = true;
        }

        private void LoadScheduleDictionary(DateTimePeriod period)
        {
	        var longPeriod = new DateOnlyPeriod(new DateOnly(period.StartDateTime.AddDays(-1)),
	                                            new DateOnly(period.EndDateTime.AddDays(1)));
	        _scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
		        _persons,
		        new ScheduleDictionaryLoadOptions(false, false),
		        longPeriod, _scenarioRepository.Current());
        }

        public void EndBatch()
        {
            _period = null;
            _persons.Clear();
            _scheduleDictionary = null;
            _isInBatchMode = false;
        }

        private IList<IShiftTradeRequest> ExtractPeriodAndPersons(IEnumerable<IPersonRequest> personRequests)
        {
            ShiftTradeRequestPersonExtractor shiftTradeRequestPersonExtractor = new ShiftTradeRequestPersonExtractor();
            IList<IShiftTradeRequest> shiftTradeRequests = new List<IShiftTradeRequest>();
            foreach (IPersonRequest personRequest in personRequests)
            {
                if (!personRequest.IsPending) continue;
                IShiftTradeRequest shiftTradeRequest = personRequest.Request as IShiftTradeRequest;
                if (shiftTradeRequest == null) continue;

                shiftTradeRequestPersonExtractor.ExtractPersons(shiftTradeRequest);
                shiftTradeRequests.Add(shiftTradeRequest);

                if (_period.HasValue)
                {
                    _period = _period.Value.MaximumPeriod(personRequest.Request.Period);
                }
                else
                {
                    _period = personRequest.Request.Period;
                }
            }
            _persons = shiftTradeRequestPersonExtractor.Persons;
            return shiftTradeRequests;
        }

        public void Check(IShiftTradeRequest shiftTradeRequest)
        {
            if (shiftTradeRequest == null) return;
            if (!_isInBatchMode)
            {
                IList<IShiftTradeRequest> shiftTradeRequests =
                    ExtractPeriodAndPersons(new List<IPersonRequest> {(IPersonRequest) shiftTradeRequest.Parent});
                if (shiftTradeRequests.Count == 0 || !_period.HasValue || _persons.Count == 0) return;
								LoadScheduleDictionary(_period.Value);
            }
            VerifyShiftTradeIsUnchanged(_scheduleDictionary, shiftTradeRequest, _authorization);
        }

        internal static bool VerifyShiftTradeIsUnchanged(IScheduleDictionary scheduleDictionary, IShiftTradeRequest shiftTradeRequest, IPersonRequestCheckAuthorization authorization)
        {
            bool scheduleUnchanged = true;
            foreach (IShiftTradeSwapDetail shiftTradeSwapDetail in shiftTradeRequest.ShiftTradeSwapDetails)
            {
                IScheduleRange rangeFrom = scheduleDictionary[shiftTradeSwapDetail.PersonFrom];
                IScheduleRange rangeTo = scheduleDictionary[shiftTradeSwapDetail.PersonTo];

                IScheduleDay partFrom = rangeFrom.ScheduledDay(shiftTradeSwapDetail.DateFrom);
                IScheduleDay partTo = rangeTo.ScheduledDay(shiftTradeSwapDetail.DateTo);
                long checksumFrom = new ShiftTradeChecksumCalculator(partFrom).CalculateChecksum();
                long checksumTo = new ShiftTradeChecksumCalculator(partTo).CalculateChecksum();

                shiftTradeSwapDetail.SchedulePartFrom = partFrom;
                shiftTradeSwapDetail.SchedulePartTo = partTo;

                if (shiftTradeSwapDetail.ChecksumFrom != checksumFrom ||
                    shiftTradeSwapDetail.ChecksumTo != checksumTo)
                {
                    shiftTradeRequest.Refer(authorization);
                    scheduleUnchanged = false;
                }
            }
            return scheduleUnchanged;
        }
    }
}