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
using System.Windows.Shapes;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WpfControls.Demo
{
    /// <summary>
    /// Interaction logic for PersonAccountModelDemoxaml.xaml
    /// </summary>
    public partial class PersonAccountModelDemo : Window
    {
        
        public PersonAccountModelDemo()
        {
            InitializeComponent();
            
            this.DataContext = AccountFactory.LoadAllPersonsAsViewModels();
        }

       
    }
}
