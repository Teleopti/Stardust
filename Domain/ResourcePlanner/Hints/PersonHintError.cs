using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class PersonHintError
	{
		public PersonHintError()
		{
		}

		public PersonHintError(IPerson person)
		{
			PersonId = person.Id.GetValueOrDefault();
			PersonName = person.Name.ToString();
		}
		public string PersonName { get; set; }
		public Guid PersonId { get; set; }
		public string ErrorResource { get; set; }
		public List<object> ErrorResourceData { get; set; }
	}
}