using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	[TestFixture]
	public class VerifyPasswordPolicyTest
	{
		private IVerifyPasswordPolicy target;
		private IPasswordPolicy passwordPolicy;
		private ApplicationLogonInfo _applicationLogonInfo;

		[SetUp]
		public void Setup()
		{
			passwordPolicy =MockRepository.GenerateMock<IPasswordPolicy>();
			_applicationLogonInfo = new ApplicationLogonInfo();
			target = new VerifyPasswordPolicy(() => passwordPolicy);
		}

		[Test]
		public void VerifyPasswordValidForDayCountCanBeMax()
		{
			passwordPolicy.Stub(x => x.PasswordValidForDayCount).Return(int.MaxValue);
			passwordPolicy.Stub(x => x.PasswordExpireWarningDayCount).Return(6);

			PasswordPolicyResult result = target.Check(_applicationLogonInfo);
			Assert.IsTrue(result.Successful);
			Assert.IsNotNull(result);
		}

		[Test]
		public void VerifyPasswordShouldHaveBeenChanged()
		{
			passwordPolicy.Stub(x => x.PasswordValidForDayCount).Return(10);
			var passwordPolicyForUser = new ApplicationLogonInfoTest();
			PasswordPolicyResult result = target.Check(passwordPolicyForUser);
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

			PasswordPolicyResult result = target.Check(_applicationLogonInfo);
			Assert.IsTrue(result.Successful);
			Assert.IsTrue(result.HasMessage);
			Assert.IsFalse(string.IsNullOrEmpty(result.Message));
		}
	}

	internal class ApplicationLogonInfoTest : ApplicationLogonInfo
	{
		public override DateTime LastPasswordChange { get { return DateTime.Today.AddDays(-15); }}
	}
	
}
