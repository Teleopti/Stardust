using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.IntegrationTest.Core
{
	[TenantTest]
	public class AdminAccessTokenRepositoryTest : ISetup
	{
		public ITenantUnitOfWork TenantUnitOfWork;
		public AccountController AccountController;
		public MutableNow Now;
		
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<MutableNow>().For<INow>();
		}
		[Test]
		public void AccessTokenShouldBecomeInvalidAfter41Minutes()
		{
			var currentTime = new DateTime(2017, 08, 26, 12, 0, 0, DateTimeKind.Utc);
			Now.Is(currentTime);
			
			DataSourceHelper.CreateDatabasesAndDataSource(new NoTransactionHooks(), "TestData");
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
			
			DataSourceHelper.CreateDatabasesAndDataSource(new NoTransactionHooks(), "TestData");
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
			
			DataSourceHelper.CreateDatabasesAndDataSource(new NoTransactionHooks(), "TestData");
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