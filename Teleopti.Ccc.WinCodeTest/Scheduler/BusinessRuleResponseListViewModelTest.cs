using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class BusinessRuleResponseListViewModelTest
    {
        private BusinessRuleResponseListViewModel _target;
        private IBusinessRuleResponse _businessRuleResponse;
        private IBusinessRuleResponse[] _businessRuleResponses;
        private MockRepository _stubs;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _stubs = new MockRepository();
            _stubs.Record();
            _person = _stubs.Stub<IPerson>();
            _person.Name = new Name("John", "Doe");
            _businessRuleResponse = _stubs.Stub<IBusinessRuleResponse>();
            _businessRuleResponses = new[] {_businessRuleResponse};
            Expect.Call(_businessRuleResponse.Message).Return("the message");
            Expect.Call(_businessRuleResponse.Person).Return(_person);
            _stubs.ReplayAll();

            _target = new BusinessRuleResponseListViewModel(_businessRuleResponses);
        }

        [Test]
        public void ShouldProvideBusinessRuleViewModels()
        {
            Assert.That(_target.BusinessRuleResponses.Count(), Is.EqualTo(_businessRuleResponses.Count()));
        }
    }
}