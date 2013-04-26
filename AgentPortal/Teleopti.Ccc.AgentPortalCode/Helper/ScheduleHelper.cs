using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Common.Factory;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public class ScheduleHelper : IScheduleHelper
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static IList<ICustomScheduleAppointment> LoadSchedules(PersonDto person, DateTimePeriodDto period)
        {
            var startDate = new DateOnlyDto {DateTime = period.LocalStartDateTime};

            var endDate = new DateOnlyDto {DateTime = period.LocalEndDateTime};

        	var query = new GetSchedulesByPersonQueryDto
        	            	{
        	            		StartDate = startDate,
        	            		EndDate = endDate,
        	            		TimeZoneId = StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.TimeZoneId,
        	            		PersonId = person.Id.GetValueOrDefault()
        	            	};

            //Fill with Assignments
            var schedulePartDtos = SdkServiceHelper.SchedulingService.GetSchedulesByQuery(query);

            AgentScheduleStateHolder.Instance().FillAgentSchedulePartDictionary(schedulePartDtos);
            return ScheduleAppointmentFactory.Create(schedulePartDtos);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ICollection<ValidatedSchedulePartDto> Validate(PersonDto loggedOnPerson, DateOnly dateInPeriod, bool useStudentAvailability)
        {
            var dateOnlyDto = new DateOnlyDto {DateTime = dateInPeriod.Date};
            loggedOnPerson.CultureLanguageId = CultureInfo.CurrentCulture.LCID;

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
