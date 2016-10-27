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

	    [TestCase("{EmployeeNumber} - {FirstName} {LastName}", "UPDATE mart.dim_person SET person_name = SUBSTRING(ISNULL([employment_number], '') + N' - ' + ISNULL([first_name], '') + N' ' + ISNULL([last_name], ''), 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit")]
		[TestCase("{EmployeeNumber}", "UPDATE mart.dim_person SET person_name = SUBSTRING(ISNULL([employment_number], ''), 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit")]
		[TestCase("{FirstName} {LastName} #123# {EmployeeNumber}", "UPDATE mart.dim_person SET person_name = SUBSTRING(ISNULL([first_name], '') + N' ' + ISNULL([last_name], '') + N' #123# ' + ISNULL([employment_number], ''), 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit")]
		[TestCase("123{::][}{__ {FirstName} {LastName} #{Firstname}# ", "UPDATE mart.dim_person SET person_name = SUBSTRING(N'123{::][}{__ ' + ISNULL([first_name], '') + N' ' + ISNULL([last_name], '') + N' #{Firstname}# ', 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit")]
		[TestCase("No names in reports", "UPDATE mart.dim_person SET person_name = SUBSTRING(N'No names in reports', 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit")]
		[TestCase("{FirstName} '; Drop Database; ", "UPDATE mart.dim_person SET person_name = SUBSTRING(ISNULL([first_name], '') + N' ''; Drop Database; ', 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit")]
		[TestCase("{FirstName}{FirstName}", "UPDATE mart.dim_person SET person_name = SUBSTRING(ISNULL([first_name], '') + ISNULL([first_name], ''), 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit")]
		[TestCase("", "UPDATE mart.dim_person SET person_name = SUBSTRING(N'', 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit")]
		[TestCase("{FirstName}лаудем  伴年聞早 無巣目個 지에 그들을", "UPDATE mart.dim_person SET person_name = SUBSTRING(ISNULL([first_name], '') + N'лаудем  伴年聞早 無巣目個 지에 그들을', 0, 200), update_date=GETUTCDATE() WHERE [business_unit_code] = :BusinessUnit")]
		public void ShouldFormatAnalyticsSqlQuery(string format, string result)
	    {
			var target = new CommonNameDescriptionSetting(format);
		    var sql = target.BuildSqlUpdateForAnalytics();
		    sql.ToLower().Should().Be.EqualTo(result.ToLower());
	    }
    }
}