using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Common.Clipboard;
using Teleopti.Ccc.AgentPortalCode.Common.Factory;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule
{
    public class AgentSchedulePresenter : AgentSchedulePresenterBase
    {
        private readonly ClipHandler<ICustomScheduleAppointment> _scheduleClipHandler;

        public AgentSchedulePresenter(IAgentScheduleViewBase view, AgentScheduleStateHolder stateHolder, ClipHandler<ICustomScheduleAppointment> clipHandler)
            : base(view, stateHolder)
        {
            _scheduleClipHandler = clipHandler;
        }

        public ClipHandler<ICustomScheduleAppointment> ScheduleClipHandler
        {
            get { return _scheduleClipHandler; }
        }

        /// <summary>
        /// Loads the agent schedules.
        /// </summary>
        /// <param name="period">The period.</param>
        public void LoadAgentSchedules(DateTimePeriodDto period)
        {
            ScheduleStateHolder.AgentScheduleDictionary.Clear(true);

            ScheduleStateHolder.AgentSchedulePeriod = period;
            ScheduleStateHolder.FillScheduleDictionary(ScheduleHelper.LoadSchedules(ScheduleStateHolder.Person, period));
            if (ScheduleStateHolder.CanVisualize(ScheduleAppointmentTypes.Request))
            {
                var requests = SdkServiceHelper.SchedulingService.GetPersonRequests(ScheduleStateHolder.Person,
                                                                                    period.LocalStartDateTime,
                                                                                    period.LocalEndDateTime);
                IList<ICustomScheduleAppointment> scheduleAppointments = ScheduleAppointmentFactory.Create(requests);
                ScheduleStateHolder.FillScheduleDictionary(scheduleAppointments);
            }
            ScheduleStateHolder.FillScheduleDictionary(PublicNoteHelper.LoadPublicNotes(ScheduleStateHolder.Person, period));
        }

        /// <summary>
        /// Loads the schedule messenger schedule.
        /// </summary>
        /// <param name="utcDate">The UTC date.</param>
        public void LoadScheduleMessengerSchedule(DateTime utcDate)
        {
            DateTime startDate = utcDate.AddHours(-12);
            DateTime endDate = startDate.AddHours(24).AddTicks(-1);
            DateTimePeriodDto period = new DateTimePeriodDto();
            period.UtcStartTime = startDate;
            period.UtcEndTime = endDate;
            ScheduleStateHolder.ScheduleMessengerPeriod = period;
            DateOnlyDto dateOnlyDto = new DateOnlyDto();
            dateOnlyDto.DateTime = DateTime.SpecifyKind(utcDate.ToLocalTime(),DateTimeKind.Unspecified); //This could result in something really wrong, but more correct than today

            SchedulePartDto schedulePart = GetSchedulePart(dateOnlyDto, 1);

            IList<SchedulePartDto> schedulePartDtos = new List<SchedulePartDto>{schedulePart};

            IList<ICustomScheduleAppointment> scheduleDataCollection = ScheduleAppointmentFactory.Create(schedulePartDtos);
            ScheduleStateHolder.FillScheduleMessengerDictionary(scheduleDataCollection, DateTime.Today);
        }

        private SchedulePartDto GetSchedulePart(DateOnlyDto dateOnlyDto, int attemptNumber)
        {
            try
            {
            	var query = new GetSchedulesByPersonQueryDto
            	            	{
            	            		StartDate = dateOnlyDto,
            	            		EndDate = dateOnlyDto,
            	            		PersonId = ScheduleStateHolder.Person.Id.GetValueOrDefault(),
            	            		TimeZoneId = ScheduleStateHolder.Person.TimeZoneId
            	            	};
                return SdkServiceHelper.SchedulingService.GetSchedulesByQuery(query).First();
            }
            catch (WebException webException)
            {
                //Could not connect to server sleep and retry
                if (attemptNumber>5)
                {
                    throw new TimeoutException(UserTexts.Resources.CanNotFindTheServiceEndPoint,webException);
                }
                Random rnd = new Random();
                Thread.Sleep(rnd.Next(7,13)*1000);
                return GetSchedulePart(dateOnlyDto, ++attemptNumber);
            }
        }
    }
}
