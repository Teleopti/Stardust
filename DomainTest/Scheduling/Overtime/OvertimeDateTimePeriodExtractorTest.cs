using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[TestFixture]
	public class OvertimeDateTimePeriodExtractorTest
	{
		private OvertimeDateTimePeriodExtractor _target;
		private DateTimePeriod _shiftPeriod;
		private DateTime _shiftEndingTime;
		private DateTime _shiftStartTime;
		private int _minimumResolution;
		private MinMax<TimeSpan> _overtimeDuration;
		private IVisualLayerCollection _visualLayerCollection;
		private MockRepository _mock;
		private IVisualLayer _visualLayerLast;
		private IVisualLayer _visualLayerFirst;
		private IMultiplicatorDefinitionSet _multiplicatorDefinitionSet;
		private IList<IVisualLayer> _visualLayers;
		private IList<IOvertimeSkillIntervalData> _overtimeSkillIntervalDatas;

		[SetUp]
		public void SetUp()
		{
			_target = new OvertimeDateTimePeriodExtractor();
			_shiftStartTime = new DateTime(2014, 02, 26, 15, 0, 0, DateTimeKind.Utc);
			_shiftEndingTime = new DateTime(2014, 02, 26, 16, 0, 0, DateTimeKind.Utc);
			_shiftPeriod = new DateTimePeriod(_shiftStartTime, _shiftEndingTime);
			_minimumResolution = 15;
			_overtimeDuration = new MinMax<TimeSpan>(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));
			_mock = new MockRepository();
			_visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();

			_multiplicatorDefinitionSet = _mock.StrictMock<IMultiplicatorDefinitionSet>();
			var activity = ActivityFactory.CreateActivity("activity");
			_visualLayerLast = new VisualLayer(activity, new DateTimePeriod(_shiftEndingTime.AddMinutes(-15), _shiftEndingTime),
				activity);
			_visualLayerFirst = new VisualLayer(activity, new DateTimePeriod(_shiftStartTime, _shiftStartTime.AddMinutes(15)),
				activity);
			_visualLayers = new List<IVisualLayer> {_visualLayerFirst, _visualLayerLast};
			var overtimeSkillIntervalData =
				new OvertimeSkillIntervalData(new DateTimePeriod(_shiftStartTime.AddHours(-5), _shiftEndingTime.AddHours(5)), 0, 0);
			_overtimeSkillIntervalDatas = new List<IOvertimeSkillIntervalData> { overtimeSkillIntervalData };
		}

		[Test]
		public void ShouldReturnPeriodBeforeAndAfterWithNoSplit()
		{
			var expectedBefore = new DateTimePeriod(_shiftStartTime.AddMinutes(-15), _shiftStartTime);
			var expectedAfter = new DateTimePeriod(_shiftEndingTime, _shiftEndingTime.AddMinutes(15));
			var specifiedPeriod = new DateTimePeriod(_shiftStartTime.AddHours(-10), _shiftEndingTime.AddHours(10));

			using (_mock.Record())
			{
				Expect.Call(_visualLayerCollection.Period()).Return(_shiftPeriod);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.Extract(_minimumResolution, _overtimeDuration, _visualLayerCollection, specifiedPeriod, _overtimeSkillIntervalDatas);
				Assert.AreEqual(2, result.Count());
				Assert.AreEqual(expectedBefore, result.First());
				Assert.AreEqual(expectedAfter, result.Last());	
			}	
		}

		[Test]
		public void ShouldReturnNoPeriodIfSkillIsClosed()
		{
			var specifiedPeriod = new DateTimePeriod(_shiftStartTime.AddHours(-10), _shiftEndingTime.AddHours(10));

			using (_mock.Record())
			{
				Expect.Call(_visualLayerCollection.Period()).Return(_shiftPeriod);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.Extract(_minimumResolution, _overtimeDuration, _visualLayerCollection, specifiedPeriod, new List<IOvertimeSkillIntervalData>());
				Assert.AreEqual(0, result.Count());
			}
		}

		[Test]
		public void ShouldReturnNoPeriodBeforeWhenFirstLayerIsOvertime()
		{
			var expectedAfter = new DateTimePeriod(_shiftEndingTime, _shiftEndingTime.AddMinutes(15));
			var specifiedPeriod = new DateTimePeriod(_shiftStartTime.AddHours(-10), _shiftEndingTime.AddHours(10));

			((VisualLayer)_visualLayerFirst).DefinitionSet = _multiplicatorDefinitionSet;
			_visualLayers = new List<IVisualLayer> { _visualLayerFirst, _visualLayerLast };

			using (_mock.Record())
			{
				Expect.Call(_visualLayerCollection.Period()).Return(_shiftPeriod);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
				Expect.Call(_multiplicatorDefinitionSet.MultiplicatorType).Return(MultiplicatorType.Overtime);
			}

			using (_mock.Playback())
			{
				var result = _target.Extract(_minimumResolution, _overtimeDuration, _visualLayerCollection, specifiedPeriod, _overtimeSkillIntervalDatas);
				Assert.AreEqual(expectedAfter, result.Single());
			}		
		}

		[Test]
		public void ShouldReturnNoPeriodAfterWhenLastLayerIsOvertime()
		{
			var expectedBefore = new DateTimePeriod(_shiftStartTime.AddMinutes(-15), _shiftStartTime);
			var specifiedPeriod = new DateTimePeriod(_shiftStartTime.AddHours(-10), _shiftEndingTime.AddHours(10));

			((VisualLayer)_visualLayerLast).DefinitionSet = _multiplicatorDefinitionSet;
			_visualLayers = new List<IVisualLayer> { _visualLayerFirst, _visualLayerLast };

			using (_mock.Record())
			{
				Expect.Call(_visualLayerCollection.Period()).Return(_shiftPeriod);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
				Expect.Call(_multiplicatorDefinitionSet.MultiplicatorType).Return(MultiplicatorType.Overtime);
			}

			using (_mock.Playback())
			{
				var result = _target.Extract(_minimumResolution, _overtimeDuration, _visualLayerCollection, specifiedPeriod, _overtimeSkillIntervalDatas);
				Assert.AreEqual(expectedBefore, result.Single());
			}
		}

		[Test]
		public void ShouldReturnNoPeriodBeforeWhenFirstLayerIsAbsence()
		{
			var expectedAfter = new DateTimePeriod(_shiftEndingTime, _shiftEndingTime.AddMinutes(15));
			var specifiedPeriod = new DateTimePeriod(_shiftStartTime.AddHours(-10), _shiftEndingTime.AddHours(10));

			((VisualLayer) _visualLayerFirst).HighestPriorityAbsence = AbsenceFactory.CreateAbsence("absence");
			_visualLayers = new List<IVisualLayer> { _visualLayerFirst, _visualLayerLast };

			using (_mock.Record())
			{
				Expect.Call(_visualLayerCollection.Period()).Return(_shiftPeriod);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.Extract(_minimumResolution, _overtimeDuration, _visualLayerCollection, specifiedPeriod, _overtimeSkillIntervalDatas);
				Assert.AreEqual(expectedAfter, result.Single());
			}		
		}

		[Test]
		public void ShouldReturnNoPeriodAfterWhenLastLayerIsAbsence()
		{
			var expectedBefore = new DateTimePeriod(_shiftStartTime.AddMinutes(-15), _shiftStartTime);
			var specifiedPeriod = new DateTimePeriod(_shiftStartTime.AddHours(-10), _shiftEndingTime.AddHours(10));

			((VisualLayer)_visualLayerLast).HighestPriorityAbsence = AbsenceFactory.CreateAbsence("absence");
			_visualLayers = new List<IVisualLayer> { _visualLayerFirst, _visualLayerLast };

			using (_mock.Record())
			{
				Expect.Call(_visualLayerCollection.Period()).Return(_shiftPeriod);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.Extract(_minimumResolution, _overtimeDuration, _visualLayerCollection, specifiedPeriod, _overtimeSkillIntervalDatas);
				Assert.AreEqual(expectedBefore, result.Single());
			}
		}

		[Test]
		public void ShouldReturnNoPeriodsWhenSpecifiedPeriodDontIntersectWithShift()
		{
			var specifiedPeriod = new DateTimePeriod(_shiftEndingTime.AddMinutes(15), _shiftEndingTime.AddMinutes(30));
			_overtimeDuration = new MinMax<TimeSpan>(specifiedPeriod.ElapsedTime(), specifiedPeriod.ElapsedTime());

			using (_mock.Record())
			{
				Expect.Call(_visualLayerCollection.Period()).Return(_shiftPeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.Extract(_minimumResolution, _overtimeDuration, _visualLayerCollection, specifiedPeriod, new List<IOvertimeSkillIntervalData>());
				Assert.AreEqual(0, result.Count());	
			}	
		}

		[Test]
		public void ShouldReturnPeriodsWhenSpecifiedPeriodContainsThem()
		{
			var specifiedPeriod = new DateTimePeriod(_shiftStartTime.AddMinutes(-30), _shiftEndingTime.AddMinutes(30));
			_overtimeDuration = new MinMax<TimeSpan>(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));
			var expectedBefore = new DateTimePeriod(_shiftStartTime.AddMinutes(-15), _shiftStartTime);
			var expectedAfter = new DateTimePeriod(_shiftEndingTime, _shiftEndingTime.AddMinutes(15));

			using (_mock.Record())
			{
				Expect.Call(_visualLayerCollection.Period()).Return(_shiftPeriod);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.Extract(_minimumResolution, _overtimeDuration, _visualLayerCollection, specifiedPeriod, _overtimeSkillIntervalDatas);
				Assert.AreEqual(2, result.Count());
				Assert.AreEqual(expectedBefore, result.First());
				Assert.AreEqual(expectedAfter, result.Last());	
			}	
		}

		[Test]
		public void ShouldNotReturnPeriodsWhenSpecifiedPeriodDontContainsThem()
		{
			var specifiedPeriod = new DateTimePeriod(_shiftStartTime.AddMinutes(-15), _shiftEndingTime.AddMinutes(15));
			_overtimeDuration = new MinMax<TimeSpan>(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));

			using (_mock.Record())
			{
				Expect.Call(_visualLayerCollection.Period()).Return(_shiftPeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.Extract(_minimumResolution, _overtimeDuration, _visualLayerCollection, specifiedPeriod, new List<IOvertimeSkillIntervalData>());
				Assert.AreEqual(0, result.Count());	
			}	
		}

		[Test]
		public void ShouldReturnPeriodsWhenSpecifiedPeriodIsAdjacentToShift()
		{
			var specifiedPeriod = new DateTimePeriod(_shiftStartTime.AddMinutes(-15), _shiftStartTime);
			_overtimeDuration = new MinMax<TimeSpan>(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));
			var expectedBefore = new DateTimePeriod(_shiftStartTime.AddMinutes(-15), _shiftStartTime);

			using (_mock.Record())
			{
				Expect.Call(_visualLayerCollection.Period()).Return(_shiftPeriod);
				Expect.Call(_visualLayerCollection.GetEnumerator()).Return(_visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.Extract(_minimumResolution, _overtimeDuration, _visualLayerCollection, specifiedPeriod, _overtimeSkillIntervalDatas);
				Assert.AreEqual(expectedBefore, result.Single());
			}
		}
	}
}
