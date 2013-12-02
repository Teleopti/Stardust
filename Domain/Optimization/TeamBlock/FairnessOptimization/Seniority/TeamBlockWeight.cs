using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public class TeamBlockWeight : IEquatable<TeamBlockWeight>
    {
        public int TeamWeight { get; set; }
        public int BlockWeight { get; set; }
        public virtual bool Equals(TeamBlockWeight other)
        {
            if (TeamWeight == other.TeamWeight && BlockWeight == other.BlockWeight)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return (TeamWeight.GetHashCode()  * 397) ^ (BlockWeight.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            var teamBlockWeight = obj as TeamBlockWeight;
            return teamBlockWeight != null && Equals(teamBlockWeight);
        }
        
    }
}