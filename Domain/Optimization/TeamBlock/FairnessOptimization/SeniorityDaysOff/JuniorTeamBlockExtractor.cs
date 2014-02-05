﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface IJuniorTeamBlockExtractor
    {
        ITeamBlockInfo GetJuniorTeamBlockInfo(List<ITeamBlockPoints> swappableTeamBlocksPoints);
    }

    public class JuniorTeamBlockExtractor : IJuniorTeamBlockExtractor
    {
        public  ITeamBlockInfo GetJuniorTeamBlockInfo(List<ITeamBlockPoints> swappableTeamBlocksPoints)
        {
            var value = double.MaxValue;
            ITeamBlockInfo juniorTeamBlock = null;
            foreach (var teamBlockPoint in swappableTeamBlocksPoints)
            {
                if (teamBlockPoint.Points < value)
                {
                    value = teamBlockPoint.Points;
                    juniorTeamBlock = teamBlockPoint.TeamBlockInfo;
                }
            }
            return juniorTeamBlock;
        }
    }
}
