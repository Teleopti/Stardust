using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public abstract class Layer<T> : ILayer<T>
    {
        private DateTimePeriod _period;
        private T _payload;

	    protected Layer(T payload, DateTimePeriod period)
        {
            InParameter.NotNull("payload", payload);
            _period = period;
            _payload = payload;
        }

        protected Layer()
        {
        }

        public virtual DateTimePeriod Period
        {
            get { return _period; }
            set
            {
                _period = value;
            }
        }

        public virtual T Payload
        {
            get { return _payload; }
        }

        public virtual int OrderIndex
        {
            get
            {
                int ret;
                if (Parent == null)
                {
					ret = -1;
                }
                else
                {
                    ret = findOrderIndex();
                }
	            
                return ret;
            }
			
        }


        protected virtual int findOrderIndex()
        {
			return ((ILayerCollectionOwner<T>)Parent).LayerCollection.IndexOf(this);
        }


		public virtual bool AdjacentTo(IPeriodized layer)
        {
            InParameter.NotNull("layer", layer);
            return (Period.StartDateTime == layer.Period.EndDateTime || 
                        Period.EndDateTime == layer.Period.StartDateTime);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "persistedactivity")]
		public virtual void SetParent(IEntity parent)
		{
			throw new NotSupportedException("Only persistedactivity layer supports parenting.");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "persistedactivity")]
		public virtual IEntity Parent
	    {
		    get
		    {
					throw new NotSupportedException("Only persistedactivity layer supports parenting."); 
		    }
	    }

	    #region ICloneableEntity<Layer<T>> Members

        public virtual object Clone()
        {
            object retObj = NoneEntityClone();

            return retObj;
        }

        public virtual ILayer<T> NoneEntityClone()
        {
            var retObj = (Layer<T>)MemberwiseClone();
	        var entity = retObj as IEntity;
			if(entity != null)
				entity.SetId(null);
            return retObj;
        }

        public virtual ILayer<T> EntityClone()
        {
            Layer<T> retObj = (Layer<T>)MemberwiseClone();
            return retObj;
        }

        #endregion
    }
}
