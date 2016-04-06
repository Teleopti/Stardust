using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification
{
    public interface ITeamMemberTerminationOnBlockSpecification
    {
        void LockTerminatedMembers(ITeamInfo teamInfo, IBlockInfo blockInfo);
    }

    public class TeamMemberTerminationOnBlockSpecification : ITeamMemberTerminationOnBlockSpecification
    {
        public void LockTerminatedMembers(ITeamInfo teamInfo, IBlockInfo blockInfo)
        {
            foreach (var person in teamInfo.GroupMembers)
            {
                foreach (var day in blockInfo.BlockPeriod.DayCollection())
                {
                    if (person.TerminalDate < day)
						teamInfo.LockMember(new DateOnlyPeriod(day, day), person );
                       
                }
            }  
        }
    }
}