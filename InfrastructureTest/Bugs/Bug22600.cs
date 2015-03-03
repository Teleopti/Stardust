using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
	public class Bug22600 : DatabaseTest
	{
		private IGroupPage groupPage;
		private IPerson personInRootGroup;
		private IPerson personInChildGroup;

		protected override void SetupForRepositoryTest()
		{
			CleanUpAfterTest();
			personInRootGroup = PersonFactory.CreatePerson("Person1");
			personInChildGroup = PersonFactory.CreatePerson("Person2");
			groupPage = new GroupPage("Contract Basis Page");
			var rootPersonGroup = new RootPersonGroup("unit1");
			var childPersonGroup = new ChildPersonGroup("subUnit1");
			groupPage.AddRootPersonGroup(rootPersonGroup);
			rootPersonGroup.AddChildGroup(childPersonGroup);
			rootPersonGroup.AddPerson(personInRootGroup);
			childPersonGroup.AddPerson(personInChildGroup);

			PersistAndRemoveFromUnitOfWork(personInRootGroup);
			PersistAndRemoveFromUnitOfWork(personInChildGroup);
			PersistAndRemoveFromUnitOfWork(groupPage);
			UnitOfWork.PersistAll();
		}

		[Test]
		public void MergeNoChangesShouldNotReturnAnyChanges()
		{
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				groupPage = uow.Merge(groupPage);
				uow.PersistAll().Should().Be.Empty();
			}
		}

		[Test]
		public void AddingPersonToRootGroupShouldGenerateOneChange()
		{
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
			groupPage.RootGroupCollection[0].ChildGroupCollection[0].RemovePerson(personInChildGroup);
			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				groupPage = uow.Merge(groupPage);
				uow.PersistAll().Count().Should().Be.EqualTo(1);
			}
		}

	}
}