using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Time
{
    /// <summary>
    /// Colelction that holds intervals
    /// </summary>
    public class IntervalCollection : ObservableCollection<IntervalViewModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalCollection"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="interval">The interval.</param>
        /// <remarks>
        /// Creates Intervals based on the period and the interval
        /// </remarks>
        public IntervalCollection(DateTimePeriod period, TimeSpan interval)
        {
            period.Intervals(interval).ForEach(p => Add(new IntervalViewModel() { Period = p }));
        }


        /// <summary>
        /// Changes the specified period and recreates the intervals
        /// </summary>
        /// <param name="period">The period.</param>
        public void Change(DateTimePeriod period)
        {
            TimeSpan interval = this.First().Period.ElapsedTime(); 
            Change(period.Intervals(interval));
        }

        /// <summary>
        /// Changes the specified interval and recreates the intervals
        /// </summary>
        /// <param name="interval">The interval.</param>
        public void Change(TimeSpan interval)
        {
            Change(TotalPeriod().Intervals(interval));
        }

        private void Change(IList<DateTimePeriod> periods)
        {
            //Do as little as possible to the collection, every change triggers CollectionChange that can trigger changes in the gui
            IEqualityComparer<IntervalViewModel> comparer = new IntervalViewModelComparer();

            IList<IntervalViewModel> newModels = new List<IntervalViewModel>();
            periods.ForEach(p => newModels.Add(new IntervalViewModel() { Period = p }));

            var alreadyExisting = this.Intersect(newModels, comparer);
            this.Except(alreadyExisting, comparer).ToList().ForEach(vm => Remove(vm)); 
            newModels.Except(alreadyExisting, comparer).ToList().ForEach(Add);
        }

        private DateTimePeriod TotalPeriod()
        {

            return new DateTimePeriod(this.Min(p => p.Period.StartDateTime),this.Max(p=>p.Period.EndDateTime));
        }

        #region compare
        private class IntervalViewModelComparer : IEqualityComparer<IntervalViewModel>
        {
            public bool Equals(IntervalViewModel x, IntervalViewModel y)
            {
                return (x.Period == y.Period);
            }

            public int GetHashCode(IntervalViewModel model)
            {
                return model.Period.GetHashCode();
            }
        }
        #endregion
    }

   
}
