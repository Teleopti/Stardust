using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

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

		public ITeamInfoFactory Create(IEnumerable<IPerson> allPermittedPersons, IScheduleDictionary scheduleDictionary, GroupPageLight groupPageLight)
		{
			_groupPersonBuilderWrapper.Reset();
			if (groupPageLight.Type == GroupPageType.SingleAgent)
			{
				_groupPersonBuilderWrapper.SetSingleAgentTeam();
			}
			else
			{
				_groupPersonBuilderForOptimizationFactory.Create(allPermittedPersons, scheduleDictionary, groupPageLight);
			}

			return new TeamInfoFactory(_groupPersonBuilderWrapper);
		}
	}
}