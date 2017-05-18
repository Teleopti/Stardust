using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Common
{
    public class PersonProvider : IPersonProvider
    {
        private readonly List<IPerson> _innerPersons = new List<IPerson>();

	    public PersonProvider(IEnumerable<IPerson> persons)
        {
            SetInnerPersons(persons);
        }

        public PersonProvider(IPersonRepository personRepository)
        {
            PersonRepository = personRepository;
        }

        protected IList<IPerson> InnerPersons => _innerPersons;

	    protected void SetInnerPersons(IEnumerable<IPerson> persons)
        {
			_innerPersons.AddRange(persons);
        }

        protected IPersonRepository PersonRepository { get; }

	    public virtual IList<IPerson> GetPersons()
        {
            if(PersonRepository!=null)
                SetInnerPersons(PersonRepository.FindAllSortByName());
            return InnerPersons;
        }

        public bool DoLoadByPerson { get; set; }
    }
}