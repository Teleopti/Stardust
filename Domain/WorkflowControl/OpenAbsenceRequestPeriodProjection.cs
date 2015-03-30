using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class OpenAbsenceRequestPeriodProjection : IOpenAbsenceRequestPeriodProjection
	{
		private readonly IOpenAbsenceRequestPeriodExtractor _openAbsenceRequestPeriodExtractor;

		private IList<DateOnlyPeriodWithAbsenceRequestPeriod> _layerCollectionOriginal;
		private CultureInfo _cultureInfo;

		public OpenAbsenceRequestPeriodProjection(IOpenAbsenceRequestPeriodExtractor openAbsenceRequestPeriodExtractor)
		{
			_openAbsenceRequestPeriodExtractor = openAbsenceRequestPeriodExtractor;
		}

		public IList<IAbsenceRequestOpenPeriod> GetProjectedPeriods(DateOnlyPeriod limitToPeriod, CultureInfo personCulture)
		{
			_cultureInfo = personCulture;

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
						currentTime = layerEndTime.AddDays(1);
						layerFound = true;
						break;
					}
				}
				if (!layerFound)
					currentTime = findNextTimeSlot(currentTime);
			}

			return workingColl.Where(w => w.GetPeriod(DateOnly.Today).Intersection(limitToPeriod).HasValue).ToList();
		}

		private void AddBottomLayer(DateOnlyPeriod limitToPeriod)
		{

			var denyReason = UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonClosedPeriod", _cultureInfo);
		
			foreach (var absenceRequestOpenPeriod in _openAbsenceRequestPeriodExtractor.AllPeriods)
			{
				
				var result = checkingForOpenPeriod(absenceRequestOpenPeriod);
				if (!string.IsNullOrEmpty(result))
				{
					denyReason = result;
					continue;
				}
				

				result = checkingForPeriod(limitToPeriod, absenceRequestOpenPeriod);
				if (!string.IsNullOrEmpty(result))
					denyReason = result;

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

		private string checkingForOpenPeriod(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			string denyReason = null;

			if (isPeriodOpensLater(absenceRequestOpenPeriod) && isNextOpenPeriod(absenceRequestOpenPeriod))
			{
				denyReason = string.Format(_cultureInfo,
				                           UserTexts.Resources.ResourceManager.GetString(
					                           "RequestDenyReasonPeriodOpenAfterSendRequest", _cultureInfo),
				                           absenceRequestOpenPeriod.OpenForRequestsPeriod.StartDate.ToShortDateString(_cultureInfo));
			}
			return denyReason;
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

		private DateOnly earlierDate = DateOnly.MaxValue;
		private bool isNextPeriod(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			var dateToCheck = absenceRequestOpenPeriod.GetPeriod(_openAbsenceRequestPeriodExtractor.ViewpointDate).StartDate;
			if (dateToCheck < earlierDate)
			{
				earlierDate = dateToCheck;
				return true;
			}
			return false;
		}

		private bool isPeriodOutside(DateOnlyPeriod requestPeriod, IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			DateOnlyPeriod period = absenceRequestOpenPeriod.GetPeriod(_openAbsenceRequestPeriodExtractor.ViewpointDate);
			var isPeriodInsideOrEqual = (period.StartDate <= requestPeriod.StartDate && period.EndDate >= requestPeriod.EndDate);
			return !isPeriodInsideOrEqual;
		}

		private string checkingForPeriod(DateOnlyPeriod requestPeriod, IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			string denyReason = null;
			DateOnlyPeriod period = absenceRequestOpenPeriod.GetPeriod(_openAbsenceRequestPeriodExtractor.ViewpointDate);

			if (isPeriodOutside(requestPeriod, absenceRequestOpenPeriod) && isNextPeriod(absenceRequestOpenPeriod))
			{
				denyReason = string.Format(_cultureInfo,
				                           UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonNoPeriod", _cultureInfo),
				                           period.ToShortDateString(_cultureInfo));
			}
			return denyReason;
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
			DateOnly retTime = new DateOnly(DateTime.MaxValue);
			foreach (DateOnlyPeriodWithAbsenceRequestPeriod layer in _layerCollectionOriginal)
			{
				DateOnly layerTime = layer.Period.StartDate;
				if (layerTime > currentTime && layerTime < retTime)
					retTime = layerTime;
			}
			return retTime;
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