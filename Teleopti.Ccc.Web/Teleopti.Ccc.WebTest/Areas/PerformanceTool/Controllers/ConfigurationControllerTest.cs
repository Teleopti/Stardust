using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.PerformanceTool.Controllers
{
	public class ConfigurationControllerTest
	{
		[Test]
		public void ShouldReturnAbsenceId()
		{
			var absenceRepository = new FakeAbsenceRepository();
			var target = new PerformanceToolConfigurationController(absenceRepository, new FakeLoggedOnUser(PersonFactory.CreatePersonWithId()));

			var absence = AbsenceFactory.CreateAbsenceWithId();
			absenceRepository.Add(absence);

			var result = target.GetAAbsenceId().Result<Guid>();
			result.Should().Be.EqualTo(absence.Id.Value);
		}

		[Test]
		public void ShouldReturnPersonId()
		{
			var person = PersonFactory.CreatePersonWithId();
			var target = new PerformanceToolConfigurationController(new FakeAbsenceRepository(), new FakeLoggedOnUser(person));

			var result = target.GetAPersonId().Result<Guid>();
			result.Should().Be.EqualTo(person.Id.Value);
		}
	}
}