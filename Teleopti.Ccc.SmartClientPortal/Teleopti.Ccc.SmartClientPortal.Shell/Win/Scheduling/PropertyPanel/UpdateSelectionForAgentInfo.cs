using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.PropertyPanel
{
	public class UpdateSelectionForAgentInfo
	{
		private readonly ToolStripSplitButton _statusLabelTime;
		private readonly ToolStripStatusLabel _statusLabelTag;

		public UpdateSelectionForAgentInfo(ToolStripSplitButton statusLabelTime, ToolStripStatusLabel statusLabelTag)
		{
			_statusLabelTime = statusLabelTime;
			_statusLabelTag = statusLabelTag;
		}

		public void Update(IList<IScheduleDay> selectedSchedules, IScheduleViewBase scheduleView,
			ISchedulerStateHolder schedulerStateHolder, AgentInfoControl agentInfo, ScheduleTimeType scheduleTimeType,
			bool showInfoPanel)
		{
			if (scheduleView == null)
				return;

			var personDic = new Dictionary<IPerson, IScheduleRange>();
			var dateList = new HashSet<DateOnly>();
			var selectedTags = new HashSet<IScheduleTag>();
			var timeLabel = LanguageResourceHelper.Translate("XXSelectionTooLarge");
			var tagLabel = timeLabel;

			if (selectedSchedules.Count <= 30000)
			{
				var totalTime = TimeSpan.Zero;
				timeLabel = setTimeLabelHeader(scheduleTimeType);
				foreach (IScheduleDay scheduleDay in selectedSchedules)
				{
					totalTime += getTotalTime(scheduleTimeType, scheduleDay);
					dateList.Add(scheduleDay.DateOnlyAsPeriod.DateOnly);
					selectedTags.Add(scheduleDay.ScheduleTag());
					if (!personDic.ContainsKey(scheduleDay.Person))
						personDic.Add(scheduleDay.Person, schedulerStateHolder.Schedules[scheduleDay.Person]);
				}

				timeLabel = string.Concat(timeLabel, ": ", DateHelper.HourMinutesString(totalTime.TotalMinutes));
				var selectedTagsText = UpdateSelectionForAgentInfo.selectedTagsText(selectedTags);
				tagLabel = string.Concat(" ", LanguageResourceHelper.Translate("XXScheduleTagColon"), " ", selectedTagsText); LanguageResourceHelper.Translate("XXScheduleTagColon");
			}

			_statusLabelTime.Text = timeLabel;
			_statusLabelTag.Text = tagLabel;

			if (agentInfo != null && showInfoPanel)
				agentInfo.UpdateData(personDic, dateList, schedulerStateHolder.SchedulingResultState.AllPersonAccounts);

		}

		private static string selectedTagsText(HashSet<IScheduleTag> selectedTags)
		{
			string selectedTagsText = string.Empty;
			var counter = 0;
			foreach (var selectedTag in selectedTags)
			{
				if (string.Concat(selectedTagsText, selectedTag.Description).Length > 100)
				{
					selectedTagsText = string.Concat(selectedTagsText, Resources.ThreeDots);
					break;
				}

				selectedTagsText = string.Concat(selectedTagsText, selectedTag.Description);

				counter++;

				if (counter != selectedTags.Count) selectedTagsText = string.Concat(selectedTagsText, ", ");
			}

			return selectedTagsText;
		}

		private static TimeSpan getTotalTime(ScheduleTimeType scheduleTimeType, IScheduleDay scheduleDay)
		{
			var totalTime = TimeSpan.Zero;
			var projSvc = scheduleDay.ProjectionService();
			var visualLayerCollection = projSvc.CreateProjection();
			switch (scheduleTimeType)
			{
				case ScheduleTimeType.ContractTime:
					totalTime += visualLayerCollection.ContractTime();
					break;

				case ScheduleTimeType.WorkTime:
					totalTime += visualLayerCollection.WorkTime();
					break;

				case ScheduleTimeType.PaidTime:
					totalTime += visualLayerCollection.PaidTime();
					break;

				case ScheduleTimeType.OverTime:
					totalTime += visualLayerCollection.Overtime();
					break;
			}

			return totalTime;
		}

		private static string setTimeLabelHeader(ScheduleTimeType scheduleTimeType)
		{
			var label = string.Empty;
			switch (scheduleTimeType)
			{
				case ScheduleTimeType.ContractTime:
					label = LanguageResourceHelper.Translate("XXContractScheduledTime");
					break;

				case ScheduleTimeType.WorkTime:
					label = LanguageResourceHelper.Translate("XXWorkTime");
					break;

				case ScheduleTimeType.PaidTime:
					label = LanguageResourceHelper.Translate("XXPaidTime");
					break;

				case ScheduleTimeType.OverTime:
					label = LanguageResourceHelper.Translate("XXOvertime");
					break;
			}

			return label;
		}
	}
}
