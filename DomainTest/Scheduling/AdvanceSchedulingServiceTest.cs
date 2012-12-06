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

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new AdvanceSchedulingService(new SchedulingOptions());
        }

        [Test]
        public void ShouldVerifyExecution()
        {
            Assert.That(_target.Execute(new List<IScheduleMatrixPro>( ),new Dictionary<string, IWorkShiftFinderResult>() ),Is.True   );
        }

       
    }

    
}
