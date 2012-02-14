using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class PersonProvider : IPersonProvider
    {
        private readonly IList<IPerson> _innerPersons = new List<IPerson>();
        private readonly IPersonRepository _personRepository;

        public PersonProvider(IEnumerable<IPerson> persons)
        {
            SetInnerPersons(persons);
            //LoadRestrictions = true;
            //LoadNotes = true;
        }

        public PersonProvider(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
            //LoadRestrictions = true;
            //LoadNotes = true;
        }

        protected IList<IPerson> InnerPersons
        {
            get { return _innerPersons; }
        }

        protected void SetInnerPersons(IEnumerable<IPerson> persons)
        {
            foreach (IPerson person in persons)
            {
                _innerPersons.Add(person);
            }
        }

        protected IPersonRepository PersonRepository
        {
            get { return _personRepository; }
        }

        public virtual IList<IPerson> GetPersons()
        {
            if(PersonRepository!=null)
                SetInnerPersons(PersonRepository.FindAllSortByName());
            return InnerPersons;
        }

        public bool DoLoadByPerson { get; set; }
        //public bool LoadRestrictions { get; set; }
        //public bool LoadNotes { get; set; }
    }
}