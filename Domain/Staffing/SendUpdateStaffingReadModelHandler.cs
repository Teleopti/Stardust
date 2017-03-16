﻿using System;
using System.Globalization;
using log4net;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Staffing
{
    public class SendUpdateStaffingReadModelHandler : IHandleEvent<TenantMinuteTickEvent>, IRunOnHangfire
    {
        private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
	    private readonly IJobStartTimeRepository _jobStartTimeRepository;
        private readonly IBusinessUnitRepository _businessUnitRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IUpdatedByScope _updatedByScope;
        private readonly IBusinessUnitScope _businessUnitScope;
        private readonly INow _now;
        private readonly IEventPublisher _publisher;
        private readonly IConfigReader _configReader;
	    private readonly IRequestStrategySettingsReader _requestStrategySettingsReader;

        private static readonly ILog logger = LogManager.GetLogger(typeof(SendUpdateStaffingReadModelHandler));


        public SendUpdateStaffingReadModelHandler(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IBusinessUnitRepository businessUnitRepository,
			IPersonRepository personRepository, IUpdatedByScope updatedByScope, IBusinessUnitScope businessUnitScope, INow now, IEventPublisher publisher, 
			IConfigReader configReader, IRequestStrategySettingsReader requestStrategySettingsReader, IJobStartTimeRepository jobStartTimeRepository)
        {
            _currentUnitOfWorkFactory = currentUnitOfWorkFactory;
            _businessUnitRepository = businessUnitRepository;
            _personRepository = personRepository;
            _updatedByScope = updatedByScope;
            _businessUnitScope = businessUnitScope;
            _now = now;
            _publisher = publisher;
            _configReader = configReader;
	        _requestStrategySettingsReader = requestStrategySettingsReader;
	        _jobStartTimeRepository = jobStartTimeRepository;
        }

		public void Handle(TenantMinuteTickEvent @event)
        {
	        using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
	        {
		        var updateResourceReadModelIntervalMinutes =
			        _requestStrategySettingsReader.GetIntSetting("UpdateResourceReadModelIntervalMinutes", 60);

		        var currentBusinessUnit = ((ICurrentBusinessUnit) _businessUnitScope).Current();
		        var lastExecutedPerBu = _jobStartTimeRepository.LoadAll();
				var businessUnits = _businessUnitRepository.LoadAll();
		        var person = _personRepository.Get(SystemUser.Id);
		        _updatedByScope.OnThisThreadUse(person);

		        businessUnits.ForEach(businessUnit =>
		        {
			        if (lastExecutedPerBu.ContainsKey(businessUnit.Id.GetValueOrDefault()))
			        {
				        var lastExecuted = lastExecutedPerBu[businessUnit.Id.GetValueOrDefault()];
				        if (lastExecuted.AddMinutes(updateResourceReadModelIntervalMinutes) >= _now.UtcDateTime()) return;
				        _businessUnitScope.OnThisThreadUse(businessUnit);
				        _publisher.Publish(new UpdateStaffingLevelReadModelEvent
				        {
					        Days = 1
				        });
			        }
			        else
			        {
						_businessUnitScope.OnThisThreadUse(businessUnit);
						_publisher.Publish(new UpdateStaffingLevelReadModelEvent
						{
							Days = 1
						});
					}

		        });
		        _businessUnitScope.OnThisThreadUse(currentBusinessUnit);

	        }
        }
    }
}
