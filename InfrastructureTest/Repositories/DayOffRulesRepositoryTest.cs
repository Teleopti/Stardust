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
			var expected = new MinMax<int>(2,2);
			var rep = new DayOffRulesRepository(CurrUnitOfWork);
			rep.Add(new DayOffRules() {ConsecutiveDayOffs = new MinMax<int>(4,5)}.MakeDefault_UseOnlyFromTest());
			UnitOfWork.Flush();
			rep.Add(new DayOffRules() { ConsecutiveDayOffs = expected }.MakeDefault_UseOnlyFromTest());
			UnitOfWork.Flush();
			rep.LoadAll().Single(x => x.Default).ConsecutiveDayOffs
				.Should().Be.EqualTo(expected);
		}

		[Test]
		public void CanUseAddWhenUpdatingAlreadyPersistedDefault()
		{
			var dayOffSettings = new DayOffRules().MakeDefault_UseOnlyFromTest();
			var rep = new DayOffRulesRepository(CurrUnitOfWork);
			rep.Add(dayOffSettings);
			UnitOfWork.Flush();
			Assert.DoesNotThrow(() => rep.Add(dayOffSettings));
		}

		[Test]
		public void CanNotRemoveDefaultSetting()
		{
			var rep = new DayOffRulesRepository(CurrUnitOfWork);
			var defaultSetting = new DayOffRules().MakeDefault_UseOnlyFromTest();
			Assert.Throws<ArgumentException>(() => rep.Remove(defaultSetting));
		}
	}
}