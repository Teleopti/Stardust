using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	public class FetchSkillInIntradayTest
	{
		public FetchSkillInIntraday Target;
		public FakeLoadAllSkillInIntradays LoadAllSkillIntradays;

		[Test]
		public void ShouldGetAll()
		{
			var existingSkillInIntraday = new SkillInIntraday();
			LoadAllSkillIntradays.Has(existingSkillInIntraday);

			Target.GetAll().Should().Have.SameValuesAs(existingSkillInIntraday);
		}

		[Test]
		public void ShouldReturnSkillName()
		{
			var name = RandomName.Make();
			LoadAllSkillIntradays.HasWithName(name);

			Target.GetAll().Single().Name.Should().Be.EqualTo(name);
		}
	}
}