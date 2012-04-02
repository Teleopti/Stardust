using System;
using System.Windows.Controls;
using Teleopti.Analytics.Etl.ConfigTool.EtlJobSchedule.ViewModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.ConfigTool.EtlJobSchedule.View
{
	/// <summary>
	/// Interaction logic for JobHistorySelection.xaml
	/// </summary>
	public partial class JobHistorySelection : UserControl
	{
		public event EventHandler<JobHistorySelectionEventArgs> JobHistorySelectionChanged;
		private JobHistorySelectionViewModel _model;

		public JobHistorySelection()
		{
			InitializeComponent();
		}

		public void SetBaseConfiguration(IBaseConfiguration baseConfiguration)
		{
			_model = new JobHistorySelectionViewModel(baseConfiguration);
			DataContext = _model;
			comboBoxBusinessUnit.DataContext = _model.BusinessUnitCollection;
		}

		private void buttonPrevious_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			_model.PreviousPeriod();
			SelectionChanged();
		}

		private void buttonNext_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			_model.NextPeriod();
			SelectionChanged();
		}

		private void buttonRefresh_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			SelectionChanged();
		}

		private void SelectionChanged()
		{
			JobHistorySelectionChanged(this, new JobHistorySelectionEventArgs(StartDate, EndDate, BusinessUnitId));
		}

		public DateTime StartDate
		{
			get { return _model.StartDate; }
		}

		public DateTime EndDate
		{
			get { return _model.EndDate; }
		}

		public Guid BusinessUnitId
		{
			get { return _model.SelectedBusinessUnitId; }
		}

		private void comboBoxBusinessUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_model.SelectedBusinessUnitId = (Guid)comboBoxBusinessUnit.SelectedValue;
			SelectionChanged();
		}
	}
}
