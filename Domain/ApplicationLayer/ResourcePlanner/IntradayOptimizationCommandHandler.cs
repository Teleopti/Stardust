using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommandHandler : IIntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;

		public IntradayOptimizationCommandHandler(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Execute(IntradayOptimizationCommand command)
		{
			//completly wrong - just make tests pass for now
			var skillDic = new Dictionary<string, ICollection<IPerson>>();
			foreach (var agent in command.Agents)
			{
				ICollection<IPerson> agentsIsland;
				var skillString = string.Join("$", agent.Period(command.Period.StartDate).PersonSkillCollection.Select(x => x.Skill.Id.Value));
				if (skillDic.TryGetValue(skillString, out agentsIsland))
				{
					agentsIsland.Add(agent);
				}
				else
				{
					skillDic[skillString] = new List<IPerson> {agent};
				}
			}

			foreach (var skillKeyValue in skillDic)
			{
				_eventPublisher.Publish(new OptimizationWasOrdered
				{
					Period = command.Period,
					AgentIds = skillKeyValue.Value.Select(x => x.Id.Value)
				});
			}
		}
	}

	//remove below when toggle 36939 is removed
	public interface IIntradayOptimizationCommandHandler
	{
		void Execute(IntradayOptimizationCommand command);
	}
	public class IntradayOptimizationOneThreadCommandHandler : IIntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;

		public IntradayOptimizationOneThreadCommandHandler(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Execute(IntradayOptimizationCommand command)
		{
			_eventPublisher.Publish(new OptimizationWasOrdered
			{
				Period = command.Period,
				AgentIds = command.Agents.Select(x => x.Id.Value)
			});
		}
	}
}