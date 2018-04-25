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
using Teleopti.Ccc.WebTest.Areas.People.IoC;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[TestFixture, WebPeopleTest]
	public class ImportAgentControllerTest : IIsolateSystem
	{
		public FakeLoggedOnUser LoggedOnUser;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;
		public ImportAgentController Target;
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeCurrentBusinessUnit>().For<ICurrentBusinessUnit>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<Global.FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<FakeJobResultRepository>().For<IJobResultRepository>();
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
