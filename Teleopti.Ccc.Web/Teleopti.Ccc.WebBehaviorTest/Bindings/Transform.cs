using System;
using System.Globalization;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Data;


namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	public class CssClass
	{
		public string Name { get; set; }
	}

	public class LocalizedText
	{
		public string Text { get; set; }
	}

	[Binding]
	public class Transform
	{
		[StepArgumentTransformation]
		public CssClass ToClassName(string textToBeClassName)
		{
			var className = textToBeClassName.ToLower().Replace(" ", "-");
			return new CssClass { Name = className };
		}

		[StepArgumentTransformation]
		public LocalizedText To(string textToBeResourceKey)
		{
			var resourceKey = new CultureInfo("en-US").TextInfo.ToTitleCase(textToBeResourceKey).Replace(" ", "");
			var localizedText = Resources.ResourceManager.GetString(resourceKey, DataMaker.Data().MyCulture) ?? textToBeResourceKey;
			return new LocalizedText { Text = localizedText };
		}

		[StepArgumentTransformation]
		public static TimePeriod ToTimePeriod(string value)
		{
			var values = value.Split('-');
			var start = ToTimeSpan(values[0]);
			var end = start;
			if (values.Length > 1)
				end = ToTimeSpan(values[1]);
			return new TimePeriod(start, end);
		}

		[StepArgumentTransformation]
		public static TimeSpan ToTimeSpan(string value)
		{
			return TimeSpan.Parse(value);
		}

		[StepArgumentTransformation]
		public static TimeSpan? ToNullableTimeSpan(string value)
		{
			if (value == null)
				return null;
			return TimeSpan.Parse(value);
		}
	}
}