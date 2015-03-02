using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.WinCode.Common.Filter
{
    public class FilterListViewItems
    {
        private readonly IList<ListViewItem> _allItems;

        public FilterListViewItems(IList<ListViewItem> allItems)
        {
            _allItems = allItems;
        }

        public IList<ListViewItem> Filter(string filterOn)
        {
            if (string.IsNullOrEmpty(filterOn))
                return _allItems;

            var splitted = filterOn.Split(new []{" "},StringSplitOptions.RemoveEmptyEntries);
            var info = TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture;

            var toBeFiltered = new List<ListViewItem>(_allItems);

            foreach (var s in splitted)
            {
                var g = (from d in _allItems
                         where !(d.Text.ToLower(info).Contains(s.ToLower(info))
                                 || d.SubItems.OfType<ListViewItem.ListViewSubItem>().Any(
                                     i => i.Text.ToLower(info).Contains(s.ToLower(info))))
                         select d).ToList();

                foreach (var listViewItem in g)
                {
                    toBeFiltered.Remove(listViewItem);
                }
            }
            
            return toBeFiltered.Distinct().ToList();
        }
    }
}