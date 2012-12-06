using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class AdvanceSchedulingServiceTest
    {
        private MockRepository _mocks;
        private IAdvanceSchedulingService _target;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _target = new AdvanceSchedulingService(new SchedulingOptions(), _schedulingResultStateHolder);
        }

        [Test]
        public void ShouldVerifyExecution()
        {
            Assert.That(_target.Execute(new Dictionary<string, IWorkShiftFinderResult>() ),Is.True   );
        }

       
    }

    
}
