﻿using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class ExportToScenarioResultView : BaseDialogForm, IExportToScenarioResultView
	{
		private ExportToScenarioResultPresenter _presenter;

		public ExportToScenarioResultView(IUnitOfWorkFactory uowFactory,
											IScheduleRepository scheduleRepository,
											IMoveDataBetweenSchedules moveDataBetweenSchedules,
											IReassociateDataForSchedules callback,
											IEnumerable<IPerson> fullyLoadedPersonsToMove,
											IEnumerable<IScheduleDay> schedulePartsToExport,
											IScenario exportScenario,
											IScheduleDictionaryPersister scheduleDictionaryPersister)
		{
			InitializeComponent();
			if (!DesignMode)
			{
				SetTexts();
				_presenter = new ExportToScenarioResultPresenter(uowFactory, this,
										scheduleRepository, moveDataBetweenSchedules, callback,
										fullyLoadedPersonsToMove, schedulePartsToExport,
										exportScenario, scheduleDictionaryPersister);
			}
		}

		private void ExportToScenarioResultView_Load(object sender, EventArgs e)
		{
			_presenter.Initialize();
			BackColor = ColorHelper.OfficeBlue;
			// All this bloody blue I hate it
			//groupBoxWarnings.BackColor = ColorHelper.OfficeBlue;
			//groupBoxInfo.BackColor = ColorHelper.OfficeBlue;
			
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
	}
}
