using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
    public class PersonContainer
    {
        private readonly IPerson _person;

        public PersonContainer(IPerson person)
        {
            _person = person;
        }

        public IPerson Person
        {
            get { return _person; }
        }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string DataSource { get; set; }
    }
}