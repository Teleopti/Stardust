using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class OvertimeRequestPeriodProjection : IOvertimeRequestPeriodProjection
	{
		private readonly IList<IOvertimeRequestOpenPeriod> _overtimeRequestOpenPeriodList;
		private readonly CultureInfo _dateCulture;
		private readonly CultureInfo _languageCulture;

		public OvertimeRequestPeriodProjection(IList<IOvertimeRequestOpenPeriod> overtimeRequestOpenPeriodList, CultureInfo dateCulture, CultureInfo languageCulture)
		{
			_overtimeRequestOpenPeriodList = overtimeRequestOpenPeriodList;
			_dateCulture = dateCulture;
			_languageCulture = languageCulture;
		}

		public IList<IOvertimeRequestOpenPeriod> GetProjectedOvertimeRequestsOpenPeriods(DateOnlyPeriod requestPeriod)
		{
			var results = new List<IOvertimeRequestOpenPeriod>();
			var filteredPeriods = _overtimeRequestOpenPeriodList.Select(p => new DateOnlyOvertimeRequestOpenPeriod
			{
				AutoGrantType = p.AutoGrantType,
				Period = p.GetPeriod(ServiceLocatorForEntity.Now.ServerDate_DontUse()),
				EnableWorkRuleValidation = p.EnableWorkRuleValidation,
				WorkRuleValidationHandleType = p.WorkRuleValidationHandleType
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
						results.Add(new OvertimeRequestOpenDatePeriod
						{
							AutoGrantType = currentPeriod.AutoGrantType,
							Period = new DateOnlyPeriod(currentTime, layerEndTime),
							DenyReason = currentPeriod.DenyReason,
							EnableWorkRuleValidation = currentPeriod.EnableWorkRuleValidation,
							WorkRuleValidationHandleType = currentPeriod.WorkRuleValidationHandleType
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

			return results.Where(w => w.GetPeriod(ServiceLocatorForEntity.Now.ServerDate_DontUse()).Intersection(requestPeriod).HasValue).ToList();
		}

		private DateOnlyOvertimeRequestOpenPeriod createAutoDenyPeriod(IList<IOvertimeRequestOpenPeriod> overtimeRequestOpenPeriodList, IList<DateOnlyOvertimeRequestOpenPeriod> filteredOvertimeRequestOpenPeriodList)
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

			return new DateOnlyOvertimeRequestOpenPeriod
			{
				DenyReason = denyReason,
				AutoGrantType = OvertimeRequestAutoGrantType.Deny
			};
		}

		private string getDenyReasonWithSuggestedPeriod(IList<IOvertimeRequestOpenPeriod> overtimeRequestOpenPeriodList)
		{
			string denyReason;
			var denyDays = getDenyDays(overtimeRequestOpenPeriodList);
			var dayCollection = new List<DateOnly>();

			foreach (var overtimeRequestOpenPeriod in overtimeRequestOpenPeriodList)
			{
				if (overtimeRequestOpenPeriod.AutoGrantType != OvertimeRequestAutoGrantType.Deny)
				{
					dayCollection.AddRange(overtimeRequestOpenPeriod.GetPeriod(ServiceLocatorForEntity.Now.ServerDate_DontUse())
						.DayCollection().Where(a => a.CompareTo(ServiceLocatorForEntity.Now.ServerDate_DontUse()) >= 0));
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

		private IList<DateOnly> getDenyDays(IList<IOvertimeRequestOpenPeriod> overtimeRequestOpenPeriodList)
		{
			var denyDayCollection = new List<DateOnly>();
			overtimeRequestOpenPeriodList.Where(isOvertimeRequestOpenPeriodAutoDeny)
				.ToList()
				.ForEach(
					p => denyDayCollection.AddRange(p.GetPeriod(ServiceLocatorForEntity.Now.ServerDate_DontUse()).DayCollection()));
			return denyDayCollection;
		}

		private string getSuggestedPeriodDateString(List<DateOnly> dateCollection, IList<DateOnly> denyDays)
		{
			var dayCollection = dateCollection.Where(a => a.CompareTo(ServiceLocatorForEntity.Now.ServerDate_DontUse()) >= 0).ToList();
			foreach (var denyDay in denyDays)
			{
				dayCollection.Remove(denyDay);
			}
			var periods = splitToContinuousPeriods(dayCollection);
			return string.Join(",", periods.Select(p => p.ToShortDateString(_dateCulture)));
		}

		private IList<DateOnlyPeriod> splitToContinuousPeriods(IList<DateOnly> dayCollection)
		{
			var periodList = new List<DateOnlyPeriod>();
			DateOnly? startDate = null;
			for (var i = 0; i < dayCollection.Count; i++)
			{
				if (!startDate.HasValue)
				{
					startDate = dayCollection[i];
				}
				var nextDate = dayCollection[i].AddDays(1);
				if (dayCollection.Contains(nextDate)) continue;
				periodList.Add(new DateOnlyPeriod(startDate.Value, nextDate.AddDays(-1)));
				startDate = null;
			}
			return periodList;
		}


		private bool isOvertimeRequestOpenPeriodAutoDeny(IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod)
		{
			return overtimeRequestOpenPeriod.AutoGrantType == OvertimeRequestAutoGrantType.Deny;
		}

		private DateOnly findLayerEndTime(IList<DateOnlyOvertimeRequestOpenPeriod> filteredPeriodsList,
			int currentLayerIndex, DateOnlyOvertimeRequestOpenPeriod workingLayer, DateOnly currentTime)
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

		private class DateOnlyOvertimeRequestOpenPeriod
		{
			public string DenyReason { get; set; }
			public OvertimeRequestAutoGrantType AutoGrantType { get; set; }
			public DateOnlyPeriod Period { get; set; }
			public bool EnableWorkRuleValidation { get; set; }
			public OvertimeWorkRuleValidationHandleType? WorkRuleValidationHandleType { get; set; }
		}
	}

	public interface IOvertimeRequestPeriodProjection
	{
		IList<IOvertimeRequestOpenPeriod> GetProjectedOvertimeRequestsOpenPeriods(DateOnlyPeriod requestPeriod);
	}
}
