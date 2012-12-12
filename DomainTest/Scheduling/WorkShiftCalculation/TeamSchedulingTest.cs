using System;
using System.Collections.Generic;
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
            _target = new TeamScheduling(_schedulingOptions, _teamSteadyStateHolder, _teamSteadyStateMainShiftScheduler,
                _groupPersonBuilderForOptimization, _groupPersonsBuilder,_rollbackService ,_resourceOptimizationHelper,_resultStateHolder );
        }

        [Test]
        public void ShouldExecute()
        {
            var selectedDays = new DateOnlyPeriod();
            IList<IScheduleMatrixPro> matrixList = null;
            IList<IPerson> selectedPersons = null;

            using(_mock.Playback()   )
            {
                Assert.AreEqual(_target.Execute(selectedDays, matrixList, selectedPersons), null);
            }
        }
    }

    public  class TeamScheduling : ITeamScheduling
    {
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
                                IResourceOptimizationHelper resourceOptimizationHelper, ISchedulingResultStateHolder resultStateHolder)
        {
            _schedulingOptions = schedulingOptions;
            _teamSteadyStateHolder = teamSteadyStateHolder;
            _teamSteadyStateMainShiftScheduler = teamSteadyStateMainShiftScheduler;
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
            _groupPersonsBuilder = groupPersonsBuilder;
            _rollbackService = rollbackService;
            _resourceOptimizationHelper = resourceOptimizationHelper;
            _resultStateHolder = resultStateHolder;
        }

        public object Execute()
        {
            throw new NotImplementedException();
        }

        public object Execute(DateOnlyPeriod selectedDays, IList<IScheduleMatrixPro> matrixList, IList<IPerson> selectedPersons)
        {
            if (matrixList == null) throw new ArgumentNullException("matrixList");
            
            //_cancelMe = false;
            foreach (var dateOnly in selectedDays.DayCollection())
            {
                var groupPersons = _groupPersonsBuilder.BuildListOfGroupPersons(dateOnly, selectedPersons, true, _schedulingOptions );
                
                foreach (var groupPerson in groupPersons. GetRandom(groupPersons.Count, true))
                {
                    //if (backgroundWorker.CancellationPending)
                    //    return;

                    var teamSteadyStateSuccess = false;

                    if (_teamSteadyStateHolder.IsSteadyState(groupPerson))
                    {
                        //if (!_teamSteadyStateMainShiftScheduler.ScheduleTeam(dateOnly, groupPerson, this, _rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, matrixList, _resultStateHolder.Schedules))
                        //    _teamSteadyStateHolder.SetSteadyState(groupPerson, false);

                        //else teamSteadyStateSuccess = true;
                    }

                    if (!teamSteadyStateSuccess)
                    {
                        _rollbackService.ClearModificationCollection();
                        //if (!ScheduleOneDay(dateOnly, _schedulingOptions, groupPerson, matrixList))
                        //{
                        //    _rollbackService.Rollback();
                        //    _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
                        //}
                    }
                }
            }
            return null;
        }
    }

    public  interface ITeamScheduling
    {
        object Execute(DateOnlyPeriod selectedDays, IList<IScheduleMatrixPro> matrixList, IList<IPerson> selectedPersons);
    }
}
