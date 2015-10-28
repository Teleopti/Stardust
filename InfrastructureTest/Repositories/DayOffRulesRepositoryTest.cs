using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class DayOffRulesRepositoryTest : RepositoryTest<DayOffRules>
	{
		protected override DayOffRules CreateAggregateWithCorrectBusinessUnit()
		{
			return new DayOffRules
			{
				ConsecutiveDayOffs = new MinMax<int>(2,6),
				DayOffsPerWeek = new MinMax<int>(4,6),
				ConsecutiveWorkdays = new MinMax<int>(5,7)
			};
		}

		protected override void VerifyAggregateGraphProperties(DayOffRules loadedAggregateFromDatabase)
		{
			var expected = CreateAggregateWithCorrectBusinessUnit();
			loadedAggregateFromDatabase.DayOffsPerWeek.Should().Be.EqualTo(expected.DayOffsPerWeek);
			loadedAggregateFromDatabase.ConsecutiveDayOffs.Should().Be.EqualTo(expected.ConsecutiveDayOffs);
			loadedAggregateFromDatabase.ConsecutiveWorkdays.Should().Be.EqualTo(expected.ConsecutiveWorkdays);
		}

		protected override Repository<DayOffRules> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new DayOffRulesRepository(currentUnitOfWork);
		}

		[Test]
		public void CanAddMulitpleNonDefaults()
		{
			var rep = new DayOffRulesRepository(CurrUnitOfWork);
			rep.Add(new DayOffRules());
			UnitOfWork.Flush();
			rep.Add(new DayOffRules());
			UnitOfWork.Flush();
		}

		[Test]
		public void WhenAddingTwoDefaultSettingsLastWins()
		{
			var rep = new DayOffRulesRepository(CurrUnitOfWork);
			var lastDefault = DayOffRules.CreateDefault();
			rep.Add(DayOffRules.CreateDefault());
			UnitOfWork.Flush();
			rep.Add(lastDefault);
			UnitOfWork.Flush();
			rep.Default().ConsecutiveDayOffs
				.Should().Be.EqualTo(lastDefault);
		}

		[Test]
		public void CanUseAddWhenUpdatingAlreadyPersistedDefault()
		{
			var dayOffSettings = DayOffRules.CreateDefault();
			var rep = new DayOffRulesRepository(CurrUnitOfWork);
			rep.Add(dayOffSettings);
			UnitOfWork.Flush();
			Assert.DoesNotThrow(() => rep.Add(dayOffSettings));
		}

		[Test]
		public void CanNotRemoveDefaultSetting()
		{
			var rep = new DayOffRulesRepository(CurrUnitOfWork);
			var defaultSetting = DayOffRules.CreateDefault();
			Assert.Throws<ArgumentException>(() => rep.Remove(defaultSetting));
		}

		//TODO: this should be handled differently when views/workflow is more stable -> don't create implicitly here
		[Test]
		public void ShouldReturnDefaultValuesIfNotPresentInDb()
		{
			var defaultSetting = DayOffRules.CreateDefault();
			var defaultInDb = new DayOffRulesRepository(CurrUnitOfWork).Default();

			defaultInDb.DayOffsPerWeek.Should().Be.EqualTo(defaultSetting.DayOffsPerWeek);
			defaultInDb.ConsecutiveDayOffs.Should().Be.EqualTo(defaultSetting.ConsecutiveDayOffs);
			defaultInDb.ConsecutiveWorkdays.Should().Be.EqualTo(defaultSetting.ConsecutiveWorkdays);
		}
	}
}