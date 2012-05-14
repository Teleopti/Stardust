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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static IList<ICustomScheduleAppointment> LoadSchedules(PersonDto person, DateTimePeriodDto period)
        {
            var startDate = new DateOnlyDto {DateTime = period.LocalStartDateTime, DateTimeSpecified = true};

            var endDate = new DateOnlyDto {DateTime = period.LocalEndDateTime, DateTimeSpecified = true};

        	var query = new GetSchedulesByPersonQueryHandlerDto
        	            	{
        	            		StartDate = startDate,
        	            		EndDate = endDate,
        	            		TimeZoneId = StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.TimeZoneId,
        	            		PersonId = person.Id
        	            	};

            //Fill with Assignments
            IList<SchedulePartDto> schedulePartDtos = SdkServiceHelper.SchedulingService.GetSchedulesByQuery(query);

            AgentScheduleStateHolder.Instance().FillAgentSchedulePartDictionary(schedulePartDtos);
            return ScheduleAppointmentFactory.Create(schedulePartDtos);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
