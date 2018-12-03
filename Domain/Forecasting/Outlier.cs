using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Class for Outlier, formerly known as Exception
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-05-14
    /// </remarks>
    public class Outlier : VersionedAggregateRootWithBusinessUnit, IOutlier
    {
        private Description _description;
        private IList<DateOnly> _dates;
        private readonly IWorkload _workload;

        /// <summary>
        /// Initializes a new instance of the <see cref="Outlier"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        protected Outlier()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Outlier"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        public Outlier(Description description) : this()
        {
            _description = description;
            _dates = new List<DateOnly>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Outlier"/> class.
        /// </summary>
        /// <param name="workload">The workload.</param>
        /// <param name="description">The description.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        public Outlier(IWorkload workload, Description description) : this(description)
        {
            _workload = workload;
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-15
        /// </remarks>
        public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Gets the workload.
        /// </summary>
        /// <value>The workload.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        public virtual IWorkload Workload
        {
            get { return _workload; }
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
        public virtual IList<DateOnly> GetDatesByPeriod(DateOnlyPeriod period)
        {
            IEnumerable<DateOnly> dates = _dates.Where(period.Contains);

            return dates.ToList();
        }

        /// <summary>
        /// Adds the date.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        public virtual void AddDate(DateOnly dateTime)
        {
            if (!_dates.Contains(dateTime))
                _dates.Add(dateTime);
        }

        /// <summary>
        /// Removes the date.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        public virtual void RemoveDate(DateOnly dateTime)
        {
            if (_dates.Contains(dateTime))
                _dates.Remove(dateTime);
        }

        /// <summary>
        /// Gets all dates.
        /// </summary>
        /// <value>All dates.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        public virtual IList<DateOnly> Dates
        {
            get { return _dates; }
        }

        /// <summary>
        /// Gets the outliers by dates.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="outliers">The outliers.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-16
        /// </remarks>
        public static IDictionary<DateOnly, IOutlier> GetOutliersByDates(DateOnlyPeriod period, IList<IOutlier> outliers)
        {
            return (from o in outliers
                    from d in o.GetDatesByPeriod(period)
                    select new { Outlier = o, Date = d })
                .ToDictionary(d => d.Date, o => o.Outlier);
            //TODO! Fix this when having multiple outliers on one date...
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id set to null.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-16
        /// </remarks>
        public virtual IOutlier NoneEntityClone()
        {
            Outlier retobj = (Outlier)MemberwiseClone();
            retobj.SetId(null);
            retobj._dates = new List<DateOnly>(_dates);
            return retobj;
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id as this T.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-16
        /// </remarks>
        public virtual IOutlier EntityClone()
        {
            Outlier retobj = (Outlier)MemberwiseClone();
            retobj._dates = new List<DateOnly>(_dates);
            return retobj;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-16
        /// </remarks>
        public virtual object Clone()
        {
            return EntityClone();
        }
    }
}
