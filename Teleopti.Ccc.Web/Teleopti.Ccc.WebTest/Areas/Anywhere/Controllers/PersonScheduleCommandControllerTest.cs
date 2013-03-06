using System;
using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class PersonScheduleCommandControllerTest
	{
		[Test]
		public void ShouldReturnEmptyResult() { 
			var target = new PersonScheduleCommandController();

			var result = target.AddFullDayAbsence(
				new AddFullDayAbsenceCommand
					{
						PersonId = Guid.NewGuid(),
						AbsenceId = Guid.NewGuid(),
						StartDate = DateOnly.Today,
						EndDate = DateOnly.Today,
					});

			result.Should().Be.OfType<EmptyResult>();
		}
	}
}