using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.MultiplicatorDefinitionSetHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.MultiplicatorDefinitionSetHandlers
{
	[TestFixture]
	public class AnalyticsOvertimeUpdaterTest
	{
		private AnalyticsOvertimeUpdater _target;

		private FakeAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private IAnalyticsOvertimeRepository _analyticsOvertimeRepository;
		private IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;

		private MultiplicatorDefinitionSet multiplicatorDefinitionSet;

		[SetUp]
		public void Setup()
		{
			_analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			_analyticsOvertimeRepository = new FakeAnalyticsOvertimeRepository();
			_multiplicatorDefinitionSetRepository = new FakeMultiplicatorDefinitionSetRepository();
			multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("Test", MultiplicatorType.Overtime).WithId(Guid.NewGuid());
			_multiplicatorDefinitionSetRepository.Add(multiplicatorDefinitionSet);

			_target = new AnalyticsOvertimeUpdater(_analyticsOvertimeRepository, _analyticsBusinessUnitRepository, _multiplicatorDefinitionSetRepository);
		}

		[Test, TestCaseSource(nameof(overtimeEvents))]
		public void ShouldAddOrUpdate(MultiplicatorDefinitionSetChangedBase @event)
		{
			@event.MultiplicatorDefinitionSetId = multiplicatorDefinitionSet.Id.GetValueOrDefault();
			_target.Handle((dynamic)@event);

			var analyticsOvertime = _analyticsOvertimeRepository.Overtimes().FirstOrDefault(x => x.OvertimeCode == multiplicatorDefinitionSet.Id);
			analyticsOvertime.Should().Not.Be.Null();
		}

		[Test, TestCaseSource(nameof(nonOvertimeEvents))]
		public void ShouldDoNothingWhenWrongTypeOfMultiplicatorDefinitionSet(MultiplicatorDefinitionSetChangedBase @event)
		{
			@event.MultiplicatorDefinitionSetId = multiplicatorDefinitionSet.Id.GetValueOrDefault();

			_target.Handle((dynamic)@event);

			_analyticsOvertimeRepository.Overtimes().Should().Be.Empty();
		}

		#region TestData
		private static readonly object[] overtimeEvents =
		{
			new object[] {createEvent<MultiplicatorDefinitionSetCreated>(MultiplicatorType.Overtime)},
			new object[] {createEvent<MultiplicatorDefinitionSetChanged>(MultiplicatorType.Overtime)},
			new object[] {createEvent<MultiplicatorDefinitionSetDeleted>(MultiplicatorType.Overtime) }
		};

		private static readonly object[] nonOvertimeEvents =
		{
			new object[] {createEvent<MultiplicatorDefinitionSetCreated>(MultiplicatorType.OBTime)},
			new object[] {createEvent<MultiplicatorDefinitionSetChanged>(MultiplicatorType.OBTime) },
			new object[] {createEvent<MultiplicatorDefinitionSetDeleted>(MultiplicatorType.OBTime) }
		};

		

		private static T createEvent<T>(MultiplicatorType type) where T : MultiplicatorDefinitionSetChangedBase, new()
		{
			return new T
			{
				LogOnBusinessUnitId = Guid.NewGuid(),
				MultiplicatorDefinitionSetId = Guid.NewGuid(),
				MultiplicatorType = type
			};
		}
		#endregion
	}
}