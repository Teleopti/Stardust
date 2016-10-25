using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class TeamInfoFactoryFactory
	{
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;
		private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;

		public TeamInfoFactoryFactory(IGroupPersonBuilderWrapper groupPersonBuilderWrapper,
			IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory)
		{
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
			_groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
		}

		public ITeamInfoFactory Create(IScheduleDictionary scheduleDictionary, GroupPageLight groupPageLight)
		{
			_groupPersonBuilderWrapper.Reset();
			if (groupPageLight.Type == GroupPageType.SingleAgent)
			{
				_groupPersonBuilderWrapper.SetSingleAgentTeam();
			}
			else
			{
				_groupPersonBuilderForOptimizationFactory.Create(scheduleDictionary, groupPageLight);
			}

			return new TeamInfoFactory(_groupPersonBuilderWrapper);
		}
	}
}