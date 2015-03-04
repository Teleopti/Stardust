using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
	[TestFixture, SetUICulture("en-US")]
	public class PersonGeneralModelTest
	{
		private PersonGeneralModel _target;
		private IPerson _base;
		private IPersonPeriod _personPeriod;
		private IPrincipalAuthorization _principalAuthorization;

		[SetUp]
		public void TestInit()
		{
			_base = PersonFactory.CreatePerson();
			ITeam team = TeamFactory.CreateSimpleTeam();
			_personPeriod = PersonPeriodFactory.CreatePersonPeriod
				 (DateOnly.Today,
				  PersonContractFactory.CreatePersonContract("testContract",
																			"TestSchedule",
																			"TestPartTimePercentage"),
				  team);
			_base.AddPersonPeriod(_personPeriod);
			_principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			_target = new PersonGeneralModel(_base, new UserDetail(_base), _principalAuthorization,
				new PersonAccountUpdaterDummy(), "Teleopti");
		}

		[Test]
		public void VerifyPassingPersonInConstructor()
		{
			_base.Name = new Name("FirstName", "LastName");
			Assert.AreEqual(_target.FirstName, "FirstName");
		}

		[Test]
		public void VerifySetFirstName()
		{
			const string setValue = "FirstName";
			_target.FirstName = setValue;

			// Test get method
			string getValue = _target.FirstName;

			// Perform Assert Tests
			Assert.AreSame(setValue, getValue);
		}

		[Test]
		public void VerifySetLastName()
		{
			const string setValue = "LastName";
			_target.LastName = setValue;

			// Test get method
			string getValue = _target.LastName;

			// Perform Assert Tests
			Assert.AreSame(setValue, getValue);
		}

		[Test]
		public void VerifySetFullName()
		{
			const string setValue = "LastName";
			_target.LastName = setValue;

			// Test get method
			string getValue = _target.FullName;

			// Perform Assert Tests
			Assert.AreEqual(_base.Name.ToString(), getValue);
		}

		[Test]
		public void VerifySetEmail()
		{
			const string setValue = "Mail";
			_target.Email = setValue;

			// Test get method
			string getValue = _target.Email;

			// Perform Assert Tests
			//TODO: Write Assert Tests for Email()
			Assert.AreSame(setValue, getValue);
		}

		[Test]
		public void VerifySetEmployeeNumber()
		{
			const string setValue = "EmployeeNumber";
			_target.EmployeeNumber = setValue;

			//Test get method
			var getValue = _target.EmployeeNumber;

			//Perform Test
			Assert.AreEqual(setValue, getValue);
		}

		[Test]
		public void VerifySetNote()
		{
			const string setValue = "TestNoteTestNote";
			_target.Note = setValue;

			//Test get method
			var getValue = _target.Note;

			//Perform Test
			Assert.AreEqual(setValue, getValue);
		}

		[Test]
		public void VerifySetLanguage()
		{
			int setValue = 1034;
			_target.Language = setValue;

			//Test get method
			int getValue = _target.Language;

			//Perform Test
			Assert.AreEqual(setValue, getValue);
			Assert.AreEqual(setValue, _target.LanguageLCID);
			//-------------------------------------------
			setValue = 0;
			_target.Language = setValue;
			getValue = _target.ContainedEntity.PermissionInformation.Culture().LCID; //Hmm dunno

			//Perform Test
			Assert.AreEqual(getValue, _target.Language);
		}

		[Test]
		public void VerifySetCulture()
		{
			int setValue = 1034;
			_target.Culture = setValue;

			//Test get method
			int getValue = _target.Culture;

			//Perform Test
			Assert.AreEqual(setValue, getValue);
			Assert.AreEqual(setValue, _target.CultureLCID);

			//-------------------------------------------
			setValue = 0;
			_target.Culture = setValue;
			getValue = 1033;

			//Perform Test
			Assert.AreEqual(getValue, _target.Culture);
		}

		[Test]
		public void VerifySetTimeZone()
		{
			const string setValue = "W. Europe Standard Time";
			_target.TimeZone = setValue;

			//Test get method
			var getValue = _target.TimeZone;

			//Perform Test
			Assert.AreEqual(setValue, getValue);
		}

		[Test]
		public void VerifySetWorkflowControlSet()
		{
			Assert.IsNull(_base.WorkflowControlSet);
			Assert.AreEqual(PersonGeneralModel.NullWorkflowControlSet, _target.WorkflowControlSet);
			var newWorkflowControlSet = new WorkflowControlSet("My");
			_target.WorkflowControlSet = newWorkflowControlSet;

			//Perform Test
			Assert.AreEqual(newWorkflowControlSet, _target.WorkflowControlSet);
			Assert.AreEqual(newWorkflowControlSet, _base.WorkflowControlSet);

			_target.WorkflowControlSet = PersonGeneralModel.NullWorkflowControlSet;
			Assert.IsNull(_base.WorkflowControlSet);
			Assert.AreEqual(PersonGeneralModel.NullWorkflowControlSet, _target.WorkflowControlSet);
		}

		[Test]
		public void VerifyWindowsLogOnName()
		{
			const string setValue = "WinUser123";
			_principalAuthorization.Expect(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword,
																DateOnly.Today, _base)).Return(true);

			_target.LogOnName = setValue;

			//Test get method
			var getValue = _target.LogOnName;

			//Perform Test
			Assert.AreEqual(setValue, getValue);
		}

		[Test]
		public void ShouldCheckPasswordOnChangeOfApplicationLogOnName()
		{
			const string setValue = "AppUser123";

			_base = MockRepository.GenerateMock<IPerson>();
			var userDetail = MockRepository.GenerateMock<IUserDetail>();
			var authInfo = MockRepository.GenerateMock<IApplicationAuthenticationInfo>();

			_base.Stub(x => x.ApplicationAuthenticationInfo).Return(authInfo).Repeat.AtLeastOnce();
			authInfo.Stub(x => x.ApplicationLogOnName = setValue);
			authInfo.Stub(x => x.Password).Return("");
			authInfo.Stub(x => x.ApplicationLogOnName).Return("AppUser123");

			_principalAuthorization.Stub(
				x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword, DateOnly.Today, _base))
				.Return(true);
			_base.Stub(x => x.ChangePassword("", null, userDetail)).Return(true);

			_target = new PersonGeneralModel(_base, userDetail, _principalAuthorization, new PersonAccountUpdaterDummy(),
				"Teleopti") {ApplicationLogOnName = setValue};

			_target.ApplicationLogOnName.Should().Be.EqualTo(setValue);
		}

		[Test]
		public void ShouldSayValidPasswordWhenApplicationLogOnNameIsEmpty()
		{
			const string setValue = "";
			_base = MockRepository.GenerateMock<IPerson>();
			var userDetail = MockRepository.GenerateMock<IUserDetail>();
			_principalAuthorization.Stub(
				x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword, DateOnly.Today, _base))
				.Return(true);
			_base.Stub(x => x.ApplicationAuthenticationInfo = null);

			_target = new PersonGeneralModel(_base, userDetail, _principalAuthorization, new PersonAccountUpdaterDummy(),
				"Teleopti") {ApplicationLogOnName = setValue};
			_target.IsValid.Should().Be.True();
		}

		[Test]
		public void VerifyPassword()
		{
			const string setValue = "passwordX07";
			_base = MockRepository.GenerateMock<IPerson>();
			var userDetail = MockRepository.GenerateMock<IUserDetail>();
			var authInfo = MockRepository.GenerateMock<IApplicationAuthenticationInfo>();

			_base.Stub(x => x.ApplicationAuthenticationInfo).Return(authInfo).Repeat.AtLeastOnce();
			authInfo.Stub(x => x.ApplicationLogOnName).Return("userX07");
			_principalAuthorization.Stub(
				x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword, DateOnly.Today, _base))
				.Return(true);
			_base.Stub(x => x.ChangePassword(setValue, null, userDetail)).Return(true);

			_target = new PersonGeneralModel(_base, userDetail, _principalAuthorization, new PersonAccountUpdaterDummy(),
				"Teleopti") {Password = setValue};
		}

		[Test]
		public void VerifySetIsAgent()
		{
			bool expectedValue = (_base.Period(DateOnly.Today) != null);
			_target.IsAgent.Should().Be.EqualTo(expectedValue);
		}

		[Test]
		public void VerifySetIsUser()
		{
			var info = _base.PermissionInformation;
			var expectedValue = (info != null);
			_target.IsUser.Should().Be.EqualTo(expectedValue);
		}

		[Test]
		public void VerifyTerminalDate()
		{
			var dateOnly = DateOnly.Today;
			_target.TerminalDate = dateOnly;
			Assert.AreEqual(_target.TerminalDate, dateOnly);

			_target.TerminalDate = null;
			_target.TerminalDate.Should().Be.EqualTo(null);
		}

		[Test]
		public void VerifyLanguageInfo()
		{
			var culture = new Culture(0, "No Language");
			_target.LanguageInfo = culture;
			_target.LanguageInfo.Id.Should().Be.EqualTo(culture.Id);
		}

		[Test]
		public void VerifyCultureInfo()
		{
			var culture = new Culture(0, "No Language");
			_target.CultureInfo = culture;
			_target.CultureInfo.Id.Should().Be.EqualTo(culture.Id);
		}

		[Test]
		public void VerifyRoles()
		{
			IList<IApplicationRole> roles = new List<IApplicationRole> { new ApplicationRole { DescriptionText = "Test" } };

			_target.SetAvailableRoles(roles);
			Assert.AreEqual(string.Empty, _target.Roles);

			_target.Roles = "Test";
			Assert.AreEqual("Test", _target.Roles);
		}

		[Test]
		public void VerifyCanBold()
		{
			Assert.IsFalse(_target.CanBold);
			_target.CanBold = true;
			Assert.IsTrue(_target.CanBold);
		}

		[Test]
		public void ShouldNotGray()
		{
			Assert.IsFalse(_target.CanGray);
		}

		[Test]
		public void ShouldHaveAnEmptyPasswordFromBeginning()
		{
			Assert.That(_target.Password, Is.Empty);
		}

		[Test]
		public void ShouldSayValidWhenSettingPasswordIfLogOnNameIsEmpty()
		{
			_target.ApplicationLogOnName = "";
			_target.Password = "x";
			Assert.That(_target.IsValid, Is.True);
			Assert.That(_target.Password, Is.Empty);
		}

		[Test]
		public void ShouldShowEmptyWorkflowControlSetIfItWasDeleted()
		{
			var newWorkflowControlSet = new WorkflowControlSet("WCS ToBeDeleted");
			_target.WorkflowControlSet = newWorkflowControlSet;
			newWorkflowControlSet.SetDeleted();

			Assert.AreEqual(PersonGeneralModel.NullWorkflowControlSet, _target.WorkflowControlSet);
		}

		[Test]
		public void ShouldCheckIfUserHasPermissionToChangeLogOnAndPassword()
		{
			_principalAuthorization.Stub(
				x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword, DateOnly.Today, _base))
				.Return(true);

			_target.ApplicationLogOnName = "";
		}

		[Test]
		public void ShouldResetChangeLogonCheck()
		{
			_principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword, DateOnly.Today, _base)).Return(true).Repeat.Twice();

			_target.ApplicationLogOnName = "";
			_target.ResetLogonDataCheck();
			_target.ApplicationLogOnName = "";
		}

		[Test]
		public void ShouldNotChangeIfUserHasNoPermissionToChangeLogOnAndPassword()
		{
			const string oldLogOnInfo = "oldLogOnInfo";
			_base.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo { ApplicationLogOnName = oldLogOnInfo, Password = oldLogOnInfo };
			_base.AuthenticationInfo = new AuthenticationInfo { Identity = oldLogOnInfo };
			_principalAuthorization.Stub(
				x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword, DateOnly.Today, _base))
				.Return(false);

			_target.ApplicationLogOnName = "";
			_target.LogOnName = "";
			_target.Password = "";

			Assert.That(_target.ApplicationLogOnName, Is.EqualTo(oldLogOnInfo));
			Assert.That(_target.LogOnName, Is.EqualTo(oldLogOnInfo));
			Assert.That(_target.Password, Is.EqualTo(oldLogOnInfo));
		}

		[Test]
		public void ShouldKeepTrackOfChangedTenantData()
		{
			const string setValue = "passwordX07";
			_base = MockRepository.GenerateMock<IPerson>();
			var userDetail = MockRepository.GenerateMock<IUserDetail>();
			var authInfo = MockRepository.GenerateMock<IApplicationAuthenticationInfo>();

			_base.Stub(x => x.ApplicationAuthenticationInfo).Return(authInfo).Repeat.AtLeastOnce();
			authInfo.Stub(x => x.ApplicationLogOnName).Return("userX07");
			authInfo.Stub(x => x.Password).Return(setValue);
			_principalAuthorization.Stub(
				x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword, DateOnly.Today, _base))
				.Return(true);
			_base.Stub(x => x.ChangePassword(setValue, null, userDetail)).Return(true).Repeat.Twice();

			_target = new PersonGeneralModel(_base, userDetail, _principalAuthorization, new PersonAccountUpdaterDummy(),"Teleopti");

			_target.TenantData.Changed.Should().Be.False();
			_target.Password = setValue;
			_target.TenantData.Changed.Should().Be.True();
			_target.TenantData.Changed = false;

			_target.TenantData.Changed.Should().Be.False();
			_target.ApplicationLogOnName = "userngt";
			_target.TenantData.Changed.Should().Be.True();
			_target.TenantData.Changed = false;

			_target.TenantData.Changed.Should().Be.False();
			_target.LogOnName = "domain\\user";
			_target.TenantData.Changed.Should().Be.True();
			_target.TenantData.Changed = false;

			_target.TenantData.Changed.Should().Be.False();
			_target.TerminalDate = new DateOnly(2015,3,4);
			_target.TenantData.Changed.Should().Be.True();

		}

	}
}
