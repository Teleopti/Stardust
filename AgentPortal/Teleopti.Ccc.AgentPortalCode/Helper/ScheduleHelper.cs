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
            DateOnlyDto startDate = new DateOnlyDto();
            startDate.DateTime = period.LocalStartDateTime;
            startDate.DateTimeSpecified = true;

            DateOnlyDto endDate = new DateOnlyDto();
            endDate.DateTime = period.LocalEndDateTime;
            endDate.DateTimeSpecified = true;

            //Fill with Assignments
            IList<SchedulePartDto> schedulePartDtos = SdkServiceHelper.SchedulingService.GetScheduleParts(person, startDate, endDate, StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.TimeZoneId);

            AgentScheduleStateHolder.Instance().FillAgentSchedulePartDictionary(schedulePartDtos);
            return ScheduleAppointmentFactory.Create(schedulePartDtos);
        }

        public IList<ValidatedSchedulePartDto> Validate(PersonDto loggedOnPerson, DateOnly dateInPeriod)
        {
            DateOnlyDto dateOnlyDto = new DateOnlyDto();
            dateOnlyDto.DateTime = dateInPeriod.Date;
            dateOnlyDto.DateTimeSpecified = true;
            loggedOnPerson.CultureLanguageId = CultureInfo.CurrentCulture.LCID;
            loggedOnPerson.CultureLanguageIdSpecified = true;
            
            IList<ValidatedSchedulePartDto> returnList = SdkServiceHelper.SchedulingService.GetValidatedSchedulePartsOnSchedulePeriod(loggedOnPerson, dateOnlyDto, loggedOnPerson.TimeZoneId);
            return returnList;
        }
    }
}
