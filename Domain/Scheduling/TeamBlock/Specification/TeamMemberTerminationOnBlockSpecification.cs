namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification
{
    public interface ITeamMemberTerminationOnBlockSpecification
    {
        bool IsSatisfy(ITeamInfo teamInfo, IBlockInfo blockInfo);
    }

    public class TeamMemberTerminationOnBlockSpecification : ITeamMemberTerminationOnBlockSpecification
    {
        public bool IsSatisfy(ITeamInfo teamInfo, IBlockInfo blockInfo)
        {
            foreach (var person in teamInfo.GroupMembers)
            {
                foreach (var day in blockInfo.BlockPeriod.DayCollection())
                {
                    if (person.TerminalDate < day)
                        return false;
                }
            }
            return true;
        }
    }
}