using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core
{
	//Remove me when #36904 is done!
	public class TranslatedTexts
	{
		private ISet<string> _keys;

		public IDictionary<string, string> For(CultureInfo culture)
		{
			if(_keys==null)
				throw new NotSupportedException("Init method needs to be called before getting translated texts!");

			return _keys.ToDictionary(key=> key, key => UserTexts.Resources.ResourceManager.GetString(key, culture));
		}

		//This is hack due to #36838. Make sure to not call GetResourceSet at the same time as resources are read.
		public void Init()
		{
			if (_keys != null)
				throw new NotSupportedException("Only call Init once!");

			var keys = new HashSet<string>();
			foreach (DictionaryEntry resourceSet in UserTexts.Resources.ResourceManager.GetResourceSet(CultureInfo.GetCultureInfo("en"), true, true))
			{
				keys.Add(resourceSet.Key.ToString());
			}
			_keys = keys;
		}
	}

	//Remove me when #36904 is done!
	public class TranslatedTextsStarter : IBootstrapperTask
	{
		private readonly TranslatedTexts _translatedTexts;

		public TranslatedTextsStarter(TranslatedTexts translatedTexts)
		{
			_translatedTexts = translatedTexts;
		}

		public Task Execute(IAppBuilder application)
		{
			_translatedTexts.Init();
			return null;
		}
	}
}