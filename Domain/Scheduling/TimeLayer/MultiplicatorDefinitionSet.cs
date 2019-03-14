using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.TimeLayer
{
	public class MultiplicatorDefinitionSet : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IMultiplicatorDefinitionSet, IDeleteTag
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

	    private T createEvent<T>() where T: MultiplicatorDefinitionSetChangedBase, new()
	    {
		    return new T
		    {
			    MultiplicatorDefinitionSetId = Id.GetValueOrDefault(),
			    MultiplicatorType = MultiplicatorType
		    };
	    }

	    public override void NotifyTransactionComplete(DomainUpdateType operation)
	    {
		    base.NotifyTransactionComplete(operation);
			switch (operation)
			{
				case DomainUpdateType.Insert:
					AddEvent(createEvent<MultiplicatorDefinitionSetCreated>());
					break;
				case DomainUpdateType.Update:
					AddEvent(createEvent<MultiplicatorDefinitionSetChanged>());
					break;
				case DomainUpdateType.Delete:
					AddEvent(createEvent<MultiplicatorDefinitionSetDeleted>());
					break;
			}
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
                throw new ArgumentException("MultiplicatorType must be same for definition and definitionset", nameof(definition));

            definition.SetParent(this);
            _definitionCollection.Add(definition);
        }

        public virtual void AddDefinitionAt(IMultiplicatorDefinition definition, int orderIndex)
        {
            if (definition.Multiplicator.MultiplicatorType != MultiplicatorType)
                throw new ArgumentException("MultiplicatorType must be same for definition and definitionset", nameof(definition));

            definition.SetParent(this);
            _definitionCollection.Insert(orderIndex, definition);
        }

        public virtual void RemoveDefinition(IMultiplicatorDefinition definition)
        {
            _definitionCollection.Remove(definition);
        }

        public virtual void MoveDefinitionUp(IMultiplicatorDefinition definition)
        {
            var index = _definitionCollection.IndexOf(definition);
            if (index > 0)
            {
                _definitionCollection.Remove(definition);
                _definitionCollection.Insert(index - 1, definition);
            }
        }

        public virtual void MoveDefinitionDown(IMultiplicatorDefinition definition)
        {
            var index = _definitionCollection.IndexOf(definition);
            if (index < _definitionCollection.Count - 1)
            {
                _definitionCollection.Remove(definition);
                _definitionCollection.Insert(index + 1, definition);
            }
        }

        public virtual IList<IMultiplicatorLayer> CreateProjectionForPeriod(DateOnlyPeriod period, TimeZoneInfo timeZoneInfo)
        {
            var unMergedList = new List<IMultiplicatorLayer>();
            foreach (var definition in _definitionCollection)
            {
                unMergedList.AddRange(definition.GetLayersForPeriod(period, timeZoneInfo));
            }

            if (unMergedList.Count == 0) return unMergedList;

            IList<IMultiplicatorLayer> workingColl = new List<IMultiplicatorLayer>();
            var startTimeTemp = unMergedList.Min(p => p.Period.StartDateTime);

            var endTime = unMergedList.Max(p => p.Period.EndDateTime);
            var currentTime = startTimeTemp;

	        while (currentTime < endTime)
            {
                var layerFound = false;
                for (var inverseLoop = unMergedList.Count - 1; inverseLoop >= 0; inverseLoop--)
                {
	                var workingLayer = unMergedList[inverseLoop];
	                if (workingLayer.Period.Contains(currentTime))
                    {
                        var layerEndTime = findLayerEndTime(inverseLoop, workingLayer, currentTime, unMergedList);
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
            var layerEndTime = workingLayer.Period.EndDateTime;
            if (currentLayerIndex != layers.Count - 1)
            {
                var orgLayerCount = layers.Count;
                for (var higherPrioLoop = currentLayerIndex + 1; higherPrioLoop < orgLayerCount; higherPrioLoop++)
                {
                    var higherPrioLayerPeriod = layers[higherPrioLoop].Period;
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
            var retTime = DateTime.MaxValue;
            foreach (var layer in layers)
            {
                var layerTime = layer.Period.StartDateTime;
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
            var clone = (MultiplicatorDefinitionSet)MemberwiseClone();
            clone.ReInitializeCollections();
            clone.SetId(null);
            foreach (var definition in _definitionCollection)
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
            var clone = (MultiplicatorDefinitionSet)MemberwiseClone();
            clone.ReInitializeCollections();
            foreach (var definition in _definitionCollection)
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
