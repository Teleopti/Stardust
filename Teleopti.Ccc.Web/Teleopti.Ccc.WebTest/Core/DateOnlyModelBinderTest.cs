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
	public class DateOnlyModelBinderTest
	{
		[Test]
		public void ShouldReturnNullWhenKeyNotDefined()
		{
			var bindingContext = getDefaultBindingContext();

			var binder = new DateOnlyModelBinder {Year = null, Month="myMonth"};
			var result = binder.BindModel(null, bindingContext);

			result.Should().Be.Null();
		}

		private static ModelBindingContext getDefaultBindingContext()
		{
			var dict = new NameValueCollection
			           	{
			           		{"month", "5"},
			           		{"day", "31"},
			           		{"year", "2011"}
			           	};
			return new ModelBindingContext
			       	{
			       		FallbackToEmptyPrefix = true,
			       		ValueProvider = new NameValueCollectionValueProvider(dict, CultureInfo.CurrentCulture)
			       	};
		}

		[Test]
		public void ShouldReconizeDateFromYearMonthDayPartsWithDefaultNameWithModel()
		{
			var expectedResult = new DateOnly(2011, 5, 31);

			
			var binder = new DateOnlyModelBinder();

			var result = (DateOnly)binder.BindModel(null, getDefaultBindingContext());

			result.Should().Be.EqualTo(expectedResult);
		}


		[Test]
		public void ShouldReconizeDateFromYearMonthDayParts()
		{
			var expectedResult = new DateOnly(2011, 5, 31);

			var dict = new NameValueCollection
			           	{
			           		{"mymonth", "5"},
			           		{"myday", "31"},
			           		{"myyear", "2011"}
			           	};

			var bindingContext = new ModelBindingContext
			                     	{
			                     		FallbackToEmptyPrefix = true,
			                     		ValueProvider = new NameValueCollectionValueProvider(dict, CultureInfo.CurrentCulture)
			                     	};

			var binder = new DateOnlyModelBinder {Year = "myyear", Month = "mymonth", Day = "myday"};

			var result = (DateOnly) binder.BindModel(null, bindingContext);

			result.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReconizeDateFromModelNamePartWithFixedFormat()
		{
			var expectedResult = new DateOnly(2011, 9, 2);

			var dict = new NameValueCollection
			           	{
			           		{"date", "2011-09-02"}
			           	};

			var bindingContext = new ModelBindingContext
			{
				ModelName = "date",
				FallbackToEmptyPrefix = true,
				ValueProvider = new NameValueCollectionValueProvider(dict, CultureInfo.CurrentCulture)
			};

			var binder = new DateOnlyModelBinder();

			var result = (DateOnly)binder.BindModel(null, bindingContext);

			result.Should().Be.EqualTo(expectedResult);
		}

		[Test, SetCulture("ar-SA"), SetUICulture("ar-SA")]
		public void ShouldReconizeArabicDateFromModelNamePartWithFixedFormat()
		{
			var expectedResult = new DateOnly(2011, 6, 07);

			var dict = new NameValueCollection
			           	{
			           		{"date", "1432-07-05"}
			           	};

			var bindingContext = new ModelBindingContext
			{
				ModelName = "date",
				FallbackToEmptyPrefix = true,
				ValueProvider = new NameValueCollectionValueProvider(dict, CultureInfo.CurrentCulture)
			};

			var binder = new DateOnlyModelBinder();

			var result = (DateOnly)binder.BindModel(null, bindingContext);

			result.Should().Be.EqualTo(expectedResult);
		}


		[Test, SetCulture("ar-SA"), SetUICulture("ar-SA")]
		public void ShouldReadArabicYearMonthDayPartsToValidDate()
		{
			var expectedResult = new DateOnly(2011, 6, 07);
			// 1432-07-05
			var dict = new NameValueCollection
			           	{
			           		{"month", "07"},
			           		{"day", "05"},
			           		{"year", "1432"}
			           	};

			var bindingContext = new ModelBindingContext
			                     	{
			                     		FallbackToEmptyPrefix = true,
			                     		ValueProvider = new NameValueCollectionValueProvider(dict, CultureInfo.CurrentCulture)
			                     	};

			var binder = new DateOnlyModelBinder();

			var result = (DateOnly) binder.BindModel(null, bindingContext);

			result.Should().Be.EqualTo(expectedResult);
		}
	}
}