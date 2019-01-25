using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public interface ISearchPersonView
    {
        void SetPresenter(SearchPersonPresenter presenter);
        IPerson SelectedPerson();
        void FillGridListControl();
    }

    public class SearchPersonPresenter
    {
        private IEnumerable<IPerson> _searchablePersons;
        private ISearchPersonView _view;

        public SearchPersonPresenter(ISearchPersonView view)
        {
            _view = view;
        }

        public void SetSearchablePersons(IEnumerable<IPerson> searchablePersons)
        {
            _searchablePersons = searchablePersons;
        }

        public ICollection<IPerson> Search(string searchText)
        {
            CultureInfo cultureInfo = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
            string lowerSearchText = searchText.ToLower(cultureInfo);
            ICollection<IPerson> personQuery =
                    (from
                        person in _searchablePersons
                    where
                        person.Name.ToString(NameOrderOption.LastNameFirstName).ToLower(cultureInfo).Contains(lowerSearchText) ||
                        person.Name.ToString(NameOrderOption.LastNameFirstName).ToLower(cultureInfo).Replace(",","").Contains(lowerSearchText) ||
                        person.Name.ToString(NameOrderOption.FirstNameLastName).ToLower(cultureInfo).Contains(lowerSearchText) ||
                        //person.Email.Contains(searchCriteria.SearchText) ||
                        person.EmploymentNumber.ToLower(cultureInfo).Contains(lowerSearchText) ||
                        person.Note.ToLower(cultureInfo).Contains(lowerSearchText)
                        //person.WindowsLogOnName.Contains(searchCriteria.SearchText) ||
                        //person.ApplicationLogOnName.Contains(searchCriteria.SearchText)
                    select person).ToList();

            return personQuery;

        }

        public void OnTextBox1TextChanged()
        {
            _view.FillGridListControl();  
        }
    }


}