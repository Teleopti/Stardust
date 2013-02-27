using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class PersonScheduleViewModelMapperTest
	{
		[Test]
		public void ShouldMapPersonName()
		{
			var target = new PersonScheduleViewModelMapper();
			var person = new Person {Name = new Name("Pierra", "B")};

			var result = target.Map(new PersonScheduleData {Person = person});

			result.Name.Should().Be("Pierra B");
		}
	}
}