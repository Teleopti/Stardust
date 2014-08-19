using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Collections;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Editor
{
    /// <summary>
    /// Model for editing a LayerViewModel
    /// It holds all the settings that will be altered, when updating, the settings wiil be applied to the layer
    /// (period, activity/absence)
    /// </summary>
    public class EditLayerViewModel:DependencyObject,ILayerEditor
    {
	    private readonly ShowOnlyCollection<IPayload> _selectablePayloads = new ShowOnlyCollection<IPayload>();

        public IList<IPayload> SelectablePayloads
        {
            get { return _selectablePayloads; }
        }

        public ILayerViewModel Layer
        {
            get { return (ILayerViewModel) GetValue(LayerProperty); }
            set { SetValue(LayerProperty, value); }
        }

        public static readonly DependencyProperty LayerProperty =
            DependencyProperty.Register("Layer", typeof (ILayerViewModel), typeof (EditLayerViewModel), new UIPropertyMetadata(null,LayerChanged));

        private static void LayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ILayerViewModel layer = e.NewValue as ILayerViewModel;
            ILayerViewModel oldLayer = e.OldValue as ILayerViewModel;
            EditLayerViewModel model = (EditLayerViewModel)d;
            if (oldLayer != null) oldLayer.PropertyChanged -= model.LayerPropertyChanged;
            if(layer!=null)
            {
                 model.Period.Start = layer.Period.StartDateTime;
                 model.Period.End = layer.Period.EndDateTime;
                 layer.PropertyChanged += model.LayerPropertyChanged;
                 model._selectablePayloads.SetFilter(layer.Payload.GetType());
                 model.IsEnabled = true;
                
                //Pick the correct Payload if it exists in the collection:
                if(CollectionViewSource.GetDefaultView(model.SelectablePayloads).Contains(layer.Payload)) 
                {
                    CollectionViewSource.GetDefaultView(model.SelectablePayloads).MoveCurrentTo(layer.Payload);
                }
            }
            else
            {
                model.IsEnabled = false;
            }
        }

        public bool IsEnabled { get; private set; }

        private void LayerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName=="Period")
            {
                Period.Start = Layer.Period.StartDateTime;
                Period.End = Layer.Period.EndDateTime;
            }
        }

        public DateTimePeriodViewModel Period
        {
            get; 
            private set;
        }

        public CommandModel UpdateLayerCommand { get; private set; }
        public CommandModel ChangePayloadCommand { get; private set; }

        public EditLayerViewModel()
        {
	        Period = new DateTimePeriodViewModel();
            UpdateLayerCommand = CommandModelFactory.CreateCommandModel(UpdateLayer,UpdateLayerCanExecute, UserTexts.Resources.Update);
            ChangePayloadCommand = CommandModelFactory.CreateCommandModel(ChangePayload, ChangePayloadCanExecute, UserTexts.Resources.Update);
                                                                               
        }

        private bool ChangePayloadCanExecute()
        {
            return (currentpayload() != null &&  currentpayload() != Layer.Payload);
        }

        private void ChangePayload()
        {
            UpdateLayer();                
        }

        private bool UpdateLayerCanExecute()
        {
            return Layer != null && Layer.IsMovePermitted();
        }

        //Set the selected properties on the model and trigger LayerUpdated
        //Henrik 2010-06-07, LayerUpdated should create some eventagg event instead (or also), i think
		private void UpdateLayer()
		{
			Layer.Period = Period.DateTimePeriod;
			Layer.Payload = CollectionViewSource.GetDefaultView(SelectablePayloads).CurrentItem != null
			                	? (IPayload) CollectionViewSource.GetDefaultView(SelectablePayloads).CurrentItem
			                	: Layer.Payload;

		
			Layer.UpdatePeriod();
			
			var handler = LayerUpdated;
			if (handler != null)
			{
				handler(this, new CustomEventArgs<ILayerViewModel>(Layer));
			}
		}

    	private IPayload currentpayload()
        {
           return CollectionViewSource.GetDefaultView(SelectablePayloads).CurrentItem as IPayload;
        }

        public event EventHandler<CustomEventArgs<ILayerViewModel>> LayerUpdated;
    }
}
