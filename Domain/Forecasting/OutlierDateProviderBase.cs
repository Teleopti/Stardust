using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Abstract base class for outlier date providers
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-05-14
    /// </remarks>
    public abstract class OutlierDateProviderBase : AggregateEntity, IOutlierDateProvider
    {
        private string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutlierDateProviderBase"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-15
        /// </remarks>
        protected OutlierDateProviderBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutlierDateProviderBase"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-15
        /// </remarks>
        protected OutlierDateProviderBase(string name) : this()
        {
            InParameter.NotStringEmptyOrNull("name", name);
            _name = name;
        }

        /// <summary>
        /// Gets the dates.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        public abstract IList<DateOnly> GetDates(DateOnlyPeriod period);

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-15
        /// </remarks>
        public virtual string Name
        {
            get { return _name; }
            set {
                InParameter.NotStringEmptyOrNull("name", value);
                _name = value; }
        }

        public abstract object Clone();
        public abstract IOutlierDateProvider NoneEntityClone();
        public abstract IOutlierDateProvider EntityClone();
    }
}
