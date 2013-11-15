﻿using System.Collections.Generic;
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
	    private readonly ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;

	    public ValidatedTeamBlockInfoExtractor(ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator,  ITeamBlockInfoFactory teamBlockInfoFactory, ITeamSteadyStateHolder teamSteadyStateHolder, ITeamBlockSchedulingCompletionChecker teamBlockSchedulingCompletionChecker)
        {
            _teamBlockSteadyStateValidator = teamBlockSteadyStateValidator;
            _teamBlockInfoFactory = teamBlockInfoFactory;
            _teamSteadyStateHolder = teamSteadyStateHolder;
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
            bool singleAgentTeam = schedulingOptions.GroupOnGroupPageForTeamBlockPer != null &&
                                       schedulingOptions.GroupOnGroupPageForTeamBlockPer.Key == "SingleAgentTeam";
            ITeamBlockInfo teamBlockInfo;
            if (schedulingOptions.UseTeamBlockPerOption)
                teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer,
                                                                          schedulingOptions
                                                                              .BlockFinderTypeForAdvanceScheduling,
                                                                          singleAgentTeam, allPersonMatrixList);
            else
                teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer, BlockFinderType.SingleDay,
                                                                          singleAgentTeam, allPersonMatrixList);
            return teamBlockInfo;
        }
    }
}
