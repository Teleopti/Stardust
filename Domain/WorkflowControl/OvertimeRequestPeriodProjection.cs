using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class OvertimeRequestPeriodProjection
	{
		private readonly IList<OvertimeRequestSkillTypeFlatOpenPeriod> _overtimeRequestOpenPeriodList;
		private readonly CultureInfo _dateCulture;
		private readonly CultureInfo _languageCulture;
		private readonly DateOnly _viewpointDate;

		public OvertimeRequestPeriodProjection(IList<OvertimeRequestSkillTypeFlatOpenPeriod> overtimeRequestOpenPeriodList, 
			CultureInfo dateCulture, CultureInfo languageCulture, DateOnly viewpointDate)
		{
			_overtimeRequestOpenPeriodList = overtimeRequestOpenPeriodList;
			_dateCulture = dateCulture;
			_languageCulture = languageCulture;
			_viewpointDate = viewpointDate;
		}

		public IList<OvertimeRequestSkillTypeFlatOpenPeriod> GetProjectedOvertimeRequestsOpenPeriods(DateOnlyPeriod requestPeriod)
		{
			var results = new List<OvertimeRequestSkillTypeFlatOpenPeriod>();
			var matchedPeriods = _overtimeRequestOpenPeriodList.Select(p => new OvertimeRequestSkillTypeFlatOpenPeriod
			{
				AutoGrantType = p.AutoGrantType,
				Period = p.OriginPeriod.GetPeriod(_viewpointDate),
				EnableWorkRuleValidation = p.EnableWorkRuleValidation,
				WorkRuleValidationHandleType = p.WorkRuleValidationHandleType,
				SkillType = p.SkillType,
				OriginPeriod = p.OriginPeriod,
				OrderIndex = p.OrderIndex
			}
			).Where(p => p.Period.Contains(requestPeriod)).ToList();

			setAvailableDaysAndDenyReason(_overtimeRequestOpenPeriodList, matchedPeriods);

			var autoDenyPeriod =
				createAutoDenyPeriod(_overtimeRequestOpenPeriodList, matchedPeriods);
			autoDenyPeriod.Period = requestPeriod;
			matchedPeriods.Insert(0, autoDenyPeriod);

			var startTime = matchedPeriods.Min(p => p.Period.StartDate);
			var endTime = matchedPeriods.Max(d => d.Period.EndDate);
			var currentTime = startTime;

			while (currentTime <= endTime)
			{
				for (var inverseLoopIndex = matchedPeriods.Count - 1; inverseLoopIndex >= 0; inverseLoopIndex--)
				{
					var currentPeriod = matchedPeriods[inverseLoopIndex];
					if (currentPeriod.Period.Contains(currentTime))
					{
						var layerEndTime = findLayerEndTime(matchedPeriods, inverseLoopIndex, currentPeriod, currentTime);
						results.Add(new OvertimeRequestSkillTypeFlatOpenPeriod
						{
							AutoGrantType = currentPeriod.AutoGrantType,
							Period = new DateOnlyPeriod(currentTime, layerEndTime),
							DenyReason = currentPeriod.DenyReason,
							EnableWorkRuleValidation = currentPeriod.EnableWorkRuleValidation,
							WorkRuleValidationHandleType = currentPeriod.WorkRuleValidationHandleType,
							SkillType = currentPeriod.SkillType,
							OriginPeriod = currentPeriod.OriginPeriod,
							OrderIndex = currentPeriod.OrderIndex,
							AvailableDays = currentPeriod.AvailableDays
						});

						if (inverseLoopIndex < matchedPeriods.Count - 1 &&
							layerEndTime == matchedPeriods[inverseLoopIndex + 1].Period.StartDate)
							currentTime = layerEndTime;
						else
							currentTime = layerEndTime.AddDays(1);
						break;
					}
				}
			}

			return results.Where(w => w.Period.Intersection(requestPeriod).HasValue).ToList();
		}

		private OvertimeRequestSkillTypeFlatOpenPeriod createAutoDenyPeriod(
			IList<OvertimeRequestSkillTypeFlatOpenPeriod> allOvertimeRequestOpenPeriods
			, IList<OvertimeRequestSkillTypeFlatOpenPeriod> matchedPeriods)
		{
			if (allOvertimeRequestOpenPeriods.Count == 0)
			{
				return createAutoDenyPeriodWhenNoOvertimeRequestPeriod();
			}

			if (matchedPeriods.Count == 0)
				return createAutoDenyPeriodWhenNoOvertimeRequestPeriodIsMatched(allOvertimeRequestOpenPeriods);

			return new OvertimeRequestSkillTypeFlatOpenPeriod
			{
				DenyReason = string.Empty,
				AutoGrantType = OvertimeRequestAutoGrantType.Deny
			};
		}

		private OvertimeRequestSkillTypeFlatOpenPeriod createAutoDenyPeriodWhenNoOvertimeRequestPeriod()
		{
			return new OvertimeRequestSkillTypeFlatOpenPeriod
			{
				DenyReason = Resources.ResourceManager.GetString("OvertimeRequestDenyReasonClosedPeriod", _languageCulture),
				AutoGrantType = OvertimeRequestAutoGrantType.Deny
			};
		}

		private OvertimeRequestSkillTypeFlatOpenPeriod createAutoDenyPeriodWhenNoOvertimeRequestPeriodIsMatched(
			IList<OvertimeRequestSkillTypeFlatOpenPeriod> allOvertimeRequestOpenPeriods)
		{
			var deniedResultOfOutOfOpenPeriod = getDeniedResultOfOutOfOpenPeriod(allOvertimeRequestOpenPeriods);
			var denyReason = deniedResultOfOutOfOpenPeriod.DenyReason;
			var availableDays = deniedResultOfOutOfOpenPeriod.AvailableDays;

			return new OvertimeRequestSkillTypeFlatOpenPeriod
			{
				DenyReason = denyReason,
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				AvailableDays = availableDays
			};
		}

		private void setAvailableDaysAndDenyReason(
			IList<OvertimeRequestSkillTypeFlatOpenPeriod> allOvertimeRequestOpenPeriods,
			IList<OvertimeRequestSkillTypeFlatOpenPeriod> matchedPeriods)
		{
			IList<DateOnly> availableDays = new List<DateOnly>();

			foreach (var filteredOvertimeRequestOpenPeriod in matchedPeriods)
			{
				if (filteredOvertimeRequestOpenPeriod.AutoGrantType != OvertimeRequestAutoGrantType.Deny) continue;

				var deniedResultOfOutOfOpenPeriod = getDeniedResultOfOutOfOpenPeriod(allOvertimeRequestOpenPeriods);
				string denyReason;
				if (deniedResultOfOutOfOpenPeriod.AvailableDays != null && deniedResultOfOutOfOpenPeriod.AvailableDays.Any())
				{
					denyReason = deniedResultOfOutOfOpenPeriod.DenyReason;
					availableDays = deniedResultOfOutOfOpenPeriod.AvailableDays;
				}
				else
				{
					denyReason = Resources.ResourceManager.GetString("OvertimeRequestDenyReasonAutodeny", _languageCulture);
				}
				filteredOvertimeRequestOpenPeriod.AvailableDays = availableDays;
				filteredOvertimeRequestOpenPeriod.DenyReason = denyReason;
			}
		}

		private DeniedResultOfOutOfOpenPeriod getDeniedResultOfOutOfOpenPeriod(
			IList<OvertimeRequestSkillTypeFlatOpenPeriod> overtimeRequestOpenPeriodList)
		{
			var denyDays = getDenyDays(overtimeRequestOpenPeriodList);
			var dayCollection = new List<DateOnly>();

			foreach (var overtimeRequestOpenPeriod in overtimeRequestOpenPeriodList)
			{
				if (overtimeRequestOpenPeriod.AutoGrantType != OvertimeRequestAutoGrantType.Deny)
				{
					dayCollection.AddRange(overtimeRequestOpenPeriod.OriginPeriod.GetPeriod(_viewpointDate)
						.DayCollection().Where(a => a.CompareTo(_viewpointDate) >= 0));
				}
			}

			if (dayCollection.Count > 0)
			{
				return getDeniedResultOfOutOfOpenPeriod(dayCollection.Distinct().OrderBy(x => x.Date).ToList(), denyDays);
			}

			return new DeniedResultOfOutOfOpenPeriod
			{
				DenyReason = Resources.ResourceManager.GetString("OvertimeRequestDenyReasonClosedPeriod", _languageCulture)
			};
		}

		private IList<DateOnly> getDenyDays(IList<OvertimeRequestSkillTypeFlatOpenPeriod> overtimeRequestOpenPeriodList)
		{
			var denyDayCollection = new List<DateOnly>();
			overtimeRequestOpenPeriodList.Where(isOvertimeRequestOpenPeriodAutoDeny)
				.ToList()
				.ForEach(
					p => denyDayCollection.AddRange(p.OriginPeriod.GetPeriod(_viewpointDate).DayCollection()));
			return denyDayCollection;
		}

		private DeniedResultOfOutOfOpenPeriod getDeniedResultOfOutOfOpenPeriod(List<DateOnly> dateCollection, IList<DateOnly> denyDays)
		{
			var dayCollection = dateCollection.Where(a => a.CompareTo(_viewpointDate) >= 0).ToList();
			foreach (var denyDay in denyDays)
			{
				dayCollection.Remove(denyDay);
			}

			var periods = dayCollection.SplitToContinuousPeriods();
			var suggestedPeriodDateString = string.Join(",", periods.Select(p => p.ToShortDateString(_dateCulture)));

			var denyReason = string.Format(_languageCulture,
				Resources.ResourceManager.GetString("OvertimeRequestDenyReasonNoPeriod", _languageCulture),
				suggestedPeriodDateString);

			return new DeniedResultOfOutOfOpenPeriod
			{
				DenyReason = denyReason,
				AvailableDays = dayCollection
			};
		}

		private bool isOvertimeRequestOpenPeriodAutoDeny(OvertimeRequestSkillTypeFlatOpenPeriod overtimeRequestOpenPeriod)
		{
			return overtimeRequestOpenPeriod.AutoGrantType == OvertimeRequestAutoGrantType.Deny;
		}

		private DateOnly findLayerEndTime(IList<OvertimeRequestSkillTypeFlatOpenPeriod> filteredPeriodsList,
			int currentLayerIndex, OvertimeRequestSkillTypeFlatOpenPeriod workingLayer, DateOnly currentTime)
		{
			DateOnly layerEndTime = workingLayer.Period.EndDate;
			if (currentLayerIndex != filteredPeriodsList.Count - 1)
			{
				var orgLayerCount = filteredPeriodsList.Count;
				for (var higherPrioLoop = currentLayerIndex + 1; higherPrioLoop < orgLayerCount; higherPrioLoop++)
				{
					DateOnlyPeriod higherPrioLayerPeriod = filteredPeriodsList[higherPrioLoop].Period;
					if (workingLayer.Period.Contains(higherPrioLayerPeriod.StartDate.AddDays(-1)) &&
						higherPrioLayerPeriod.EndDate > currentTime &&
						higherPrioLayerPeriod.StartDate < layerEndTime)
					{
						layerEndTime = higherPrioLayerPeriod.StartDate.AddDays(-1);
					}
				}
			}
			return layerEndTime;
		}

		private struct DeniedResultOfOutOfOpenPeriod
		{
			public string DenyReason { get; set; }

			public IList<DateOnly> AvailableDays { get; set; }
		}
	}
}
