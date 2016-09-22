using System;
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
		}

		public string FriendlyName
		{
			get { return Resources.BusinessRuleOverlappingFriendlyName3; }
		}

		private IEnumerable<IBusinessRuleResponse> checkDay(IDictionary<IPerson, IScheduleRange> rangeClones, IScheduleDay scheduleDay)
		{
			var responsList = new List<IBusinessRuleResponse>();
			IPerson person = scheduleDay.Person;
			DateOnly dateToCheck = scheduleDay.DateOnlyAsPeriod.DateOnly;
			IScheduleRange currentSchedules = rangeClones[person];
			var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;			
			while (oldResponses.Contains(createResponse(person, dateToCheck, "remove")))
			{
				oldResponses.Remove(createResponse(person, dateToCheck, "remove"));
			}

			var overlappingLayersList = GetOverlappingLayerses(rangeClones, scheduleDay);
			foreach (var overlappingLayerse in overlappingLayersList)
			{
				var businessRuleResponse = createResponse(person, dateToCheck, overlappingLayerse);
				responsList.Add(businessRuleResponse);
				oldResponses.Add(businessRuleResponse);
			}

			return responsList;
		}


		public IList<OverlappingLayers> GetOverlappingLayerses(IDictionary<IPerson, IScheduleRange> rangeClones,
			IScheduleDay scheduleDay)
		{
			if(!scheduleDay.HasProjection())
				return new List<OverlappingLayers>();

			var layers = scheduleDay.PersonAssignment(true).MainActivities().ToArray();
			var meetings = scheduleDay.PersonMeetingCollection().ToArray();
			var personalActivities = scheduleDay.PersonAssignment().PersonalActivities().ToArray();
			return getOverlappingLayerses(layers,meetings,personalActivities);
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
							LayerBelowId = layerWithLowPriority.Id.GetValueOrDefault(),
							LayerBelowName = layerWithLowPriority.Payload.Name,
							LayerBelowPeriod = layerWithLowPriority.Period,
							LayerAboveId = layerWithHighPriority.Id.GetValueOrDefault(),
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
							LayerBelowId = layerWithLowPriority.Id.GetValueOrDefault(),
							LayerBelowName = layerWithLowPriority.Payload.Name,
							LayerBelowPeriod = layerWithLowPriority.Period,
							LayerAboveId = personMeeting.Id.GetValueOrDefault(),
							LayerAboveName = personMeeting.ToLayer().Payload.Name,
							LayerAbovePeriod = personMeeting.Period
						};
						result.Add(overlappingLayerIssue);
					}
				}

				foreach (var personalShiftLayer in personalShiftLayers)
				{
					if (personalShiftLayer.Period.Intersect(layerWithLowPriority.Period))
					{
						var overlappingLayerIssue = new OverlappingLayers
						{
							LayerBelowId = layerWithLowPriority.Id.GetValueOrDefault(),
							LayerBelowName = layerWithLowPriority.Payload.Name,
							LayerBelowPeriod = layerWithLowPriority.Period,
							LayerAboveId = personalShiftLayer.Id.GetValueOrDefault(),
							LayerAboveName = personalShiftLayer.Payload.Name,
							LayerAbovePeriod = personalShiftLayer.Period
						};
						result.Add(overlappingLayerIssue);
					}
				}
			}

			return result;
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message)
		{
			var dop = new DateOnlyPeriod(dateOnly, dateOnly);
			DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			IBusinessRuleResponse response = new BusinessRuleResponse(typeof(NotOverwriteLayerRule), message, _haltModify, IsMandatory, period, person, dop, FriendlyName) { Overridden = !_haltModify };
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
				dop,
				FriendlyName) {Overridden = !_haltModify};			
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


		public class OverlappingLayers
		{
			public Guid LayerBelowId { get; set; }
			public string LayerBelowName { get; set; }
			public DateTimePeriod LayerBelowPeriod { get; set; }
			public Guid LayerAboveId { get; set; }
			public string LayerAboveName { get; set; }
			public DateTimePeriod LayerAbovePeriod { get; set; }
		}

	}
}
