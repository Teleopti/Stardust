using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;

namespace Teleopti.Ccc.WebTest.Areas.Start.Core
{
	[TestFixture]
	public class AuthenticationModelBinderTest
	{

		private static AuthenticationModelBinder Target()
		{
			return new AuthenticationModelBinder(
				new IAuthenticationType[]
					{
						new ApplicationIdentityProviderAuthenticationType(new Lazy<IIdentityLogon>(() => null), null, new Lazy<ILogLogonAttempt>(() => null)),
						new WindowsAuthenticationType(new Lazy<IIdentityLogon>(() => null), null, new Lazy<ILogLogonAttempt>(() => null))
					}
				);
		}

		private static ModelBindingContext BindingContext(NameValueCollection values)
		{
			return new ModelBindingContext
				{
					ValueProvider = new NameValueCollectionValueProvider(
						values,
						CultureInfo.CurrentCulture)
				};
		}

		[Test]
		public void ShouldBindWindowsAuthenticationModel()
		{
			var target = Target();
			var bindingContext = BindingContext(new NameValueCollection
				{
					{"type", "windows"}
				});

			var result = target.BindModel(null, bindingContext) as WindowsAuthenticationModel;

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldBindApplicationIdentityAuthenticationModel()
		{
			var target = Target();
			var bindingContext = BindingContext(new NameValueCollection
				{
					{"type", "application_token"}
				});

			var result = target.BindModel(null, bindingContext) as ApplicationIdentityAuthenticationModel;

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldThrowOnUnknownType()
		{
			var target = Target();
			var bindingContext = BindingContext(new NameValueCollection
				{
					{"type", "non existing"}
				});

			Assert.Throws<NotImplementedException>(() => target.BindModel(null, bindingContext));
		}
	}
}