using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class BusinessRuleResponseViewModelTest
    {
        private IBusinessRuleResponse _businessRuleResponse;
        private BusinessRuleResponseViewModel _target;
        private IPerson _person;
        private MockRepository _stubs;

        [SetUp]
        public void Setup()
        {
            _stubs = new MockRepository();
            _stubs.Record();
            _person = _stubs.Stub<IPerson>();
			_person.Stub(x => x.Name).Return(new  Name("John", "Doe"));
            _businessRuleResponse = _stubs.Stub<IBusinessRuleResponse>();
            Expect.Call(_businessRuleResponse.Message).Return("the message");
            Expect.Call(_businessRuleResponse.Person).Return(_person);
            _stubs.ReplayAll();

            _target = new BusinessRuleResponseViewModel(_businessRuleResponse);

        }

        [Test]
        public void ShouldProvideBasicProperties()
        {
            Assert.That(_target.Name, Is.EqualTo("John Doe"));
            Assert.That(_target.Message, Is.EqualTo("the message"));
        }
    }
}