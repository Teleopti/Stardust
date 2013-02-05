using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public class TeamExtractor : ITeamExtractor
    {
        private readonly IList<IScheduleMatrixPro> _matrixLit;
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
        private List<int> _processedIndex;
        private static Random _random;

        public TeamExtractor(IList<IScheduleMatrixPro> matrixLit, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
        {
            _matrixLit = matrixLit;
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
            _random = new Random();
            _processedIndex = new List<int>();
        }

        public IGroupPerson GetRandomTeam(DateOnly dateOnly )
        {
            var selectedPersonList = _matrixLit.Select(scheduleMatrixPro => scheduleMatrixPro.Person).ToList();
            int randomeNum;
            do
            {
                randomeNum = _random.Next(selectedPersonList.Count);
                if (!_processedIndex.Contains(randomeNum))
                    break;
                randomeNum = int.MinValue;

            } while (_processedIndex.Count != selectedPersonList.Count);
            if (randomeNum == int.MinValue)
                return null;
            _processedIndex.Add(randomeNum);
            var selectedPerson = selectedPersonList[randomeNum];
            return _groupPersonBuilderForOptimization.BuildGroupPerson(selectedPerson, dateOnly);
        }

    }

    public interface ITeamExtractor
    {
        IGroupPerson GetRandomTeam(DateOnly dateOnly);
    }
}