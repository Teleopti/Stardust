﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class NotOverwriteLayerRule : INewBusinessRule
	{
		private string friendlyName => Resources.BusinessRuleOverlappingFriendlyName3;

		public bool IsMandatory => false;

		public bool HaltModify { get; set; } = true;

		public bool Configurable => true;

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

		public string Description => Resources.DescriptionOfNotOverwriteLayerRule;

		private IEnumerable<IBusinessRuleResponse> checkDay(IDictionary<IPerson, IScheduleRange> rangeClones, IScheduleDay scheduleDay)
		{
			var responsList = new List<IBusinessRuleResponse>();
			var person = scheduleDay.Person;
			var dateToCheck = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var oldResponses = rangeClones[person].BusinessRuleResponseInternalCollection;
			var removeRuleResponse = createResponse(person, dateToCheck, "remove");
			while (oldResponses.Contains(removeRuleResponse))
			{
				oldResponses.Remove(removeRuleResponse);
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

			var personAssignment = scheduleDay.PersonAssignment();
			var layers = (personAssignment?.MainActivities() ?? Enumerable.Empty<IMainShiftLayer>()).ToArray();
			var meetings = scheduleDay.PersonMeetingCollection().ToArray();
			var personalActivities = (personAssignment?.PersonalActivities() ?? Enumerable.Empty<IPersonalShiftLayer>()).ToArray();
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

					if (!layerWithHighPriority.Period.Intersect(layerWithLowPriority.Period))
						continue;

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

				foreach (var personMeeting in meetings)
				{
					if (!personMeeting.Period.Intersect(layerWithLowPriority.Period)) continue;

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

				foreach (var personalShiftLayer in personalShiftLayers)
				{
					if (!personalShiftLayer.Period.Intersect(layerWithLowPriority.Period)) continue;

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

			return result;
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message)
		{
			var dop = new DateOnlyPeriod(dateOnly, dateOnly);
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			return new BusinessRuleResponse(typeof(NotOverwriteLayerRule), message, HaltModify, IsMandatory, period, person, dop,
				friendlyName) {Overridden = !HaltModify};
		}

		private IBusinessRuleResponse createResponse(IPerson person,DateOnly dateOnly, OverlappingLayers overlappingLayers)
		{
			var dop = dateOnly.ToDateOnlyPeriod();
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var errorMessage = createErrorMessage(person, overlappingLayers);
			IBusinessRuleResponse response = new BusinessRuleResponse(
				typeof(NotOverwriteLayerRule),
				errorMessage,
				HaltModify,
				IsMandatory,
				period,
				person,
				dop,
				friendlyName) {Overridden = !HaltModify};
			return response;
		}

		private string createErrorMessage(IPerson person, OverlappingLayers overlappingLayers)
		{
			var errorMessage = Resources.BusinessRuleOverlappingErrorMessage3;
			var currentUiCulture = Thread.CurrentThread.CurrentCulture;
			var loggedOnTimezone = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;

			var layerBelowTimePeriod = overlappingLayers.LayerBelowPeriod.TimePeriod(loggedOnTimezone);
			var layerAboveTimePeriod = overlappingLayers.LayerAbovePeriod.TimePeriod(loggedOnTimezone);

			var ret = string.Format(currentUiCulture,
				errorMessage,
				overlappingLayers.LayerBelowName,
				layerBelowTimePeriod.ToShortTimeString(currentUiCulture),
				overlappingLayers.LayerAboveName,
				layerAboveTimePeriod.ToShortTimeString(currentUiCulture));
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