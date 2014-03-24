using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using DotNetOpenAuth.OpenId;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class CheckStartupExceptionAttributeTest
	{
		private CheckStartupResultAttribute target;

		[SetUp]
		public void Setup()
		{
			target = new CheckStartupResultAttribute();
			ApplicationStartModule.ErrorAtStartup = null;
			ApplicationStartModule.TasksFromStartup = null;
		}

		[TearDown]
		public void TearDown()
		{
			ApplicationStartModule.ErrorAtStartup = null;
			ApplicationStartModule.TasksFromStartup = null;
		}

		//todo: remarked for now - to expensive to test
		//[Test]
		//public void ShouldPassThroughIfNoException()
		//{
		//    target.OnAuthorization(new AuthorizationContext());
		//}

		[Test]
		public void ShouldRethrowStartupExceptionIfExists()
		{
			ApplicationStartModule.ErrorAtStartup = new Exception();
			Assert.Throws<Exception>(() => target.OnAuthorization(null));
		}

		[Test]
		public void ShouldHaveLowestOrderOfAll()
		{
			var definedAuthorizeAttributesWithEmpty = from type in typeof (CheckStartupResultAttribute).Assembly.GetTypes()
			                                       where typeof (AuthorizeAttribute).IsAssignableFrom(type) && type.GetConstructors().Any(x => x.GetParameters().Length.Equals(0))
			                                       select (AuthorizeAttribute) Activator.CreateInstance(type);
			var definedAuthorizeAttributesWithAuthenticationModuleConstructor = from type in typeof(CheckStartupResultAttribute).Assembly.GetTypes()
											 where typeof(AuthorizeAttribute).IsAssignableFrom(type) && type.GetConstructors().Any(x => x.GetParameters().Any(y => y.ParameterType == typeof(IAuthenticationModule)))
											 select (AuthorizeAttribute)Activator.CreateInstance(type, MockRepository.GenerateMock<IAuthenticationModule>(), MockRepository.GenerateMock<IIdentityProviderProvider>());

			var targetAttributeExcluded = definedAuthorizeAttributesWithEmpty.Concat(definedAuthorizeAttributesWithAuthenticationModuleConstructor).Where(authorizeAttribute => authorizeAttribute.GetType() != target.GetType());

			foreach (var authorizeAttribute in targetAttributeExcluded)
			{
				authorizeAttribute.Order.Should().Be.GreaterThan(target.Order);
			}
		}

		[Test]
		public void ShouldImplementIAuthorizationFilterToEnsureItsRunBeforeOtherFilters()
		{
			Assert.That(typeof(IAuthorizationFilter).IsAssignableFrom(target.GetType()));
		}

		[Test]
		public void ShouldAlwaysAuthorize()
		{
			var type = target.GetType();
			var method = type.GetMethod("AuthorizeCore", BindingFlags.NonPublic | BindingFlags.Instance);
			var isAuthorized = (bool)method.Invoke(target, new object[] { null });

			Assert.That(isAuthorized);
		}


	}
}