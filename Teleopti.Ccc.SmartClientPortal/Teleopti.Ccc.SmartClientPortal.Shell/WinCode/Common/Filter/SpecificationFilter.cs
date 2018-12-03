using System.Collections.Generic;
using System.Windows.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Filter
{
    /// <summary>
    /// Filter for applying specifications on a observablecollection of T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Filters OUT the items that matches the specification
    /// The Show-method must match the WPF filter signature, so it cannot be Show(T model)
    /// Created by: henrika
    /// Created date: 2009-05-29
    /// </remarks>
    public class SpecificationFilter<T> 
    {
        public ISpecification<T> Filter { get; set; }

        /// <summary>
        /// Shows the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        /// <remarks>
        /// The signature should match the filter-method for CollectionViewSource
        /// Its reversed because its a filter
        /// Created by: henrika
        /// Created date: 2009-05-29
        /// </remarks>
        public bool FilterOutSpecification(object model)
        {
            InParameter.MustBeTrue("model", model is T);
            return (Filter == null) || !Filter.IsSatisfiedBy((T)model);
        }


        /// <summary>
        /// Filters all but objects satisfied by the specification specfification.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-30
        /// </remarks>
        public bool FilterAllButSpecification(object model)
        {
            InParameter.MustBeTrue("model", model is T);
            return (Filter != null) && Filter.IsSatisfiedBy((T)model);
        }

        /// <summary>
        /// Filters the collection.
        /// </summary>
        /// <param name="targetCollection">The target collection.</param>
        /// <remarks>
        /// Instead of setting the filter to the Show-method, you can send in a collection that gets filtered
        /// Created by: henrika
        /// Created date: 2009-05-29
        /// </remarks>
        public void FilterCollection(IEnumerable<T> targetCollection)
        {
            var defaultView = CollectionViewSource.GetDefaultView(targetCollection);
            defaultView.Filter = FilterOutSpecification;
        }

        public void FilterAllButCollection(IEnumerable<T> targetCollection)
        {
            var defaultView = CollectionViewSource.GetDefaultView(targetCollection);
            defaultView.Filter = FilterAllButSpecification;
        }
    }
}
