using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class OvertimeRequestPeriodProjection : IOvertimeRequestPeriodProjection
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
			var filteredPeriods = _overtimeRequestOpenPeriodList.Select(p => new OvertimeRequestSkillTypeFlatOpenPeriod
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

			var autoDenyPeriod =
				createAutoDenyPeriod(_overtimeRequestOpenPeriodList, filteredPeriods);
			autoDenyPeriod.Period = requestPeriod;
			filteredPeriods.Insert(0, autoDenyPeriod);

			var startTime = filteredPeriods.Min(p => p.Period.StartDate);
			var endTime = filteredPeriods.Max(d => d.Period.EndDate);
			var currentTime = startTime;

			while (currentTime <= endTime)
			{
				for (var inverseLoopIndex = filteredPeriods.Count - 1; inverseLoopIndex >= 0; inverseLoopIndex--)
				{
					var currentPeriod = filteredPeriods[inverseLoopIndex];
					if (currentPeriod.Period.Contains(currentTime))
					{
						var layerEndTime = findLayerEndTime(filteredPeriods, inverseLoopIndex, currentPeriod, currentTime);
						results.Add(new OvertimeRequestSkillTypeFlatOpenPeriod
						{
							AutoGrantType = currentPeriod.AutoGrantType,
							Period = new DateOnlyPeriod(currentTime, layerEndTime),
							DenyReason = currentPeriod.DenyReason,
							EnableWorkRuleValidation = currentPeriod.EnableWorkRuleValidation,
							WorkRuleValidationHandleType = currentPeriod.WorkRuleValidationHandleType,
							SkillType = currentPeriod.SkillType,
							OriginPeriod = currentPeriod.OriginPeriod,
							OrderIndex = currentPeriod.OrderIndex
						});

						if (inverseLoopIndex < filteredPeriods.Count - 1 &&
							layerEndTime == filteredPeriods[inverseLoopIndex + 1].Period.StartDate)
							currentTime = layerEndTime;
						else
							currentTime = layerEndTime.AddDays(1);
						break;
					}
				}
			}

			return results.Where(w => w.Period.Intersection(requestPeriod).HasValue).ToList();
		}

		private OvertimeRequestSkillTypeFlatOpenPeriod createAutoDenyPeriod(IList<OvertimeRequestSkillTypeFlatOpenPeriod> overtimeRequestOpenPeriodList, IList<OvertimeRequestSkillTypeFlatOpenPeriod> filteredOvertimeRequestOpenPeriodList)
		{
			string denyReason = null;
			if (overtimeRequestOpenPeriodList.Count == 0)
			{
				denyReason = Resources.ResourceManager.GetString("OvertimeRequestDenyReasonClosedPeriod", _languageCulture);
			}

			if (filteredOvertimeRequestOpenPeriodList.Count == 0)
			{
				denyReason = getDenyReasonWithSuggestedPeriod(overtimeRequestOpenPeriodList);
			}
			else
			{
				foreach (var filteredOvertimeRequestOpenPeriod in filteredOvertimeRequestOpenPeriodList)
				{
					if (filteredOvertimeRequestOpenPeriod.AutoGrantType == OvertimeRequestAutoGrantType.Deny)
					{
						denyReason = Resources.ResourceManager.GetString("OvertimeRequestDenyReasonAutodeny", _languageCulture);
						filteredOvertimeRequestOpenPeriod.DenyReason = denyReason;
					}
				}
			}

			return new OvertimeRequestSkillTypeFlatOpenPeriod
			{
				DenyReason = denyReason,
				AutoGrantType = OvertimeRequestAutoGrantType.Deny
			};
		}

		private string getDenyReasonWithSuggestedPeriod(IList<OvertimeRequestSkillTypeFlatOpenPeriod> overtimeRequestOpenPeriodList)
		{
			string denyReason;
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
				denyReason = string.Format(_languageCulture,
					Resources.ResourceManager.GetString("OvertimeRequestDenyReasonNoPeriod", _languageCulture),
					string.Join(",", getSuggestedPeriodDateString(dayCollection.Distinct().OrderBy(x => x.Date).ToList(), denyDays)));
			}
			else
			{
				denyReason = Resources.ResourceManager.GetString("OvertimeRequestDenyReasonClosedPeriod", _languageCulture);
			}

			return denyReason;
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

		private string getSuggestedPeriodDateString(List<DateOnly> dateCollection, IList<DateOnly> denyDays)
		{
			var dayCollection = dateCollection.Where(a => a.CompareTo(_viewpointDate) >= 0).ToList();
			foreach (var denyDay in denyDays)
			{
				dayCollection.Remove(denyDay);
			}
			var periods = dayCollection.SplitToContinuousPeriods();
			return string.Join(",", periods.Select(p => p.ToShortDateString(_dateCulture)));
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
	}

	public interface IOvertimeRequestPeriodProjection
	{
		IList<OvertimeRequestSkillTypeFlatOpenPeriod> GetProjectedOvertimeRequestsOpenPeriods(DateOnlyPeriod requestPeriod);
	}
}
