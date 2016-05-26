using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.MultiplicatorDefinitionSetHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
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
			_analyticsOvertimeRepository = MockRepository.GenerateMock<IAnalyticsOvertimeRepository>();
			_multiplicatorDefinitionSetRepository = MockRepository.GenerateMock<IMultiplicatorDefinitionSetRepository>();

			multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("Test", MultiplicatorType.Overtime);
			multiplicatorDefinitionSet.SetId(Guid.NewGuid());
			_multiplicatorDefinitionSetRepository.Stub(x => x.Get(multiplicatorDefinitionSet.Id.GetValueOrDefault()))
				.Return(multiplicatorDefinitionSet);

			_target = new AnalyticsOvertimeUpdater(_analyticsOvertimeRepository, _analyticsBusinessUnitRepository, _multiplicatorDefinitionSetRepository);
		}

		[Test, TestCaseSource(nameof(overtimeEvents))]
		public void ShouldAddOrUpdate(MultiplicatorDefinitionSetChangedBase @event)
		{
			@event.MultiplicatorDefinitionSetId = multiplicatorDefinitionSet.Id.GetValueOrDefault();
			_target.Handle((dynamic)@event);

			_analyticsOvertimeRepository.AssertWasCalled(r =>
					r.AddOrUpdate(Arg<AnalyticsOvertime>.Matches(ao =>
								ao.IsDeleted == multiplicatorDefinitionSet.IsDeleted &&
								ao.OvertimeCode == multiplicatorDefinitionSet.Id.GetValueOrDefault() &&
								ao.OvertimeName == multiplicatorDefinitionSet.Name)));
		}

		[Test, TestCaseSource(nameof(nonOvertimeEvents))]
		public void ShouldDoNothingWhenWrongTypeOfMultiplicatorDefinitionSet(MultiplicatorDefinitionSetChangedBase @event)
		{
			@event.MultiplicatorDefinitionSetId = multiplicatorDefinitionSet.Id.GetValueOrDefault();

			_target.Handle((dynamic)@event);

			_analyticsOvertimeRepository.AssertWasNotCalled(r => r.AddOrUpdate(Arg<AnalyticsOvertime>.Is.Anything));
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