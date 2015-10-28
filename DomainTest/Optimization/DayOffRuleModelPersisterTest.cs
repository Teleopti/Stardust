using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class DayOffRuleModelPersisterTest
	{
		public FakeDayOffRulesRepository DayOffRulesRepository;
		public IDayOffRulesModelPersister Target;

		[Test]
		public void ShouldUpdate()
		{
			var existing = new DayOffRules().WithId();
			DayOffRulesRepository.Add(existing);

			var model = new DayOffRulesModel
			{
				Id = existing.Id.Value,
				MinDayOffsPerWeek = 2,
				MaxDayOffsPerWeek = 6,
				MinConsecutiveDayOffs = 1,
				MaxConsecutiveDayOffs = 1,
				MinConsecutiveWorkdays = 2,
				MaxConsecutiveWorkdays = 2
			};

			Target.Persist(model);

			var inDb = DayOffRulesRepository.Get(existing.Id.Value);
			inDb.DayOffsPerWeek.Should().Be.EqualTo(new MinMax<int>(model.MinDayOffsPerWeek, model.MaxDayOffsPerWeek));
			inDb.ConsecutiveDayOffs.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveDayOffs, model.MaxConsecutiveDayOffs));
			inDb.ConsecutiveWorkdays.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveWorkdays, model.MaxConsecutiveWorkdays));
			inDb.Default.Should().Be.False();
		}

		[Test]
		public void ShouldUpdateDefault()
		{
			var existing = DayOffRules.CreateDefault().WithId();
			DayOffRulesRepository.Add(existing);

			var model = new DayOffRulesModel
			{
				Id = existing.Id.Value,
				MinDayOffsPerWeek = 2,
				MaxDayOffsPerWeek = 3,
				MinConsecutiveDayOffs = 1,
				MaxConsecutiveDayOffs = 4,
				MinConsecutiveWorkdays = 1,
				MaxConsecutiveWorkdays = 1,
				Default = true
			};

			Target.Persist(model);

			var inDb = DayOffRulesRepository.Get(existing.Id.Value);
			inDb.DayOffsPerWeek.Should().Be.EqualTo(new MinMax<int>(model.MinDayOffsPerWeek, model.MaxDayOffsPerWeek));
			inDb.ConsecutiveDayOffs.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveDayOffs, model.MaxConsecutiveDayOffs));
			inDb.ConsecutiveWorkdays.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveWorkdays, model.MaxConsecutiveWorkdays));
			inDb.Default.Should().Be.True();
		}

		[Test]
		public void ShouldInsert()
		{
			var model = new DayOffRulesModel
			{
				MinDayOffsPerWeek = 1,
				MaxDayOffsPerWeek = 2,
				MinConsecutiveDayOffs = 3,
				MaxConsecutiveDayOffs = 4,
				MinConsecutiveWorkdays = 5,
				MaxConsecutiveWorkdays = 6
			};

			Target.Persist(model);

			var inDb = DayOffRulesRepository.LoadAll().Single();
			inDb.DayOffsPerWeek.Should().Be.EqualTo(new MinMax<int>(model.MinDayOffsPerWeek, model.MaxDayOffsPerWeek));
			inDb.ConsecutiveDayOffs.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveDayOffs, model.MaxConsecutiveDayOffs));
			inDb.ConsecutiveWorkdays.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveWorkdays, model.MaxConsecutiveWorkdays));
			inDb.Default.Should().Be.False();
		}

		[Test]
		public void ShouldInsertDefault()
		{
			var model = new DayOffRulesModel
			{
				MinDayOffsPerWeek = 1,
				MaxDayOffsPerWeek = 1,
				MinConsecutiveDayOffs = 2,
				MaxConsecutiveDayOffs = 2,
				MinConsecutiveWorkdays = 3,
				MaxConsecutiveWorkdays = 3,
				Default = true
			};

			Target.Persist(model);

			var inDb = DayOffRulesRepository.LoadAll().Single();
			inDb.DayOffsPerWeek.Should().Be.EqualTo(new MinMax<int>(model.MinDayOffsPerWeek, model.MaxDayOffsPerWeek));
			inDb.ConsecutiveDayOffs.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveDayOffs, model.MaxConsecutiveDayOffs));
			inDb.ConsecutiveWorkdays.Should().Be.EqualTo(new MinMax<int>(model.MinConsecutiveWorkdays, model.MaxConsecutiveWorkdays));
			inDb.Default.Should().Be.True();
		}
	}
}