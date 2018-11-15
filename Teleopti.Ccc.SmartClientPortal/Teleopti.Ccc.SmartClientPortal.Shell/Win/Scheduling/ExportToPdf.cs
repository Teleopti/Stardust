using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.ScheduleReporting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public class ExportToPdf
	{
		private readonly IScheduleViewBase _scheduleView;
		private readonly SchedulingScreen _schedulingScreen;
		private readonly SchedulingScreenState _schedulerStateHolder;
		private readonly CultureInfo _culture;
		private readonly bool _rightToLeft;

		public ExportToPdf(IScheduleViewBase scheduleView, SchedulingScreen schedulingScreen, SchedulingScreenState schedulerStateHolder, CultureInfo culture, bool rightToLeft)
		{
			_scheduleView = scheduleView;
			_schedulingScreen = schedulingScreen;
			_schedulerStateHolder = schedulerStateHolder;
			_culture = culture;
			_rightToLeft = rightToLeft;
		}

		public void Export(bool shiftsPerDay)
		{
			bool individualReport;
			bool teamReport;
			bool singleFile;
			ScheduleReportDetail detail;
			bool publicNote;

			IList<IScheduleDay> selection = _scheduleView.SelectedSchedules();

			// Temporary solution for SPI 7833
			if (selection.Count == 0) return;

			using (var dialog = new ScheduleReportDialog(shiftsPerDay))
			{
				dialog.ShowDialog(_schedulingScreen);
				if (dialog.DialogResult != DialogResult.OK)
					return;

				teamReport = dialog.TeamReport;
				individualReport = dialog.Individual;
				singleFile = dialog.OneFile;
				detail = dialog.DetailLevel;
				publicNote = dialog.ShowPublicNote;
			}

			IDictionary<IPerson, string> personDic = new Dictionary<IPerson, string>();

			foreach (var part in selection)
			{
				if (!personDic.ContainsKey(part.Person))
					personDic.Add(part.Person, _schedulerStateHolder.CommonNameDescriptionScheduleExport.BuildFor(part.Person));
			}
			var period = new PeriodExtractorFromScheduleParts().ExtractPeriod(selection).Value;

			string path;

			if (individualReport && !singleFile)
			{
				using (var browser = new FolderBrowser())
				{
					browser.StartLocation = FolderBrowserFolder.Personal;
					DialogResult result = browser.ShowDialog(_schedulingScreen);
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
					DialogResult result = dialog.ShowDialog(_schedulingScreen);
					if (result != DialogResult.OK)
						return;
					path = dialog.FileName;
				}

			}

			var manager = new ScheduleToPdfManager();

			if (teamReport)
			{
				manager.ExportTeam(_schedulerStateHolder.SchedulerStateHolder.TimeZoneInfo, _culture, personDic,
								   period, _schedulerStateHolder.SchedulerStateHolder.SchedulingResultState,
								   _rightToLeft, detail, _schedulingScreen, path);
				return;
			}
			if (shiftsPerDay)
			{
				ScheduleToPdfManager.ExportShiftsPerDay(_schedulerStateHolder.SchedulerStateHolder.TimeZoneInfo, _culture, personDic,
														period, _schedulerStateHolder.SchedulerStateHolder.SchedulingResultState,
														detail, publicNote, _schedulingScreen, path);
				return;
			}

			manager.ExportIndividual(_schedulerStateHolder.SchedulerStateHolder.TimeZoneInfo, _culture, personDic,
									 period, _schedulerStateHolder.SchedulerStateHolder.SchedulingResultState,
									 _rightToLeft, detail, _schedulingScreen, singleFile, path);
		}
	}
}
