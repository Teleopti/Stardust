using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture,MyTimeWebTest]
	public class TeamScheduleControllerTest
	{
		public TeamScheduleController Target;
		public FakePersonRepository PersonRepository;

		[Test]
		public void ShouldGiveSuccessResponseButWithRightBusinessReasonWhenThereIsNoDefaultTeam()
		{
			var person = PersonFactory.CreatePerson("test");
			person.TerminatePerson(new DateOnly(2018, 2, 5), new PersonAccountUpdaterDummy());
			PersonRepository.Has(person);

			var response = Target.DefaultTeam(new DateOnly(2018, 2, 6));

			dynamic content = response.Data;
			Type typeOfContent = content.GetType();
			var exist = typeOfContent.GetProperties().Where(p => p.Name.Equals("DefaultTeam")).Any();
			Assert.That(exist, Is.EqualTo(false));
			Assert.That((object)content.Message, Is.EqualTo(Resources.NoTeamsAvailable));
		}

		[Test]
		public void ShouldReturnRequestPartialView()
		{
			var date = DateOnly.Today.AddDays(1);
			var id = Guid.NewGuid();
			var result = Target.Index(date, id);

			result.ViewName.Should().Be.EqualTo("TeamSchedulePartial");
			result.Model.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnNewRequestPartialView()
		{
			var date = DateOnly.Today.AddDays(1);
			var id = Guid.NewGuid();
			var result = Target.NewIndex(date, id);

			result.ViewName.Should().Be.EqualTo("NewTeamSchedulePartial");
			result.Model.Should().Not.Be.Null();
		}
	}
}