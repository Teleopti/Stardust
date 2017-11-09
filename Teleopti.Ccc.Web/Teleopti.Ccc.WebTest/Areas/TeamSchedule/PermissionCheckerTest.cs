using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule
{
	[TestFixture, DomainTest]
	public class PermissionCheckerTestNoMock : ISetup
	{
		public IPermissionChecker Target;
		public Global.FakePermissionProvider PermissionProvider;
		
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<PermissionChecker>().For<IPermissionChecker>();
			system.UseTestDouble<Global.FakePermissionProvider>().For<IPermissionProvider>();
		}
			
		[Test]
		public void ShouldReturnErrorMessageWhenHasNoModifyWriteProtectedSchedulePermission()
		{
			var date = DateOnly.Today;
			var person = new Person();
			var wfc = WorkflowControlSetFactory.CreateWorkFlowControlSet(AbsenceFactory.CreateAbsence("absence"),
				new ApproveAbsenceRequestWithValidators(), false);
			wfc.WriteProtection = -5;
			person.WorkflowControlSet = wfc;
			PermissionProvider.Enable();
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence, person, date );

			var result = Target.CheckAddFullDayAbsenceForPerson(person, date);

			result.Should().Be.EqualTo(Resources.WriteProtectSchedule);
		}
	}
	[TestFixture]
	public class PermissionCheckerTest
	{
		private IPermissionProvider _permissionProvider;

		[SetUp]
		public void SetUp()
		{
			_permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
		}

		[Test]
		public void ShouldReturnNullWhenHasFullDayPermission()
		{
			var date = new DateOnly(2015,12,31);
			var person = new Person();
			_permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence, date, person)).Return(true);

			var target = new PermissionChecker(_permissionProvider);
			var result = target.CheckAddFullDayAbsenceForPerson(person, date);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnErrorMessageWhenHasNoFullDayPermission()
		{
			var date = new DateOnly(2015, 12, 31);
			var person = new Person();
			var expectedError = string.Format(Resources.NoPermissionAddFullDayAbsenceForAgent, person.Name);
			_permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence, date, person)).Return(false);

			var target = new PermissionChecker(_permissionProvider);
			var result = target.CheckAddFullDayAbsenceForPerson(person, date);

			result.Should().Be.EqualTo(expectedError);
		}
		
		[Test]
		public void ShouldReturnNullWhenHasIntradayPermission()
		{
			var date = new DateOnly(2015,12,31);
			var person = new Person();
			_permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence, date, person)).Return(true);

			var target = new PermissionChecker(_permissionProvider);
			var result = target.CheckAddIntradayAbsenceForPerson(person, date);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnErrorMessageWhenHasNoIntradayPermission()
		{
			var date = new DateOnly(2015, 12, 31);
			var person = new Person();
			var expectedError = string.Format(Resources.NoPermisionAddIntradayAbsenceForAgent, person.Name);
			_permissionProvider.Stub(x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence, date, person)).Return(false);

			var target = new PermissionChecker(_permissionProvider);
			var result = target.CheckAddIntradayAbsenceForPerson(person, date);

			result.Should().Be.EqualTo(expectedError);
		}
	}
	
}
