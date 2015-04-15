using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	[TestFixture]
	public class TenantCheckPasswordChangeTest
	{
		private ITenantCheckPasswordChange target;
		private IPasswordPolicy passwordPolicy;
		private PasswordPolicyForUser _passwordPolicyForUser;

		[SetUp]
		public void Setup()
		{
			passwordPolicy =MockRepository.GenerateMock<IPasswordPolicy>();
			var personInfo = new PersonInfo { Id = Guid.NewGuid() };
			_passwordPolicyForUser = new PasswordPolicyForUser(personInfo);
			target = new TenantCheckPasswordChange(() => passwordPolicy);
		}

		[Test]
		public void VerifyPasswordValidForDayCountCanBeMax()
		{
			passwordPolicy.Stub(x => x.PasswordValidForDayCount).Return(int.MaxValue);
			passwordPolicy.Stub(x => x.PasswordExpireWarningDayCount).Return(6);

			AuthenticationResult result = target.Check(_passwordPolicyForUser);
			Assert.IsTrue(result.Successful);
			Assert.IsNotNull(result);
		}

		[Test]
		public void VerifyPasswordShouldHaveBeenChanged()
		{
			passwordPolicy.Stub(x => x.PasswordValidForDayCount).Return(10);
			var passwordPolicyForUser = new PasswordPolicyForUserForTest();
			AuthenticationResult result = target.Check(passwordPolicyForUser);
			Assert.IsFalse(result.Successful);
			Assert.IsTrue(result.HasMessage);
			Assert.IsTrue(result.PasswordExpired);
			Assert.AreEqual(UserTexts.Resources.LogOnFailedPasswordExpired, result.Message);
		}

		[Test]
		public void VerifyPasswordMustSoonBeChanged()
		{
			passwordPolicy.Stub(x => x.PasswordValidForDayCount).Return(5);
			passwordPolicy.Stub(x => x.PasswordExpireWarningDayCount).Return(6);

			AuthenticationResult result = target.Check(_passwordPolicyForUser);
			Assert.IsTrue(result.Successful);
			Assert.IsTrue(result.HasMessage);
			Assert.IsFalse(string.IsNullOrEmpty(result.Message));
		}
	}

	internal class PasswordPolicyForUserForTest : PasswordPolicyForUser
	{
		public override DateTime LastPasswordChange { get { return DateTime.Today.AddDays(-15); }}
	}
	
}
