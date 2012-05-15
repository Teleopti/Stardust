using System;
using System.Collections.Generic;
using System.Drawing;
using Syncfusion.Schedule;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.AgentPortalCode.Requests.RequestMaster;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.Common.Factory
{
    public static class ScheduleAppointmentFactory
    {
        private static ListObjectList _lableValueCollection;
        private static ScheduleAppointmentColorTheme _colorTheme = ScheduleAppointmentColorTheme.SystemColor;

        public static void Init(ListObjectList labelCollection)
        {
            _lableValueCollection = labelCollection;
        }

        public static ScheduleAppointmentColorTheme ColorTheme
        {
            get { return _colorTheme; }
            set { _colorTheme = value; }
        }

        public static ICustomScheduleAppointment Create(ProjectedLayerDto projectedLayerDto, PersonMeetingDto[] personMeetingDtos)
        {
            ICustomScheduleAppointment scheduleITem = createScheduleItemFromProjectedLayer(projectedLayerDto, personMeetingDtos);

            return scheduleITem;
        }

        public static IList<ICustomScheduleAppointment> Create(IList<SchedulePartDto> schedulePartCollection)
        {
            IList<ICustomScheduleAppointment> scheduleItemCollection = new List<ICustomScheduleAppointment>();

            foreach (SchedulePartDto schedulePartDto in schedulePartCollection)
            {
                ICustomScheduleAppointment scheduleItem;
                //For some reason Anders also wants to display scheduled day off when an overtime shift is scheduled...
                if ((schedulePartDto.PersonDayOff != null && schedulePartDto.ProjectedLayerCollection.Length == 0) || (schedulePartDto.PersonDayOff != null && schedulePartDto.ProjectedLayerCollection.Length > 0 && schedulePartDto.ProjectedLayerCollection[0].OvertimeDefinitionSetId != null))
                {
                    scheduleItem = create(schedulePartDto.PersonDayOff);
                    scheduleItemCollection.Add(scheduleItem);
                }
                if (!(schedulePartDto.PersonDayOff != null && schedulePartDto.ProjectedLayerCollection.Length == 0))
                {
                    foreach (ProjectedLayerDto projectedLayerDto in schedulePartDto.ProjectedLayerCollection)
                    {
                        scheduleItem = Create(projectedLayerDto, schedulePartDto.PersonMeetingCollection);
                        scheduleItemCollection.Add(scheduleItem);
                    }
                }
            }

            return MultipleDayAppointmentSplitFactory.Split(scheduleItemCollection);
        }


        private static ICustomScheduleAppointment create(PersonDayOffDto dayOffDto)
        {
            ICustomScheduleAppointment scheduleItem = null;

            if (dayOffDto != null)
            {
                scheduleItem = new CustomScheduleAppointment
                               	{
                               		Subject = dayOffDto.Name,
                               		AllDay = true,
                               		StartTime = dayOffDto.Period.LocalStartDateTime,
                               		EndTime = dayOffDto.Period.LocalEndDateTime,
                               		AllowDrag = false,
                               		AllowResize = false,
                               		AllowClickable = false,
                               		AllowCopy = false,
                               		AllowOpen = false,
                               		AllowDelete = false,
                               		AllowMultipleDaySplit = true,
                               		DisplayColor = Color.LightSeaGreen,
                               		Status = ScheduleAppointmentStatusTypes.Unchanged,
                               		AppointmentType = ScheduleAppointmentTypes.DayOff,
                               		Tag = dayOffDto
                               	};
            }
            return scheduleItem;
        }

        public static IList<ICustomScheduleAppointment> Create(IList<PersonRequestDto> requestPartDtoCollection)
        {
            IList<ICustomScheduleAppointment> scheduleItemCollection = new List<ICustomScheduleAppointment>();

            foreach (PersonRequestDto personRequestDto in requestPartDtoCollection)
            {
                if (personRequestDto.Request == null) return null;

                var scheduleItems =  new List<ICustomScheduleAppointment>();
            	ICustomScheduleAppointment scheduleItem = null;
                if (personRequestDto.Request is AbsenceRequestDto)
                {
                    scheduleItem = createScheduleItemFromAbsenceRequest(personRequestDto.Request);
                }
                else if (personRequestDto.Request is ShiftTradeRequestDto)
                {
                    scheduleItems.AddRange(createScheduleItemFromShiftTradeRequest(personRequestDto.Request));
                }
                else if (personRequestDto.Request is TextRequestDto)
                {
                    scheduleItem = createScheduleItemFromTextRequest(personRequestDto.Request);
                }
				if (scheduleItem != null)
				{
					scheduleItems.Add(scheduleItem);
				}

            	foreach (var scheduleAppointment in scheduleItems)
            	{
					scheduleAppointment.Tag = personRequestDto;
					scheduleAppointment.Subject = personRequestDto.Subject;
					scheduleItemCollection.Add(scheduleAppointment);
					scheduleAppointment.ToolTip = ScheduleAppointmentToolTip.Custom;
					scheduleAppointment.CustomToolTip = new RequestDetailRow(personRequestDto, StateHolder.Instance.State.SessionScopeData.LoggedOnPerson).Details;
				}
			}
                

            return MultipleDayAppointmentSplitFactory.Split(scheduleItemCollection);
        }

        public static IList<ICustomScheduleAppointment> Create(IList<PublicNoteDto> publicNoteCollection)
        {
            IList<ICustomScheduleAppointment> scheduleItemCollection = new List<ICustomScheduleAppointment>();

            foreach (PublicNoteDto publicNoteDto in publicNoteCollection)
            {
            	ICustomScheduleAppointment scheduleItem = createScheduleItemFromPublicNote(publicNoteDto);

            	if (scheduleItem != null)
                {
                    scheduleItem.Tag = publicNoteDto;
                    scheduleItem.Subject = publicNoteDto.ScheduleNote;
                    scheduleItem.ToolTip = ScheduleAppointmentToolTip.Custom;
                    scheduleItem.CustomToolTip = publicNoteDto.ScheduleNote;
                    scheduleItemCollection.Add(scheduleItem);
                }
            }

        	return scheduleItemCollection;
        }

        private static ICustomScheduleAppointment createScheduleItemFromProjectedLayer(ProjectedLayerDto projectedLayerDto, IEnumerable<PersonMeetingDto> personMeetingDtos)
        {
            ICustomScheduleAppointment scheduleItem = null;
            if (projectedLayerDto != null)
            {
                Color c = ColorHelper.CreateColorFromDto(projectedLayerDto.DisplayColor);

                scheduleItem = new CustomScheduleAppointment {Subject = projectedLayerDto.Description};
            	if (!string.IsNullOrEmpty(projectedLayerDto.MeetingId))
                {
                    foreach (PersonMeetingDto personMeetingDto in personMeetingDtos)
                    {
                        if (personMeetingDto.MeetingId == projectedLayerDto.MeetingId)
                        {
                            scheduleItem.Subject = personMeetingDto.Subject;
                            scheduleItem.LocationValue = personMeetingDto.Location;
                            scheduleItem.CustomToolTip = personMeetingDto.Description;
                            scheduleItem.Content = personMeetingDto.Description;
                            break;
                        }
                    }
                }

                scheduleItem.AllDay = false;
                scheduleItem.LabelValue = getLableValue(c);
                scheduleItem.StartTime = projectedLayerDto.Period.LocalStartDateTime;
                scheduleItem.EndTime = projectedLayerDto.Period.LocalEndDateTime;
                scheduleItem.AllowDrag = false;
                scheduleItem.AllowResize = false;
                scheduleItem.AllowClickable = false;
                scheduleItem.AllowCopy = false;
                scheduleItem.AllowOpen = false;
                scheduleItem.AllowDelete = false;
                scheduleItem.AllowMultipleDaySplit = true;
                scheduleItem.DisplayColor = c;
                scheduleItem.Status = ScheduleAppointmentStatusTypes.Unchanged;
                scheduleItem.AppointmentType = ScheduleAppointmentTypes.Activity;
                scheduleItem.Tag = projectedLayerDto;
            }

            return scheduleItem;
        }

        private static ICustomScheduleAppointment createScheduleItemFromAbsenceRequest(RequestDto requestPartDto)
        {
            ICustomScheduleAppointment scheduleItem = null;
            var absenceRequestDto = requestPartDto as AbsenceRequestDto;

            if (absenceRequestDto != null)
            {
                Color c = ColorHelper.CreateColorFromDto(absenceRequestDto.Absence.DisplayColor);

                scheduleItem = new CustomScheduleAppointment
                               	{
                               		AllDay = false,
                               		LabelValue = getLableValue(c),
                               		StartTime = absenceRequestDto.Period.LocalStartDateTime,
                               		EndTime = absenceRequestDto.Period.LocalEndDateTime,
                               		AllowDrag = true,
                               		AllowResize = true,
                               		AllowClickable = true,
                               		AllowCopy = false,
                               		AllowOpen = true,
                               		AllowDelete = false,
                               		AllowMultipleDaySplit = true,
                               		DisplayColor = c,
                               		Status = ScheduleAppointmentStatusTypes.Unchanged,
                               		AppointmentType = ScheduleAppointmentTypes.Request
                               	};

            	DateTime allDayEndDateTime = scheduleItem.StartTime.AddDays(1).AddMinutes(-1); //Is it 1 min or 1 sec?!?!
                scheduleItem.AllDay = (scheduleItem.StartTime.TimeOfDay == TimeSpan.Zero && scheduleItem.EndTime.TimeOfDay == allDayEndDateTime.TimeOfDay);
            }
            return scheduleItem;
        }


        private static IEnumerable<ICustomScheduleAppointment> createScheduleItemFromShiftTradeRequest(RequestDto requestPartDto)
        {
        	var shiftTradeRequestDto = requestPartDto as ShiftTradeRequestDto;
        	var ret = new List<ICustomScheduleAppointment>();
            if (shiftTradeRequestDto != null)
            {
				foreach (var detail in shiftTradeRequestDto.ShiftTradeSwapDetails)
				{
                        ICustomScheduleAppointment scheduleItem = new CustomScheduleAppointment
                                                                      {
                                                                          AllDay = true,
                                                                          LabelValue = 0,
                                                                          StartTime = detail.DateFrom.DateTime,
                                                                          EndTime = detail.DateTo.DateTime.AddDays(1).AddSeconds(-1),
                                                                          AllowDrag = true,
                                                                          AllowResize = true,
                                                                          AllowClickable = true,
                                                                          AllowCopy = false,
                                                                          AllowOpen = true,
                                                                          AllowDelete = false,
                                                                          AllowMultipleDaySplit = false,
                                                                          DisplayColor = Color.DarkBlue,
                                                                          Status =
                                                                              ScheduleAppointmentStatusTypes.Unchanged,
                                                                          AppointmentType =
                                                                              ScheduleAppointmentTypes.Request
                                                                      };
                        ret.Add(scheduleItem);
                    
				}
            }

            return ret;
        }

        private static ICustomScheduleAppointment createScheduleItemFromTextRequest(RequestDto requestPartDto)
        {
            ICustomScheduleAppointment scheduleItem = null;
            var textReqtestDto = requestPartDto as TextRequestDto;

            if (textReqtestDto != null)
            {
                scheduleItem = new CustomScheduleAppointment
                               	{
                               		AllDay = true,
                               		LabelValue = 0,
                               		StartTime = textReqtestDto.Period.LocalStartDateTime,
                               		EndTime = textReqtestDto.Period.LocalEndDateTime,
                               		AllowDrag = true,
                               		AllowResize = true,
                               		AllowClickable = true,
                               		AllowCopy = false,
                               		AllowOpen = true,
                               		AllowDelete = false,
                               		AllowMultipleDaySplit = false,
                               		DisplayColor = Color.DarkBlue,
                               		Status = ScheduleAppointmentStatusTypes.Unchanged,
                               		AppointmentType = ScheduleAppointmentTypes.Request
                               	};
            }

            return scheduleItem;
        }

        private static ICustomScheduleAppointment createScheduleItemFromPublicNote(PublicNoteDto publicNoteDto)
        {
            ICustomScheduleAppointment scheduleItem = null;

            if (publicNoteDto != null)
            {
                scheduleItem = new CustomScheduleAppointment
                               	{
                               		AllDay = true,
                               		LabelValue = 1,
                               		StartTime = publicNoteDto.Date.DateTime,
                               		EndTime = publicNoteDto.Date.DateTime,
                               		AllowDrag = false,
                               		AllowResize = false,
                               		AllowClickable = true,
                               		AllowCopy = false,
                               		AllowOpen = false,
                               		AllowDelete = false,
                               		AllowMultipleDaySplit = false,
                               		DisplayColor = Color.DarkBlue,
                               		Status = ScheduleAppointmentStatusTypes.Unchanged,
                               		AppointmentType = ScheduleAppointmentTypes.PublicNote   
                               	};
            }

            return scheduleItem;
        }

        private static int getLableValue(Color c)
        {
            int returnValue = 0;

            if (_colorTheme != ScheduleAppointmentColorTheme.DefaultColor)
            {
                if (_lableValueCollection != null)
                {
                    foreach (ListObject listObject in _lableValueCollection)
                    {
                        if (listObject.ColorMember.Equals(c))
                        {
                            returnValue = listObject.ValueMember;
                            break;
                        }
                    }
                }
            }

            return returnValue;
        }
    }
}
