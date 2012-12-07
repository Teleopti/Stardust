using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class AdvanceSchedulingServiceTest
    {
        private MockRepository _mocks;
        private IAdvanceSchedulingService _target;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private ISkillDayPeriodIntervalData _skillDayPeriodIntervalData;
        private IDynamicBlockFinder _dynamicBlockFinder;
        private ITeamExtractor _teamExtractor;
        private IRestrictionAggregator _restrictionAggregator;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _skillDayPeriodIntervalData =  _mocks.StrictMock<ISkillDayPeriodIntervalData>();
            _dynamicBlockFinder = _mocks.StrictMock<IDynamicBlockFinder>();
            _teamExtractor = _mocks.StrictMock<ITeamExtractor>();
            _restrictionAggregator = _mocks.StrictMock<IRestrictionAggregator>();
            _target = new AdvanceSchedulingService(_skillDayPeriodIntervalData, _dynamicBlockFinder, _teamExtractor, _restrictionAggregator);
        }

        [Test]
        public void ShouldVerifyExecution()
        {
            Assert.That(_target.Execute(new Dictionary<string, IWorkShiftFinderResult>() ),Is.True   );
        }

       
    }

    
}
