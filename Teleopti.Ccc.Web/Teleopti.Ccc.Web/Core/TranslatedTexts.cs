using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Teleopti.Ccc.Web.Core
{
	//Remove me when #36904 is done!
	public class TranslatedTexts
	{
		private static readonly Lazy<string[]> _keys = new Lazy<string[]>(initKeys);

		public IDictionary<string, string> For(CultureInfo culture)
		{
			return _keys.Value.ToDictionary(key=> key, key => UserTexts.Resources.ResourceManager.GetString(key, culture));
		}

		//This is hack due to #36838. Make sure to not call GetResourceSet at the same time as resources are read.
		private static string[] initKeys()
		{
			return
				UserTexts.Resources.ResourceManager.GetResourceSet(CultureInfo.GetCultureInfo("en"), true, true)
					.OfType<DictionaryEntry>()
					.Select(k => k.Key.ToString())
					.ToArray();
		}
	}
}