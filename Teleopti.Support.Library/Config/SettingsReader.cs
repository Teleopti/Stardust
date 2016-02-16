using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Support.Library.Config
{
    public class SettingsReader
    {
        public IList<SearchReplace> GetSearchReplaceList(string fromSettings)
        {
			return (from setting in fromSettings.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries) select setting.Split(new[] { '|' }, 2) into values where values.GetUpperBound(0).Equals(1) select new SearchReplace { SearchFor = values[0], ReplaceWith = values[1] }).ToList();
        }
    }
}