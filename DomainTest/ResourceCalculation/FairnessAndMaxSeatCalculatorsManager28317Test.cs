using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class FairnessAndMaxSeatCalculatorsManager28317Test
	{

		private MockRepository _mock;
		private IFairnessAndMaxSeatCalculatorsManager _target;
		private ISeatLimitationWorkShiftCalculator2 _seatLimitationWorkShiftCalculator;
		private IPerson _person;
		private IShiftProjectionCache _shiftProjectionCache;
		private IDictionary<ISkill, ISkillStaffPeriodDictionary> _maxSeatSkillPeriodsDictionary;
		private IList<IWorkShiftCalculationResultHolder> _allValues;
		private DateOnly _dateOnly;
		private SchedulingOptions _options;
		private IShiftCategory _shiftCategory;
		private IVisualLayerCollection _visualLayerCollection;


		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_seatLimitationWorkShiftCalculator = _mock.StrictMock<ISeatLimitationWorkShiftCalculator2>();
			_target = new FairnessAndMaxSeatCalculatorsManager28317(_seatLimitationWorkShiftCalculator);
			_person = _mock.StrictMock<IPerson>();
			_shiftProjectionCache = _mock.StrictMock<IShiftProjectionCache>();
			_allValues = new List<IWorkShiftCalculationResultHolder>
			{
				new WorkShiftCalculationResult {Value = 1, ShiftProjection = _shiftProjectionCache}
			};
			_maxSeatSkillPeriodsDictionary = _mock.StrictMock<IDictionary<ISkill, ISkillStaffPeriodDictionary>>();
			_dateOnly = new DateOnly(2011, 4, 28);
			_options = new SchedulingOptions
			{
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime,
				UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak

			};
			_shiftCategory = new ShiftCategory("test");
			_visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();

		}

		[Test]
		public void ShouldOnlyUseEqualNumberOfShiftCategory()
		{
			

			using (_mock.Record())
			{
				Expect.Call(_shiftProjectionCache.MainShiftProjection).Return(_visualLayerCollection);
				Expect.Call(_seatLimitationWorkShiftCalculator.CalculateShiftValue(_person, _visualLayerCollection,
					_maxSeatSkillPeriodsDictionary, _options.UserOptionMaxSeatsFeature)).Return(2);
			}
			using (_mock.Playback())
			{
				IList<IWorkShiftCalculationResultHolder> result = _target.RecalculateFoundValues(_allValues, 3,
					_person, _dateOnly, _maxSeatSkillPeriodsDictionary, TimeSpan.FromHours(8), _options);
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(3, result[0].Value);
			}

		}

	}
}