using System;

namespace Teleopti.Support.Code.Tool
{
    public class SearchForAttribute : Attribute
    {
        private readonly string _searchFor;

        public SearchForAttribute(string searchFor)
         {
             _searchFor = searchFor;
         }

        public string SearchFor { get { return _searchFor; } }
    }
}