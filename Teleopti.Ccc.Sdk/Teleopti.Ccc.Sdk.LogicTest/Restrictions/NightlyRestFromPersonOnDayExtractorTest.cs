using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.Restrictions
{
	[TestFixture]
	public class NightlyRestFromPersonOnDayExtractorTest
	{
		[Test]
		public void ShouldReturnNightlyRestFromWorkTimeDirective()
		{
			var dateOnly = new DateOnly(2011, 9, 1);
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePersonWithId(),
				dateOnly);
			person.Period(dateOnly).PersonContract.Contract.WorkTimeDirective =
				new WorkTimeDirective(TimeSpan.FromDays(0), TimeSpan.FromDays(5), TimeSpan.FromHours(12), TimeSpan.FromDays(2));

			var dateOnlyDto = new DateOnlyDto { DateTime = dateOnly.Date };

			var _target = new NightlyRestFromPersonOnDayExtractor(person);
			Assert.That(_target.NightlyRestOnDay(dateOnlyDto), Is.EqualTo(TimeSpan.FromHours(12)));
		}
	}
}