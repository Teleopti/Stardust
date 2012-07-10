using System;
using System.Drawing;
using System.Collections;
using Syncfusion.Schedule;
using Teleopti.Ccc.AgentPortal.AgentSchedule;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Common.Factory;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortal.ScheduleControlDataProvider
{
    /// <summary>
    /// represnt the calss that act as data provider for Schedule Control
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2008-11-18
    /// </remarks>
    public class CustomScheduleDataProvider : ScheduleDataProvider
    {
        private readonly AgentScheduleView _scheduleView;

        /// <summary>
        /// Returns a the subset of MasterList between the 2 dates.
        /// </summary>
        /// <param name="startDate">Starting date limit for the returned items.</param>
        /// <param name="endDate">Ending date limit for the returned items.</param>
        /// <returns>Returns a the subset of MasterList.</returns>
        public override IScheduleAppointmentList GetSchedule(DateTime startDate, DateTime endDate)
        {
            //Not very nice, but to fix Uminas stranage timezone problem, remove 1 second!!!
            //The problem occurs cause the whole Syncfusion schedule control returns an elegal
            //date in some occations i.e. 2011-04-01 00:00 Jordan Time Zone (DTS day) when we try
            //to convert that ilegal date to utc it will crash. Need to figure out how to
            //get a "standard" time of day (transition time minus 1 second?!?). 

            var orgPeriod = new DateTimePeriodDto();
            /*orgPeriod.UtcStartTime = AgentPortalTimeZoneHelper.ConvertToUniversalTime(startDate.Date.AddSeconds(-1));
            orgPeriod.UtcEndTime = AgentPortalTimeZoneHelper.ConvertToUniversalTime(endDate.Date.AddHours(24).AddSeconds(-1));*/
            orgPeriod.LocalStartDateTime = startDate.Date.AddSeconds(-1);
            orgPeriod.LocalEndDateTime = endDate.Date.AddHours(24).AddSeconds(-1);
            orgPeriod.LocalStartDateTimeSpecified = true;
            orgPeriod.LocalEndDateTimeSpecified = true;
            /*orgPeriod.UtcStartTimeSpecified = true;
            orgPeriod.UtcEndTimeSpecified = true;*/

            var loadPeriod = new DateTimePeriodDto();

            /*loadPeriod.UtcStartTime = AgentPortalTimeZoneHelper.ConvertToUniversalTime(startDate.Date.AddDays(-1).AddSeconds(-1));
            loadPeriod.UtcEndTime = AgentPortalTimeZoneHelper.ConvertToUniversalTime(endDate.Date.AddHours(24).AddSeconds(-1));*/
            loadPeriod.LocalStartDateTime = startDate.Date.AddDays(-1).AddSeconds(-1);
            loadPeriod.LocalEndDateTime = endDate.Date.AddHours(24).AddSeconds(-1);
            loadPeriod.LocalStartDateTimeSpecified = true;
            loadPeriod.LocalEndDateTimeSpecified = true;
            /*loadPeriod.UtcStartTimeSpecified = true;
            loadPeriod.UtcEndTimeSpecified = true;*/

            if (_scheduleView.IsNeedToReloadData)
            {
                _scheduleView.LoadAgentSchedules(loadPeriod);
                //Raise Schedule Type Changed Event
                _scheduleView.ScheduleControlHost.OnScheduleTypeChange();
            }
            _scheduleView.IsNeedToReloadData = true;
            ScheduleAppointmentTypes itemTypes = AgentScheduleStateHolder.Instance().VisualizingScheduleAppointmentTypes;
            return AgentScheduleStateHolder.Instance().AgentScheduleDictionary.ScheduleAppointments(orgPeriod, itemTypes);
        }

        /// <summary>
        /// Returns a the subset of MasterList between the 2 dates.
        /// </summary>
        /// <param name="day">Date for the returned items.</param>
        /// <returns>Returns a the subset of MasterList.</returns>
        public override IScheduleAppointmentList GetScheduleForDay(DateTime day)
        {
            DateTime startDate = day.Date;
            DateTime endDate = startDate;

            return GetSchedule(startDate, endDate);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomScheduleDataProvider"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-11-20
        /// </remarks>
        public CustomScheduleDataProvider(AgentScheduleView scheduleView)
        {
            LabelList.Clear();
            LabelList.AddRange(GetTheLabels());
            ScheduleAppointmentFactory.Init(LabelList);

            _scheduleView = scheduleView;
        }

        /// <summary>
        /// Gets the lables.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-11-20
        /// </remarks>
        private static ICollection GetTheLabels()
        {
            int lableValue = 2;
            var labels = new ListObjectList();
            var arrActivityDto = SdkServiceHelper.SchedulingService.GetActivities(new LoadOptionDto { LoadDeleted = false, LoadDeletedSpecified = true });
            var arrAbsenceDto = SdkServiceHelper.SchedulingService.GetAbsences(new AbsenceLoadOptionDto { LoadDeleted = false, LoadDeletedSpecified = true });


            //Load Default Color
            var defaultColor = Color.FromArgb(192, 210, 234); // outlook 2007 blue color
            var defaultLabel = new ListObject(0, "Default", defaultColor);
            labels.Add(defaultLabel);

            //Load Public Note Color
            Color publicNoteColor = Color.FromArgb(252, 240, 173); // post-it canary yellow
            labels.Add(new ListObject(1, "Public Note", publicNoteColor));
            

            // Load activity colors
            foreach (ActivityDto dto in arrActivityDto)
            {
                var color = ColorHelper.CreateColorFromDto(dto.DisplayColor);
                
                // check what font is acually used!
                var font = new Font("Arial", 8, FontStyle.Regular);
                
                if (dto.Description.Length > 18) dto.Description = dto.Description.Substring(0, 18);

                Size textSize = System.Windows.Forms.TextRenderer.MeasureText(dto.Description, font);
                while (textSize.Width > 90)
                {
                    dto.Description = dto.Description.Substring(0, dto.Description.Length - 4) +
                                      UserTexts.Resources.ThreeDots;
                    textSize = System.Windows.Forms.TextRenderer.MeasureText(dto.Description, font);
                }

                var label = new ListObject(lableValue++, dto.Description, color);
                labels.Add(label);
            }

            foreach (AbsenceDto dto in arrAbsenceDto)
            {
                var color = ColorHelper.CreateColorFromDto(dto.DisplayColor);
                var label = new ListObject(lableValue++, dto.Name, color);
                labels.Add(label);
            }

            return labels;
        }
    }

}

