using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Class for Outlier, formerly known as Exception
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-05-14
    /// </remarks>
    public class Outlier : AggregateRootWithBusinessUnit, IOutlier
    {
        private Description _description;
        private IList<IOutlierDateProvider> _outlierDateProviders;
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
            _outlierDateProviders = new List<IOutlierDateProvider>();
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
            if (_outlierDateProviders.Count > 0)
            {
                _outlierDateProviders.ForEach(o => dates = dates.Concat(o.GetDates(period)));
            }

            return dates.ToList();
        }

        /// <summary>
        /// Adds the date provider.
        /// </summary>
        /// <param name="dateProvider">The date provider.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        public virtual void AddDateProvider(IOutlierDateProvider dateProvider)
        {
            _outlierDateProviders.Add(dateProvider);
            dateProvider.SetParent(this);
        }

        /// <summary>
        /// Removes the date provider.
        /// </summary>
        /// <param name="dateProvider">The date provider.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        public virtual void RemoveDateProvider(IOutlierDateProvider dateProvider)
        {
            if (_outlierDateProviders.Contains(dateProvider))
            {
                dateProvider.SetParent(null);
                _outlierDateProviders.Remove(dateProvider);
            }
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
        /// Gets all date providers.
        /// </summary>
        /// <value>All date providers.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-16
        /// </remarks>
        public virtual ReadOnlyCollection<IOutlierDateProvider> OutlierDateProviders
        {
            get { return new ReadOnlyCollection<IOutlierDateProvider>(_outlierDateProviders); }
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
            retobj._outlierDateProviders = new List<IOutlierDateProvider>();
            foreach (IOutlierDateProvider dateProvider in _outlierDateProviders)
            {
                var cloneProvider = dateProvider.NoneEntityClone();
                cloneProvider.SetParent(retobj);
                retobj._outlierDateProviders.Add(cloneProvider);
            }
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
            retobj._outlierDateProviders = new List<IOutlierDateProvider>();
            foreach (IOutlierDateProvider dateProvider in _outlierDateProviders)
            {
                var cloneProvider = dateProvider.EntityClone();
                cloneProvider.SetParent(retobj);
                retobj._outlierDateProviders.Add(cloneProvider);
            }
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
