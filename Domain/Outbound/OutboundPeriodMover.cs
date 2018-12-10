using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Outbound
{
	public interface IOutboundPeriodMover
	{
        void Move(IOutboundCampaign campaign, DateOnlyPeriod oldPeriod);
	}

	public class OutboundPeriodMover : IOutboundPeriodMover
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ICreateOrUpdateSkillDays _createOrUpdateSkillDays;

		public OutboundPeriodMover(ISkillDayRepository skillDayRepository, IScenarioRepository scenarioRepository, ICreateOrUpdateSkillDays createOrUpdateSkillDays)
		{
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
			_createOrUpdateSkillDays = createOrUpdateSkillDays;
		}

        public void Move(IOutboundCampaign campaign, DateOnlyPeriod oldPeriod)
		{
			_skillDayRepository.Delete(oldPeriod, campaign.Skill, _scenarioRepository.LoadDefaultScenario());
			foreach (var dateOnly in oldPeriod.DayCollection())
			{
				campaign.ClearActualBacklog(dateOnly);
				campaign.ClearProductionPlan(dateOnly);
			}

			_createOrUpdateSkillDays.Create(campaign.Skill, campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone), campaign.CampaignTasks(), campaign.AverageTaskHandlingTime(),
				campaign.WorkingHours);
		}
	}
}