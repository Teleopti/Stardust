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
			var groupedByPerson = scheduleDays.GroupBy(s => s.Person);

			foreach (var scheduleDay in groupedByPerson)
			{
				if (!scheduleDay.Any()) continue;
				var period = new DateOnlyPeriod(scheduleDay.Min(s => s.DateOnlyAsPeriod.DateOnly).AddDays(-1),
					scheduleDay.Max(s => s.DateOnlyAsPeriod.DateOnly).AddDays(1));
				var schedules =
					rangeClones[scheduleDay.Key].ScheduledDayCollection(period)
						.ToDictionary(k => k.DateOnlyAsPeriod.DateOnly, s => s);
				responseList.AddRange(checkDay(period, schedules));
			}

			return responseList;
		}

		private IEnumerable<IBusinessRuleResponse> checkDay(DateOnlyPeriod period,
			IDictionary<DateOnly, IScheduleDay> schedules)
		{
			var responsList = new List<IBusinessRuleResponse>();

			foreach (var date in period.DayCollection())
			{
				if (!schedules.ContainsKey(date)) continue;

				var scheduleDay = schedules[date];
				var personAssignment = scheduleDay.PersonAssignment();

				if (personAssignment == null) continue;

				var layers = scheduleDay.PersonAssignment().ShiftLayers.Where(l => l is IMainShiftLayer).ToArray();
				if(layers.Length == 0) continue;

				var meetings = scheduleDay.PersonMeetingCollection().ToArray();
				var personalActivities = personAssignment.PersonalActivities().ToArray();

				responsList.AddRange(getOverlappingLayerses(layers, meetings, personalActivities).Select(ls => createResponse(scheduleDay.Person, date, ls)));
			}

			return responsList;
		}
		

		private IList<OverlappingLayers> getOverlappingLayerses(IShiftLayer[] layers, IPersonMeeting[] meetings, IPersonalShiftLayer[] personalShiftLayers )
		{
			var result = new List<OverlappingLayers>();
			if (layers.Length == 0) return result;

			for(var i = 0;i < layers.Length;i++)
			{
				var layerBelow = layers[i];
				if (layerBelow.Payload.AllowOverwrite) continue;

				for(var j = i + 1; j < layers.Length;j++)
				{
					var layerAbove = layers[j];
					if (layerBelow.Period.Intersect(layerAbove.Period))
					{
						result.Add(new OverlappingLayers
						{
							LayerBelowName = layerBelow.Payload.Name,
							LayerBelowPeriod = layerBelow.Period,
							LayerAboveName = layerAbove.Payload.Name,
							LayerAbovePeriod = layerAbove.Period
						});
					}
				}

				result.AddRange(from meeting in meetings
					where meeting.Period.Intersect(layerBelow.Period)
					select new OverlappingLayers
					{
						LayerBelowName = layerBelow.Payload.Name,
						LayerBelowPeriod = layerBelow.Period,
						LayerAboveName = meeting.ToLayer().Payload.Name,
						LayerAbovePeriod = meeting.Period
					});

				result.AddRange(from personalShiftLayer in personalShiftLayers
					where layerBelow.Period.Intersect(personalShiftLayer.Period)
					select new OverlappingLayers
					{
						LayerBelowName = layerBelow.Payload.Name,
						LayerBelowPeriod = layerBelow.Period,
						LayerAboveName = personalShiftLayer.Payload.Name,
						LayerAbovePeriod = personalShiftLayer.Period
					});
			}

			return result;
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
				dop);			
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
