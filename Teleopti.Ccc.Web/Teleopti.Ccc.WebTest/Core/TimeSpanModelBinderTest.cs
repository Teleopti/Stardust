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
	public class TimeSpanModelBinderTest
	{
		[Test]
		public void ShouldSupportCurrentCultureFormatOfTime()
		{
			var expectedTime = TimeSpan.FromHours(3).Add(TimeSpan.FromMinutes(39));
			
			var dict = new NameValueCollection
			           	{
			           		{"time", TimeHelper.GetLongHourMinuteTimeString(expectedTime, CultureInfo.CurrentCulture)}
			           	};

			var bindingContext = new ModelBindingContext
			{
				ModelName = "time",
				ValueProvider = new NameValueCollectionValueProvider(dict, CultureInfo.CurrentCulture)
			};

			var binder = new TimeSpanModelBinder();

			var result = (TimeSpan)binder.BindModel(null, bindingContext);

			result.Should().Be.EqualTo(expectedTime);
		}

		[Test]
		public void ShouldConsiderSingleNumberAsHour()
		{
			var expectedTime = TimeSpan.FromHours(3);

			var dict = new NameValueCollection
			           	{
			           		{"time", "3"}
			           	};

			var bindingContext = new ModelBindingContext
			{
				ModelName = "time",
				ValueProvider = new NameValueCollectionValueProvider(dict, CultureInfo.CurrentCulture)
			};

			var binder = new TimeSpanModelBinder();

			var result = (TimeSpan)binder.BindModel(null, bindingContext);

			result.Should().Be.EqualTo(expectedTime);
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

			var binder = new TimeSpanModelBinder(nullable: true);

			var result = (TimeSpan?)binder.BindModel(null, bindingContext);
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

			var binder = new TimeSpanModelBinder();
				
			var result = binder.BindModel(null, bindingContext);

			bindingContext.ModelState["timeError"].Errors[0].ErrorMessage
				.Should().Contain("ballefjong");
			result.Should().Be.EqualTo(TimeSpan.Zero);
		}

		[Test]
		public void ShouldWorkWithNegativeNumbers()
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

			var binder = new TimeSpanModelBinder();

			var result = binder.BindModel(null, bindingContext);

			result.Should().Be.EqualTo(TimeSpan.FromHours(-9));
		}

		[Test]
		public void ShouldWorkLargeTimeSpans()
		{
			var dict = new NameValueCollection
			           	{
			           		{"time", "26:00"}
			           	};
			var bindingContext = new ModelBindingContext
			{
				ModelName = "time",
				ValueProvider = new NameValueCollectionValueProvider(dict, CultureInfo.CurrentCulture)
			};

			var binder = new TimeSpanModelBinder();

			var result = binder.BindModel(null, bindingContext);

			result.Should().Be.EqualTo(TimeSpan.FromHours(26));
		}
	}
}