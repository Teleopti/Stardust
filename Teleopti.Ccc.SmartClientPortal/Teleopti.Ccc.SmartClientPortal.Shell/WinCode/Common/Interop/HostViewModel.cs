using System.Windows;

namespace Teleopti.Ccc.WinCode.Common.Interop
{
    /// <summary>
    /// Used for hosting multiple wpf usercontrols in winform with description/header
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-01-13
    /// </remarks>
    public class HostViewModel : DependencyObject,IHostViewModel
    {
        public object ModelHeader
        {
            get { return GetValue(ModelHeaderProperty); }
            private set { SetValue(ModelHeaderProperty, value); }
        }

        public static readonly DependencyProperty ModelHeaderProperty =
            DependencyProperty.Register("ModelHeader", typeof (object), typeof (HostViewModel), new UIPropertyMetadata(string.Empty));


        public object ModelContent
        {
            get { return GetValue(ModelContentProperty); }
            private set { SetValue(ModelContentProperty, value); }
        }

        public void UpdateItem(object header, object content)
        {
            ModelHeader = header;
            ModelContent = content;
        }

        public static readonly DependencyProperty ModelContentProperty =
            DependencyProperty.Register("ModelContent", typeof (object), typeof (HostViewModel), new UIPropertyMetadata(string.Empty));

        public HostViewModel(object header, object content)
        {
            ModelHeader = header;
            ModelContent = content;
        }
    }
}
