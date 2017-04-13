using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Teleopti.Ccc.WinCode.Common.Interop
{
    /// <summary>
    /// Class for hosting multiple objects in winform with description
    /// Build view to represent content/header
    /// </summary>
    /// <remarks>
    /// Will always show a item if exists
    /// Created by: henrika
    /// Created date: 2009-01-13
    /// </remarks>
    public class MultipleHostViewModel : IMultipleHostViewModel
    {
        private readonly ObservableCollection<HostViewModel> _models = new ObservableCollection<HostViewModel>();

        public void Add(object header,object content)
        {
            _models.Add(new HostViewModel(header,content));
        }

        public ObservableCollection<HostViewModel> Items
        {
            get { return _models; }
        }

        public object CurrentHeader
        {
            get
            {
                if (Current != null) return Current.ModelHeader;
                return null;
            }
        }

        public object CurrentItem
        {
            get
            {
                if (Current != null) return Current.ModelContent;
                return null;
            }
        }

        public HostViewModel Current
        {
            get
            {
                
                HostViewModel ret = CollectionViewSource.GetDefaultView(Items).CurrentItem as HostViewModel;
                if (Items.Count > 0 && ret == null)
                    ret =  Items[0];
                return ret;
            }
        }

       
    }
}
