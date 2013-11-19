using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.TimeLayer
{
    public class MultiplicatorDefinitionSet : VersionedAggregateRootWithBusinessUnit, IMultiplicatorDefinitionSet, IDeleteTag
    {
        private IList<IMultiplicatorDefinition> _definitionCollection;
        private string _name;
        private readonly MultiplicatorType _multiplicatorType;
        private bool _isDeleted;

        protected MultiplicatorDefinitionSet(){}

        public MultiplicatorDefinitionSet(string name, MultiplicatorType multiplicatorType)
            : this()
        {
            _name = name;
            _multiplicatorType = multiplicatorType;
            _definitionCollection = new List<IMultiplicatorDefinition>();
        }

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual IList<IMultiplicatorDefinition> DefinitionCollection
        {
            get { return _definitionCollection; }
        }

        public virtual MultiplicatorType MultiplicatorType
        {
            get { return _multiplicatorType; }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual void AddDefinition(IMultiplicatorDefinition definition)
        {
            if(definition.Multiplicator.MultiplicatorType != MultiplicatorType)
                throw new ArgumentException("MultiplicatorType must be same for definition and definitionset", "definition");

            definition.SetParent(this);
            _definitionCollection.Add(definition);
        }

        public virtual void AddDefinitionAt(IMultiplicatorDefinition definition, int orderIndex)
        {
            if (definition.Multiplicator.MultiplicatorType != MultiplicatorType)
                throw new ArgumentException("MultiplicatorType must be same for definition and definitionset", "definition");

            definition.SetParent(this);
            _definitionCollection.Insert(orderIndex, definition);
        }

        public virtual void RemoveDefinition(IMultiplicatorDefinition definition)
        {
            _definitionCollection.Remove(definition);
        }

        public virtual void MoveDefinitionUp(IMultiplicatorDefinition definition)
        {
            int index = _definitionCollection.IndexOf(definition);
            if (index > 0)
            {
                _definitionCollection.Remove(definition);
                _definitionCollection.Insert(index - 1, definition);
            }
        }

        public virtual void MoveDefinitionDown(IMultiplicatorDefinition definition)
        {
            int index = _definitionCollection.IndexOf(definition);
            if (index < _definitionCollection.Count - 1)
            {
                _definitionCollection.Remove(definition);
                _definitionCollection.Insert(index + 1, definition);
            }
        }

        public virtual IList<IMultiplicatorLayer> CreateProjectionForPeriod(DateOnlyPeriod period, TimeZoneInfo timeZoneInfo)
        {
            List<IMultiplicatorLayer> unMergedList = new List<IMultiplicatorLayer>();
            foreach (var definition in _definitionCollection)
            {
                unMergedList.AddRange(definition.GetLayersForPeriod(period, timeZoneInfo));
            }

            if (unMergedList.Count == 0) return unMergedList;

            IList<IMultiplicatorLayer> workingColl = new List<IMultiplicatorLayer>();
            DateTime startTimeTemp = unMergedList.Min(p => p.Period.StartDateTime);

            DateTime endTime = unMergedList.Max(p => p.Period.EndDateTime);
            DateTime currentTime = startTimeTemp;
            IMultiplicatorLayer workingLayer;

            while (currentTime < endTime)
            {
                bool layerFound = false;
                for (int inverseLoop = unMergedList.Count - 1; inverseLoop >= 0; inverseLoop--)
                {
                    workingLayer = unMergedList[inverseLoop];
                    if (workingLayer.Period.Contains(currentTime))
                    {
                        DateTime layerEndTime = findLayerEndTime(inverseLoop, workingLayer, currentTime, unMergedList);
                        IMultiplicatorLayer newLayer = new MultiplicatorLayer(this, workingLayer.Payload,
                                                                              new DateTimePeriod(
                                                                                  currentTime,
                                                                                  layerEndTime));
                        workingLayer.LayerOriginalPeriod = workingLayer.Period;
                        workingColl.Add(newLayer);
                        currentTime = layerEndTime;
                        layerFound = true;
                        break;
                    }
                }
                if (!layerFound)
                    currentTime = findNextTimeSlot(currentTime, unMergedList);
            }
            return workingColl;
        }

        private static DateTime findLayerEndTime(int currentLayerIndex, IMultiplicatorLayer workingLayer, DateTime currentTime, IList<IMultiplicatorLayer> layers)
        {
            DateTime layerEndTime = workingLayer.Period.EndDateTime;
            if (currentLayerIndex != layers.Count - 1)
            {
                int orgLayerCount = layers.Count;
                for (int higherPrioLoop = currentLayerIndex + 1; higherPrioLoop < orgLayerCount; higherPrioLoop++)
                {
                    DateTimePeriod higherPrioLayerPeriod = layers[higherPrioLoop].Period;
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

        private static DateTime findNextTimeSlot(DateTime currentTime, IEnumerable<IMultiplicatorLayer> layers)
        {
            DateTime retTime = DateTime.MaxValue;
            foreach (IMultiplicatorLayer layer in layers)
            {
                DateTime layerTime = layer.Period.StartDateTime;
                if (layerTime > currentTime && layerTime < retTime)
                    retTime = layerTime;
            }
            return retTime;
        }

        private void ReInitializeCollections()
        {
            _definitionCollection = new List<IMultiplicatorDefinition>();
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id set to null.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-27
        /// </remarks>
        public virtual IMultiplicatorDefinitionSet NoneEntityClone()
        {
            MultiplicatorDefinitionSet clone = (MultiplicatorDefinitionSet)MemberwiseClone();
            clone.ReInitializeCollections();
            clone.SetId(null);
            foreach (IMultiplicatorDefinition definition in _definitionCollection)
                clone.AddDefinition(definition.NoneEntityClone());
            return clone;
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id as this T.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        public virtual IMultiplicatorDefinitionSet EntityClone()
        {
            MultiplicatorDefinitionSet clone = (MultiplicatorDefinitionSet)MemberwiseClone();
            clone.ReInitializeCollections();
            foreach (IMultiplicatorDefinition definition in _definitionCollection)
                clone.AddDefinition(definition.NoneEntityClone());
            return clone;
        }

        public virtual object Clone()
        {
            return EntityClone();
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}
