using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Teleopti.Ccc.WpfControls.Demo.ViewModels;

namespace Teleopti.Ccc.WpfControls.Demo.Views
{
    /// <summary>
    /// Interaction logic for PersonAccountdayView.xaml
    /// </summary>
    public partial class PersonAccountDayView : UserControl
    {
        public PersonAccountDayView()
        {
            InitializeComponent();
        }

        //Todo: change this to command and make it communicate directly to the viewmodel
        private void Get_Used_Clicked(object sender, RoutedEventArgs e)
        {
            PersonAccountDayViewModel model = this.DataContext as PersonAccountDayViewModel;
            if (model != null)
            {
                model.GetData();
            }
        }
    }
}
