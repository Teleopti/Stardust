using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class NotOverwriteLayerRule : INewBusinessRule
	{
		private readonly string _errorMessage = string.Empty;
		private bool _haltModify = true;

		public string ErrorMessage
		{
			get { return _errorMessage; }
		}


		public bool IsMandatory
		{
			get { return false; }
		}

		public bool HaltModify
		{
			get { return _haltModify; }
			set { _haltModify = value; }
		}

		public bool ForDelete { get; set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
		{
			var responseList = new List<IBusinessRuleResponse>();
			foreach (var scheduleDay in scheduleDays)
			{
				responseList.AddRange(checkDay(rangeClones, scheduleDay));
			}

			return responseList;

			//var responseList = new List<IBusinessRuleResponse>();
			//var groupedByPerson = scheduleDays.GroupBy(s => s.Person);

			//foreach (var scheduleDay in groupedByPerson)
			//{
			//	if (!scheduleDay.Any()) continue;
			//	var period = new DateOnlyPeriod(scheduleDay.Min(s => s.DateOnlyAsPeriod.DateOnly).AddDays(-1),
			//		scheduleDay.Max(s => s.DateOnlyAsPeriod.DateOnly).AddDays(1));
			//	var schedules =
			//		rangeClones[scheduleDay.Key].ScheduledDayCollection(period)
			//			.ToDictionary(k => k.DateOnlyAsPeriod.DateOnly, s => s);
			//	responseList.AddRange(checkDay(period, schedules, rangeClones));
			//}

			//return responseList;
		}

		private IEnumerable<IBusinessRuleResponse> checkDay(IDictionary<IPerson, IScheduleRange> rangeClones, IScheduleDay scheduleDay)
		{
			var responsList = new List<IBusinessRuleResponse>();
			IPerson person = scheduleDay.Person;
			DateOnly dateToCheck = scheduleDay.DateOnlyAsPeriod.DateOnly;
			IScheduleRange currentSchedules = rangeClones[person];
			var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
			//foreach (var date in period.DayCollection())
			//{

			//	if (!schedules.ContainsKey(date)) continue;

			//	var scheduleDay = schedules[date];
			//	var person = scheduleDay.Person;
			//	IScheduleRange currentSchedules = rangeClones[person];
			//	var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
			while (oldResponses.Contains(createResponse(person, dateToCheck, "remove")))
			{
				oldResponses.Remove(createResponse(person, dateToCheck, "remove"));
			}

			if (!scheduleDay.HasProjection())
				return new List<IBusinessRuleResponse>();
				//	var personAssignment = scheduleDay.PersonAssignment();
				//	if (personAssignment == null) continue;
			var layers = scheduleDay.PersonAssignment().MainActivities().ToArray();
			//	var layers = scheduleDay.PersonAssignment().ShiftLayers.Where(l => l is IMainShiftLayer).ToArray();
			//	if(layers.Length == 0) continue;

			var meetings = scheduleDay.PersonMeetingCollection().ToArray();
			var personalActivities = scheduleDay.PersonAssignment().PersonalActivities().ToArray();
			var overlappingLayersList = getOverlappingLayerses(layers, meetings, personalActivities);
			foreach (var overlappingLayerse in overlappingLayersList)
			{
				var businessRuleResponse = createResponse(person, dateToCheck, overlappingLayerse);
				responsList.Add(businessRuleResponse);
				oldResponses.Add(businessRuleResponse);
			}
			//var responses =
			//	getOverlappingLayerses(layers, meetings, personalActivities)
			//		.Select(ls => createResponse(scheduleDay.Person, dateToCheck, ls));

			//	foreach (var businessRuleResponse in responses)
			//	{
			//		responsList.Add(businessRuleResponse);
			//		oldResponses.Add(businessRuleResponse);
			//	}

			//}

			return responsList;
		}
		

		private IList<OverlappingLayers> getOverlappingLayerses(IMainShiftLayer[] layers, IPersonMeeting[] meetings, IPersonalShiftLayer[] personalShiftLayers )
		{
			var result = new List<OverlappingLayers>();
			if (layers.Length == 0) return result;

			for(var i = 0;i < layers.Length;i++)
			{
				var layerWithLowPriority = layers[i];
				if (layerWithLowPriority.Payload.AllowOverwrite) continue;

				for (var j = i + 1; j < layers.Length; j++)
				{
					var layerWithHighPriority = layers[j];
					if(layerWithHighPriority.Payload.Equals(layerWithLowPriority.Payload))
						continue;

					if (layerWithHighPriority.Period.Intersect(layerWithLowPriority.Period))
					{
						result.Add(new OverlappingLayers
						{
							LayerBelowName = layerWithLowPriority.Payload.Name,
							LayerBelowPeriod = layerWithLowPriority.Period,
							LayerAboveName = layerWithHighPriority.Payload.Name,
							LayerAbovePeriod = layerWithHighPriority.Period
						});
					}
				}

				foreach (var personMeeting in meetings)
				{
					if (personMeeting.Period.Intersect(layerWithLowPriority.Period))
					{
						var overlappingLayerIssue = new OverlappingLayers
						{
							LayerBelowName = layerWithLowPriority.Payload.Name,
							LayerBelowPeriod = layerWithLowPriority.Period,
							LayerAboveName = personMeeting.ToLayer().Payload.Name,
							LayerAbovePeriod = personMeeting.Period
						};
						result.Add(overlappingLayerIssue);
					}
				}

				//result.AddRange(from meeting in meetings
				//	where meeting.Period.Intersect(layerBelow.Period)
				//	select new OverlappingLayers
				//	{
				//		LayerBelowName = layerBelow.Payload.Name,
				//		LayerBelowPeriod = layerBelow.Period,
				//		LayerAboveName = meeting.ToLayer().Payload.Name,
				//		LayerAbovePeriod = meeting.Period
				//	});

				foreach (var personalShiftLayer in personalShiftLayers)
				{
					if (personalShiftLayer.Period.Intersect(layerWithLowPriority.Period))
					{
						var overlappingLayerIssue = new OverlappingLayers
						{
							LayerBelowName = layerWithLowPriority.Payload.Name,
							LayerBelowPeriod = layerWithLowPriority.Period,
							LayerAboveName = personalShiftLayer.Payload.Name,
							LayerAbovePeriod = personalShiftLayer.Period
						};
						result.Add(overlappingLayerIssue);
					}
				}

				//result.AddRange(from personalShiftLayer in personalShiftLayers
				//	where layerBelow.Period.Intersect(personalShiftLayer.Period)
				//	select new OverlappingLayers
				//	{
				//		LayerBelowName = layerBelow.Payload.Name,
				//		LayerBelowPeriod = layerBelow.Period,
				//		LayerAboveName = personalShiftLayer.Payload.Name,
				//		LayerAbovePeriod = personalShiftLayer.Period
				//	});
			}

			return result;
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message)
		{
			var dop = new DateOnlyPeriod(dateOnly, dateOnly);
			DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			IBusinessRuleResponse response = new BusinessRuleResponse(typeof(NotOverwriteLayerRule), message, _haltModify, IsMandatory, period, person, dop) { Overridden = !_haltModify };
			return response;
		}

		private IBusinessRuleResponse createResponse(IPerson person,DateOnly dateOnly, OverlappingLayers overlappingLayers)
		{
			var dop = new DateOnlyPeriod(dateOnly,dateOnly);
			DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			string errorMessage = createErrorMessage(overlappingLayers);
			IBusinessRuleResponse response = new BusinessRuleResponse(
				typeof(NotOverwriteLayerRule),
				errorMessage,
				_haltModify,
				IsMandatory,
				period,
				person,
				dop) {Overridden = !_haltModify};			
			return response;
		}

		private static string createErrorMessage(OverlappingLayers overlappingLayers)
		{
			var loggedOnCulture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			var loggedOnTimezone = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;

			var layerBelowTimePeriod = overlappingLayers.LayerBelowPeriod.TimePeriod(loggedOnTimezone);
			var layerAboveTimePeriod = overlappingLayers.LayerAbovePeriod.TimePeriod(loggedOnTimezone);

			string ret = string.Format(loggedOnCulture,
									   Resources.BusinessRuleOverlappingErrorMessage3,
									   overlappingLayers.LayerBelowName,
									   layerBelowTimePeriod.ToShortTimeString(loggedOnCulture),
									   overlappingLayers.LayerAboveName,
									   layerAboveTimePeriod.ToShortTimeString(loggedOnCulture));
			return ret;
		}


		private class OverlappingLayers
		{
			public string LayerBelowName { get; set; }
			public DateTimePeriod LayerBelowPeriod { get; set; }
			public string LayerAboveName { get; set; }
			public DateTimePeriod LayerAbovePeriod { get; set; }
		}

	}
}
