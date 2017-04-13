using System.Windows;
using System.Windows.Controls;
using Teleopti.Ccc.WinCode.Common.Time.Timeline;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Time.Timeline.Views
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class TickMarkTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            TickMarkViewModel model = item as TickMarkViewModel; 
            if (model!=null)
            {
                FrameworkElementFactory fef = new FrameworkElementFactory(typeof(TickMarkView));
                return new DataTemplate {VisualTree = fef};
            }
            return null;
        }
    }
}