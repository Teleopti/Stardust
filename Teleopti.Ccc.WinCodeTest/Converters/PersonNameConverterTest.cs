using System.Windows;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters;

namespace Teleopti.Ccc.WinCodeTest.Converters
{
    [TestFixture]
    public class PersonNameConverterTest
    {
        private PersonNameConverter _target;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new PersonNameConverter();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyProperty()
        {
            CommonNameDescriptionSetting setting = new CommonNameDescriptionSetting();
            PersonNameConverter.Setting = setting;
            Assert.IsNotNull(PersonNameConverter.Setting);
        }

        [Test]
        public void VerifyConvert()
        {
            IPerson person = _mocks.StrictMock<IPerson>();
            Expect.Call(person.Name).Return(new Name("My", "Name")).Repeat.Any();
            Expect.Call(person.EmploymentNumber).Return("");
            _mocks.ReplayAll();

            PersonNameConverter.Setting = new CommonNameDescriptionSetting();
            object value = _target.Convert(person, null, null, null);
            _mocks.VerifyAll();

            Assert.AreEqual("My Name", value);
        }

        [Test]
        public void VerifyConvertBack()
        {
            object value = _target.ConvertBack(null, null, null, null);
            Assert.AreEqual(DependencyProperty.UnsetValue, value);
        }
    }
}
