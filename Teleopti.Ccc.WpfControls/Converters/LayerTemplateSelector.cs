using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Converters
{
    /// <summary>
    /// Templateselector for layers when templating ILayer interface or need for dynamic change of DataTemplate
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-07-08
    /// </remarks>
    public class LayerTemplateSelector : DataTemplateSelector
    {
        private FrameworkElement resourceHolder;
        /// <summary>
        /// Gets the DataTemplate
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="container">The container.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-07-08
        /// </remarks>
        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            resourceHolder = container as FrameworkElement;

            if (item != null && item is ILayer)
            {
                if (item is ActivityLayer)
                {
                    return GetTemplateFromResource("ILayerTemplate");
                }
            }
            return null;
           
        }

        private DataTemplate GetTemplateFromResource(string nameOfTemplate)
        {
            if (resourceHolder != null)
            {
                //Tries to find the resource, otherwise a empty DataTemplate is return (will call ToString)
                try
                {
                    return resourceHolder.FindResource(nameOfTemplate) as DataTemplate;
                }
                catch (ResourceReferenceKeyNotFoundException ex)
                {
                    Debug.WriteLine("Resource " + nameOfTemplate + " not found");
                    Debug.WriteLine(ex.InnerException.ToString());
                }
            }
            return new DataTemplate();
        }
    }
}