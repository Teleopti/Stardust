using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.WcfService.Factory;

namespace Teleopti.Ccc.Sdk.WcfService.QueryHandler
{
    public class GetValidatedSchedulePartsForStudentAvailabilityQueryHandler : IHandleQuery<GetValidatedSchedulePartsForStudentAvailabilityQueryDto, ICollection<ValidatedSchedulePartDto>>
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IFactoryProvider _factoryProvider;

        public GetValidatedSchedulePartsForStudentAvailabilityQueryHandler(ILifetimeScope lifetimeScope, IFactoryProvider factoryProvider)
        {
            _lifetimeScope = lifetimeScope;
            _factoryProvider = factoryProvider;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ICollection<ValidatedSchedulePartDto> Handle(GetValidatedSchedulePartsForStudentAvailabilityQueryDto query)
        {
            using (var inner = _lifetimeScope.BeginLifetimeScope())
            {
                return
                    _factoryProvider.CreateScheduleFactory(inner).GetValidatedSchedulePartsOnSchedulePeriod(
                        query.Person,
                        query.DateInPeriod,
                        query.TimeZoneId,
                        true);
            }
        }
    }
}
