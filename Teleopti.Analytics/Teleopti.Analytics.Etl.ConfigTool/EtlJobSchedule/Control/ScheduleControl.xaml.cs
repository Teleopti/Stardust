using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Windows;
using System.Windows.Forms;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.JobSchedule;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Interfaces.Domain;
using MessageBox = System.Windows.MessageBox;
using MessageBoxOptions = System.Windows.MessageBoxOptions;
using UserControl = System.Windows.Controls.UserControl;

namespace Teleopti.Analytics.Etl.ConfigTool.EtlJobSchedule.Control
{
    /// <summary>
    /// Interaction logic for ScheduleControl.xaml
    /// </summary>
    public partial class ScheduleControl : UserControl
    {
        private readonly ObservableCollection<IEtlJobSchedule> _observableCollection;
        private readonly Repository _repository;
    	private IBaseConfiguration _baseConfiguration;

    	public ScheduleControl()
        {
            InitializeComponent();

            if (isInDesignMode) return;

			string connectionString = ConfigurationManager.AppSettings["datamartConnectionString"];
            if (connectionString != null)
            {

                _repository = new Repository(connectionString);
                var etlScheduleCollection = new EtlJobScheduleCollection(_repository, null, DateTime.Now);

                _observableCollection = new ObservableCollection<IEtlJobSchedule>(etlScheduleCollection);
                DataContext = _observableCollection;
            }
        }

        private static bool isInDesignMode
        {
            get
            {
                var prop = DesignerProperties.IsInDesignModeProperty;
                return
                    (bool)
                    DependencyPropertyDescriptor.FromProperty(prop, typeof (FrameworkElement)).Metadata.DefaultValue;

            }
        }

        private void MenuItemEdit(object sender, RoutedEventArgs e)
        {
            // Edit
            var etlSchedule = (IEtlJobSchedule)lv.SelectedItem;
			var jobSchedule = new JobSchedule(etlSchedule, _observableCollection, _baseConfiguration);

            if (jobSchedule.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void MenuItemDelete(object sender, RoutedEventArgs e)
        {
            // Delete
            if (lv.SelectedItem != null)
            {
                MessageBoxResult dialogResult = MessageBox.Show("Are you sure you want to remove the job schedule?",
                                                            "Confirm",
                                                            MessageBoxButton.YesNo, MessageBoxImage.Question,
                                                            MessageBoxResult.No,
                                                            (FlowDirection == System.Windows.FlowDirection.RightToLeft)
                                                                ? MessageBoxOptions.RtlReading |
                                                                  MessageBoxOptions.RightAlign
                                                                : MessageBoxOptions.None);
                //(this.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                //    ? MessageBoxOptions.RtlReading |
                //      MessageBoxOptions.RightAlign
                //    : 0);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    var etlSchedule = (IEtlJobSchedule)lv.SelectedItem;
                    _repository.DeleteSchedule(etlSchedule.ScheduleId);
                    _observableCollection.Remove(etlSchedule);
                }
            }
        }

        private void MenuItemNew(object sender, RoutedEventArgs e)
        {
            // New
			var jobSchedule = new JobSchedule(null, _observableCollection, _baseConfiguration);

            if (jobSchedule.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void MenuItemSelect(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lv.SelectedItem != null)
            {
                MenuItemEdit(sender, e);
            }
        }

    	public void SetBaseConfiguration(IBaseConfiguration baseConfiguration)
    	{
    		_baseConfiguration = baseConfiguration;
    	}
    }
}