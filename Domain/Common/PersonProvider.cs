using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class PersonProvider : IPersonProvider
    {
        private readonly IList<IPerson> _innerPersons;

	    public PersonProvider(IEnumerable<IPerson> persons)
        {
			_innerPersons = persons.ToList();
        }

	    public IList<IPerson> GetPersons()
        {
            return _innerPersons;
        }

        public bool DoLoadByPerson { get; set; }
    }
}