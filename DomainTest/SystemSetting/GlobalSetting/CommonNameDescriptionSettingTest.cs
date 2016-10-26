using System;
using NUnit.Framework;
using SharpTestsEx;
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
		public void ShouldCheckParameter()
		{
			Assert.Throws<ArgumentNullException>(() => _target2.BuildCommonNameDescription((IPerson) null));
		}

		[Test]
		public void ShouldCheckParameterForCommonNameDescriptionSettingScheduleExport()
		{
			Assert.Throws<ArgumentNullException>(() => _target2.BuildCommonNameDescription((ILightPerson) null));
		}

        [Test]
        public void ShouldFormat()
        {
            var target = new CommonNameDescriptionSetting("{EmployeeNumber} - {FirstName} {LastName}");

            var name = target.BuildCommonNameDescription("John", "Smith", "123");

            name.Should().Be("123 - John Smith");
        }

	    [TestCase("{EmployeeNumber} - {FirstName} {LastName}", "UPDATE mart.dim_person SET person_name = [employment_number] + ' - ' + [first_name] + ' ' + [last_name] WHERE person_id != -1")]
		[TestCase("{EmployeeNumber}", "UPDATE mart.dim_person SET person_name = [employment_number] WHERE person_id != -1")]
		[TestCase("{FirstName} {LastName} #123# {EmployeeNumber}", "UPDATE mart.dim_person SET person_name = [first_name] + ' ' + [last_name] + ' #123# ' + [employment_number] WHERE person_id != -1")]
		[TestCase("123{::][}{__ {FirstName} {LastName} #{Firstname}# ", "UPDATE mart.dim_person SET person_name = '123{::][}{__ ' + [first_name] + ' ' + [last_name] + ' #{Firstname}# ' WHERE person_id != -1")]
		[TestCase("No names in reports", "UPDATE mart.dim_person SET person_name = 'No names in reports' WHERE person_id != -1")]
		[TestCase("{FirstName} '; Drop Database; ", "UPDATE mart.dim_person SET person_name = [first_name] + ' ''; Drop Database; ' WHERE person_id != -1")]
		[TestCase("{FirstName}{FirstName}", "UPDATE mart.dim_person SET person_name = [first_name] + [first_name] WHERE person_id != -1")]
		[TestCase("", "UPDATE mart.dim_person SET person_name = '' WHERE person_id != -1")]
		public void ShouldFormatAnalyticsSqlQuery(string format, string result)
	    {
			var target = new CommonNameDescriptionSetting(format);
		    var sql = target.BuildSqlUpdateForAnalytics();
		    sql.ToLower().Should().Be.EqualTo(result.ToLower());
	    }
    }
}