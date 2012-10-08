using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ShiftCategoryFairness
{
    [TestFixture]
    public class ShiftCategoryFairnessContractTimeCheckTest
    {
        private MockRepository _mocks;
        private IShiftCategoryFairnessContractTimeChecker _target;
        private IScheduleDay _scheduleDay1, _scheduleDay2;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new ShiftCategoryFairnessContractTimeChecker();
            _scheduleDay1 = _mocks.DynamicMock<IScheduleDay>();
            _scheduleDay2 = _mocks.DynamicMock<IScheduleDay>();
        }

        [Test]
        public void SameShouldReturnTrue()
        {
            var projectionService = _mocks.DynamicMock<IProjectionService>();
            var projection = _mocks.DynamicMock<IVisualLayerCollection>();
            var contractTime = new TimeSpan(14, 59, 39);

            Expect.Call(_scheduleDay1.ProjectionService()).Return(projectionService);
            Expect.Call(projectionService.CreateProjection()).Return(projection);
            Expect.Call(projection.ContractTime()).Return(contractTime); 
            
            Expect.Call(contractTime.Equals(contractTime));
            
            _mocks.ReplayAll();
            Assert.That(_target.Check(_scheduleDay1, _scheduleDay1), Is.True);
            _mocks.VerifyAll();
        }

        [Test]
        public void DifferentShouldReturnFalse()
        {
            var projectionService = _mocks.DynamicMock<IProjectionService>();
            var projection = _mocks.DynamicMock<IVisualLayerCollection>();
            var projectionServiceTwo = _mocks.DynamicMock<IProjectionService>();
            var projectionTwo = _mocks.DynamicMock<IVisualLayerCollection>(); 
            var contractTime = new TimeSpan(14, 59, 39);
            var contractTimeTwo = new TimeSpan(23, 59, 59);

            Expect.Call(_scheduleDay1.ProjectionService()).Return(projectionService);
            Expect.Call(projectionService.CreateProjection()).Return(projection);
            Expect.Call(projection.ContractTime()).Return(contractTime);

            Expect.Call(_scheduleDay2.ProjectionService()).Return(projectionServiceTwo);
            Expect.Call(projectionServiceTwo.CreateProjection()).Return(projectionTwo);
            Expect.Call(projectionTwo.ContractTime()).Return(contractTimeTwo);

            Expect.Call(contractTime.Equals(contractTimeTwo));

            _mocks.ReplayAll();
            Assert.That(_target.Check(_scheduleDay1, _scheduleDay2), Is.False);
            _mocks.VerifyAll();
        }

    }
}
