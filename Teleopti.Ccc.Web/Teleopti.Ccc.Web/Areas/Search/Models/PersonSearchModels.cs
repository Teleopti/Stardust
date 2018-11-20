using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.Search.Models
{
	public class FindPersonViewModel
	{
		public FindPersonViewModel(IEnumerable<IPerson> persons)
		{
			Persons = persons.ToList().ConvertAll(p =>
				new PersonViewModel {FirstName = p.Name.FirstName, LastName = p.Name.LastName, Id = p.Id.GetValueOrDefault().ToString()});
		}

		public List<PersonViewModel> Persons { get; set; }
	}

	[DebuggerDisplay("{FirstName} {LastName} {Id}")]
	public class PersonViewModel
	{
		public string Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}