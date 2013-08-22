using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public class UpdateSelectionInfo
	{
		private readonly ToolStripStatusLabel _statusLabelContractTime;
		private readonly ToolStripStatusLabel _statusLabelTag;

		public UpdateSelectionInfo(ToolStripStatusLabel statusLabelContractTime, ToolStripStatusLabel statusLabelTag)
		{
			_statusLabelContractTime = statusLabelContractTime;
			_statusLabelTag = statusLabelTag;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.ToolStripItem.set_Text(System.String)")]
		public void Update(IList<IScheduleDay> selectedSchedules, IScheduleViewBase scheduleView, ISchedulerStateHolder schedulerStateHolder, FormAgentInfo agentInfo, AgentInfoControl agentInfoControl)
		{
			if (scheduleView != null)
			{
				IDictionary<IPerson, IScheduleRange> personDic = new Dictionary<IPerson, IScheduleRange>();
				HashSet<DateOnly> dateList = new HashSet<DateOnly>();
				TimeSpan totalTime = TimeSpan.Zero;

				var selectedTags = new List<IScheduleTag>();

				foreach (IScheduleDay scheduleDay in selectedSchedules)
				{
					IProjectionService projSvc = scheduleDay.ProjectionService();
					totalTime += projSvc.CreateProjection().ContractTime();

					dateList.Add(scheduleDay.DateOnlyAsPeriod.DateOnly);
					if (!personDic.ContainsKey(scheduleDay.Person))
						personDic.Add(scheduleDay.Person, schedulerStateHolder.Schedules[scheduleDay.Person]);


					if (!selectedTags.Contains(scheduleDay.ScheduleTag()))
						selectedTags.Add(scheduleDay.ScheduleTag());
				}

				if (agentInfo != null)
					agentInfo.UpdateData(personDic, dateList, schedulerStateHolder.SchedulingResultState,
										  schedulerStateHolder.SchedulingResultState.AllPersonAccounts);
				if (agentInfoControl != null)
					agentInfoControl.UpdateData(personDic, dateList, schedulerStateHolder.SchedulingResultState,
										  schedulerStateHolder.SchedulingResultState.AllPersonAccounts);

				_statusLabelContractTime.Text = string.Concat(Resources.ContractScheduledTime, " ",
																	  DateHelper.HourMinutesString(
																		  totalTime.TotalMinutes));

				var selectedTagsText = string.Empty;
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

				_statusLabelTag.Text = string.Concat(Resources.ScheduleTagColon, " ", selectedTagsText);
			}	
		}
	}
}
