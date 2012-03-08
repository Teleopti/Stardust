﻿using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.WcfService.Factory;

namespace Teleopti.Ccc.Sdk.WcfService.QueryHandler
{
    public class GetValidatedSchedulePartsForPreferenceQueryHandler : IHandleQuery<GetValidatedSchedulePartsForPreferenceQueryDto, ICollection<ValidatedSchedulePartDto>>
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IFactoryProvider _factoryProvider;

        public GetValidatedSchedulePartsForPreferenceQueryHandler(ILifetimeScope lifetimeScope, IFactoryProvider factoryProvider)
        {
            _lifetimeScope = lifetimeScope;
            _factoryProvider = factoryProvider;
        }

        public ICollection<ValidatedSchedulePartDto> Handle(GetValidatedSchedulePartsForPreferenceQueryDto query)
        {
            using (var inner = _lifetimeScope.BeginLifetimeScope())
            {
                return
                    _factoryProvider.CreateScheduleFactory(inner).GetValidatedSchedulePartsOnSchedulePeriod(
                        query.Person,
                        query.DateInPeriod,
                        query.TimeZoneId,
                        false);
            }
        }
    }
}
