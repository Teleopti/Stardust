using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Filter
{
    public abstract class SpecificationFilterViewModelBase : DependencyObject,INotifyPropertyChanged
    {
        #region properties
        public bool FilterIsActive
        {
            get { return (bool)GetValue(FilterIsActiveProperty); }
            set { SetValue(FilterIsActiveProperty, value); }
        }
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public bool ShowOnlyBySpecification
        {
            get { return (bool)GetValue(ShowOnlyBySpecificationProperty); }
            set { SetValue(ShowOnlyBySpecificationProperty, value); }
        }

        #region backing dep props
        public static readonly DependencyProperty ShowOnlyBySpecificationProperty =
            DependencyProperty.Register("ShowOnlyBySpecification", typeof(bool), typeof(SpecificationFilterViewModelBase), new UIPropertyMetadata(false, ShowOnlyBySpecificationChanged));
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(SpecificationFilterViewModelBase), new UIPropertyMetadata(false));
        public static readonly DependencyProperty FilterIsActiveProperty =
            DependencyProperty.Register("FilterIsActive", typeof(bool), typeof(SpecificationFilterViewModelBase), new UIPropertyMetadata(false));

        #endregion
        #endregion

        #region abstract
        protected abstract void ShowOnlyBySpecificationChanged();
        public abstract int Total { get; }
        public abstract int Shown { get; }
        public abstract int Filtered { get; }
        #endregion

        #region private
        private static void ShowOnlyBySpecificationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((SpecificationFilterViewModelBase)d).ShowOnlyBySpecificationChanged();
            }
        }
        #endregion

        public  event PropertyChangedEventHandler PropertyChanged;

        protected void SendPropertyChanged(string property)
        {
        	PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }

    public class SpecificationFilterViewModel<T> : SpecificationFilterViewModelBase
    {
        #region fields
        private SpecificationFilter<T> _specificationFilter;
        private IEnumerable<T> _targetCollection;
        #endregion

        #region  properties for view
        public IEnumerable<T> TargetCollection { get { return _targetCollection;}  }
        public CommandModel FilterOnOffCommand { get; private set; }
        public CommandModel FilterSelectedOnOffCommand { get; private set; }
        public ICollectionView FilteredOutView { get; private set; }
        public ICollectionView ShowOnlyView { get; private set; }
        public ICollectionView TotalView { get; private set; }


        #endregion

        public SpecificationFilterViewModel(IEnumerable<T> targetCollection, ISpecification<T> filter)
        {
            _specificationFilter = new SpecificationFilter<T>() { Filter = filter };
            SetTargetCollection(targetCollection);
            FilterOnOffCommand = CommandModelFactory.CreateCommandModel(ToggleFilter, UserTexts.Resources.Filter);
            FilterSelectedOnOffCommand = CommandModelFactory.CreateCommandModel(delegate
            {
                if (IsSelected) ToggleFilter();
            }
            , UserTexts.Resources.Filter);

         

        }

        public void SetTargetCollection(IEnumerable<T> targetCollection)
        {
            _targetCollection = targetCollection;
            TotalView = new CollectionView(_targetCollection);
            FilteredOutView = new CollectionView(_targetCollection);
            ShowOnlyView = new CollectionView(_targetCollection);
            FilteredOutView.Filter = _specificationFilter.FilterOutSpecification;
            ShowOnlyView.Filter = _specificationFilter.FilterAllButSpecification;
            SetTotals();
        }

        #region private
        private void ToggleFilter()
        {
            FilterIsActive = !FilterIsActive;
            SetFilterOnDefaultView();
        }

        private void SetFilterOnDefaultView()
        {
            if (FilterIsActive)
            {
                if (ShowOnlyBySpecification)
                    CollectionViewSource.GetDefaultView(_targetCollection).Filter =
                        _specificationFilter.FilterAllButSpecification;
                else _specificationFilter.FilterCollection(_targetCollection);
            }
            else
                CollectionViewSource.GetDefaultView(_targetCollection).Filter = null;
        }

        private void SetTotals()
        {
            //This needs to be done when the filter is changed as well....
            //For now, just Notify.....
            SendPropertyChanged(nameof(Total));
            SendPropertyChanged(nameof(Filtered));
            SendPropertyChanged(nameof(Shown));
           
        }
        #endregion

        protected override void ShowOnlyBySpecificationChanged()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(_targetCollection);
            if (view.Filter != null) SetFilterOnDefaultView();
        }

        public override int Total
        {
            get { return _targetCollection.Count(); }
        }

        public override int Shown
        {
            get { return _targetCollection.Count() - Filtered; }
        }

        public override int Filtered
        {
            get
            {
                return _targetCollection.Count(i => _specificationFilter.Filter.IsSatisfiedBy(i));
            }
        }
    }
}
