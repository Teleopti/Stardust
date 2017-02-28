using System;
using System.Globalization;
using log4net;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
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
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
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
            var now = _now.UtcDateTime();
            var configuredNow = _configReader.AppConfig("FakeIntradayUtcStartDateTime");
            if (configuredNow != null)
            {
                try
                {
                    now = DateTime.ParseExact(configuredNow, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture).Utc();
                }
                catch
                {
                    logger.Warn("The app setting 'FakeIntradayStartDateTime' is not specified correctly. Format your datetime as 'yyyy-MM-dd HH:mm' ");
                }
            }

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
				        if (lastExecuted.AddMinutes(updateResourceReadModelIntervalMinutes) < _now.UtcDateTime())
				        {
					        _businessUnitScope.OnThisThreadUse(businessUnit);
							_publisher.Publish(new UpdateStaffingLevelReadModelEvent()
					        {
								StartDateTime = now.AddHours(-25),
						        EndDateTime = now.AddHours(25)
					        });
				        }
			        }
			        else
			        {
						_businessUnitScope.OnThisThreadUse(businessUnit);
						_publisher.Publish(new UpdateStaffingLevelReadModelEvent()
						{
							StartDateTime = now.AddHours(-25),
							EndDateTime = now.AddHours(25)
						});
					}

		        });
		        _businessUnitScope.OnThisThreadUse(currentBusinessUnit);

	        }
        }
    }
}
