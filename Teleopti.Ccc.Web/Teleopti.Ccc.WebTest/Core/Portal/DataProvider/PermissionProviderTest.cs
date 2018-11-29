using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;


namespace Teleopti.Ccc.WebTest.Core.Portal.DataProvider
{
	[TestFixture]
	public class PermissionProviderTest
	{
		[Test]
		public void ShouldReturnFalseIfNoApplicationFunctionPermission()
		{
			var principalAuthorization = MockRepository.GenerateMock<IAuthorization>();

			principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath")).Return(false);

			var target = new PermissionProvider(principalAuthorization);

			var result = target.HasApplicationFunctionPermission("ApplicationFunctionPath");

			result.Should().Be(false);
		}

		[Test]
		public void ShouldReturnTrueIfApplicationFunctionPermission()
		{
			var principalAuthorization = MockRepository.GenerateMock<IAuthorization>();

			principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath")).Return(true);

			var target = new PermissionProvider(principalAuthorization);

			var result = target.HasApplicationFunctionPermission("ApplicationFunctionPath");

			result.Should().Be(true);
		}

		[Test]
		public void ShouldReturnFalseIfNoPersonPermission()
		{
			var principalAuthorization = MockRepository.GenerateMock<IAuthorization>();
			var person = new Person();

			principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath", DateOnly.Today, person)).Return(false);

			var target = new PermissionProvider(principalAuthorization);

			var result = target.HasPersonPermission("ApplicationFunctionPath", DateOnly.Today, person);

			result.Should().Be(false);
		}

		[Test]
		public void ShouldReturnTrueIfPersonPermission()
		{
			var principalAuthorization = MockRepository.GenerateMock<IAuthorization>();
			var person = new Person();

			principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath", DateOnly.Today, person)).Return(true);

			var target = new PermissionProvider(principalAuthorization);

			var result = target.HasPersonPermission("ApplicationFunctionPath", DateOnly.Today, person);

			result.Should().Be(true);
		}

		[Test]
		public void ShouldReturnFalseIfNoTeamPermission()
		{
			var principalAuthorization = MockRepository.GenerateMock<IAuthorization>();
			var team = new Team();

			principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath", DateOnly.Today, team)).Return(false);

			var target = new PermissionProvider(principalAuthorization);

			var result = target.HasTeamPermission("ApplicationFunctionPath", DateOnly.Today, team);

			result.Should().Be(false);
		}

		[Test]
		public void ShouldReturnTrueIfTeamPermission()
		{
			var principalAuthorization = MockRepository.GenerateMock<IAuthorization>();
			var team = new Team();

			principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath", DateOnly.Today, team)).Return(true);

			var target = new PermissionProvider(principalAuthorization);

			var result = target.HasTeamPermission("ApplicationFunctionPath", DateOnly.Today, team);

			result.Should().Be(true);
		}

		[Test]
		public void ShouldReturnFalseIfNoSitePermission()
		{
			var principalAuthorization = MockRepository.GenerateMock<IAuthorization>();
			var site = new Site(".");

			principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath", DateOnly.Today, site)).Return(false);

			var target = new PermissionProvider(principalAuthorization);

			var result = target.HasSitePermission("ApplicationFunctionPath", DateOnly.Today, site);

			result.Should().Be(false);
		}

		[Test]
		public void ShouldReturnTrueIfSitePermission()
		{
			var principalAuthorization = MockRepository.GenerateMock<IAuthorization>();
			var site = new Site(".");

			principalAuthorization.Stub(x => x.IsPermitted("ApplicationFunctionPath", DateOnly.Today, site)).Return(true);

			var target = new PermissionProvider(principalAuthorization);

			var result = target.HasSitePermission("ApplicationFunctionPath", DateOnly.Today, site);

			result.Should().Be(true);
		}

		[Test]
		public void ShouldReturnTrueIfScheduleIsPublished()
		{
			var principalAuthorization = MockRepository.GenerateMock<IAuthorization>();

			var person = new Person();
			IWorkflowControlSet workflowControlSet = new WorkflowControlSet("d");
			workflowControlSet.SchedulePublishedToDate = new DateTime(2100, 1, 1);
			person.WorkflowControlSet = workflowControlSet;

			var target = new PermissionProvider(principalAuthorization);

			var result = target.IsPersonSchedulePublished(new DateOnly(2000, 1, 2), person);

			result.Should().Be(true);
		}

		[Test]
		public void ShouldReturnFalseIfScheduleNotPublished()
		{
			var principalAuthorization = MockRepository.GenerateMock<IAuthorization>();

			var person = new Person();
			IWorkflowControlSet workflowControlSet = new WorkflowControlSet("d");
			workflowControlSet.SchedulePublishedToDate = new DateTime(2000, 1, 1);
			person.WorkflowControlSet = workflowControlSet;

			var target = new PermissionProvider(principalAuthorization);

			var result = target.IsPersonSchedulePublished(new DateOnly(2001, 1, 2), person);

			result.Should().Be(false);
		}
	}
}