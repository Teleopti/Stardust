﻿using System.Windows.Controls;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.View
{
	/// <summary>
	/// Interaction logic for JobHistoryMain.xaml
	/// </summary>
	public partial class JobHistoryMain : UserControl
	{
		public JobHistoryMain()
		{
			InitializeComponent();
			historySelection.JobHistorySelectionChanged += new System.EventHandler<ViewModel.JobHistorySelectionEventArgs>(historySelection_JobHistorySelectionChanged);
		}

		void historySelection_JobHistorySelectionChanged(object sender, ViewModel.JobHistorySelectionEventArgs e)
		{
			historyTree.LoadData(e.StartDate, e.EndDate, e.BusinessUnitId);
		}

		public void SetBaseConfiguration(IBaseConfiguration baseConfiguration)
		{
			historySelection.SetBaseConfiguration(baseConfiguration);
			historyTree.LoadData(historySelection.StartDate, historySelection.EndDate, historySelection.BusinessUnitId);
		}
	}
}
