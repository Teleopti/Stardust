using System.Windows;
using System.Windows.Controls;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.CustomPanels.Grouping
{
    public abstract class BaseGroupingPanel : Panel
    {
        protected abstract Size MeasureElements(Size availableSize, UIElementCollection elements);

        protected abstract Size ArrangeElements(Size finalSize, UIElementCollection elements);

        protected sealed override Size MeasureOverride(Size availableSize)
        {
            return MeasureElements(availableSize, InternalChildren);

        }

        protected sealed override Size ArrangeOverride(Size finalSize)
        {
            Size ret = ArrangeElements(finalSize, InternalChildren);
            return ret;
        }
    }
}
