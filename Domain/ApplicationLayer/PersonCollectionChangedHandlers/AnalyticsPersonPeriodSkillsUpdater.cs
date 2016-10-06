using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	[EnabledBy(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439)]
	public class AnalyticsPersonPeriodSkillsUpdater :
		IHandleEvent<AnalyticsPersonPeriodSkillsChangedEvent>,
	    IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsPersonPeriodSkillsUpdater));
		private readonly IAnalyticsSkillRepository _analyticsSkillRepository;

		public AnalyticsPersonPeriodSkillsUpdater(IAnalyticsSkillRepository analyticsSkillRepository)
		{
			_analyticsSkillRepository = analyticsSkillRepository;
		}

		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(AnalyticsPersonPeriodSkillsChangedEvent @event)
		{
			var personPeriodId = @event.AnalyticsPersonPeriodId;
			var businessUnitId = @event.AnalyticsBusinessUnitId;

			// Delete current references
			logger.Debug($"Deleting fact_agent_skill for person period id '{personPeriodId}'");
			_analyticsSkillRepository.DeleteAgentSkillForPersonId(personPeriodId);

			if (!@event.AnalyticsActiveSkillsId.Any() && !@event.AnalyticsInactiveSkillsId.Any())
				return;

			// Insert current parts
			logger.Debug($"Insert new fact_agent_skill references for person period id '{personPeriodId}' ");
			foreach (var activeSkillId in @event.AnalyticsActiveSkillsId)
			{
				_analyticsSkillRepository.AddAgentSkill(personPeriodId, activeSkillId, true, businessUnitId);
			}
			foreach (var inactiveSkillId in @event.AnalyticsInactiveSkillsId)
			{
				_analyticsSkillRepository.AddAgentSkill(personPeriodId, inactiveSkillId, false, businessUnitId);
			}
		}
	}
}