using System.Globalization;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class GroupPageStepDefinitions
	{
		[Given(@"there is a group page with")]
		public void GivenThereIsAGroupPageWith(Table table)
		{
			DataMaker.ApplyFromTable<GroupPageConfigurable>(table);
		}

		[Given(@"'(.*)' is on '(.*)' of group page '(.*)'")]
		public void GivenInOnOfGroupPage(string person, string @group, string page)
		{
			DataMaker.Person(person).Apply(new PersonGroupConfigurable
				{
					Page = page,
					Group = @group
				});
		}

	}

	public class GroupPageConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string Group { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var page = new GroupPage(Name);

			var groups = Group.Split(',').Select(g => g.Trim());
			groups.ForEach(g => page.AddRootPersonGroup(new RootPersonGroup(g)));

			new GroupPageRepository(uow).Add(page);
		}

	}

	public class PersonGroupConfigurable : IUserDataSetup
	{
		public string Page { get; set; }
		public string Group { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var page = new GroupPageRepository(uow).LoadAll().Single(p => p.Description.Name == Page);
			var group = page.RootGroupCollection.Single(g => g.Description.Name == Group);
			group.AddPerson(user);
		}
	}

}