using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Presentation
{
    public class ReportUserSelectorAuditingModel
    {
        public Guid Id { get; private set; }
        public string Text { get; private set; }
        public IPerson Person { get; private set; }

        //private readonly IList<IPerson> _persons = new List<IPerson>();

        public ReportUserSelectorAuditingModel(Guid value, string text)
        {
            Id = value;
            Text = text;
        }

        public ReportUserSelectorAuditingModel(IPerson person)
        {
            if (person == null)
                throw new ArgumentNullException("person");

            if(person.Id == null)
                throw new ArgumentNullException("person");
           

            Id = (Guid)person.Id;
            Text = person.Name.ToString(NameOrderOption.FirstNameLastName);
            Person = person;
        }
    }
}
