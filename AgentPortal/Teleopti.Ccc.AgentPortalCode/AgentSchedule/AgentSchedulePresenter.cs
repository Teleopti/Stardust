using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Common.Factory;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule
{
    public class AgentSchedulePresenter : AgentSchedulePresenterBase
    {
        public AgentSchedulePresenter(IAgentScheduleViewBase view, AgentScheduleStateHolder stateHolder)
            : base(view, stateHolder)
        {
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
            var localDate = utcDate.ToLocalTime();
            DateTimePeriodDto period = new DateTimePeriodDto();
            period.UtcStartTime = startDate;
            period.UtcEndTime = endDate;
            ScheduleStateHolder.ScheduleMessengerPeriod = period;
            DateOnlyDto dateOnlyStartDate = new DateOnlyDto();
            dateOnlyStartDate.DateTime = DateTime.SpecifyKind(localDate.AddDays(-1),DateTimeKind.Unspecified); //This could result in something really wrong, but more correct than today

            DateOnlyDto dateOnlyEndDate = new DateOnlyDto();
            dateOnlyEndDate.DateTime = DateTime.SpecifyKind(localDate, DateTimeKind.Unspecified); //This could result in something really wrong, but more correct than today

            var schedulePartDtos = GetSchedulePart(dateOnlyStartDate, dateOnlyEndDate, 1);

            IList<ICustomScheduleAppointment> scheduleDataCollection = ScheduleAppointmentFactory.Create(schedulePartDtos).Where(l => l.StartTime>=localDate.Date).ToList();
            ScheduleStateHolder.FillScheduleMessengerDictionary(scheduleDataCollection, DateTime.Today);
        }

        private IEnumerable<SchedulePartDto> GetSchedulePart(DateOnlyDto dateOnlyStartDto, DateOnlyDto dateOnlyEndDto, int attemptNumber)
        {
            try
            {
            	var query = new GetSchedulesByPersonQueryDto
            	            	{
            	            		StartDate = dateOnlyStartDto,
            	            		EndDate = dateOnlyEndDto,
            	            		PersonId = ScheduleStateHolder.Person.Id.GetValueOrDefault(),
            	            		TimeZoneId = ScheduleStateHolder.Person.TimeZoneId
            	            	};
                return SdkServiceHelper.SchedulingService.GetSchedulesByQuery(query);
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
                return GetSchedulePart(dateOnlyStartDto, dateOnlyEndDto, ++attemptNumber);
            }
        }
    }
}
