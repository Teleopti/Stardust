using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Collection
{
    public class PersonCollection : IPersonCollection
    {
        private readonly IList<IPerson> _wrappedCollection;
        private readonly string _functionPath;
        private readonly DateOnly _queryDate;

        public PersonCollection(string functionPath, IEnumerable<IPerson> allPersons, DateOnly queryDate)
        {
            _wrappedCollection = new List<IPerson>(allPersons);
            _functionPath = functionPath;
            _queryDate = queryDate;
        }

        public IEnumerable<IPerson> AllPermittedPersons
        {
            get
            {
                var authorization = PrincipalAuthorization.Current_DONTUSE();
                IList<IPerson> tempList = new List<IPerson>();
                foreach (IPerson person in _wrappedCollection)
                {
                    if (authorization.IsPermitted(_functionPath, _queryDate, person))
                        tempList.Add(person);
                }
                return new ReadOnlyCollection<IPerson>(tempList);
            }
        }
    }
}
