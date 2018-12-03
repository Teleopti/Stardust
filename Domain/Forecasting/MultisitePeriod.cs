using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class MultisitePeriod : AggregateEntity, IMultisitePeriod
    {
        private readonly DateTimePeriod _period;
        private IDictionary<IChildSkill, Percent> _distribution;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultisitePeriod"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="distribution">The distribution.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public MultisitePeriod(DateTimePeriod period, IDictionary<IChildSkill, Percent> distribution) : this()
        {
            _period = period;
            _distribution = new Dictionary<IChildSkill, Percent>(distribution);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultisitePeriod"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-01
        /// </remarks>
        protected MultisitePeriod() { }

        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-01
        /// </remarks>
        public virtual DateTimePeriod Period
        {
            get { return _period; }
        }

        /// <summary>
        /// Gets the distribution.
        /// </summary>
        /// <value>The distribution.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public virtual IDictionary<IChildSkill, Percent> Distribution
        {
            get { return new ReadOnlyDictionary<IChildSkill, Percent>(_distribution); }
        }

        /// <summary>
        /// Sets the percentage.
        /// </summary>
        /// <param name="childSkill">The child skill.</param>
        /// <param name="percentage">The percentage.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public virtual void SetPercentage(IChildSkill childSkill, Percent percentage)
        {
	        Percent value;
	        if (!_distribution.TryGetValue(childSkill, out value) || value != percentage)
            {
                OnChangeMultisitePeriodData();
            }
            _distribution[childSkill] = percentage;
        }

        /// <summary>
        /// Called when [change multisite period data].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-22
        /// </remarks>
        protected virtual void OnChangeMultisitePeriodData()
        {
			if (Parent is IMultisiteDay multisiteDay)
            {
                multisiteDay.UpdateTemplateName();
                return;
            }

			if (Parent is IMultisiteDayTemplate multisiteDayTemplate)
            {
                multisiteDayTemplate.IncreaseVersionNumber();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public virtual bool IsValid
        {
            get
            {
                decimal sum = 0m;
                foreach (Percent percent in _distribution.Values)
                    sum += (decimal)percent.Value;
                return (sum == 1m ||
                        sum == 0m);
            }
        }

        #region ICloneable Members

        ///<summary>
        ///Creates a new object that is a copy of the current instance.
        ///</summary>
        ///
        ///<returns>
        ///A new object that is a copy of this instance.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public virtual object Clone()
        {
            return NoneEntityClone();
        }

        #endregion

        public virtual IMultisitePeriod NoneEntityClone()
        {
            MultisitePeriod retobj = (MultisitePeriod)MemberwiseClone();
            retobj.SetId(null);
            retobj._distribution = new Dictionary<IChildSkill, Percent>();
            foreach (var keyValuePair in _distribution)
            {
                retobj._distribution.Add(keyValuePair.Key, keyValuePair.Value);
            }
            return retobj;
        }

        public virtual IMultisitePeriod EntityClone()
        {
            MultisitePeriod retobj = (MultisitePeriod)MemberwiseClone();
            retobj._distribution = new Dictionary<IChildSkill, Percent>();
            foreach (var keyValuePair in _distribution)
            {
                retobj._distribution.Add(keyValuePair.Key, keyValuePair.Value);
            }
            return retobj;
        }
    }
}