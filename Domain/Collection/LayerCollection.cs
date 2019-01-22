using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
    public class LayerCollection<T> : ReadOnlyCollection<ILayer<T>>, ILayerCollection<T>
    {
        private readonly ILayerCollectionOwner<T> _owner;

        internal LayerCollection(ILayerCollectionOwner<T> owner, List<ILayer<T>> layerList) : base(layerList)
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
		    InParameter.NotNull(nameof(item), item);
		    _owner?.OnAdd(item);
		    Items.Add(item);
		    var itemAsPersistedLayer = item as IAggregateEntity;
		    itemAsPersistedLayer?.SetParent((IEntity) _owner);
	    }

		public virtual void AddRange(IEnumerable<ILayer<T>> items)
		{
			InParameter.NotNull(nameof(items), items);
			var enumerable = items as ILayer<T>[] ?? items.ToArray();
			_owner?.OnAdd(enumerable);
			((List<ILayer<T>>)Items).AddRange(enumerable);
			var parent = (IEntity)_owner;
			foreach (var item in enumerable.OfType<IAggregateEntity>())
			{
				item.SetParent(parent);
			}
		}

		public DateTimePeriod? Period()
        {
	        return Items.OuterPeriod();
        }
		
        public void Clear()
        {
            Items.Clear();
        }

	    public void Insert(int index, ILayer<T> item)
	    {
		    var itemAsPersistedLayer = item as IAggregateEntity;
		    itemAsPersistedLayer?.SetParent((IEntity) _owner);
		    Items.Insert(index, item);
	    }
    }
}