using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
    [TestFixture]
    public class TeamSchedulingTest
    {

        private MockRepository _mock;
        private ITeamScheduling _target;
        private ISchedulingOptions _schedulingOptions;
        private ITeamSteadyStateHolder _teamSteadyStateHolder;
        private ITeamSteadyStateMainShiftScheduler _teamSteadyStateMainShiftScheduler;
        private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
        private  IGroupPersonsBuilder _groupPersonsBuilder;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private readonly ISchedulingResultStateHolder _resultStateHolder;

        [SetUp ]
        public void Setup()
        {
            _mock = new MockRepository();
            //_target = new TeamScheduling(_schedulingOptions, _teamSteadyStateHolder, _teamSteadyStateMainShiftScheduler,
            //    _groupPersonBuilderForOptimization, _groupPersonsBuilder,_rollbackService ,_resourceOptimizationHelper,_resultStateHolder );
        }

        [Test]
        public void ShouldExecute()
        {
            var selectedDays = new DateOnlyPeriod();
            IList<IScheduleMatrixPro> matrixList = null;
            IList<IPerson> selectedPersons = null;

            using(_mock.Playback()   )
            {
                //Assert.AreEqual(_target.Execute(selectedDays, matrixList, selectedPersons), null);
            }
        }
    }

   
}
