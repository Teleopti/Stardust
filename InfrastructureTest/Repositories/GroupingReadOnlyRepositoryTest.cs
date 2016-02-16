using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	public class GroupingReadOnlyRepositoryTest : DatabaseTest
	{
		private IGroupingReadOnlyRepository _target;

		protected override void SetupForRepositoryTest()
		{
			_target = new GroupingReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
		}

		[Test]
		public void ShouldGroupPagesFromReadModel()
		{
			var items = _target.AvailableGroupPages();
			items.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadAvailableGroupsWithPageIdFromReadModel()
		{
			var items = _target.AvailableGroups(new ReadOnlyGroupPage {PageId = Group.PageMainId, PageName = "xxMain"},
				DateOnly.Today);
			items.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadAvailableGroupsFromReadModel()
		{
			var items = _target.AvailableGroups(new ReadOnlyGroupPage { PageName = "xxMain" }, DateOnly.Today);
			items.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadDetailsForGroupFromReadModel()
		{
			var items = _target.DetailsForGroup(Guid.Empty, DateOnly.Today);
			items.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadDetailsForGroupFromReadModelForRange()
		{
			var items = _target.DetailsForGroup(Guid.Empty, new DateOnlyPeriod(2001,1,1,2001,1,2));
			items.Count().Should().Be.EqualTo(0);
		}

        [Test]
        public void ShouldCallUpdateReadModelWithoutCrash()
        {
            _target.UpdateGroupingReadModel(new Guid[] { Guid.NewGuid() });
        }

        [Test]
        public void ShouldCallUpdateGroupingReadModelGroupPageWithoutCrash()
        {
            _target.UpdateGroupingReadModelGroupPage(new Guid[] { Guid.NewGuid() });
        }

        [Test]
        public void ShouldCallUpdateGroupingReadModelDataWithoutCrash()
        {
            _target.UpdateGroupingReadModelData(new Guid[] { Guid.NewGuid() });
        }
	}
}
