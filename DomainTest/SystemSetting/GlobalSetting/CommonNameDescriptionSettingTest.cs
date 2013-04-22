using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SystemSetting.GlobalSetting
{
    [TestFixture]
    public class CommonNameDescriptionSettingTest
    {
        private CommonNameDescriptionSetting _target;
        private CommonNameDescriptionSettingScheduleExport _target2;

        [SetUp]
        public void Setup()
        {
            _target = new CommonNameDescriptionSetting();
            _target2 = new CommonNameDescriptionSettingScheduleExport();
        }

        [Test]
        public void CanCreateObject()
        {
            Assert.IsNotNull(_target);
            Assert.IsNotNull(_target2);
        }

        [Test]
        public void CanBuildCommonNameDescription()
        {
            IPerson person = PersonFactory.CreatePerson("Kalle", "Kula");
            person.EmploymentNumber = "10";
            //System.Console.Out.WriteLine(_target.BuildCommonNameDescription(person));
            Assert.AreEqual("Kalle Kula", _target.BuildCommonNameDescription(person));
            Assert.AreEqual("Kalle Kula", _target2.BuildCommonNameDescription(person));

            CommonNameDescriptionSetting newCommonNameDescriptionSetting = new CommonNameDescriptionSetting(CommonNameDescriptionSetting.EmployeeNumber + " - " + CommonNameDescriptionSetting.FirstName + " " + CommonNameDescriptionSetting.LastName);
            CommonNameDescriptionSettingScheduleExport newCommonNameDescriptionSettingScheduleExport = new CommonNameDescriptionSettingScheduleExport(CommonNameDescriptionSettingScheduleExport.EmployeeNumber + " - " + CommonNameDescriptionSettingScheduleExport.FirstName + " " + CommonNameDescriptionSettingScheduleExport.LastName);
            Assert.AreEqual("10 - Kalle Kula", newCommonNameDescriptionSetting.BuildCommonNameDescription(person));
            Assert.AreEqual("10 - Kalle Kula", newCommonNameDescriptionSettingScheduleExport.BuildCommonNameDescription(person));

            newCommonNameDescriptionSetting.AliasFormat = CommonNameDescriptionSetting.LastName;
            newCommonNameDescriptionSettingScheduleExport.AliasFormat = CommonNameDescriptionSettingScheduleExport.LastName;
            Assert.AreEqual("Kula", newCommonNameDescriptionSetting.BuildCommonNameDescription(person));
            Assert.AreEqual("Kula", newCommonNameDescriptionSettingScheduleExport.BuildCommonNameDescription(person));

	        ILightPerson lightPerson = new PersonSelectorBuiltIn
		        {
					EmploymentNumber = "10",
					FirstName = "Kalle",
					LastName = "Kula"
		        };
			var newCommonNameDescriptionSettingLightPerson = new CommonNameDescriptionSetting(CommonNameDescriptionSetting.EmployeeNumber + " - " + CommonNameDescriptionSetting.FirstName + " " + CommonNameDescriptionSetting.LastName);
            var newCommonNameDescriptionSettingScheduleExportLightPerson = new CommonNameDescriptionSettingScheduleExport(CommonNameDescriptionSettingScheduleExport.EmployeeNumber + " - " + CommonNameDescriptionSettingScheduleExport.FirstName + " " + CommonNameDescriptionSettingScheduleExport.LastName);
			Assert.AreEqual("10 - Kalle Kula", newCommonNameDescriptionSettingLightPerson.BuildCommonNameDescription(lightPerson));
            Assert.AreEqual("10 - Kalle Kula", newCommonNameDescriptionSettingScheduleExportLightPerson.BuildCommonNameDescription(lightPerson));
			newCommonNameDescriptionSettingLightPerson.AliasFormat = CommonNameDescriptionSetting.LastName;
			newCommonNameDescriptionSettingScheduleExportLightPerson.AliasFormat = CommonNameDescriptionSettingScheduleExport.LastName;
			Assert.AreEqual("Kula", newCommonNameDescriptionSetting.BuildCommonNameDescription(lightPerson));
            Assert.AreEqual("Kula", newCommonNameDescriptionSettingScheduleExportLightPerson.BuildCommonNameDescription(lightPerson));
        }

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldCheckParameter()
		{
			IPerson person = null;
			_target2.BuildCommonNameDescription(person);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldCheckParameterForCommonNameDescriptionSettingScheduleExport()
		{
			ILightPerson person = null;
			_target2.BuildCommonNameDescription(person);
		}
    }
}