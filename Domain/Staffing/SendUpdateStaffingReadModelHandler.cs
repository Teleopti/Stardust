using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
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
	    private readonly IRequestStrategySettingsReader _requestStrategySettingsReader;

        public SendUpdateStaffingReadModelHandler(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IBusinessUnitRepository businessUnitRepository,
			IPersonRepository personRepository, IUpdatedByScope updatedByScope, IBusinessUnitScope businessUnitScope, INow now, IEventPublisher publisher, 
			IRequestStrategySettingsReader requestStrategySettingsReader, IJobStartTimeRepository jobStartTimeRepository)
        {
            _currentUnitOfWorkFactory = currentUnitOfWorkFactory;
            _businessUnitRepository = businessUnitRepository;
            _personRepository = personRepository;
            _updatedByScope = updatedByScope;
            _businessUnitScope = businessUnitScope;
            _now = now;
            _publisher = publisher;
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
				var businessUnits = _businessUnitRepository.LoadAll();
		        var person = _personRepository.Get(SystemUser.Id);
		        _updatedByScope.OnThisThreadUse(person);

		        businessUnits.ForEach(businessUnit =>
		        {
					_businessUnitScope.OnThisThreadUse(businessUnit);
					if (!_jobStartTimeRepository.CheckAndUpdate(updateResourceReadModelIntervalMinutes)) return;
					
			        _publisher.Publish(new UpdateStaffingLevelReadModelEvent
			        {
				        Days = 1
			        });

				});
		        _businessUnitScope.OnThisThreadUse(currentBusinessUnit);

	        }
        }
    }
}
