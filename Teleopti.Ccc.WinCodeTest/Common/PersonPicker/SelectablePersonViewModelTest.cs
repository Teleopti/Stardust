using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common.PersonPicker;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.PersonPicker
{
    [TestFixture]
    public class SelectablePersonViewModelTest
    {
        private SelectablePersonViewModel _target;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson("Roger");
            _target = new SelectablePersonViewModel(_person);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsFalse((bool)SelectablePersonViewModel.IsSelectedProperty.DefaultMetadata.DefaultValue);
            _target.IsSelected = true;
            Assert.IsTrue(_target.IsSelected);
            Assert.AreEqual(_person,_target.Person);
        }
    }
}
