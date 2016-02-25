using System;
using System.Linq;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Support.Library.Config
{
    public class TextToTags
    {
        public Tags ParseText(string text)
        {
			var searchReplaces = new Tags();
	        var fromFile = (
		        from setting in text.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
		        select setting.Split(new[] {'|'}, 2)
		        into values
		        where values.GetUpperBound(0).Equals(1)
		        select new
		        {
			        SearchFor = values[0],
			        ReplaceWith = values[1]
		        });
	        fromFile.ForEach(
		        x => searchReplaces.Set(x.SearchFor, x.ReplaceWith)
		        );
			return searchReplaces;
        }
    }
}