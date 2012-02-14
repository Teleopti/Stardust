using System;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.Common.Factory;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public static class PublicNoteHelper
    {
        public static IList<ICustomScheduleAppointment> LoadPublicNotes(PersonDto person, DateTimePeriodDto period)
        {
            var publicNoteLoadOptionDto = new PublicNoteLoadOptionDto { LoadPerson = person };

            DateTime startDateTime = DateTime.SpecifyKind(period.LocalStartDateTime, DateTimeKind.Unspecified);
            DateTime endDateTime = DateTime.SpecifyKind(period.LocalEndDateTime, DateTimeKind.Unspecified);
            var startDateOnly = new DateOnlyDto { DateTime = startDateTime, DateTimeSpecified = true };
            var endDateOnly = new DateOnlyDto { DateTime = endDateTime, DateTimeSpecified = true };

            return
                ScheduleAppointmentFactory.Create(
                    SdkServiceHelper.SchedulingService.GetPublicNotes(publicNoteLoadOptionDto,
                                                                        startDateOnly,
                                                                        endDateOnly));
        }
    }
}
