using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.GroupPageCreator
{
	[TestFixture]
	public class OptionalColumnGroupPageTest
	{
		private OptionalColumnGroupPage _target;
		private IOptionalColumn _optionalColumn;

		[SetUp]
		public void Setup()
		{
			_target = new OptionalColumnGroupPage();
			_optionalColumn = new OptionalColumn("MyOptColGroupPage").WithId();
		}

		[Test]
		public void	ShouldCreateGroupPageForGivenOptionalColumn()
		{
			var person1 = new Person().WithId();
			person1.AddOptionalColumnValue(new OptionalColumnValue("group 1"), _optionalColumn);
			var person2 = new Person().WithId();
			person2.AddOptionalColumnValue(new OptionalColumnValue("group 2"), _optionalColumn);
			var persons = new [] { person1, person2 };
			
			var options = new GroupPageOptions(persons) { CurrentGroupPageName = _optionalColumn.Name };
			var groupPage = _target.CreateGroupPage(new[] {_optionalColumn}, options);

			groupPage.Description.Name.Should().Be.EqualTo(_optionalColumn.Name);
			groupPage.DescriptionKey.Should().Be.EqualTo(null);
			groupPage.RootGroupCollection.Count.Should().Be.EqualTo(2);
			var group1 = groupPage.RootGroupCollection.SingleOrDefault(x => x.Description.Name == "group 1");
			var group2 = groupPage.RootGroupCollection.SingleOrDefault(x => x.Description.Name == "group 2");
			group1.PersonCollection.Should().Contain(person1);
			group2.PersonCollection.Should().Contain(person2);
		}

		[Test]
		public void ShouldOnlyCreateGroupPageForFirstOptionalColumnInList()
		{
			var optionalColumn2 = new OptionalColumn("Optional Column 2").WithId();
			var person1 = new Person().WithId();
			person1.AddOptionalColumnValue(new OptionalColumnValue("group 1"), _optionalColumn);
			var person2 = new Person().WithId();
			person2.AddOptionalColumnValue(new OptionalColumnValue("group 2"), optionalColumn2);
			var persons = new[] { person1, person2 };

			var options = new GroupPageOptions(persons) { CurrentGroupPageName = _optionalColumn.Name };
			var groupPage = _target.CreateGroupPage(new[] { _optionalColumn, optionalColumn2 }, options);

			groupPage.Description.Name.Should().Be.EqualTo(_optionalColumn.Name);
			groupPage.RootGroupCollection.Count.Should().Be.EqualTo(1);
			var group1 = groupPage.RootGroupCollection.SingleOrDefault(x => x.Description.Name == "group 1");
			group1.PersonCollection.Count.Should().Be.EqualTo(1);
			group1.PersonCollection.First().Should().Be.SameInstanceAs(person1);
		}

		[Test]
		public void ShouldHandleEmptyEntityList()
		{
			var groupPage = _target.CreateGroupPage(new List<IOptionalColumn>(), new GroupPageOptions(new List<IPerson>()));

			groupPage.Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldHandleOptionalColumnWithoutPersonValues()
		{
			var options = new GroupPageOptions(new[] { new Person().WithId() }) { CurrentGroupPageName = _optionalColumn.Name };

			_target = new OptionalColumnGroupPage();
			var groupPage = _target.CreateGroupPage(new[] { _optionalColumn }, options);

			groupPage.Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldNotCreateGroupPageForEmptyvaluesOnOptionalColumn()
		{
			var person1 = new Person().WithId();
			person1.AddOptionalColumnValue(new OptionalColumnValue(string.Empty), _optionalColumn);
			var persons = new[] { person1 };

			var options = new GroupPageOptions(persons) { CurrentGroupPageName = _optionalColumn.Name };
			var groupPage = _target.CreateGroupPage(new[] { _optionalColumn }, options);

			groupPage.Should().Be.EqualTo(null);
		}
	}
}