using System.Windows;
using System.Windows.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls
{
    /// <summary>
    /// Simple TemplateSelector for connection layers to templates based on the properties
    /// henrika TODO: Change to use DisplayObjects instead of TemplateSelector
    /// This will be removed when ScheduleParts layercollection is fixed
    /// </summary>
    public class LayerTemplateSelector:DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ContentPresenter presenter = container as ContentPresenter;

            if (item is MeetingLayerViewModel)
            {
                return presenter.FindResource("MeetingLayerViewModelTemplate") as DataTemplate;
            }

            ILayerViewModel layer = item as ILayerViewModel;
            if(layer!=null)
            {
                return (layer.IsProjectionLayer) ? presenter.FindResource("ProjectionLayerViewModelTemplate") as DataTemplate: presenter.FindResource("LayerViewModelTemplate") as DataTemplate;
            }
         
            
            return presenter.FindResource("ILayerTemplate") as DataTemplate;
        }
    }
}
