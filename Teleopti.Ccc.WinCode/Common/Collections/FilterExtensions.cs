using System.Collections.ObjectModel;
using System.Windows.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Common.Filter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Collections
{
    /// <summary>
    /// Extensionmethods for ObservableCollection for filtering
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-06-01
    /// </remarks>
    public static class FilterExtensions
    {
        /// <summary>
        /// Filters the by specification. This is set on the default view of the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="specification">The specification.</param>
        /// <remarks>
        /// Filters the default view
        /// Shows everything in collection but items satisfied by the specification
        /// Created by: henrika
        /// Created date: 2009-06-01
        /// </remarks>
        public static void FilterOutBySpecification<T>(this ObservableCollection<T> collection, ISpecification<T> specification)
        {
            var specificationFilter = new SpecificationFilter<T>();
            specificationFilter.Filter = specification;
            specificationFilter.FilterCollection(collection);
        }

        /// <summary>
        /// Creates the filtered view.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="specification">The specification.</param>
        /// <returns></returns>
        /// <remarks>
        /// Shows everything in collection but items satisfied by the specification
        /// Easier to do the cast here to ListCollectionView instead of ICollectionView (Obs. coll will always create ListCollectionView)
        /// Created by: henrika
        /// Created date: 2009-06-01
        /// </remarks>
        public static ListCollectionView CreateFilteredView<T>(this ObservableCollection<T> collection, ISpecification<T> specification)
        {
            CollectionViewSource source = new CollectionViewSource();
            source.Source = collection;
            SpecificationFilter<T> specificationFilter = new SpecificationFilter<T>();
            specificationFilter.Filter = specification;
            source.View.Filter += specificationFilter.FilterOutSpecification;
            return (ListCollectionView)source.View;
        }
    }
}
