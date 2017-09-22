using System;
using System.Net.Http;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[TestFixture, PeopleCommandTest]
	public class ImportAgentControllerTest : ISetup
	{
		public FakeLoggedOnUser LoggedOnUser;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;
		public ImportAgentController Target;
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCurrentBusinessUnit>().For<ICurrentBusinessUnit>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<Global.FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble<FakeJobResultRepository>().For<IJobResultRepository>();
		}

		
		[Test]
		public void ShouldNotCreateJobIfRequestContentIsNull()
		{

			Target.Request = new HttpRequestMessage();
			var exception = Target.NewImportAgentJob().Exception;
			exception.InnerException.GetType().Should().Be(typeof(ArgumentNullException));
			(exception.InnerException as ArgumentNullException).Message.Should().Equals(Resources.NoInput);
		}

	}
}
