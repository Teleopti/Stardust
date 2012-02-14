using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.WinCode.Common.Models;

namespace Teleopti.Ccc.WinCode.Common
{
    public abstract class LayerGroupViewModel:DataModel
    {
        private readonly IList<ILayerViewModel> _source;
        private ICollectionView _layers;

        protected LayerGroupViewModel(string description, IList<ILayerViewModel> models)
        {
            Description = description;
            _source = models;
        }

        public string Description { get; private set; }


        public ICollectionView Layers
        {
            get
            {
                if (_layers == null)
                {
                    CollectionViewSource viewSource = new CollectionViewSource() { Source = _source };
                    _layers = viewSource.View;
                    _layers.Filter = FilterLayers;
                }
                return _layers;
            }
        }

        private bool FilterLayers(object obj)
        {
            return FilterByTypeSpecification.And(FilterByProjection).IsSatisfiedBy(obj as ILayerViewModel);
        }

        public Specification<ILayerViewModel> FilterByTypeSpecification
        {
            get; protected set;
        }

        public Specification<ILayerViewModel> FilterByProjection
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the index of the order they should be shown in.
        /// </summary>
        /// <value>The index of the order.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-07-26
        /// </remarks>
        public int OrderIndex { get; set; }
    }

    public class LayerGroupViewModel<T> : LayerGroupViewModel
    {
        public LayerGroupViewModel(string description, IList<ILayerViewModel> models)
            : base(description, models)
        {
            FilterByTypeSpecification = new LayerViewModelFilter<T>();
            FilterByProjection = new ProjectionFilter();
        }

        private class ProjectionFilter : Specification<ILayerViewModel>
        {
            public override bool IsSatisfiedBy(ILayerViewModel obj)
            {
                return (!obj.IsProjectionLayer);
            }
        }

        private class LayerViewModelFilter<TU> : Specification<ILayerViewModel>
        {
            public override bool IsSatisfiedBy(ILayerViewModel obj)
            {
                return (obj is TU);
            }
        }
    }
}