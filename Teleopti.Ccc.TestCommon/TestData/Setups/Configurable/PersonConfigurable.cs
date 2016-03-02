using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonConfigurable : IDataSetup
	{
		public string Name { get; set; }

		public IPerson Person { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var repository = new PersonRepository(currentUnitOfWork);
			Person = PersonFactory.CreatePerson();
			SetName(Person, Name);
			repository.Add(Person);
		}

		public static void SetName(IPerson person, string name)
		{
			if (string.IsNullOrEmpty(name))
				return;
			if (name.Contains(" "))
			{
				var splitted = name.Split(' ');
				person.Name = new Name(splitted[0], splitted[1]);
				return;
			}
			person.Name = new Name(name, "");
		}
	}
}