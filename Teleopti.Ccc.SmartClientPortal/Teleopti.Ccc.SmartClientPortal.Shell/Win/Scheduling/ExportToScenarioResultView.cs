using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_HideExportSchedule_81161)]
	public partial class ExportToScenarioResultView : BaseDialogForm, IExportToScenarioResultView
	{
		private ExportToScenarioResultPresenter _presenter;

		public ExportToScenarioResultView(IUnitOfWorkFactory uowFactory,
											IScheduleStorage scheduleStorage,
											IMoveDataBetweenSchedules moveDataBetweenSchedules,
											IReassociateDataForSchedules callback,
											IEnumerable<IPerson> fullyLoadedPersonsToMove,
											IEnumerable<IScheduleDay> schedulePartsToExport,
											IScenario exportScenario,
											IScheduleDictionaryPersister scheduleDictionaryPersister,
											IExportToScenarioAccountPersister exportToScenarioAccountPersister,
											IExportToScenarioAbsenceFinder exportToScenarioAbsenceFinder,
											IDictionary<IPerson, IPersonAccountCollection> allPersonAccounts,
											ICollection<DateOnly> datesToExport)
		{
			InitializeComponent();
			if (!DesignMode)
			{
				SetTexts();
				_presenter = new ExportToScenarioResultPresenter(uowFactory, this,
										scheduleStorage, moveDataBetweenSchedules, callback,
										fullyLoadedPersonsToMove, schedulePartsToExport,
										exportScenario, scheduleDictionaryPersister,
										exportToScenarioAccountPersister,
										exportToScenarioAbsenceFinder,
										allPersonAccounts,
										datesToExport
										);
			}
		}

		private void exportToScenarioResultViewLoad(object sender, EventArgs e)
		{
			_presenter.Initialize();
			Text = Resources.ExportToOtherScenario;
			gridControl1.RightToLeft = RightToLeft;
		}

		public void CloseForm()
		{
			Close();
		}

		public void SetScenarioText(string headerText)
		{
			lblScenarios.Text = headerText;
		}

		public void SetWarningText(IEnumerable<ExportToScenarioWarningData> validationWarnings)
		{
			gridControl1.RowCount = validationWarnings.Count();
			int row = 1;
			foreach (ExportToScenarioWarningData warning in validationWarnings)
			{
				gridControl1[row, 1].CellValue = " * " + warning.PersonName;
				gridControl1[row, 2].CellValue = warning.WarningInfo;
				gridControl1[row, 1].CellType = "Static";
				gridControl1[row, 2].CellType = "Static";
				row++;
			}
			gridControl1.ColWidths.ResizeToFit(GridRangeInfo.Cols(1, 2));
		}

		public void DisableBodyText()
		{
			// why hide it?
			//gridControl1.Visible = false;
		}

		public void SetAgentText(string agentText)
		{
			lblNoOfAgents.Text = agentText;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			_presenter.OnCancel();
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			_presenter.OnConfirm();
		}

		private void releaseManagedResources()
		{
			_presenter = null;
		}

		public void ShowDataSourceException(DataSourceException exception)
		{
			using (var view = new SimpleExceptionHandlerView(exception,
																	Resources.ExportToOtherScenario,
																	Resources.ServerUnavailable))
			{
				view.ShowDialog();
			}
		}

		public void DisableInteractions()
		{
			btnOk.Enabled = false;
			btnCancel.Enabled = false;
			spinningProgressControl1.Visible = true;
			ControlBox = false;
		}

		public void EnableInteractions()
		{
			btnOk.Enabled = true;
			btnCancel.Enabled = true;
			spinningProgressControl1.Visible = false;
			ControlBox = true;
		}
	}
}
