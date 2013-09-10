using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework.Constraints;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	public class StringContainsAnyLanguageResourceConstraint : Constraint
	{
		private readonly List<string> _texts = new List<string>();

		public StringContainsAnyLanguageResourceConstraint(string resourceOrText)
		{
			if (resourceOrText.Contains(" "))
				resourceOrText = new CultureInfo("en-US").TextInfo.ToTitleCase(resourceOrText).Replace(" ", "");
			// add any browser language in which tests need to run on here
			_texts.Add(Resources.ResourceManager.GetString(resourceOrText, new CultureInfo("en-US")));
			_texts.Add(Resources.ResourceManager.GetString(resourceOrText, new CultureInfo("sv-SE")));
		}

		public override bool Matches(object actual)
		{
			if (actual == null)
				return false;
			this.actual = actual;
			var actualString = (string)actual;
			return _texts.Any(actualString.Contains);
		}

		public override void WriteDescriptionTo(MessageWriter writer)
		{
			writer.WriteMessageLine("Should contain a string in any language: ");
			_texts.ForEach(s =>
				{
					writer.WriteMessageLine("");
					writer.WriteExpectedValue(s);
				});
		}
	}
}