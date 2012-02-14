using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
    [TestFixture]
    public class ModuleSpecificationTest
    {
        private ISpecification<IApplicationFunction> target;
        private MockRepository mocks;
        private IApplicationFunction applicationFunction;
        
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            applicationFunction = mocks.DynamicMock<IApplicationFunction>();
            target = new ModuleSpecification();
        }

        [Test]
        public void ShouldHandleModuleFunction()
        {
            using (mocks.Record())
            {
                Expect.Call(applicationFunction.Level).Return(1);
                Expect.Call(applicationFunction.SortOrder).Return(3);
                Expect.Call(applicationFunction.Parent).Return(applicationFunction);
            }
            using (mocks.Playback())
            {
                target.IsSatisfiedBy(applicationFunction).Should().Be.True();
            }
        }

        [Test]
        public void ShouldHandleOtherFunctions()
        {
            using (mocks.Record())
            {
                Expect.Call(applicationFunction.SortOrder).Return(3);
                Expect.Call(applicationFunction.Level).Return(2);
                Expect.Call(applicationFunction.Parent).Return(applicationFunction);
            }
            using (mocks.Playback())
            {
                target.IsSatisfiedBy(applicationFunction).Should().Be.False();
            }
        }
    }
}