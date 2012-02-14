using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class CheckStartupExceptionAttributeTest
	{
		private CheckStartupExceptionAttribute target;

		[SetUp]
		public void Setup()
		{
			target = new CheckStartupExceptionAttribute();
			ApplicationStartModule.ErrorAtStartup = null;
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
		public void ShouldHaveLowestOrdeOfAll()
		{
			var definedAuthorizeAttributes = from type in typeof (CheckStartupExceptionAttribute).Assembly.GetTypes()
			                                       where typeof (AuthorizeAttribute).IsAssignableFrom(type)
			                                       select (AuthorizeAttribute) Activator.CreateInstance(type);
			var targetAttributeExcluded = definedAuthorizeAttributes.Where(authorizeAttribute => authorizeAttribute.GetType() != target.GetType());

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