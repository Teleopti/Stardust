using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class DayOffTemplateRepositoryTest : RepositoryTest<IDayOffTemplate>
	{
		private IDayOffTemplate _dayOff;
		private IDayOffTemplate _dayOff2;
		private IDayOffTemplate _dayOff3;
		private readonly Description _description = new Description("Day Off Test");
		private readonly TimeSpan _timeSpanTargetLength = new TimeSpan(1, 1, 1, 1);
		private readonly TimeSpan _timeSpanFlexibility = new TimeSpan(2, 2, 2, 2);
		private readonly TimeSpan _timeSpanAnchor = new TimeSpan(3, 3, 3, 3);

		/// <summary>
		/// Runs every test. Implemented by repository's concrete implementation.
		/// </summary>
		protected override void ConcreteSetup()
		{
			_dayOff = DayOffFactory.CreateDayOff();
		}

		/// <summary>
		/// Creates an aggregate using the Bu of logged in user.
		/// Should be a "full detailed" aggregate
		/// </summary>
		/// <returns></returns>
		protected override IDayOffTemplate CreateAggregateWithCorrectBusinessUnit()
		{
			DayOffTemplate dayOff = DayOffFactory.CreateDayOff();
			dayOff.ChangeDescription(_description.Name, _description.ShortName);
			dayOff.SetTargetAndFlexibility(_timeSpanTargetLength, _timeSpanFlexibility);
			dayOff.Anchor = _timeSpanAnchor;

			return dayOff;
		}

		/// <summary>
		/// Verifies the aggregate graph properties.
		/// </summary>
		/// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
		protected override void VerifyAggregateGraphProperties(IDayOffTemplate loadedAggregateFromDatabase)
		{
			IDayOffTemplate dayOff = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(dayOff.Description, loadedAggregateFromDatabase.Description);
			Assert.AreEqual(dayOff.TargetLength, loadedAggregateFromDatabase.TargetLength);
			Assert.AreEqual(dayOff.Flexibility, loadedAggregateFromDatabase.Flexibility);
			Assert.AreEqual(dayOff.Anchor, loadedAggregateFromDatabase.Anchor);
			Assert.AreEqual(dayOff.PayrollCode, loadedAggregateFromDatabase.PayrollCode);
		}

		protected override Repository<IDayOffTemplate> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return DayOffTemplateRepository.DONT_USE_CTOR2(currentUnitOfWork);
		}
		
		[Test]
		public void VerifyCanLoadDayOffsSortedByDescription()
		{
			addFewDayOffs();
			var dayOffList = DayOffTemplateRepository.DONT_USE_CTOR(UnitOfWork).FindAllDayOffsSortByDescription();
			Assert.AreEqual("AAA", dayOffList[0].Description.Name);
		}

		[Test]
		public void ShouldOnlyReturnActivedDayOffsAndSortedByDescription()
		{
			addFewDayOffs();
			((IDeleteTag)_dayOff).SetDeleted();
			PersistAndRemoveFromUnitOfWork(_dayOff);

			var dayOffList = DayOffTemplateRepository.DONT_USE_CTOR(UnitOfWork).FindAllDayOffsSortByDescription();
			dayOffList.Count.Should().Be.EqualTo(2);
			dayOffList[0].Description.Name.Should().Be.EqualTo("AAA");
			dayOffList[1].Description.Name.Should().Be.EqualTo("CCC");
		}

		private void addFewDayOffs()
		{
			_dayOff = new DayOffTemplate(new Description("BBB"));
			_dayOff2 = new DayOffTemplate(new Description("AAA"));
			_dayOff3 = new DayOffTemplate(new Description("CCC"));

			PersistAndRemoveFromUnitOfWork(_dayOff);
			PersistAndRemoveFromUnitOfWork(_dayOff2);
			PersistAndRemoveFromUnitOfWork(_dayOff3);
		}
	}
}