using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
    public class LayerCollection<T> : ReadOnlyCollection<ILayer<T>>, ILayerCollection<T>
    {
        private readonly ILayerCollectionOwner<T> _owner;

        internal LayerCollection(ILayerCollectionOwner<T> owner, IList<ILayer<T>> layerList) : base(layerList)
        {
            _owner = owner;
        }

        public LayerCollection() : base(new List<ILayer<T>>())
        {
        }

        public virtual bool Remove(ILayer<T> item)
        {
            return Items.Remove(item);
        }

        public virtual void Add(ILayer<T> item)
        {
            InParameter.NotNull("layer", item);
            if(_owner!=null)
                _owner.OnAdd(item);
            Items.Add(item);
	        var itemAsPersistedLayer = item as IPersistedLayer<T>;
					if (itemAsPersistedLayer != null)
					{
						itemAsPersistedLayer.SetParent((IEntity)_owner);						
					}
        }

        public DateTimePeriod? Period()
        {
            var count = Count;
            if (count == 0)
                return null;

            var firstPeriod = Items[0].Period;
            var min = firstPeriod.StartDateTime;
            var max = firstPeriod.EndDateTime;
            
            for (var i = 1; i < count; i++)
            {
                var period = Items[i].Period;
                if (period.StartDateTime < min)
                    min = period.StartDateTime;
                if (period.EndDateTime > max)
                    max = period.EndDateTime;
            }
            
            return new DateTimePeriod(min, max);
        }

        public DateTime? FirstStart()
        {
	        var count = Count;
            if (count == 0) return null;

	        DateTime firstStart = Items[0].Period.StartDateTime;
	        for (int i = 0; i < count; i++)
	        {
		        DateTime startDateTime = Items[i].Period.StartDateTime;
		        if (startDateTime < firstStart)
			        firstStart = startDateTime;
	        }
	        return firstStart;
        }

        public DateTime? LatestEnd()
        {
			var count = Count;
			if (count == 0) return null;

			DateTime firstEnd = Items[0].Period.EndDateTime;
			for (int i = 0; i < count; i++)
			{
				DateTime startDateTime = Items[i].Period.EndDateTime;
				if (startDateTime > firstEnd)
					firstEnd = startDateTime;
			}
			return firstEnd;
        }

        public void Clear()
        {
            Items.Clear();
        }

        public void AddRange(ILayerCollection<T> layerCollection)
        {
            layerCollection.ForEach(Add);
        }

        public void Insert(int index, ILayer<T> item)
        {
            Items.Insert(index, item);
        }

        public void MoveUpLayer(ILayer<T> layer)
        {
            var index = Items.IndexOf(layer);
            if (index == -1) return;
            Items.Remove(layer);
            Items.Insert(Math.Max(0, index - 1), layer);
        }

        public void MoveDownLayer(ILayer<T> layer)
        {
            var index = Items.IndexOf(layer);
            if (index == -1) return;
            Items.Remove(layer);
            Items.Insert(Math.Min(Items.Count, index + 1), layer);
        }

        public bool CanMoveUpLayer(ILayer<T> layer)
        {
            return Items.IndexOf(layer) > 0;
        }

        public bool CanMoveDownLayer(ILayer<T> layer)
        {
            return Items.Contains(layer) && Items.Last() != layer;
        }
    }
}