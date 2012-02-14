using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public abstract class Layer<T> : AggregateEntity, ILayer<T>
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

        object ILayer.Payload
        {
            get { return Payload; }
            set { Payload = (T)value; }
        }

        public virtual T Payload
        {
            get { return _payload; }
            set 
            {
                InParameter.NotNull("value", value);
                _payload = value; 
            }
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


        private int findOrderIndex()
        {
            return ((ILayerCollectionOwner<T>) Parent).LayerCollection.IndexOf(this);
        }

        public virtual void ChangeLayerPeriodEnd(TimeSpan timeSpan)
        {
            _period = _period.ChangeEndTime(timeSpan);
        }

        public virtual void ChangeLayerPeriodStart(TimeSpan timeSpan)
        {
            _period = _period.ChangeStartTime(timeSpan);
        }

        public virtual void MoveLayer(TimeSpan timeSpan)
        {
            _period = _period.MovePeriod(timeSpan);
        }

        public virtual void Transform(ILayer<T> layer)
        {
            _period = layer.Period;
            _payload = layer.Payload;
        }

        public virtual bool AdjacentTo(ILayer<T> layer)
        {
            InParameter.NotNull("layer", layer);
            return (Period.StartDateTime == layer.Period.EndDateTime || 
                        Period.EndDateTime == layer.Period.StartDateTime);
        }

        #region ICloneableEntity<Layer<T>> Members

        public virtual object Clone()
        {
            object retObj = NoneEntityClone();

            return retObj;
        }

        public virtual ILayer<T> NoneEntityClone()
        {
            Layer<T> retObj = (Layer<T>)MemberwiseClone();
            ((IEntity)retObj).SetId(null);
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
