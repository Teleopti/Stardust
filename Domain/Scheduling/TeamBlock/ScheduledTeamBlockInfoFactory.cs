using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class ScheduledTeamBlockInfoFactory
	{
		private readonly MatrixListFactory _matrixListFactory;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;
		private readonly ITeamBlockGenerator _teamBlockGenerator;
		private readonly LockDaysOnTeamBlockInfos _lockDaysOnTeamBlockInfos;

		public ScheduledTeamBlockInfoFactory(MatrixListFactory matrixListFactory,
														TeamInfoFactoryFactory teamInfoFactoryFactory,
														ITeamBlockGenerator teamBlockGenerator,
														LockDaysOnTeamBlockInfos lockDaysOnTeamBlockInfos)
		{
			_matrixListFactory = matrixListFactory;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
			_teamBlockGenerator = teamBlockGenerator;
			_lockDaysOnTeamBlockInfos = lockDaysOnTeamBlockInfos;
		}

		public IEnumerable<ITeamBlockInfo> Create(DateOnlyPeriod period, IEnumerable<IPerson> agentsToOptimize, IScheduleDictionary schedules, IPerson[] allAgents, SchedulingOptions schedulingOptions)
		{
			var allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(schedules, allAgents, period);
			_teamInfoFactoryFactory.Create(allAgents, schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);
			var teamBlockInfos = _teamBlockGenerator.Generate(allAgents, allMatrixes, period, agentsToOptimize, schedulingOptions);

			_lockDaysOnTeamBlockInfos.LockUnscheduleDaysAndRemoveEmptyTeamBlockInfos(teamBlockInfos);
			return teamBlockInfos;
		}
	}
}