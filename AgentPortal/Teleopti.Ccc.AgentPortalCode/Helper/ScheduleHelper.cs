using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Common.Factory;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public class ScheduleHelper : IScheduleHelper
    {
        public static IList<ICustomScheduleAppointment> LoadSchedules(PersonDto person, DateTimePeriodDto period)
        {
            var startDate = new DateOnlyDto {DateTime = period.LocalStartDateTime, DateTimeSpecified = true};

            var endDate = new DateOnlyDto {DateTime = period.LocalEndDateTime, DateTimeSpecified = true};

            //Fill with Assignments
            IList<SchedulePartDto> schedulePartDtos = SdkServiceHelper.SchedulingService.GetScheduleParts(person, startDate, endDate, StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.TimeZoneId);

            AgentScheduleStateHolder.Instance().FillAgentSchedulePartDictionary(schedulePartDtos);
            return ScheduleAppointmentFactory.Create(schedulePartDtos);
        }

        public IList<ValidatedSchedulePartDto> Validate(PersonDto loggedOnPerson, DateOnly dateInPeriod, bool useStudentAvailability)
        {
            var dateOnlyDto = new DateOnlyDto {DateTime = dateInPeriod.Date, DateTimeSpecified = true};
            loggedOnPerson.CultureLanguageId = CultureInfo.CurrentCulture.LCID;
            loggedOnPerson.CultureLanguageIdSpecified = true;

            return useStudentAvailability
                       ? SdkServiceHelper.SchedulingService.GetValidatedSchedulePartsOnSchedulePeriodByQuery(
                           new GetValidatedSchedulePartsForStudentAvailabilityQueryDto
                               {
                                   DateInPeriod = dateOnlyDto,
                                   Person = loggedOnPerson,
                                   TimeZoneId = loggedOnPerson.TimeZoneId
                               })
                       : SdkServiceHelper.SchedulingService.GetValidatedSchedulePartsOnSchedulePeriodByQuery(
                           new GetValidatedSchedulePartsForPreferenceQueryDto
                               {
                                   DateInPeriod = dateOnlyDto,
                                   Person = loggedOnPerson,
                                   TimeZoneId = loggedOnPerson.TimeZoneId
                               });
        }
    }
}
