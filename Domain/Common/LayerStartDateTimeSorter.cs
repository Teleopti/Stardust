using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Sorts a list of Layers according to startdatetime
    /// </summary>
    public class LayerStartDateTimeSorter<T> : ILayerSorter<T>
    {
        private readonly bool _ascSortOrder;

        /// <summary>
        /// Creates instance of LayerStartDateTimeSorter
        /// </summary>
        /// <param name="ascendingSortOrder"></param>
        public LayerStartDateTimeSorter(bool ascendingSortOrder)
        {
            _ascSortOrder = ascendingSortOrder;
        }

        #region IComparer<T> Members

        /// <summary>
        /// Returns an int value indicating sort order
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(ILayer<T> x, ILayer<T> y)
        {
            InParameter.NotNull("x", x);
            InParameter.NotNull("y", y);
            if (_ascSortOrder)
            {
                return x.Period.StartDateTime.CompareTo(y.Period.StartDateTime);
            }
            return x.Period.StartDateTime.CompareTo(y.Period.StartDateTime)*-1;
        }

        #endregion
    }
}