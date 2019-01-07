using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Autofac;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.ConfigTool.Transformer;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.Control
{
	/// <summary>
	/// Interaction logic for EtlControlTree.xaml
	/// </summary>
	public partial class EtlControlTree : IDisposable
	{
		private readonly BackgroundWorker _logonWorker = new BackgroundWorker();
		private JobCollectionFactory _jobCollectionFactory;
		private ObservableCollection<IJob> _jobCollection;
		private IEnumerable<TenantInfo> _tenantCollection = new List<TenantInfo>();

		public event EventHandler<AlarmEventArgs> JobRun;
		public event EventHandler<AlarmEventArgs> JobSelectionChanged;
		public event EventHandler<AlarmEventArgs> InitialJobNowAvailable;

		public EtlControlTree()
		{
			InitializeComponent();

			if (isInDesignMode) return;

			_logonWorker.DoWork += logonWorker_DoWork;
			_logonWorker.RunWorkerCompleted += logonWorker_RunWorkerCompleted;
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

		private void logonWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_tenantCollection = App.Container.Resolve<ITenants>().LoadedTenants();
			
			InitialJobNowAvailable(sender, new AlarmEventArgs(_jobCollection[0]));
			SetEnableStateForJobCollection();
			DataContext = _jobCollection;
			tlv.SelectedItemChanged += tlv_SelectedItemChanged;
			tlv.AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(itemExpanded));

		}

		internal void SetEnableStateForJobCollection()
		{
			var dataSourceCollection = new DataSourceValidCollection(true, ConfigurationManager.AppSettings["datamartConnectionString"]);
			bool isJobEnabled = (dataSourceCollection.Count > 0);

			foreach (IJob job in _jobCollection)
			{
				if (job.NeedsParameterDataSource)
				{
					// Set enable state for depending of datasource existing or not
					job.Enabled = isJobEnabled;
				}
			}
		}

		private static void itemExpanded(object sender, RoutedEventArgs e)
		{
			var item = e.OriginalSource as TreeViewItem;

			if (item != null)
			{
				item.IsSelected = true;
			}
		}

		void tlv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			IJob selectedJob = getSelectedJob(tlv.SelectedItem);
			if (selectedJob != null)
			{
				JobSelectionChanged(this, new AlarmEventArgs(selectedJob));
			}
		}

		void logonWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			Thread.CurrentThread.CurrentCulture = (CultureInfo)e.Argument;
			_jobCollection = _jobCollectionFactory.JobCollection;
		}

		private void menuItemExecute_Click(object sender, RoutedEventArgs e)
		{
			if (JobRun != null)
			{
				var jobToRun = tlv.SelectedItem as IJob;
				//ClearJobStepResult(jobToRun);
				if (jobToRun != null)
				{
					SetExecuteEnabledState(false);
					JobRun(this, new AlarmEventArgs(jobToRun));
				}
			}

		}

		public void SetExecuteEnabledState(bool isEnabled)
		{
			menuItemExecute.IsEnabled = isEnabled;
		}

		public IEnumerable<TenantInfo> TenantCollection
		{
			get { return _tenantCollection; }
		}

		private IJob getSelectedJob(object selectedItem)
		{
			var currentJob = selectedItem as IJob;
			if (currentJob == null)
			{
				// If not IJob then it is IJobStep. Then we need to find the parent job
				var jobStep = selectedItem as IJobStep;

				foreach (IJob job in _jobCollection)
				{
					currentJob = job;
					if (job.StepList.Contains(jobStep))
					{
						//Parent job found - exit loop
						break;
					}
				}
			}

			return currentJob;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);

		}

		protected virtual void Dispose(bool disposing)
		{
			_logonWorker.Dispose();
		}

		public void LoadJobTree(IBaseConfiguration baseConfiguration)
		{
			_jobCollectionFactory = new JobCollectionFactory(baseConfiguration);
			_logonWorker.RunWorkerAsync(CultureInfo.CurrentCulture);
		}
	}
}