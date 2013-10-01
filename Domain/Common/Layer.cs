using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public abstract class Layer<T> : ILayer<T>
    {
        private DateTimePeriod _period;
        private readonly T _payload;

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


        public virtual object Clone()
        {
            object retObj = NoneEntityClone();

            return retObj;
        }

        public virtual ILayer<T> NoneEntityClone()
        {
            var retObj = (ILayer<T>)MemberwiseClone();
	        var entity = retObj as IEntity;
			if(entity != null)
				entity.SetId(null);
            return retObj;
        }

        public virtual ILayer<T> EntityClone()
        {
            return (ILayer<T>)MemberwiseClone();
        }

    }
}
