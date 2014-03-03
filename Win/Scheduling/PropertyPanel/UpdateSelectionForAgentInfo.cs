using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
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

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.ToolStripItem.set_Text(System.String)")]
            public void Update(IList<IScheduleDay> selectedSchedules, IScheduleViewBase scheduleView, ISchedulerStateHolder schedulerStateHolder, AgentInfoControl agentInfo, ScheduleTimeType scheduleTimeType)
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
                        IVisualLayerCollection visualLayerCollection = projSvc.CreateProjection();
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


                        dateList.Add(scheduleDay.DateOnlyAsPeriod.DateOnly);
                        if (!personDic.ContainsKey(scheduleDay.Person))
                            personDic.Add(scheduleDay.Person, schedulerStateHolder.Schedules[scheduleDay.Person]);


                        if (!selectedTags.Contains(scheduleDay.ScheduleTag()))
                            selectedTags.Add(scheduleDay.ScheduleTag());
                    }

                    if (agentInfo != null)
                        agentInfo.UpdateData(personDic, dateList, schedulerStateHolder,
											  schedulerStateHolder.SchedulingResultState.AllPersonAccounts);

                    string label = string.Empty;
                    switch (scheduleTimeType)
                    {
                        case ScheduleTimeType.ContractTime:
                            label = Resources.ContractScheduledTime;
                            break;

                        case ScheduleTimeType.WorkTime:
                            label = Resources.WorkTime;
                            break;

                        case ScheduleTimeType.PaidTime:
                            label = Resources.PaidTime;
                            break;

                        case ScheduleTimeType.OverTime:
                            label = Resources.Overtime;
                            break;
                    }
                    _statusLabelTime.Text = string.Concat(label, ": ", DateHelper.HourMinutesString(totalTime.TotalMinutes));

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
