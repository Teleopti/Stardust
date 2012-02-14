using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.SimpleSample.Model;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class ScheduleRepository
    {
        private readonly ITeleoptiSchedulingService _teleoptiSchedulingService;

        public ScheduleRepository(ITeleoptiSchedulingService teleoptiSchedulingService)
        {
            _teleoptiSchedulingService = teleoptiSchedulingService;
        }

        public IEnumerable<ScheduleModel> GetScheduleModels(ScheduleLoadOptions scheduleLoadOptions)
        {
            var foundDtos = scheduleLoadOptions.LoadScheduleParts(_teleoptiSchedulingService);
            var scheduleModels = foundDtos.Select(scheduleModel).ToList();
            return scheduleModels;
        }

        private static ScheduleModel scheduleModel(SchedulePartDto d)
        {
            var model = new ScheduleModel
                       {
                           PersonId = d.PersonId,
                           ContractTime = d.ContractTime.Subtract(DateTime.MinValue),
                           IsFullDayAbsence = d.IsFullDayAbsence,
                           BelongsToDate = d.Date.DateTime,
                           StartTime = d.ProjectedLayerCollection.Count>0 ? d.ProjectedLayerCollection.Min(p => p.Period.LocalStartDateTime) : d.Date.DateTime,
                           EndTime = d.ProjectedLayerCollection.Count>0 ? d.ProjectedLayerCollection.Max(p=>p.Period.LocalEndDateTime) : d.Date.DateTime,
                           DayOffName = d.PersonDayOff!=null ? d.PersonDayOff.Name : string.Empty,
                           DayOffPayrollCode = d.PersonDayOff!=null ? d.PersonDayOff.PayrollCode : string.Empty,
                           ShiftCategoryName = getShiftCategoryName( d.PersonAssignmentCollection),
                       };

            foreach (var projectedLayerDto in d.ProjectedLayerCollection)
            {
                var detail = new ScheduleDayDetailModel
                                 {
                                     DisplayColor =
                                         Color.FromArgb(projectedLayerDto.DisplayColor.Alpha,
                                                        projectedLayerDto.DisplayColor.Red,
                                                        projectedLayerDto.DisplayColor.Green,
                                                        projectedLayerDto.DisplayColor.Blue),
                                     LayerStart = projectedLayerDto.Period.LocalStartDateTime,
                                     LayerEnd = projectedLayerDto.Period.LocalEndDateTime,
                                     PayloadId = projectedLayerDto.PayloadId,
                                     OvertimeDefinitionSetId = projectedLayerDto.OvertimeDefinitionSetId,
                                 };

                if (projectedLayerDto.MeetingId.HasValue)
                {
                    var meetingDetail = getMeetingDetail(d, projectedLayerDto.MeetingId.Value);

                    detail.MeetingId = meetingDetail.MeetingId;
                    detail.MeetingLocation = meetingDetail.Location;
                    detail.MeetingSubject = meetingDetail.Subject;
                }
                model.ScheduleDetails.Add(detail);
            }

            return model;
        }

        private static MeetingDetail getMeetingDetail(SchedulePartDto schedulePartDto, Guid meetingId)
        {
            var meeting = schedulePartDto.PersonMeetingCollection.FirstOrDefault(m => m.MeetingId == meetingId);
            if (meeting == null) return new MeetingDetail();

            return new MeetingDetail {Location = meeting.Location, Subject = meeting.Subject, MeetingId = meetingId};
        }

        private static string getShiftCategoryName(IEnumerable<PersonAssignmentDto> personAssignmentCollection)
        {
            var item = personAssignmentCollection.FirstOrDefault();
            if (item!=null)
            {
                if (item.MainShift != null)
                {
                    return item.MainShift.ShiftCategoryName;
                }
            }
            return string.Empty;
        }

        private class MeetingDetail
        {
            public string Subject { get; set; }
            public string Location { get; set; }
            public Guid MeetingId { get; set; }
        }
    }
}