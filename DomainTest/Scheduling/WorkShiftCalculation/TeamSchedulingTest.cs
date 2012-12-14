using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
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

    public  class TeamScheduling : ITeamScheduling
    {
        private readonly IGroupSchedulingService _groupSchedulingService;
        private readonly ISchedulingOptions _schedulingOptions;
        private readonly ITeamSteadyStateHolder _teamSteadyStateHolder;
        private readonly ITeamSteadyStateMainShiftScheduler _teamSteadyStateMainShiftScheduler;
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
        private readonly IGroupPersonsBuilder _groupPersonsBuilder;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private readonly ISchedulingResultStateHolder _resultStateHolder;


        public TeamScheduling(ISchedulingOptions schedulingOptions, 
                              ITeamSteadyStateHolder teamSteadyStateHolder, 
                              ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler,
                              IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization, IGroupPersonsBuilder groupPersonsBuilder, ISchedulePartModifyAndRollbackService rollbackService,
                                IResourceOptimizationHelper resourceOptimizationHelper, ISchedulingResultStateHolder resultStateHolder, IGroupSchedulingService groupSchedulingService)
        {
            _schedulingOptions = schedulingOptions;
            _teamSteadyStateHolder = teamSteadyStateHolder;
            _teamSteadyStateMainShiftScheduler = teamSteadyStateMainShiftScheduler;
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
            _groupPersonsBuilder = groupPersonsBuilder;
            _rollbackService = rollbackService;
            _resourceOptimizationHelper = resourceOptimizationHelper;
            _resultStateHolder = resultStateHolder;
            _groupSchedulingService = groupSchedulingService;
        }


        public void  Execute(DateOnlyPeriod selectedDays, IList<IScheduleMatrixPro> matrixList, IList<IPerson> selectedPersons)
        {
            if (matrixList == null) throw new ArgumentNullException("matrixList");

            _groupSchedulingService.Execute(selectedDays, matrixList, _schedulingOptions, selectedPersons,
                                            new BackgroundWorker(), _teamSteadyStateHolder,
                                            _teamSteadyStateMainShiftScheduler, _groupPersonBuilderForOptimization);
        }
    }

    public  interface ITeamScheduling
    {
        void Execute(DateOnlyPeriod selectedDays, IList<IScheduleMatrixPro> matrixList, IList<IPerson> selectedPersons);
    }
}
