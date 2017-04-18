using System.Collections.ObjectModel;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Interop
{
    public interface IMultipleHostViewModel
    {
        void Add(object header, object content);
        ObservableCollection<HostViewModel> Items { get; }
        object CurrentHeader { get; }
        object CurrentItem { get; }
        HostViewModel Current { get; }
    }
}
