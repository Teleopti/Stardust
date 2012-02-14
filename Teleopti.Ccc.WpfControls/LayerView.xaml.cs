﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using UserControl=System.Windows.Controls.UserControl;

namespace Teleopti.Ccc.WpfControls
{
    /// <summary>
    /// Interaction logic for WrapperView.xaml
    /// Does the timecalculation on dragging
    /// This could be moved directly to the viewmodel with a LOT of effort
    /// interaction is "glued" to the viewmodel here instead
    /// </summary>
    /// <remarks>
    /// The changed events are handled here instead of in the viewmodel. 
    /// Could be done with some sort of delegate-events in the future (so the gui isnt dependent on this class, just the viewmodel)
    /// </remarks>
    public partial class LayerView : UserControl
    {
        private DateTimePeriod _initialPeriod;

        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedEvent LayerChangedEvent = EventManager.RegisterRoutedEvent(
            "LayerChanged", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (LayerView));
        
       
        public event RoutedEventHandler LayerChanged
        {
            add { AddHandler(LayerChangedEvent, value); }
            remove { RemoveHandler(LayerChangedEvent,value);}
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedEvent PreviewLayerSelectedEvent = EventManager.RegisterRoutedEvent(
            "PreviewLayerSelected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LayerView));

        public event RoutedEventHandler PreviewLayerSelected
        {
            add { AddHandler(PreviewLayerSelectedEvent, value); }
            remove { RemoveHandler(PreviewLayerSelectedEvent, value); }
        }
       
       

        public LayerView()
        {
            InitializeComponent();
            PreviewMouseDown += LayerView_PreviewMouseDown;
            PreviewMouseUp += new MouseButtonEventHandler(LayerView_PreviewMouseUp);
        }

        void LayerView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(hiddenButtonForFocusThatShouldBeHandledInViewModelInstead);
            
        }

        void LayerView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ILayerViewModel model = DataContext as LayerViewModel;
            if (model!=null)
            {
                RaiseEvent(new RoutedEventArgs(PreviewLayerSelectedEvent, model));
            }

        }

       
        private void Thumb_Start_DragDelta(object sender, DragDeltaEventArgs e)
        {
            //Problem: If somebodey implements another itemcontrol, this will not work!
            ContentPresenter item = FindVisualParent<ContentPresenter>(this);
            ILayerViewModel model = DataContext as LayerViewModel;
            if (item != null && model != null)
            {
                model.StartTimeChanged(item, e.HorizontalChange);
            }
        }

        //This needs to be refactored.
        //Use EventToCommand of some sort and tie this directle to the viewmodel
        //Make sure the parameter is transformed into time before it reaches the viewmodel
        #region DragDelta
        private void Thumb_Move_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var item = FindVisualParent<ContentPresenter>(this);
            var panel = FindVisualParent<ContentPresenter>(item);
            var model = DataContext as LayerViewModel;
            
            if (item != null && model != null && panel !=null)
            {
                model.TimeChanged(item, panel, e.HorizontalChange);
            }
        }

        private void Thumb_End_DragDelta(object sender, DragDeltaEventArgs e)
        {
            
            ContentPresenter item = FindVisualParent<ContentPresenter>(this);
            ILayerViewModel model = DataContext as LayerViewModel;
            if (item != null && model != null)
            {
                model.EndTimeChanged(item, e.HorizontalChange);
            }
        }

        private void Thumb_Drag_Completed(object sender, DragCompletedEventArgs e)
        {
            ILayerViewModel model = DataContext as LayerViewModel;
            if (model != null) 
            {
                model.UpdatePeriod();
                if(model.Period.StartDateTime!=_initialPeriod.StartDateTime || model.Period.EndDateTime!=_initialPeriod.EndDateTime)RaiseEvent(new RoutedEventArgs(LayerView.LayerChangedEvent));
            }

        }
        #endregion

        //Finds the parent of a certain type
        static T FindVisualParent<T>(UIElement child) where T : UIElement
        {
            if (child == null)
            {
                return null;
            }

            UIElement parent = VisualTreeHelper.GetParent(child) as UIElement;

            while (parent != null)
            {
                T found = parent as T;
                if (found != null)
                {
                    return found;
                }
                parent = VisualTreeHelper.GetParent(parent) as UIElement;
                
            }
            return null;
        }


        private void Thumb_Drag_Started(object sender, DragStartedEventArgs e)
        {
            //take a snapshot of the period for deciding if we need to fire event for periodchanged
            ILayerViewModel model = this.DataContext as LayerViewModel;
            if (model != null) _initialPeriod = model.Period;

        }
    }
}
