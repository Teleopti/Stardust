using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class ShiftTradeRequestStatusChecker : IBatchShiftTradeRequestStatusChecker
    {
        private readonly ICurrentScenario _scenarioRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IPersonRequestCheckAuthorization _authorization;
        private IList<IPerson> _persons;
        private DateTimePeriod? _period;
        private IScheduleDictionary _scheduleDictionary;
        private bool _isInBatchMode;

		  public ShiftTradeRequestStatusChecker(ICurrentScenario scenarioRepository, IScheduleRepository scheduleRepository, IPersonRequestCheckAuthorization authorization)
        {
            _scenarioRepository = scenarioRepository;
            _scheduleRepository = scheduleRepository;
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
            _scheduleDictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(_persons),new ScheduleDictionaryLoadOptions(false, false),
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

        protected internal static bool VerifyShiftTradeIsUnchanged(IScheduleDictionary scheduleDictionary, IShiftTradeRequest shiftTradeRequest, IPersonRequestCheckAuthorization authorization)
        {
            var scheduleUnchanged = true;
            foreach (var shiftTradeSwapDetail in shiftTradeRequest.ShiftTradeSwapDetails)
            {
                var rangeFrom = scheduleDictionary[shiftTradeSwapDetail.PersonFrom];
                var rangeTo = scheduleDictionary[shiftTradeSwapDetail.PersonTo];

                var partFrom = rangeFrom.ScheduledDay(shiftTradeSwapDetail.DateFrom);
                var partTo = rangeTo.ScheduledDay(shiftTradeSwapDetail.DateTo);
				
	            var checksumFrom = shiftTradeSwapDetail.ChecksumFrom;
				if (partFrom.PersonAssignmentCollection().Any())
		            checksumFrom = new ShiftTradeChecksumCalculator(partFrom).CalculateChecksum();

	            var checksumTo = shiftTradeSwapDetail.ChecksumTo;
				if (partTo.PersonAssignmentCollection().Any())
					checksumTo = new ShiftTradeChecksumCalculator(partTo).CalculateChecksum();

                shiftTradeSwapDetail.SchedulePartFrom = partFrom;
                shiftTradeSwapDetail.SchedulePartTo = partTo;

	            if (shiftTradeSwapDetail.ChecksumFrom == checksumFrom && shiftTradeSwapDetail.ChecksumTo == checksumTo)
		            continue;

	            shiftTradeRequest.Refer(authorization);
	            scheduleUnchanged = false;
            }
            return scheduleUnchanged;
        }
    }
}