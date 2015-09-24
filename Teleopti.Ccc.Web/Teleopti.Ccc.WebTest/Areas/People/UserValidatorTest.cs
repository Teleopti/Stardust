using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Ccc.Web.Areas.People.Core.Persisters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[TestFixture]
	public class UserValidatorTest
	{
		private const string roleName = "Agent";
		private readonly UserValidator target = new UserValidator();
		private Dictionary<string, IApplicationRole> roles;
		
		private string createLongString(int length, char fillWith = 'A')
		{
			return new string(fillWith, length);
		}

		private void validateUserWithExpectErrorMessage(RawUser user, bool expectedResult, params string[] expectedErrors)
		{
			var errMsgBuilder = new StringBuilder();
			var result = target.Validate(user, roles, errMsgBuilder);
			Assert.AreEqual(result, expectedResult);

			var fullError = errMsgBuilder.ToString();
			foreach (var error in expectedErrors)
			{
				var errorMsgExists = fullError.IndexOf(error, StringComparison.CurrentCulture) > -1;
				Assert.AreEqual(true, errorMsgExists);
			}
		}

		[SetUp]
		public void Setup()
		{
			roles = new Dictionary<string, IApplicationRole>
			{
				{
					roleName.ToUpper(),
					new ApplicationRole {DescriptionText = roleName}
				}
			};
		}

		[Test]
		public void ShouldBeValidForUserWithCorrectProperties()
		{
			var errorMsg = new StringBuilder();
			var user = new RawUser
			{
				Firstname = "Jenny",
				ApplicationUserId = "abc@teleopti.com",
				Lastname = "Morgan",
				Password = "password",
				WindowsUser = ""
			};
			var result = target.Validate(user, roles, errorMsg);
			Assert.AreEqual(true, result);
			Assert.AreEqual(true, string.IsNullOrEmpty(errorMsg.ToString()));
		}

		[Test]
		public void ValidateUserWithEmptyPassword()
		{
			validateUserWithExpectErrorMessage(new RawUser
			{
				Firstname = "Jan",
				ApplicationUserId = "abcde@teleopti.com",
				Lastname = "Morgan",
				Password = "",
				WindowsUser = ""
			}, false, Resources.EmptyPasswordErrorMsgSemicolon);
		}

		[Test]
		public void ValidateUserWithoutAppLogonName()
		{
			validateUserWithExpectErrorMessage(new RawUser
			{
				Firstname = "Jenny",
				ApplicationUserId = "",
				Lastname = "Morgan",
				Password = "password",
				WindowsUser = "jennym@teleopti.com"
			}, false, Resources.NoApplicationLogonAccountErrorMsgSemicolon);
		}

		[Test]
		public void ValidateUserWithoutLogonAccount()
		{
			validateUserWithExpectErrorMessage(new RawUser
			{
				Firstname = "Jan",
				ApplicationUserId = "",
				Lastname = "Morgan",
				Password = "psss",
				WindowsUser = ""
			}, false, Resources.NoLogonAccountErrorMsgSemicolon);
		}

		[Test]
		public void ValidateUserWithEmptyFirstNameAndLastName()
		{
			validateUserWithExpectErrorMessage(new RawUser
			{
				Firstname = "",
				ApplicationUserId = "logon1@teleopti.com",
				Lastname = "",
				Password = "password",
				WindowsUser = "winlogon1@teleopti.com"
			}, false, Resources.BothFirstnameAndLastnameAreEmptyErrorMsgSemicolon);
		}

		[Test]
		public void ValidateUserWithTooLongFirstName()
		{
			validateUserWithExpectErrorMessage(new RawUser
			{
				Firstname = createLongString(26),
				ApplicationUserId = "logon2@teleopti.com",
				Lastname = "Lastname",
				Password = "password",
				WindowsUser = "winlogon2@teleopti.com"
			}, false, Resources.TooLongFirstnameErrorMsgSemicolon);
		}

		[Test]
		public void ValidateUserWithTooLongLastName()
		{
			validateUserWithExpectErrorMessage(new RawUser
			{
				Firstname = "FirstName",
				ApplicationUserId = "logon2@teleopti.com",
				Lastname = createLongString(26),
				Password = "password",
				WindowsUser = "winlogon2@teleopti.com"
			}, false, Resources.TooLongLastnameErrorMsgSemicolon);
		}

		[Test]
		public void ValidateUserWithInvalidNameAndPassword()
		{
			validateUserWithExpectErrorMessage(new RawUser
			{
				Firstname = "",
				ApplicationUserId = "app1@teleopti.com",
				Lastname = "",
				Password = "",
				WindowsUser = "logon1@teleopti.com"
			}, false,
				Resources.EmptyPasswordErrorMsgSemicolon,
				Resources.BothFirstnameAndLastnameAreEmptyErrorMsgSemicolon);
		}

		[Test]
		public void ValidateUserWithInvalidRole()
		{
			validateUserWithExpectErrorMessage(new RawUser
			{
				Firstname = "Firstname1",
				ApplicationUserId = "app1@teleopti.com",
				Lastname = "",
				Password = "psss",
				Role = roleName + ",sss0 , \"test,sss1 \"",
				WindowsUser = "logon1@teleopti.com"
			}, false,
				string.Format(Resources.RoleXNotExistErrorMsgSemicolon, "sss0, \"test,sss1\""));
		}

		[Test]
		public void ValidateUserWithTooLongAppUserId()
		{
			const string emailSuffix = "@teleopti.com";
			validateUserWithExpectErrorMessage(new RawUser
			{
				Firstname = "Jan",
				ApplicationUserId = createLongString(51 - emailSuffix.Length) + emailSuffix,
				Lastname = "Morgan",
				Password = "psss",
				Role = "",
				WindowsUser = "jennym@teleopti.com"
			}, false, Resources.TooLongApplicationUserIdErrorMsgSemicolon);
		}

		[Test]
		public void ValidateUserWithTooLongWinUserId()
		{
			validateUserWithExpectErrorMessage(new RawUser
			{
				Firstname = "Jenny",
				ApplicationUserId = "abc@teleopti.com",
				Lastname = "Morgan",
				Password = "password",
				Role = "",
				WindowsUser = createLongString(101)
			}, false, Resources.TooLongWindowsUserErrorMsgSemicolon);
		}

		[Test]
		public void ValidateUserWithAppUserIdIsNotEmail()
		{
			validateUserWithExpectErrorMessage(new RawUser
			{
				Firstname = "Jan",
				ApplicationUserId = "abca",
				Lastname = "Morgan",
				Password = "psss",
				Role = "",
				WindowsUser = "jennym@teleopti.com"
			}, false, Resources.ApplicationUserIdShouldBeAValidEmailAddressErrorMsg);
		}
	}
}
