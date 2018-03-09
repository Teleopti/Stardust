using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class SendUpdateStaffingReadModelHandler : IHandleEvent<TenantMinuteTickEvent>, IRunOnHangfire
	{
		private readonly IJobStartTimeRepository _jobStartTimeRepository;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly IRequestStrategySettingsReader _requestStrategySettingsReader;
		private readonly IUpdatedBySystemUser _updatedBySystemUser;
		private readonly IEventPublisher _publisher;
		private readonly IStaffingSettingsReader _staffingSettingsReader;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IBusinessUnitScope _businessUnitScope;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public SendUpdateStaffingReadModelHandler(IBusinessUnitRepository businessUnitRepository,
			IRequestStrategySettingsReader requestStrategySettingsReader, IJobStartTimeRepository jobStartTimeRepository,
			IUpdatedBySystemUser updatedBySystemUser, IEventPublisher publisher, IStaffingSettingsReader staffingSettingsReader,
			ISkillDayRepository skillDayRepository, IScenarioRepository scenarioRepository, IBusinessUnitScope businessUnitScope,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_businessUnitRepository = businessUnitRepository;
			_requestStrategySettingsReader = requestStrategySettingsReader;
			_jobStartTimeRepository = jobStartTimeRepository;
			_updatedBySystemUser = updatedBySystemUser;
			_publisher = publisher;
			_staffingSettingsReader = staffingSettingsReader;
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			_businessUnitScope = businessUnitScope;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public virtual void Handle(TenantMinuteTickEvent @event)
		{
			var updateResourceReadModelIntervalMinutes =
				_requestStrategySettingsReader.GetIntSetting("UpdateResourceReadModelIntervalMinutes", 60);
			var numberOfDays = _staffingSettingsReader.GetIntSetting("StaffingReadModelNumberOfDays", 14);
			List<IBusinessUnit> businessUnits;
			using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				businessUnits = _businessUnitRepository.LoadAll().ToList();
			}

			businessUnits.ForEach(businessUnit =>
			{
				var businessUnitId = businessUnit.Id.GetValueOrDefault();
				using (_businessUnitScope.OnThisThreadUse(businessUnit))
				{
					using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
					{
						using (_updatedBySystemUser.Context())
						{
							var scenario = _scenarioRepository.LoadDefaultScenario();
							if (scenario != null)
							{
								if (_skillDayRepository.HasSkillDaysWithinPeriod(DateOnly.Today, DateOnly.Today.AddDays(numberOfDays),
									businessUnit, scenario))
								{
									if (!_jobStartTimeRepository.CheckAndUpdate(updateResourceReadModelIntervalMinutes, businessUnitId)) return;

									_publisher.Publish(new UpdateStaffingLevelReadModelEvent
									{
										Days = numberOfDays,
										LogOnBusinessUnitId = businessUnitId
									});
								}
							}
						}
					}
				}
			});
		}
	}
}
