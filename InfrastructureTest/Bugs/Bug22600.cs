using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
	[DatabaseTest]
	public class Bug22600
	{
		public IPersonRepository PersonRepository;
		public IGroupPageRepository GroupPageRepository;

		private IGroupPage groupPage;
		private IPerson personInRootGroup;
		private IPerson personInChildGroup;

		private void setup()
		{
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				personInRootGroup = PersonFactory.CreatePerson("Person1");
				personInChildGroup = PersonFactory.CreatePerson("Person2");
				groupPage = new GroupPage("Contract Basis Page");
				var rootPersonGroup = new RootPersonGroup("unit1");
				var childPersonGroup = new ChildPersonGroup("subUnit1");
				groupPage.AddRootPersonGroup(rootPersonGroup);
				rootPersonGroup.AddChildGroup(childPersonGroup);
				rootPersonGroup.AddPerson(personInRootGroup);
				childPersonGroup.AddPerson(personInChildGroup);

				PersonRepository.Add(personInRootGroup);
				PersonRepository.Add(personInChildGroup);
				GroupPageRepository.Add(groupPage);

				uow.PersistAll();
			}

		}

		[Test]
		public void AddingPersonToRootGroupShouldGenerateOneChange()
		{
			setup();
			groupPage.RootGroupCollection[0].AddPerson(personInChildGroup);
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				groupPage = uow.Merge(groupPage);
				uow.PersistAll().Count().Should().Be.EqualTo(1);
			}			
		}

		[Test]
		public void AddingPersonToChildGroupShouldGenerateOneChange()
		{
			setup();
			groupPage.RootGroupCollection[0].ChildGroupCollection[0].AddPerson(personInRootGroup);
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				groupPage = uow.Merge(groupPage);
				uow.PersistAll().Count().Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void RemovingPersonToRootGroupShouldGenerateOneChange()
		{
			setup();
			groupPage.RootGroupCollection[0].RemovePerson(personInRootGroup);
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				groupPage = uow.Merge(groupPage);
				uow.PersistAll().Count().Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void RemovingPersonToChildGroupShouldGenerateOneChange()
		{
			setup();
			groupPage.RootGroupCollection[0].ChildGroupCollection[0].RemovePerson(personInChildGroup);
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				groupPage = uow.Merge(groupPage);
				uow.PersistAll().Count().Should().Be.EqualTo(1);
			}
		}

	}
}