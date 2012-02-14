using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class OpenAbsenceRequestPeriodProjection : IOpenAbsenceRequestPeriodProjection
    {
        private readonly IOpenAbsenceRequestPeriodExtractor _openAbsenceRequestPeriodExtractor;
        private IList<DateOnlyPeriodWithAbsenceRequestPeriod> _layerCollectionOriginal;

        public OpenAbsenceRequestPeriodProjection(IOpenAbsenceRequestPeriodExtractor openAbsenceRequestPeriodExtractor)
        {
            _openAbsenceRequestPeriodExtractor = openAbsenceRequestPeriodExtractor;
        }

        public IList<IAbsenceRequestOpenPeriod> GetProjectedPeriods(DateOnlyPeriod limitToDateOnlyPeriod)
        {
            IList<IAbsenceRequestOpenPeriod> workingColl = new List<IAbsenceRequestOpenPeriod>();
            _layerCollectionOriginal =
                _openAbsenceRequestPeriodExtractor.AvailablePeriods.Select(
                    d =>
                    new DateOnlyPeriodWithAbsenceRequestPeriod
                    {
                        Period = d.GetPeriod(_openAbsenceRequestPeriodExtractor.ViewpointDate),
                        AbsenceRequestOpenPeriod = d,
                        LimitPeriod = limitToDateOnlyPeriod
                    }).Where(d => d.IsWithinLimit).ToList();
            AddBottomLayer(limitToDateOnlyPeriod);

            DateOnly startTimeTemp = _layerCollectionOriginal.Min(d => d.Period.StartDate);
            DateOnly endTime = _layerCollectionOriginal.Max(d => d.Period.EndDate);
            DateOnly currentTime = startTimeTemp;
            DateOnlyPeriodWithAbsenceRequestPeriod workingLayer;

            while (currentTime <= endTime)
            {
                bool layerFound = false;
                for (int inverseLoop = _layerCollectionOriginal.Count - 1; inverseLoop >= 0; inverseLoop--)
                {
                    workingLayer = _layerCollectionOriginal[inverseLoop];
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
            return workingColl.Where(w => w.GetPeriod(DateOnly.Today).Intersection(limitToDateOnlyPeriod).HasValue).ToList();
        }

        private void AddBottomLayer(DateOnlyPeriod limitToDateOnlyPeriod)
        {
            _layerCollectionOriginal.Insert(0,
                                            new DateOnlyPeriodWithAbsenceRequestPeriod
                                            {
                                                AbsenceRequestOpenPeriod =
                                                    new AbsenceRequestOpenDatePeriod
                                                    {
                                                        Absence = null,
                                                        AbsenceRequestProcess =
                                                            new DenyAbsenceRequest
                                                            {
                                                                DenyReason =
                                                                    "RequestDenyReasonClosedPeriod"
                                                            },
                                                        Period = limitToDateOnlyPeriod,
                                                        PersonAccountValidator =
                                                            new AbsenceRequestNoneValidator(),
                                                        StaffingThresholdValidator =
                                                            new AbsenceRequestNoneValidator()
                                                    },
                                                LimitPeriod = limitToDateOnlyPeriod,
                                                Period = limitToDateOnlyPeriod
                                            });
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