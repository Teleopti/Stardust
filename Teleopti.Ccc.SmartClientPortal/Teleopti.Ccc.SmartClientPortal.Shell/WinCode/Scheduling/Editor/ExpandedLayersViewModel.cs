using System.ComponentModel;
using System.Windows;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor
{
    /// <summary>
    /// Viewmodel for an area of the LayerViewModelCollection
    /// Example: AbsenceLayers, PersonalShifts etc
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-09-01
    /// </remarks>
    public class ExpandedLayersViewModel:DependencyObject,INotifyPropertyChanged
    { 
        #region properties
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the show layers.
        /// </summary>
        /// <value>The show layers.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-09-01
        /// </remarks>
        public Visibility ShowLayers
        {
            get { return _showLayers; }
            private
            set
            {
                if (value != _showLayers)
                {
                    _showLayers = value;
                    SendPropertyChanged(nameof(ShowLayers));
                }
            }
        }

       
        /// <summary>
        /// Gets the show projection.
        /// </summary>
        /// <value>The show projection.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-09-01
        /// </remarks>
        public Visibility ShowProjection
        {
            get { return _showProjection; }
            private 
            set 
            { 
                if(value!=_showProjection)
                {
                    _showProjection = value;
                    SendPropertyChanged(nameof(ShowProjection));
                }
            }
        }

     
        /// <summary>
        /// Gets or sets a value indicating whether [hide projection in edit mode].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [hide projection in edit mode]; otherwise, <c>false</c>.
        /// </value>
        public bool HideProjectionInEditMode
        {
            get { return (bool) GetValue(HideProjectionInEditModeProperty); }
            set { SetValue(HideProjectionInEditModeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the period that the viewmodel should show
        /// </summary>
        public DateTimePeriod Period
        {
            get { return (DateTimePeriod)GetValue(PeriodProperty); }
            set { SetValue(PeriodProperty, value); }
        }

        /// <summary>
        /// Gets the underlying collection of layers
        /// </summary>
        public LayerViewModelCollection Layers { get; private set; }

        /// <summary>
        /// Gets or sets the ratio for the expanded layers
        /// </summary>
        /// <remarks>
        /// 0 = No rise for expanded layers, 1 = the rise is the same as the height of the layers:
        /// </remarks>
        public double Expanded
        {
            get { return (double)GetValue(ExpandedProperty); }
            set { SetValue(ExpandedProperty, value); }
        }

        /// <summary>
        /// Gets or sets if it should be in editmode
        /// </summary>
        /// <remarks>
        /// Editmode means that the shift will be expanded and that it will be able to move etc..
        /// </remarks>
        public bool EditMode
        {
            get { return (bool)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        /// <summary>
        /// The height of a layer
        /// </summary>
        public double LayerHeight
        {
            get { return (double)GetValue(LayerHeightProperty); }
            set { SetValue(LayerHeightProperty, value); }
        }
        #endregion
        
        #region fields

        private Visibility _showLayers=Visibility.Collapsed;
        private Visibility _showProjection=Visibility.Visible;

        public static readonly DependencyProperty HideProjectionInEditModeProperty =
           DependencyProperty.Register("HideProjectionInEditMode", typeof(bool), typeof(ExpandedLayersViewModel), new UIPropertyMetadata(true, EditModeChanged));


      
        public static readonly DependencyProperty LayerHeightProperty =
            DependencyProperty.Register("LayerHeight", typeof(double), typeof(ExpandedLayersViewModel), new UIPropertyMetadata(25d));

        public CommandModel ToggleEditModeCommand { get; private set; }

        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register("EditMode", typeof(bool), typeof(ExpandedLayersViewModel), new UIPropertyMetadata(false,EditModeChanged));

        

        public static readonly DependencyProperty PeriodProperty =
            DependencyProperty.Register("Period", typeof(DateTimePeriod), typeof(ExpandedLayersViewModel), new UIPropertyMetadata(new DateTimePeriod()));

        public static readonly DependencyProperty ExpandedProperty =
            DependencyProperty.Register("Expanded", typeof(double), typeof(ExpandedLayersViewModel), new UIPropertyMetadata(1d));
        #endregion

        public ExpandedLayersViewModel(LayerViewModelCollection layers)
        {
            Layers = layers;
            ToggleEditModeCommand = CommandModelFactory.CreateCommandModel(ToggleEdit, UserTexts.Resources.Edit);
        }

        #region methods
        private void ToggleEdit()
        {
            EditMode = !EditMode;
        }

        private static void EditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
           ((ExpandedLayersViewModel) d).SetVisability();
        }

        private void SetVisability()
        {
            if (EditMode)
            {
                ShowLayers = Visibility.Visible;
                ShowProjection = HideProjectionInEditMode ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                ShowLayers = Visibility.Collapsed;
                ShowProjection = Visibility.Visible;
            }
        }

        private void SendPropertyChanged(string property)
        {
        	PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(property));
        }

        #endregion

    }
}