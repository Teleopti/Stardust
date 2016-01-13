using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Win.Scheduling.ScheduleReporting;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public class ExportToPdfGraphical
	{
		private readonly IScheduleViewBase _scheduleView;
		private readonly SchedulingScreen _schedulingScreen;
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private readonly CultureInfo _culture;
		private readonly bool _rightToLeft;

		public ExportToPdfGraphical(IScheduleViewBase scheduleView, SchedulingScreen schedulingScreen, ISchedulerStateHolder schedulerStateHolder, CultureInfo culture, bool rightToLeft)
		{
			_scheduleView = scheduleView;
			_schedulingScreen = schedulingScreen;
			_schedulerStateHolder = schedulerStateHolder;
			_culture = culture;
			_rightToLeft = rightToLeft;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.FileDialog.set_Filter(System.String)")]
		public void Export()
		{
			var model = new ScheduleReportDialogGraphicalModel();

			using (var dialog = new ScheduleReportDialogGraphicalView(model))
			{
				dialog.ShowDialog(_schedulingScreen);

				if (dialog.DialogResult != DialogResult.OK)
					return;
			}

			var selection = _scheduleView.SelectedSchedules();

			if (selection.Count == 0)
				return;

			IDictionary<IPerson, string> personDic = new Dictionary<IPerson, string>();

			foreach (var part in selection)
			{

				if (!personDic.ContainsKey(part.Person))
					personDic.Add(part.Person, _schedulerStateHolder.CommonAgentNameScheduleExport(part.Person));
				//personDic.Add(part.Person, _schedulerState.CommonAgentName(part.Person));

			}

			string path;

			if (!model.Team && !model.OneFileForSelected)
			{
				using (var browser = new FolderBrowser())
				{
					browser.StartLocation = FolderBrowserFolder.Personal;
					var result = browser.ShowDialog(_schedulingScreen);
					if (result != DialogResult.OK)
						return;
					path = browser.DirectoryPath;
				}
			}
			else
			{
				using (var dialog = new SaveFileDialog())
				{
					dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
					dialog.DefaultExt = ".PDF";
					dialog.Filter = "PDF(*.PDF)|*.PDF";
					dialog.AddExtension = true;
					var result = dialog.ShowDialog(_schedulingScreen);
					if (result != DialogResult.OK)
						return;
					path = dialog.FileName;
				}
			}

			var period = new PeriodExctractorFromScheduleParts().ExtractPeriod(selection);

			if (model.Team)
				ScheduleToPdfManager.ExportShiftPerDayTeamViewGraphical(_culture, personDic, period,
																		_schedulerStateHolder.SchedulingResultState,
																		_rightToLeft, _schedulingScreen, path, model);
			else
				ScheduleToPdfManager.ExportShiftPerDayAgentViewGraphical(_culture, personDic, period,
																		 _schedulerStateHolder.SchedulingResultState,
																		 _rightToLeft, _schedulingScreen, path, model);
		}
	}
}
