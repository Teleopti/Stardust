using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{

    public interface ITeamBlockPoints
    {
        ITeamBlockInfo TeamBlockInfo { get; }
        double Points { get; }
    }
    /// <summary>
    /// This class will be used to calculate the points for weekday, scnerioty and shift category
    /// </summary>
    public class TeamBlockPoints : ITeamBlockPoints
    {
        public TeamBlockPoints(ITeamBlockInfo teamBlockInfo, double points)
        {
            TeamBlockInfo = teamBlockInfo;
            Points = points;
        }

        public ITeamBlockInfo TeamBlockInfo { get; private set; }
        public double Points { get; private set; }
    }
}
