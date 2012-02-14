using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// Creates projections of activity layers
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-01-29
    /// </remarks>
    public class VisualLayerProjectionService : IProjectionService
    {
        private readonly IPerson _person;
        private readonly ILayerCollection<IPayload> _layerCollectionOriginal;
        private readonly IVisualLayerFactory _standardVisualLayerFactory;

        public VisualLayerProjectionService(IPerson person)
        {
            _person = person;
            _layerCollectionOriginal = new LayerCollection<IPayload>();
            _standardVisualLayerFactory = new VisualLayerFactory();
        }

        public void Add(IShift shift)
        {
            InParameter.NotNull("shift", shift);
            IVisualLayerFactory factory = shift.CreateVisualLayerFactory();
            foreach (IActivityLayer layer in shift.LayerCollection)
            {
                _layerCollectionOriginal.Add(factory.CreateShiftSetupLayer(layer));
            }
        }

        public void Add(ILayer<IPayload> layer)
        {
            _layerCollectionOriginal.Add(layer);
        }

        public IVisualLayerCollection CreateProjection()
        {
            IList<IVisualLayer> workingColl = new List<IVisualLayer>();
            DateTime? startTimeTemp = _layerCollectionOriginal.FirstStart();
            if(startTimeTemp.HasValue)
            {
                DateTime endTime = _layerCollectionOriginal.LatestEnd().Value;
                DateTime currentTime = startTimeTemp.Value;
                IVisualLayer workingLayer;

                while (currentTime < endTime)
                {
                    bool layerFound = false;
                    for (int inverseLoop = _layerCollectionOriginal.Count - 1; inverseLoop >= 0; inverseLoop--)
                    {
                        workingLayer = (IVisualLayer)_layerCollectionOriginal[inverseLoop];
                        if (workingLayer.Period.Contains(currentTime))
                        {
                            DateTime layerEndTime = findLayerEndTime(inverseLoop, workingLayer, currentTime);
                            IVisualLayer newLayer = _standardVisualLayerFactory.CreateResultLayer(workingLayer.Payload, 
                                                                            workingLayer,
                                                                            new DateTimePeriod(currentTime,layerEndTime));
                            workingColl.Add(newLayer);
                            currentTime = layerEndTime;
                            layerFound = true;
                            break;
                        }
                    }
                    if (!layerFound)
                        currentTime = findNextTimeSlot(currentTime);
                }
            }
            return new VisualLayerCollection(_person, workingColl, new ProjectionPayloadMerger());
        }

        private DateTime findLayerEndTime(int currentLayerIndex,
                                          IVisualLayer workingLayer, 
                                          DateTime currentTime)
        {
            DateTime layerEndTime = workingLayer.Period.EndDateTime;
            if (currentLayerIndex != _layerCollectionOriginal.Count - 1)
            {
                int orgLayerCount = _layerCollectionOriginal.Count;
                for (int higherPrioLoop = currentLayerIndex + 1; higherPrioLoop < orgLayerCount; higherPrioLoop++)
                {
                    DateTimePeriod higherPrioLayerPeriod = _layerCollectionOriginal[higherPrioLoop].Period;
                    if (workingLayer.Period.Contains(higherPrioLayerPeriod.StartDateTime) &&
                        higherPrioLayerPeriod.EndDateTime > currentTime &&
                        higherPrioLayerPeriod.StartDateTime < layerEndTime)
                    {
                        layerEndTime = higherPrioLayerPeriod.StartDateTime;
                    }
                }
            }
            return layerEndTime;
        }

        private DateTime findNextTimeSlot(DateTime currentTime)
        { 
            DateTime retTime = DateTime.MaxValue;
            foreach (VisualLayer layer in _layerCollectionOriginal)
            {
                DateTime layerTime = layer.Period.StartDateTime;
                if (layerTime > currentTime && layerTime < retTime)
                    retTime = layerTime;
            }
            return retTime;
        }
    }
}