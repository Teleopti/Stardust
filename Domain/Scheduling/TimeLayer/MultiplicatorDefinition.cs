using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TimeLayer
{
    public abstract class MultiplicatorDefinition : AggregateEntity, IMultiplicatorDefinition
    {
        private IMultiplicator _multiplicator;
        private int _orderIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiplicatorDefinition"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-19
        /// </remarks>
        protected MultiplicatorDefinition()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiplicatorDefinition"/> class.
        /// </summary>
        /// <param name="multiplicator">The multiplicator.</param>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-19
        /// </remarks>
        protected MultiplicatorDefinition(IMultiplicator multiplicator) : this()
        {
            _multiplicator = multiplicator;
        }

        /// <summary>
        /// Gets the multiplicator.
        /// </summary>
        /// <value>The multiplicator.</value>
        public virtual IMultiplicator Multiplicator 
        { 
            get
            {
                return _multiplicator;
            }
            set
            {
                _multiplicator = value;
            }
        }

        /// <summary>
        /// Gets the index of the order.
        /// </summary>
        /// <value>The index of the order.</value>
        public virtual int OrderIndex
        {
            get
            {
                IMultiplicatorDefinitionSet owner = (IMultiplicatorDefinitionSet)Parent;
                InParameter.NotNull(nameof(owner), owner);
                _orderIndex = owner.DefinitionCollection.IndexOf(this);
                return _orderIndex;
            } 
            set
            {
                _orderIndex = value;
            }
        }

        /// <summary>
        /// Gets the layers for period.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="timeZoneInfo">The time zone info.</param>
        /// <returns></returns>
        public abstract IList<IMultiplicatorLayer> GetLayersForPeriod(DateOnlyPeriod period, TimeZoneInfo timeZoneInfo);

        public virtual IMultiplicatorDefinition NoneEntityClone()
        {
            IMultiplicatorDefinition clone = (IMultiplicatorDefinition)MemberwiseClone();
            clone.SetId(null);
            return clone;
        }
        
        public virtual IMultiplicatorDefinition EntityClone()
        {
            IMultiplicatorDefinition clone = (IMultiplicatorDefinition)MemberwiseClone();
            return clone;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public virtual object Clone()
        {
            return EntityClone();
        }
    }
}
