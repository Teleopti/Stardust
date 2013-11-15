using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IValidatedTeamBlockInfoExtractor
    {
        ITeamBlockInfo GetTeamBlockInfo(ITeamInfo teamInfo, DateOnly datePointer, IList<IScheduleMatrixPro> allPersonMatrixList, ISchedulingOptions schedulingOptions);
    }

    public class ValidatedTeamBlockInfoExtractor : IValidatedTeamBlockInfoExtractor
    {
        private readonly ITeamBlockSteadyStateValidator  _teamBlockSteadyStateValidator;
        private readonly ITeamBlockInfoFactory  _teamBlockInfoFactory;
        private readonly ITeamSteadyStateHolder _teamSteadyStateHolder;
        private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;

	    public ValidatedTeamBlockInfoExtractor(ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator,
	                                           ITeamBlockInfoFactory teamBlockInfoFactory,
	                                           ITeamSteadyStateHolder teamSteadyStateHolder,
	                                           ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
	                                           ITeamBlockSchedulingCompletionChecker teamBlockSchedulingCompletionChecker)
	    {
		    _teamBlockSteadyStateValidator = teamBlockSteadyStateValidator;
		    _teamBlockInfoFactory = teamBlockInfoFactory;
		    _teamSteadyStateHolder = teamSteadyStateHolder;
		    _teamBlockSchedulingOptions = teamBlockSchedulingOptions;
		    _teamBlockSchedulingCompletionChecker = teamBlockSchedulingCompletionChecker;
	    }

	    public ITeamBlockInfo GetTeamBlockInfo(ITeamInfo teamInfo, DateOnly datePointer, IList<IScheduleMatrixPro> allPersonMatrixList, ISchedulingOptions schedulingOptions )
        {
            if (teamInfo == null || schedulingOptions == null) return null;
            if (!_teamSteadyStateHolder.IsSteadyState(teamInfo.GroupPerson)) return null;
            var teamBlockInfo = createTeamBlockInfo(allPersonMatrixList, datePointer, teamInfo, schedulingOptions);
            if (teamBlockInfo == null) return null;
            if (_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlock(teamBlockInfo, datePointer)) return null;
            if (!_teamBlockSteadyStateValidator.IsBlockInSteadyState(teamBlockInfo, schedulingOptions)) return null;

            return teamBlockInfo;
        }

        private ITeamBlockInfo createTeamBlockInfo(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnly datePointer, ITeamInfo teamInfo, ISchedulingOptions schedulingOptions)
        {
            ITeamBlockInfo teamBlockInfo;
            if (schedulingOptions.UseTeamBlockPerOption)
                teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer,
                                                                          schedulingOptions
                                                                              .BlockFinderTypeForAdvanceScheduling,
                                                                          _teamBlockSchedulingOptions.IsSingleAgentTeam(schedulingOptions), allPersonMatrixList);
            else
                teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer, BlockFinderType.SingleDay,
                                                                           _teamBlockSchedulingOptions.IsSingleAgentTeam(schedulingOptions), allPersonMatrixList);
            return teamBlockInfo;
        }
    }
}
