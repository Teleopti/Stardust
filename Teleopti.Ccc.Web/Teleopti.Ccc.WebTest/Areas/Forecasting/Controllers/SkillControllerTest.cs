using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[TestFixture]
	public class SkillControllerTest
	{
		[Test]
		public void ShouldReturnBasicSkillInformation()
		{
			var rep = MockRepository.GenerateMock<ISkillRepository>();
			var skillWithId = SkillFactory.CreateSkillWithId("my skill");
			rep.Stub(x => x.LoadAll()).Return(new[] {skillWithId });
			var target = new SkillController(rep);
			var result = target.Get().Single();

			result.Should().Be.EqualTo(new NameWithId {Id = skillWithId.Id.Value, Name = skillWithId.Name});
		}
	}

}
