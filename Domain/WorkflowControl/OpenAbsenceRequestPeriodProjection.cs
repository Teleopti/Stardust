using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class OpenAbsenceRequestPeriodProjection : IOpenAbsenceRequestPeriodProjection
	{
		private readonly IOpenAbsenceRequestPeriodExtractor _openAbsenceRequestPeriodExtractor;

		private IList<DateOnlyPeriodWithAbsenceRequestPeriod> _layerCollectionOriginal;
		private CultureInfo _dateCultureInfo;
		private CultureInfo _languageCultureInfo;

		public OpenAbsenceRequestPeriodProjection(IOpenAbsenceRequestPeriodExtractor openAbsenceRequestPeriodExtractor)
		{
			_openAbsenceRequestPeriodExtractor = openAbsenceRequestPeriodExtractor;
		}

		public IList<IAbsenceRequestOpenPeriod> GetProjectedPeriods(DateOnlyPeriod limitToPeriod, CultureInfo date, CultureInfo language)
		{
			_dateCultureInfo = date;
			_languageCultureInfo = language;

			IList<IAbsenceRequestOpenPeriod> workingColl = new List<IAbsenceRequestOpenPeriod>();

			_layerCollectionOriginal =
				_openAbsenceRequestPeriodExtractor.AvailablePeriods.Select(
					d =>
					new DateOnlyPeriodWithAbsenceRequestPeriod
					{
						Period = d.GetPeriod(_openAbsenceRequestPeriodExtractor.ViewpointDate),
						AbsenceRequestOpenPeriod = d,
						LimitPeriod = limitToPeriod
					}).Where(d => d.IsWithinLimit).ToList();
			AddBottomLayer(limitToPeriod);

			DateOnly startTime = _layerCollectionOriginal.Min(d => d.Period.StartDate);
			DateOnly endTime = _layerCollectionOriginal.Max(d => d.Period.EndDate);
			DateOnly currentTime = startTime;

			while (currentTime <= endTime)
			{
				bool layerFound = false;
				for (int inverseLoop = _layerCollectionOriginal.Count - 1; inverseLoop >= 0; inverseLoop--)
				{
					DateOnlyPeriodWithAbsenceRequestPeriod workingLayer = _layerCollectionOriginal[inverseLoop];
					if (workingLayer.Period.Contains(currentTime))
					{
						DateOnly layerEndTime = findLayerEndTime(inverseLoop, workingLayer, currentTime);
						IAbsenceRequestOpenPeriod newLayer = new AbsenceRequestOpenDatePeriod
						{
							Absence = workingLayer.AbsenceRequestOpenPeriod.Absence,
							AbsenceRequestProcess =
								workingLayer.AbsenceRequestOpenPeriod.AbsenceRequestProcess,
							PersonAccountValidator =
								workingLayer.AbsenceRequestOpenPeriod.PersonAccountValidator,
							StaffingThresholdValidator =
								workingLayer.AbsenceRequestOpenPeriod.StaffingThresholdValidator,
							Period = new DateOnlyPeriod(currentTime, layerEndTime)
						};
						workingColl.Add(newLayer);
						if (inverseLoop < _layerCollectionOriginal.Count - 1 &&
							layerEndTime == _layerCollectionOriginal[inverseLoop + 1].Period.StartDate)
						{
							currentTime = layerEndTime;
						}
						else
						{
							currentTime = layerEndTime.AddDays(1);
						}
						layerFound = true;
						break;
					}
				}
				if (!layerFound)
					currentTime = findNextTimeSlot(currentTime);
			}

			return workingColl.Where(w => w.GetPeriod(ServiceLocatorForEntity.Now.ServerDate_DontUse()).Intersection(limitToPeriod).HasValue).ToList();
		}

		private void AddBottomLayer(DateOnlyPeriod limitToPeriod)
		{
			var denyReason = UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonClosedPeriod", _languageCultureInfo);
			CheckAbsenceRequestOpenPeriodResult lastCheckPeriodResult = null;
			var denyDays = getDenyDays();

			foreach (var absenceRequestOpenPeriod in _openAbsenceRequestPeriodExtractor.AllPeriods)
			{
				
				var checkPeriodResult = checkingForOpenPeriod(absenceRequestOpenPeriod);
				if (!string.IsNullOrEmpty(checkPeriodResult.DenyReason))
				{
					if (lastCheckPeriodResult == null || checkPeriodResult.HasSuggestedPeriod)
					{
						lastCheckPeriodResult = checkPeriodResult;
					}
					denyReason = lastCheckPeriodResult.DenyReason;
					continue;
				}
				
				checkPeriodResult = checkingForPeriod(limitToPeriod, absenceRequestOpenPeriod, denyDays);
				if (string.IsNullOrEmpty(checkPeriodResult.DenyReason)) continue;
				if (lastCheckPeriodResult == null || checkPeriodResult.HasSuggestedPeriod)
				{
					lastCheckPeriodResult = checkPeriodResult;
				}
				denyReason = lastCheckPeriodResult.DenyReason;
			}

	        _layerCollectionOriginal.Insert(0,
	                                        new DateOnlyPeriodWithAbsenceRequestPeriod
		                                        {
			                                        AbsenceRequestOpenPeriod =
				                                        new AbsenceRequestOpenDatePeriod
					                                        {
						                                        Absence = null,
						                                        AbsenceRequestProcess =
							                                        new DenyAbsenceRequest{ 
									                                        DenyReason = denyReason
								                                        },
						                                        Period = limitToPeriod,
						                                        PersonAccountValidator =
							                                        new AbsenceRequestNoneValidator(),
						                                        StaffingThresholdValidator =
							                                        new AbsenceRequestNoneValidator()
					                                        },
			                                        LimitPeriod = limitToPeriod,
			                                        Period = limitToPeriod
		                                        });

        }

		private IList<DateOnly> getDenyDays()
		{
			var denyDayCollection = new List<DateOnly>();
			_openAbsenceRequestPeriodExtractor.AllPeriods.Where(isAbsenceRequestOpenPeriodAutoDeny)
				.ToList()
				.ForEach(
					p => denyDayCollection.AddRange(p.GetPeriod(_openAbsenceRequestPeriodExtractor.ViewpointDate).DayCollection()));
			return denyDayCollection;
		}

		private CheckAbsenceRequestOpenPeriodResult checkingForOpenPeriod(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			var checkAbsenceRequestOpenPeriodResult = new CheckAbsenceRequestOpenPeriodResult();
			string denyReason = null;

			if (isPeriodOpensLater(absenceRequestOpenPeriod) && isNextOpenPeriod(absenceRequestOpenPeriod))
			{
				if (isAbsenceRequestOpenPeriodAutoDeny(absenceRequestOpenPeriod))
				{
					denyReason = UserTexts.Resources.RequestDenyReasonAutodeny;
				}
				else
				{
					denyReason = string.Format(_languageCultureInfo,
						UserTexts.Resources.ResourceManager.GetString(
							"RequestDenyReasonPeriodOpenAfterSendRequest", _languageCultureInfo),
						absenceRequestOpenPeriod.OpenForRequestsPeriod.StartDate.ToShortDateString(_dateCultureInfo));
					checkAbsenceRequestOpenPeriodResult.HasSuggestedPeriod = true;
				}
			}
			checkAbsenceRequestOpenPeriodResult.DenyReason = denyReason;
			return checkAbsenceRequestOpenPeriodResult;
		}

		private DateOnly earlierOpenDate = DateOnly.MaxValue;
		private bool isNextOpenPeriod(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			var dateToCheck = absenceRequestOpenPeriod.OpenForRequestsPeriod.StartDate;
			if (dateToCheck < earlierOpenDate)
			{
				earlierOpenDate = dateToCheck;
				return true;
			}
			return false;
		}

		private bool isPeriodOpensLater(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			var start = absenceRequestOpenPeriod.OpenForRequestsPeriod.StartDate;
			return start > _openAbsenceRequestPeriodExtractor.ViewpointDate;
		}

		private bool isNextPeriod(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			var dateToCheck = absenceRequestOpenPeriod.GetPeriod(_openAbsenceRequestPeriodExtractor.ViewpointDate).EndDate;
			return dateToCheck > ServiceLocatorForEntity.Now.ServerDate_DontUse();
		}

		private bool isPeriodOutside(DateOnlyPeriod requestPeriod, IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			var period = absenceRequestOpenPeriod.GetPeriod(_openAbsenceRequestPeriodExtractor.ViewpointDate);
			var isPeriodInsideOrEqual = (period.StartDate <= requestPeriod.StartDate && period.EndDate >= requestPeriod.EndDate);
			return !isPeriodInsideOrEqual;
		}

		private CheckAbsenceRequestOpenPeriodResult checkingForPeriod(DateOnlyPeriod requestPeriod, IAbsenceRequestOpenPeriod absenceRequestOpenPeriod, IList<DateOnly> denyDays)
		{
			var checkAbsenceRequestOpenPeriodResult = new CheckAbsenceRequestOpenPeriodResult();
			string denyReason = null;
			var period = absenceRequestOpenPeriod.GetPeriod(_openAbsenceRequestPeriodExtractor.ViewpointDate);

			if (isPeriodOutside(requestPeriod, absenceRequestOpenPeriod) && isNextPeriod(absenceRequestOpenPeriod))
			{
				if (isAbsenceRequestOpenPeriodAutoDeny(absenceRequestOpenPeriod))
				{
					denyReason = UserTexts.Resources.RequestDenyReasonAutodeny;
				}
				else if (!absenceRequestOpenPeriod.OpenForRequestsPeriod.Contains(period))
				{
					denyReason = UserTexts.Resources.RequestDenyReasonClosedPeriod;
				}
				else
				{
					denyReason = string.Format(_languageCultureInfo,
						UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonNoPeriod", _languageCultureInfo),
						getSuggestedPeriodDateString(period, denyDays));
					checkAbsenceRequestOpenPeriodResult.HasSuggestedPeriod = true;
				}
			}
			checkAbsenceRequestOpenPeriodResult.DenyReason = denyReason;
			return checkAbsenceRequestOpenPeriodResult;
		}

		private bool isAbsenceRequestOpenPeriodAutoDeny(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			return absenceRequestOpenPeriod?.AbsenceRequestProcess is DenyAbsenceRequest;
		}

		private DateOnly findLayerEndTime(int currentLayerIndex,
										  DateOnlyPeriodWithAbsenceRequestPeriod workingLayer,
										  DateOnly currentTime)
		{
			DateOnly layerEndTime = workingLayer.Period.EndDate;
			if (currentLayerIndex != _layerCollectionOriginal.Count - 1)
			{
				int orgLayerCount = _layerCollectionOriginal.Count;
				for (int higherPrioLoop = currentLayerIndex + 1; higherPrioLoop < orgLayerCount; higherPrioLoop++)
				{
					DateOnlyPeriod higherPrioLayerPeriod =
						_layerCollectionOriginal[higherPrioLoop].Period;
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

		private DateOnly findNextTimeSlot(DateOnly currentTime)
		{
			DateOnly retTime = DateOnly.MaxValue;
			foreach (DateOnlyPeriodWithAbsenceRequestPeriod layer in _layerCollectionOriginal)
			{
				DateOnly layerTime = layer.Period.StartDate;
				if (layerTime > currentTime && layerTime < retTime)
					retTime = layerTime;
			}
			return retTime;
		}

		private string getSuggestedPeriodDateString(DateOnlyPeriod suggestedPeriod, IList<DateOnly> denyDays)
		{
			var dayCollection = suggestedPeriod.DayCollection();
			foreach (var denyDay in denyDays)
			{
				dayCollection.Remove(denyDay);
			}
			var periods = dayCollection.SplitToContinuousPeriods();
			return string.Join(",", periods.Select(p => p.ToShortDateString(_dateCultureInfo)));
		}

		private class DateOnlyPeriodWithAbsenceRequestPeriod
		{
			public DateOnlyPeriod Period { get; set; }
			public IAbsenceRequestOpenPeriod AbsenceRequestOpenPeriod { get; set; }

			public DateOnlyPeriod LimitPeriod { get; set; }

			public bool IsWithinLimit
			{
				get { return Period.Intersection(LimitPeriod).HasValue; }
			}
		}
	}
}