using System.Windows;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.PersonPicker
{
    public class SelectablePersonViewModel:DependencyObject
    {

        public IPerson Person { get; private set; }

        public bool IsSelected
        {
            get { return (bool) GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
       
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof (bool), typeof (SelectablePersonViewModel), new UIPropertyMetadata(false));

        public SelectablePersonViewModel(IPerson person)
        {
            Person = person;
        }
    }
}
