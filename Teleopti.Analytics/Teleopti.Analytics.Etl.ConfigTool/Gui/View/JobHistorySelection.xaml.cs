using System;
using System.Windows.Controls;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.ConfigTool.Gui.ViewModel;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.View
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
			_model.IsRefreshing = true;
			_model.UpdateBusinessUnitCollection();
			_model.IsRefreshing = false;
			SelectionChanged();
		}

		private void SelectionChanged()
		{
			JobHistorySelectionChanged(this, new JobHistorySelectionEventArgs(StartDate, EndDate, BusinessUnit, ShowOnlyErrors));
		}

		public DateTime StartDate
		{
			get { return _model.StartDate; }
		}

		public DateTime EndDate
		{
			get { return _model.EndDate; }
		}

		public BusinessUnitItem BusinessUnit
		{
			get { return _model.SelectedBusinessUnit; }
		}

		public bool ShowOnlyErrors
		{
			get { return _model.ShowOnlyErrors; }
		}

		private void comboBoxBusinessUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (comboBoxBusinessUnit.SelectedValue == null || _model.IsRefreshing)
				return;

			//_model.SelectedBusinessUnit = (Guid)comboBoxBusinessUnit.SelectedValue;
			SelectionChanged();
		}

		private void checkBoxJobResultType_Changed(object sender, System.Windows.RoutedEventArgs e)
		{
			SelectionChanged();
		}
	}
}
