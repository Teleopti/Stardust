﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IValidatedTeamBlockInfoExtractor
    {
	    ITeamBlockInfo GetTeamBlockInfo(ITeamInfo teamInfo, DateOnly datePointer,
		    IList<IScheduleMatrixPro> allPersonMatrixList, ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod);
    }

    public class ValidatedTeamBlockInfoExtractor : IValidatedTeamBlockInfoExtractor
    {
        private readonly ITeamBlockSteadyStateValidator  _teamBlockSteadyStateValidator;
        private readonly ITeamBlockInfoFactory  _teamBlockInfoFactory;
        private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;

	    public ValidatedTeamBlockInfoExtractor(ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator,
	                                           ITeamBlockInfoFactory teamBlockInfoFactory,
	                                           ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
	                                           ITeamBlockSchedulingCompletionChecker teamBlockSchedulingCompletionChecker)
	    {
		    _teamBlockSteadyStateValidator = teamBlockSteadyStateValidator;
		    _teamBlockInfoFactory = teamBlockInfoFactory;
		    _teamBlockSchedulingOptions = teamBlockSchedulingOptions;
		    _teamBlockSchedulingCompletionChecker = teamBlockSchedulingCompletionChecker;
	    }

	    public ITeamBlockInfo GetTeamBlockInfo(ITeamInfo teamInfo, DateOnly datePointer,
		    IList<IScheduleMatrixPro> allPersonMatrixList, ISchedulingOptions schedulingOptions, 
			DateOnlyPeriod selectedPeriod)
	    {
		    if (teamInfo == null || schedulingOptions == null) return null;
		    var teamBlockInfo = createTeamBlockInfo(datePointer, teamInfo, schedulingOptions,
			    selectedPeriod);
		    if (teamBlockInfo == null) return null;
		    if (_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlock(teamBlockInfo, datePointer, schedulingOptions)) return null;
		    if (!_teamBlockSteadyStateValidator.IsTeamBlockInSteadyState(teamBlockInfo, schedulingOptions)) return null;

		    return teamBlockInfo;
	    }

	    private ITeamBlockInfo createTeamBlockInfo(DateOnly datePointer,
		    ITeamInfo teamInfo, ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod)
	    {
		    ITeamBlockInfo teamBlockInfo;
				if (schedulingOptions.UseBlock)
			    teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer,
						schedulingOptions.BlockFinder(),
				    _teamBlockSchedulingOptions.IsSingleAgentTeam(schedulingOptions));
		    else
			    teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer, new SingleDayBlockFinder(), 
				    _teamBlockSchedulingOptions.IsSingleAgentTeam(schedulingOptions));

		    if (teamBlockInfo == null)
			    return null;

		    var blockInfo = teamBlockInfo.BlockInfo;
		    foreach (var dateOnly in blockInfo.BlockPeriod.DayCollection())
		    {
			    if (!selectedPeriod.Contains(dateOnly))
				    blockInfo.LockDate(dateOnly);
		    }

		    return teamBlockInfo;
	    }
    }
}
