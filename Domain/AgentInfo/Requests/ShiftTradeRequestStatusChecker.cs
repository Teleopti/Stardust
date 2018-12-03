using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

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
				longPeriod, _scenarioRepository.Current()
				, true);
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
				if (!(personRequest.Request is IShiftTradeRequest shiftTradeRequest)) continue;

				shiftTradeRequestPersonExtractor.ExtractPersons(shiftTradeRequest);
				shiftTradeRequests.Add(shiftTradeRequest);

				_period = _period?.MaximumPeriod(personRequest.Request.Period) ?? personRequest.Request.Period;
			}
			_persons = shiftTradeRequestPersonExtractor.Persons;
			return shiftTradeRequests;
		}

		public void Check(IShiftTradeRequest shiftTradeRequest)
		{
			if (shiftTradeRequest == null) return;
			if (!_isInBatchMode)
			{
				var shiftTradeRequests = ExtractPeriodAndPersons(new List<IPersonRequest> { (IPersonRequest)shiftTradeRequest.Parent });

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
				updateShiftTradeSwapScheduleInformation(scheduleDictionary, shiftTradeSwapDetail);

				long checksumFrom = new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartFrom).CalculateChecksum();
				long checksumTo = new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartTo).CalculateChecksum();

				if (shiftTradeSwapDetail.ChecksumFrom != checksumFrom ||
					shiftTradeSwapDetail.ChecksumTo != checksumTo)
				{
					shiftTradeRequest.Refer(authorization);
					scheduleUnchanged = false;
				}
			}
			return scheduleUnchanged;
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