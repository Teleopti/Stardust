using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core;

namespace Teleopti.Ccc.WebTest.Core
{
	[TestFixture]
	public class TimeOfDayModelBinderTest
	{
		[Test]
		public void ShouldSupportCurrentCultureFormatOfTime()
		{
			var expectedTime = TimeSpan.FromHours(3).Add(TimeSpan.FromMinutes(39));
			var expectedResult = new TimeOfDay(expectedTime);

			var dict = new NameValueCollection
			           	{
			           		{"time", expectedTime.ToString()}
			           	};

			var bindingContext = new ModelBindingContext
			{
				ModelName = "time",
				ValueProvider = new NameValueCollectionValueProvider(dict, CultureInfo.CurrentCulture)
			};

			var binder = new TimeOfDayModelBinder();

			var result = (TimeOfDay)binder.BindModel(null, bindingContext);

			result.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldConsiderSingleNumberAsHour()
		{
			var expectedTime = TimeSpan.FromHours(3);
			var expectedResult = new TimeOfDay(expectedTime);

			var dict = new NameValueCollection
			           	{
			           		{"time", "3"}
			           	};

			var bindingContext = new ModelBindingContext
			{
				ModelName = "time",
				ValueProvider = new NameValueCollectionValueProvider(dict, CultureInfo.CurrentCulture)
			};

			var binder = new TimeOfDayModelBinder();

			var result = (TimeOfDay)binder.BindModel(null, bindingContext);

			result.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldHandleNullableTimeOfDay()
		{
			var dict = new NameValueCollection
			           	{
			           		{"time", ""}
			           	};

			var bindingContext = new ModelBindingContext
			{
				ModelName = "time",
				ValueProvider = new NameValueCollectionValueProvider(dict, CultureInfo.CurrentCulture)
			};

			var binder = new TimeOfDayModelBinder(nullable:true);

			var result = (TimeOfDay?)binder.BindModel(null, bindingContext);
			result.HasValue.Should().Be.False();
		}

		[Test]
		public void ShouldSetModelErrorIfUnrecognizedInput()
		{
			var dict = new NameValueCollection
			           	{
			           		{"time", "ballefjong"}
			           	};

			var bindingContext = new ModelBindingContext
			{
				ModelName = "time",
				ValueProvider = new NameValueCollectionValueProvider(dict, CultureInfo.CurrentCulture)
			};

			var binder = new TimeOfDayModelBinder();
				
			var result = binder.BindModel(null, bindingContext);

			bindingContext.ModelState["timeError"].Errors[0].ErrorMessage
				.Should().Contain("ballefjong");
			result.Should().Be.EqualTo(new TimeOfDay());
		}

		[Test]
		public void ShouldSetModelErrorIfNegativeNumber()
		{
			var dict = new NameValueCollection
			           	{
			           		{"time", "-9"}
			           	};
			var bindingContext = new ModelBindingContext
			{
				ModelName = "time",
				ValueProvider = new NameValueCollectionValueProvider(dict, CultureInfo.CurrentCulture)
			};

			var binder = new TimeOfDayModelBinder();

			var result = binder.BindModel(null, bindingContext);

			bindingContext.ModelState["timeError"].Errors[0].ErrorMessage
				.Should().Contain("-9");
			result.Should().Be.EqualTo(new TimeOfDay());
		}
	}
}