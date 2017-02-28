using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.People.Core;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[TestFixture]
	public class ImportAgentFileValidatorTest
	{
		private readonly ImportAgentFileValidator target = new ImportAgentFileValidator();

		[Test]
		public void ShouldValidateColumnNames()
		{
			var extracted = new []
			{
				"Firstname",
				"Lastname",
				"WindowsUser",
				"ApplicationUserId",
				"Password",
				"Role",
				"StartDate",
				"Organization",
				"Skill",
				"ExternalLogon",
				"Contract",
				"ContractSchedule",
				"PartTimePercentage",
				"ShiftBag",
				"SchedulePeriodType",
				"SchedulePeriodLength"
			};
			target.ValidateColumnNames(extracted).Should().Be.Empty();
		}

		[Test]
		public void ShouldFailIfColumnsAreNotTheSameAsExpected()
		{
			var extracted = new []
			{
				"Lastname",
				"Firstname",				
				"WindowsUser",
				"ApplicationUserId",
				"Password",
				"Role",
				"StartDate",
				"Organization",
				"Skill",
				"ExternalLogon",
				"Contract",
				"ContractSchedule",
				"PartTimePercentage",
				"ShiftBag",
				"SchedulePeriodType",
			};
			target.ValidateColumnNames(extracted).Should().Have.SameSequenceAs(new[]
			{
				"Firstname",
				"Lastname",
				"SchedulePeriodLength"
			});
		}
	

		[Test]
		public void ShouldTolerateAdditionalColumns()
		{
			var extracted = new []
			{
				"Firstname",
				"Lastname",
				"WindowsUser",
				"ApplicationUserId",
				"Password",
				"Role",
				"StartDate",
				"Organization",
				"Skill",
				"ExternalLogon",
				"Contract",
				"ContractSchedule",
				"PartTimePercentage",
				"ShiftBag",
				"SchedulePeriodType",
				"SchedulePeriodLength",
				"AdditionalColumn"
			};
			target.ValidateColumnNames(extracted).Should().Be.Empty();
		}
	}
}
