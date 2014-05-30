using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftCalculation
{
	[TestFixture]
	public class WorkShiftSelectorTest
	{
		private MockRepository _mocks;
		private IWorkShiftSelector _target;
		private IWorkShiftValueCalculator _workShiftValueCalculator;
		private IList<IShiftProjectionCache> _shiftProjectionCaches;
		private IShiftProjectionCache _shiftProjectionCache1;
		private IShiftProjectionCache _shiftProjectionCache2;
		private IDictionary<DateTime, ISkillIntervalData> _skillIntervalDatas;
		private IActivity _activity;
		private IDictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>> _skillIntervalDataForActivity;
		private IVisualLayerCollection _visualLayerCollection;
	   private IEqualWorkShiftValueDecider _equalWorkShiftValueDecider;
		private PeriodValueCalculationParameters _periodValueCalculationParameters;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_workShiftValueCalculator = _mocks.StrictMock<IWorkShiftValueCalculator>();
		    _equalWorkShiftValueDecider = _mocks.StrictMock<IEqualWorkShiftValueDecider>();
            _target = new WorkShiftSelector(_workShiftValueCalculator, _equalWorkShiftValueDecider);
			_shiftProjectionCache1 = _mocks.StrictMock<IShiftProjectionCache>();
			_shiftProjectionCache2 = _mocks.StrictMock<IShiftProjectionCache>();
			_shiftProjectionCaches = new List<IShiftProjectionCache> {_shiftProjectionCache1};
			_skillIntervalDatas = new Dictionary<DateTime, ISkillIntervalData>();
			_activity = new Activity("hej");
			_skillIntervalDataForActivity = new Dictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>>();
			_skillIntervalDataForActivity.Add(_activity, _skillIntervalDatas);
			_visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
			_periodValueCalculationParameters = new PeriodValueCalculationParameters(WorkShiftLengthHintOption.AverageWorkTime,
													 false, false, MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak,false);
		}


		[Test]
		public void ShouldCalculateAndReturn()
		{

			using (_mocks.Record())
			{
				Expect.Call(_shiftProjectionCache1.MainShiftProjection).Return(_visualLayerCollection);
				
				Expect.Call(_workShiftValueCalculator.CalculateShiftValue(_visualLayerCollection, _activity,
																		  _skillIntervalDataForActivity[_activity], _periodValueCalculationParameters, TimeZoneInfo.Utc )).Return(7);
			}

			IShiftProjectionCache result;

			using (_mocks.Playback())
			{
				result = _target.SelectShiftProjectionCache(_shiftProjectionCaches, _skillIntervalDataForActivity, _periodValueCalculationParameters, TimeZoneInfo.Utc);
			}

			Assert.IsNotNull(result);
		}

		[Test]
		public void ShouldCallEqualWorkShiftValueDeciderIfTwoShiftsHasTheSameValue()
		{
			_shiftProjectionCaches = new List<IShiftProjectionCache> { _shiftProjectionCache2, _shiftProjectionCache2 };

			using (_mocks.Record())
			{
				Expect.Call(_shiftProjectionCache2.MainShiftProjection).Return(_visualLayerCollection);
				Expect.Call(_workShiftValueCalculator.CalculateShiftValue(_visualLayerCollection, _activity,
																		  _skillIntervalDataForActivity[_activity], _periodValueCalculationParameters, TimeZoneInfo.Utc)).Return(7);

				Expect.Call(_shiftProjectionCache2.MainShiftProjection).Return(_visualLayerCollection);
				Expect.Call(_workShiftValueCalculator.CalculateShiftValue(_visualLayerCollection, _activity,
																		  _skillIntervalDataForActivity[_activity], _periodValueCalculationParameters, TimeZoneInfo.Utc)).Return(7);

				Expect.Call(_equalWorkShiftValueDecider.Decide(_shiftProjectionCache2, _shiftProjectionCache2))
				      .Return(_shiftProjectionCache2);
			}

			IShiftProjectionCache result;

			using (_mocks.Playback())
			{
				result = _target.SelectShiftProjectionCache(_shiftProjectionCaches, _skillIntervalDataForActivity, _periodValueCalculationParameters, TimeZoneInfo.Utc);
			}

			Assert.AreSame(_shiftProjectionCache2, result);
		}

		[Test]
		public void ShouldReturnCacheWithHighestValue()
		{
			_shiftProjectionCaches = new List<IShiftProjectionCache> { _shiftProjectionCache1, _shiftProjectionCache2, _shiftProjectionCache1 };

			using (_mocks.Record())
			{
				Expect.Call(_shiftProjectionCache1.MainShiftProjection).Return(_visualLayerCollection);
				Expect.Call(_workShiftValueCalculator.CalculateShiftValue(_visualLayerCollection, _activity,
																		  _skillIntervalDataForActivity[_activity], _periodValueCalculationParameters, TimeZoneInfo.Utc)).Return(7);

				Expect.Call(_shiftProjectionCache2.MainShiftProjection).Return(_visualLayerCollection);
				Expect.Call(_workShiftValueCalculator.CalculateShiftValue(_visualLayerCollection, _activity,
																		  _skillIntervalDataForActivity[_activity], _periodValueCalculationParameters, TimeZoneInfo.Utc)).Return(8);

				Expect.Call(_shiftProjectionCache1.MainShiftProjection).Return(_visualLayerCollection);
				Expect.Call(_workShiftValueCalculator.CalculateShiftValue(_visualLayerCollection, _activity,
																		  _skillIntervalDataForActivity[_activity], _periodValueCalculationParameters, TimeZoneInfo.Utc)).Return(7);
			}

			IShiftProjectionCache result;

			using (_mocks.Playback())
			{
				result = _target.SelectShiftProjectionCache(_shiftProjectionCaches, _skillIntervalDataForActivity, _periodValueCalculationParameters, TimeZoneInfo.Utc);
			}

			Assert.AreSame(_shiftProjectionCache2, result);
		}

		[Test]
		public void ShouldCheckAllActivities()
		{
			IActivity otherActivity = new Activity("other");
			_skillIntervalDataForActivity.Add(otherActivity, _skillIntervalDatas);

			using (_mocks.Record())
			{
				Expect.Call(_shiftProjectionCache1.MainShiftProjection).Return(_visualLayerCollection);
				Expect.Call(_workShiftValueCalculator.CalculateShiftValue(_visualLayerCollection, _activity,
																		  _skillIntervalDataForActivity[_activity], _periodValueCalculationParameters, TimeZoneInfo.Utc)).Return(2);

				Expect.Call(_shiftProjectionCache1.MainShiftProjection).Return(_visualLayerCollection);
				Expect.Call(_workShiftValueCalculator.CalculateShiftValue(_visualLayerCollection, otherActivity,
																		  _skillIntervalDataForActivity[_activity], _periodValueCalculationParameters, TimeZoneInfo.Utc)).Return(3);
			}

			IShiftProjectionCache result;

			using (_mocks.Playback())
			{
				result = _target.SelectShiftProjectionCache(_shiftProjectionCaches, _skillIntervalDataForActivity, _periodValueCalculationParameters, TimeZoneInfo.Utc);
			}

			Assert.IsNotNull(result);
		}

		[Test]
		public void ShouldReturnNullIfNoShiftValueOnAnySkill()
		{
			IActivity otherActivity = new Activity("other");
			_skillIntervalDataForActivity.Add(otherActivity, _skillIntervalDatas);

			using (_mocks.Record())
			{
				Expect.Call(_shiftProjectionCache1.MainShiftProjection).Return(_visualLayerCollection);
				Expect.Call(_workShiftValueCalculator.CalculateShiftValue(_visualLayerCollection, _activity,
																		  _skillIntervalDataForActivity[_activity], _periodValueCalculationParameters, TimeZoneInfo.Utc)).Return(7);

				Expect.Call(_shiftProjectionCache1.MainShiftProjection).Return(_visualLayerCollection);
				Expect.Call(_workShiftValueCalculator.CalculateShiftValue(_visualLayerCollection, otherActivity,
																		  _skillIntervalDataForActivity[_activity], _periodValueCalculationParameters, TimeZoneInfo.Utc)).Return(null);
			}

			IShiftProjectionCache result;

			using (_mocks.Playback())
			{
				result = _target.SelectShiftProjectionCache(_shiftProjectionCaches, _skillIntervalDataForActivity,_periodValueCalculationParameters, TimeZoneInfo.Utc);
			}

			Assert.IsNull(result);
		}
	}
}