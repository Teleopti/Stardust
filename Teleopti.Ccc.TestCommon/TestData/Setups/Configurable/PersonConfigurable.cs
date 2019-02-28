using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PersonConfigurable : IDataSetup
	{
		public string Name { get; set; }

		public IPerson Person { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var repository = PersonRepository.DONT_USE_CTOR(currentUnitOfWork, null, null);
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
				person.SetName(new Name(splitted[0], splitted[1]));
				return;
			}
			person.SetName(new Name(name, ""));
		}
	}
}