using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.IntegrationTest.Core
{
	[WfmAdminTest]
	public class AdminAccessTokenRepositoryTest : IIsolateSystem
	{
		public ITenantUnitOfWork TenantUnitOfWork;
		public AccountController AccountController;
		public MutableNow Now;
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<MutableNow>().For<INow>();
		}
		[Test]
		public void AccessTokenShouldBecomeInvalidAfter41Minutes()
		{
			var currentTime = new DateTime(2017, 08, 26, 12, 0, 0, DateTimeKind.Utc);
			Now.Is(currentTime);
			
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				AccountController.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "passadej", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var result = AccountController.Login(model).Content;
				result.Success.Should().Be.True();
				result.AccessToken.Should().Not.Be.NullOrEmpty();
				
				AdminAccessTokenRepository.TokenIsValid(result.AccessToken, Now).Should().Be.True();
				Now.Is(currentTime.AddMinutes(41));
				AdminAccessTokenRepository.TokenIsValid(result.AccessToken, Now).Should().Be.False();
			}
		}
		
		[Test]
		public void AccessTokenShouldStayValidOnActivity()
		{
			var currentTime = new DateTime(2017, 08, 26, 12, 0, 0, DateTimeKind.Utc);
			Now.Is(currentTime);
			
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				AccountController.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "passadej", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var result = AccountController.Login(model).Content;
				result.Success.Should().Be.True();
				result.AccessToken.Should().Not.Be.NullOrEmpty();
				
				Now.Is(currentTime.AddMinutes(25));
				AdminAccessTokenRepository.TokenIsValid(result.AccessToken, Now).Should().Be.True();
				Now.Is(currentTime.AddMinutes(50));
				AdminAccessTokenRepository.TokenIsValid(result.AccessToken, Now).Should().Be.True();
			}
		}
		
		[Test]
		public void ShouldBePossibleWithMultipleLoginsWithSameUser()
		{
			var currentTime = new DateTime(2017, 08, 26, 12, 0, 0, DateTimeKind.Utc);
			Now.Is(currentTime);
			
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				AccountController.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "passadej", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var result = AccountController.Login(model).Content;
				result.Success.Should().Be.True();
				result.AccessToken.Should().Not.Be.NullOrEmpty();
				Now.Is(currentTime.AddMinutes(25));
				AdminAccessTokenRepository.TokenIsValid(result.AccessToken, Now).Should().Be.True();
				var result2 = AccountController.Login(model).Content;
				result2.Success.Should().Be.True();
				result2.AccessToken.Should().Not.Be.NullOrEmpty();
				result2.AccessToken.Should().Not.Be.EqualTo(result.AccessToken);
				Now.Is(currentTime.AddMinutes(50));
				AdminAccessTokenRepository.TokenIsValid(result.AccessToken, Now).Should().Be.True();
				AdminAccessTokenRepository.TokenIsValid(result2.AccessToken, Now).Should().Be.True();
			}
		}

	}
}