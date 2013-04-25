using System;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.Common.Factory;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public static class PublicNoteHelper
    {
        public static IList<ICustomScheduleAppointment> LoadPublicNotes(PersonDto person, DateTimePeriodDto period)
        {
            var publicNoteLoadOptionDto = new PublicNoteLoadOptionDto { LoadPerson = person };

            DateTime startDateTime = DateTime.SpecifyKind(period.LocalStartDateTime, DateTimeKind.Unspecified);
            DateTime endDateTime = DateTime.SpecifyKind(period.LocalEndDateTime, DateTimeKind.Unspecified);
            var startDateOnly = new DateOnlyDto { DateTime = startDateTime };
            var endDateOnly = new DateOnlyDto { DateTime = endDateTime };

            return
                ScheduleAppointmentFactory.Create(
                    SdkServiceHelper.SchedulingService.GetPublicNotes(publicNoteLoadOptionDto,
                                                                        startDateOnly,
                                                                        endDateOnly));
        }
    }
}
