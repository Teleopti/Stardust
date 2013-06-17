using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.PerformanceTool.Controllers
{
	public class ConfigurationControllerTest
	{
		[Test]
		public void ShouldReturnAbsenceId()
		{
			var absenceRepository = MockRepository.GenerateMock<IAbsenceRepository>();
			var target = new ConfigurationController(absenceRepository, MockRepository.GenerateMock<ILoggedOnUser>());

			var absence = new Absence();
			absence.SetId(Guid.NewGuid());

			absenceRepository.Stub(x => x.LoadAll()).Return(new[] { absence });

			var result = target.GetAAbsenceId();

			result.Data.Should().Be.EqualTo(absence.Id.Value);
		}

		[Test]
		public void ShouldReturnPersonId()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new ConfigurationController(MockRepository.GenerateMock<IAbsenceRepository>(), loggedOnUser);

			var person = new Person();
			person.SetId(Guid.NewGuid());

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			
			var result = target.GetAPersonId();

			result.Data.Should().Be.EqualTo(person.Id.Value);
		}
	}
}