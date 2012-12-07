using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public class TeamExtractor : ITeamExtractor
    {
        private readonly List<IScheduleMatrixPro> _matrixLit;
        private List<IGroupPerson> _removedGroupPerson; 

        public TeamExtractor(List<IScheduleMatrixPro > matrixLit )
        {
            _matrixLit = matrixLit;
        }
        public IGroupPerson GetRamdomTeam()
        {
            return new GroupPerson(new List<IPerson>(), new DateOnly(), "", new Guid());
        }

    }

    public interface ITeamExtractor
    {
        IGroupPerson GetRamdomTeam();
    }
}