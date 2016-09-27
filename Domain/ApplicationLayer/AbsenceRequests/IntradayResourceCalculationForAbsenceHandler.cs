using System;
using System.Globalization;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
    [EnabledBy(Toggles.AbsenceRequests_SpeedupIntradayRequests_40754, Toggles.AbsenceRequests_UseMultiRequestProcessing_39960)]
    public class IntradayResourceCalculationForAbsenceHandler : IHandleEvent<TenantMinuteTickEvent>, IRunOnHangfire
    {
        private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
        private readonly IBusinessUnitRepository _businessUnitRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IUpdatedByScope _updatedByScope;
        private readonly IBusinessUnitScope _businessUnitScope;
        private readonly INow _now;
        private readonly IEventPublisher _publisher;
        private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
        private readonly IConfigReader _configReader;
        private static readonly ILog logger = LogManager.GetLogger(typeof(IntradayResourceCalculationForAbsenceHandler));


        public IntradayResourceCalculationForAbsenceHandler(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IBusinessUnitRepository businessUnitRepository, IPersonRepository personRepository, IUpdatedByScope updatedByScope, IBusinessUnitScope businessUnitScope, INow now, IEventPublisher publisher, IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository, IConfigReader configReader)
        {
            _currentUnitOfWorkFactory = currentUnitOfWorkFactory;
            _businessUnitRepository = businessUnitRepository;
            _personRepository = personRepository;
            _updatedByScope = updatedByScope;
            _businessUnitScope = businessUnitScope;
            _now = now;
            _publisher = publisher;
            _scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
            _configReader = configReader;
        }

        public void Handle(TenantMinuteTickEvent @event)
        {
            //for now its 20 minutes should it be 10 minutes? and its going to calculate data from now till next 24 hours

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
                var lastExecuted = _scheduleForecastSkillReadModelRepository.GetLastCalculatedTime();
                if (lastExecuted.AddMinutes(20) < _now.UtcDateTime())
                {
                    var businessUnits = _businessUnitRepository.LoadAll();
                    var person = _personRepository.Get(SystemUser.Id);
                    _updatedByScope.OnThisThreadUse(person);

                    businessUnits.ForEach(businessUnit =>
                    {
                        _businessUnitScope.OnThisThreadUse(businessUnit);
                        _publisher.Publish(new UpdateResourceCalculateReadModelEvent()
                        {
                            StartDateTime = now,
                            EndDateTime = now.AddHours(24)
                        });
                    });
                }

            }
        }
    }
}
