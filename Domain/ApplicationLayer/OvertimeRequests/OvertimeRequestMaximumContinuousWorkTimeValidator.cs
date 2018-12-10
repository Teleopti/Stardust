using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestMaximumContinuousWorkTimeValidator : IOvertimeRequestValidator
	{
		private readonly ICurrentScenario _currentScenario;
		private readonly IScheduleStorage _scheduleStorage;

		public OvertimeRequestMaximumContinuousWorkTimeValidator(ICurrentScenario currentScenario, IScheduleStorage scheduleStorage)
		{
			_currentScenario = currentScenario;
			_scheduleStorage = scheduleStorage;
		}

		public OvertimeRequestValidationResult Validate(OvertimeRequestValidationContext context)
		{
			var person = context.PersonRequest.Person;
			var enableMaximumContinuousWorkTimeCheck = person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled;

			if (!enableMaximumContinuousWorkTimeCheck)
			{
				return validResult();
			}

			var maximumContinuousWorkTime =
				person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime.GetValueOrDefault(TimeSpan.Zero);
			var minimumRestTime =
				person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold.GetValueOrDefault(TimeSpan.Zero);
			var timezone = person.PermissionInformation.DefaultTimeZone();
			var dic = getScheduleDictionary(context.PersonRequest.Request.Period.ToDateOnlyPeriod(timezone), person);
			var scheduleRange = dic[person];
			var requestPeriod = context.PersonRequest.Request.Period;

			var shiftLayers = loadVisualLayers(scheduleRange, requestPeriod, timezone);

			var continuousWorkTimeInfo = getContinuousWorkTime(shiftLayers, requestPeriod, minimumRestTime);

			if (!isSatisfiedMaximumContinuousWorkTime(continuousWorkTimeInfo, maximumContinuousWorkTime))
			{
				var result = invalidResult(continuousWorkTimeInfo.ContinuousWorkTimePeriod,
					continuousWorkTimeInfo.ContinuousWorkTime,
					maximumContinuousWorkTime, timezone);

				var maximumContinuousWorkTimeHandleType =
					person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType;

				if (maximumContinuousWorkTimeHandleType == OvertimeValidationHandleType.Pending)
				{
					result.ShouldDenyIfInValid = false;
				}
				return result;
			}

			return validResult();
		}

		private List<IVisualLayer> loadVisualLayers(IScheduleRange scheduleRange, DateTimePeriod requestPeriod,
			TimeZoneInfo timezone)
		{
			var visualLayers = new List<IVisualLayer>();
			var today = new DateOnly(requestPeriod.StartDateTimeLocal(timezone));

			visualLayers.AddRange(scheduleRange.ScheduledDay(today.AddDays(-1)).ProjectionService().CreateProjection());
			visualLayers.AddRange(scheduleRange.ScheduledDay(today).ProjectionService().CreateProjection());
			visualLayers.AddRange(scheduleRange.ScheduledDay(today.AddDays(1)).ProjectionService().CreateProjection());

			return visualLayers.OrderBy(shift => shift.Period.StartDateTime).ToList();
		}

		private continuousWorkTimeInfo getContinuousWorkTime(List<IVisualLayer> visualLayers,
			DateTimePeriod requestPeriod, TimeSpan minimumRestTime)
		{
			if (!visualLayers.Any())
			{
				return buildContinuousWorkTimeInfoBasedOnRequest(requestPeriod);
			}

			var previousShiftLayerWorktimeInfo = getPreviousContinuousWorktimeInfo(visualLayers, requestPeriod, minimumRestTime);
			var nextShiftLayerWorktimeInfo = getNextContinuousWorktimeInfo(visualLayers, requestPeriod, minimumRestTime);

			return combineContinuousWorkTimeInfo(requestPeriod, previousShiftLayerWorktimeInfo, nextShiftLayerWorktimeInfo);
		}

		private static continuousWorkTimeInfo combineContinuousWorkTimeInfo(DateTimePeriod requestPeriod, continuousWorkTimeInfo previousShiftLayerWorktimeInfo
			, continuousWorkTimeInfo nextShiftLayerWorktimeInfo)
		{
			DateTimePeriod continuousWorkTimePeriod;
			TimeSpan continuousWorkTime;

			if (previousShiftLayerWorktimeInfo != null && nextShiftLayerWorktimeInfo != null)
			{
				continuousWorkTime = previousShiftLayerWorktimeInfo.ContinuousWorkTime + requestPeriod.ElapsedTime() +
									 nextShiftLayerWorktimeInfo.ContinuousWorkTime;
				continuousWorkTimePeriod = new DateTimePeriod(previousShiftLayerWorktimeInfo.ContinuousWorkTimePeriod.StartDateTime, nextShiftLayerWorktimeInfo.ContinuousWorkTimePeriod.EndDateTime);
			}
			else if (previousShiftLayerWorktimeInfo != null)
			{
				continuousWorkTime = previousShiftLayerWorktimeInfo.ContinuousWorkTime + requestPeriod.ElapsedTime();
				continuousWorkTimePeriod =
					new DateTimePeriod(previousShiftLayerWorktimeInfo.ContinuousWorkTimePeriod.StartDateTime, requestPeriod.EndDateTime);
			}
			else if (nextShiftLayerWorktimeInfo != null)
			{
				continuousWorkTime = nextShiftLayerWorktimeInfo.ContinuousWorkTime + requestPeriod.ElapsedTime();
				continuousWorkTimePeriod = new DateTimePeriod(requestPeriod.StartDateTime, nextShiftLayerWorktimeInfo.ContinuousWorkTimePeriod.EndDateTime);
			}
			else
			{
				return buildContinuousWorkTimeInfoBasedOnRequest(requestPeriod);
			}

			return new continuousWorkTimeInfo
			{
				ContinuousWorkTime = continuousWorkTime,
				ContinuousWorkTimePeriod = continuousWorkTimePeriod
			};
		}

		private continuousWorkTimeInfo getPreviousContinuousWorktimeInfo(List<IVisualLayer> visualLayers, DateTimePeriod requestPeriod,
			TimeSpan minimumRestTime)
		{
			var beforeShiftLayers = visualLayers.Where(visualLayer =>
			{
				var isShiftBeforeRequest = visualLayer.Period.EndDateTime.CompareTo(requestPeriod.StartDateTime) <= 0;
				var isLunchOrShortBreak = isLunchOrShortBreakActivity(visualLayer);
				var isAbsence = isAbsenceLayer(visualLayer);
				return isShiftBeforeRequest && !isLunchOrShortBreak && !isAbsence;
			}).OrderBy(shift => shift.Period.StartDateTime).ToList();

			if (!beforeShiftLayers.Any())
				return null;

			var totalWorkTime = TimeSpan.Zero;
			var lastShiftLayerStartTime = default(DateTime);
			for (var i = beforeShiftLayers.Count - 1; i >= 0; i--)
			{
				var currentStart = beforeShiftLayers[i].Period.StartDateTime;
				var currentEnd = beforeShiftLayers[i].Period.EndDateTime;

				if (lastShiftLayerStartTime.CompareTo(default(DateTime)) == 0)
				{
					totalWorkTime += beforeShiftLayers[i].Period.ElapsedTime();
					lastShiftLayerStartTime = currentStart;
				}
				else if (isContinousPeriod(currentEnd, lastShiftLayerStartTime, minimumRestTime))
				{
					totalWorkTime += beforeShiftLayers[i].Period.ElapsedTime();
					lastShiftLayerStartTime = currentStart;
				}
				else
				{
					break;
				}
			}

			var continuousWorkTimeInfo = new continuousWorkTimeInfo
			{
				ContinuousWorkTime = totalWorkTime,
				ContinuousWorkTimePeriod = new DateTimePeriod(lastShiftLayerStartTime, beforeShiftLayers.Last().Period.EndDateTime)
			};

			if (isContinousPeriod(continuousWorkTimeInfo.ContinuousWorkTimePeriod.EndDateTime, requestPeriod.StartDateTime, minimumRestTime))
				return continuousWorkTimeInfo;

			return null;
		}

		private continuousWorkTimeInfo getNextContinuousWorktimeInfo(List<IVisualLayer> visualLayers, DateTimePeriod requestPeriod,
			TimeSpan minimumRestTime)
		{
			var afterShiftLayers = visualLayers.Where(visualLayer =>
			{
				var isShiftAfterRequest = visualLayer.Period.StartDateTime.CompareTo(requestPeriod.EndDateTime) >= 0;
				var isLunchOrShortBreak = isLunchOrShortBreakActivity(visualLayer);
				var isAbsence = isAbsenceLayer(visualLayer);
				return isShiftAfterRequest && !isLunchOrShortBreak && !isAbsence;
			}).OrderBy(shift => shift.Period.StartDateTime).ToList();

			if (!afterShiftLayers.Any())
				return null;

			var totalWorkTime = TimeSpan.Zero;
			var lastShiftLayerEndTime = default(DateTime);
			for (var i = 0; i < afterShiftLayers.Count; i++)
			{
				var currentStart = afterShiftLayers[i].Period.StartDateTime;
				var currentEnd = afterShiftLayers[i].Period.EndDateTime;

				if (lastShiftLayerEndTime.CompareTo(default(DateTime)) == 0)
				{
					totalWorkTime += afterShiftLayers[i].Period.ElapsedTime();
					lastShiftLayerEndTime = currentEnd;
				}
				else if (isContinousPeriod(lastShiftLayerEndTime, currentStart, minimumRestTime))
				{
					totalWorkTime += afterShiftLayers[i].Period.ElapsedTime();
					lastShiftLayerEndTime = currentEnd;
				}
				else
				{
					break;
				}
			}

			var continuousWorkTimeInfo = new continuousWorkTimeInfo
			{
				ContinuousWorkTime = totalWorkTime,
				ContinuousWorkTimePeriod = new DateTimePeriod(afterShiftLayers.First().Period.StartDateTime, lastShiftLayerEndTime)
			};

			if (isContinousPeriod(continuousWorkTimeInfo.ContinuousWorkTimePeriod.StartDateTime, requestPeriod.EndDateTime, minimumRestTime))
				return continuousWorkTimeInfo;

			return null;
		}

		private bool isContinousPeriod(DateTime previousEndTime, DateTime nextStartTime, TimeSpan minimumRestTime)
		{
			var absoluteGap = (int)Math.Abs((nextStartTime - previousEndTime).TotalMinutes);
			return absoluteGap == 0 || absoluteGap < minimumRestTime.TotalMinutes;
		}


		private bool isSatisfiedMaximumContinuousWorkTime(continuousWorkTimeInfo continuousWorkTimeInfo,
			TimeSpan maximumContinuousWorkTime)
		{
			return continuousWorkTimeInfo.ContinuousWorkTime.CompareTo(maximumContinuousWorkTime) <= 0;
		}

		private IScheduleDictionary getScheduleDictionary(DateOnlyPeriod period, IPerson person)
		{
			var schedulePeriod = period.Inflate(1);

			var dic = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false),
				schedulePeriod, _currentScenario.Current());

			return dic;
		}

		private bool isLunchOrShortBreakActivity(IVisualLayer visualLayer)
		{
			if (visualLayer.Payload is IActivity activity)
			{
				return activity.ReportLevelDetail == ReportLevelDetail.Lunch ||
					   activity.ReportLevelDetail == ReportLevelDetail.ShortBreak;
			}

			return false;
		}

		private bool isAbsenceLayer(IVisualLayer visualLayer)
		{
			return visualLayer.Payload is IAbsence;
		}

		private static OvertimeRequestValidationResult invalidResult(DateTimePeriod continuousWorkTimePeriod,
			TimeSpan continuousWorkTime, TimeSpan maximumContinuousWorkTime, TimeZoneInfo timezone)
		{
			var continuousWorkTimeString =
				$"{continuousWorkTimePeriod.StartDateTimeLocal(timezone)} - {continuousWorkTimePeriod.EndDateTimeLocal(timezone)}";
			return new OvertimeRequestValidationResult
			{
				InvalidReasons = new[]
				{
					string.Format(Resources.OvertimeRequestContinuousWorkTimeDenyReason,
						continuousWorkTimeString,
						DateHelper.HourMinutesString(continuousWorkTime.TotalMinutes),
						DateHelper.HourMinutesString(maximumContinuousWorkTime.TotalMinutes))
				},
				BrokenBusinessRules = BusinessRuleFlags.MaximumContinuousWorkTimeRule
			};
		}

		private static continuousWorkTimeInfo buildContinuousWorkTimeInfoBasedOnRequest(DateTimePeriod requestPeriod)
		{
			return new continuousWorkTimeInfo
			{
				ContinuousWorkTime = requestPeriod.ElapsedTime(),
				ContinuousWorkTimePeriod = requestPeriod
			};
		}

		private static OvertimeRequestValidationResult validResult()
		{
			return new OvertimeRequestValidationResult()
			{
				IsValid = true
			};
		}

		private class continuousWorkTimeInfo
		{
			public TimeSpan ContinuousWorkTime { get; set; }

			public DateTimePeriod ContinuousWorkTimePeriod { get; set; }
		}
	}
}