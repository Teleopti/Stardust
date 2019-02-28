using System.Windows;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Intraday
{
    /// <summary>
    /// Interaction logic for PinnedLayerView.xaml
    /// </summary>
    public partial class PinnedLayerView
    {
     
        public PinnedLayerView()
        {
            PersonNameConverterSetup();
            InitializeComponent();
        }

        //Henrika: remove this....
        static void PersonNameConverterSetup()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                PersonNameConverter.Setting = GlobalSettingDataRepository.DONT_USE_CTOR(uow).FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting());
            }
        }

        
        public void SetDayLayerViewCollection(IDayLayerViewModel model)
        {
            mainGrid.ItemsSource = model.Models;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string helpContext = HelpProvider.GetHelpString(this);
            if (string.IsNullOrEmpty(helpContext) && Parent == null) return;
            var elementParent = Parent as FrameworkElement;
            if (elementParent == null || elementParent.Parent == null) return;
            HelpProvider.SetHelpString(elementParent.Parent, helpContext);
        }
    }
}