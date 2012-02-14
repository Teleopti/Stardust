using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Payroll
{
    [TestFixture]
    public class DefinitionSetViewModelTest
    {
        private IDefinitionSetViewModel _target;
        private IMultiplicatorDefinitionSet _definitionSet;
        private string _name = "My Test Definition Set";
        private const MultiplicatorType _multiplicatorType = MultiplicatorType.Overtime;

        [SetUp]
        public void Setup()
        {
            _definitionSet = new MultiplicatorDefinitionSet(_name, _multiplicatorType);
            _target = new DefinitionSetViewModel(_definitionSet);
        }

        [Test]
        public void VerifyMultiplicatorName()
        {
            Assert.AreEqual(_name, _target.Name);
        }

        [Test]
        public void VerifyMultiplicatorType()
        {
            Assert.AreEqual(_multiplicatorType, _target.MultiplicatorType);
        }

        [Test]
        public void ShouldExposeLocalizedUpdateInformation()
        {
            Assert.IsNotNull(_target.ChangeInfo);
        }
    }
}
