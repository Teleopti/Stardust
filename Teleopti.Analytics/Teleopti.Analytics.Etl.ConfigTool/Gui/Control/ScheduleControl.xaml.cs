using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.JobSchedule;
using IContainer = Autofac.IContainer;
using MessageBox = System.Windows.MessageBox;
using MessageBoxOptions = System.Windows.MessageBoxOptions;
using UserControl = System.Windows.Controls.UserControl;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.Control
{
	/// <summary>
	/// Interaction logic for ScheduleControl.xaml
	/// </summary>
	public partial class ScheduleControl : UserControl
	{
		private readonly ObservableCollection<IEtlJobSchedule> _observableCollection;
		private readonly Repository _repository;
		private IBaseConfiguration _baseConfiguration;
		private EtlControlTree _treeControl;
		private IContainer _container;

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

		public void SetTreeControl(EtlControlTree treeControl, IContainer container)
		{
			_treeControl = treeControl;
			_container = container;
		}

		private static bool isInDesignMode
		{
			get
			{
				var prop = DesignerProperties.IsInDesignModeProperty;
				return
					(bool)
					DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement)).Metadata.DefaultValue;

			}
		}

		private void MenuItemEdit(object sender, RoutedEventArgs e)
		{
			var etlSchedule = (IEtlJobSchedule)lv.SelectedItem;
			using (var jobSchedule = new JobSchedule(etlSchedule, _observableCollection, _baseConfiguration, _treeControl.TenantCollection.Count() == 1, _container))
			{
				if (jobSchedule.ShowDialog() == DialogResult.OK)
				{

				}
			}
		}

		private void MenuItemDelete(object sender, RoutedEventArgs e)
		{
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
			using (var jobSchedule = new JobSchedule(null, _observableCollection, _baseConfiguration, _treeControl.TenantCollection.Count() == 1, _container))
			{
				if (jobSchedule.ShowDialog() == DialogResult.OK)
				{

				}
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