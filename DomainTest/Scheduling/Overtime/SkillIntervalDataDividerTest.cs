﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[TestFixture]
	public class SkillIntervalDataDividerTest
	{
		private MockRepository _mock;
		private ISkillIntervalDataDivider _target;
		private IList<ISkillIntervalData> _skillIntervalDataList;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_target = new SkillIntervalDataDivider();
		}


		[Test]
		public void ShouldTestTheSplitForFifteenMin()
		{
			var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
			_skillIntervalDataList = new List<ISkillIntervalData>();
			_skillIntervalDataList.Add(createSkillIntervalData(0, 30, 0.8));
			_skillIntervalDataList.Add(createSkillIntervalData(30, 60, 0.9));
			_skillIntervalDataList.Add(createSkillIntervalData(60, 90, 0.11));

			using (_mock.Playback())
			{
				var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 15);
				Assert.AreEqual(skillIntervalData.Count(), 6);
				int min = 0;
				foreach (var si in skillIntervalData)
				{
					Assert.AreEqual(si.Period, new DateTimePeriod(startDateTime.AddMinutes(min), startDateTime.AddMinutes(min + 15)));
					min = min + 15;
				}
			}
		}

		[Test]
		public void ShouldTestTheSplitForTenMin()
		{
			var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
			_skillIntervalDataList = new List<ISkillIntervalData>();
			_skillIntervalDataList.Add(createSkillIntervalData(0, 30, 0.8));
			_skillIntervalDataList.Add(createSkillIntervalData(30, 60, 0.9));
			_skillIntervalDataList.Add(createSkillIntervalData(60, 90, 0.11));

			using (_mock.Playback())
			{
				var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 10);
				Assert.AreEqual(skillIntervalData.Count(), 9);
				int min = 0;
				foreach (var si in skillIntervalData)
				{
					Assert.AreEqual(si.Period, new DateTimePeriod(startDateTime.AddMinutes(min), startDateTime.AddMinutes(min + 10)));
					min = min + 10;
				}
			}
		}

		[Test]
		public void ShouldTestTheSplitForFiveMin()
		{
			var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
			_skillIntervalDataList = new List<ISkillIntervalData>();
			_skillIntervalDataList.Add(createSkillIntervalData(0, 30, 0.8));
			_skillIntervalDataList.Add(createSkillIntervalData(30, 60, 0.9));
			_skillIntervalDataList.Add(createSkillIntervalData(60, 90, 0.11));

			using (_mock.Playback())
			{
				var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 5);
				Assert.AreEqual(skillIntervalData.Count(), 18);
				int min = 0;
				foreach (var si in skillIntervalData)
				{
					Assert.AreEqual(si.Period, new DateTimePeriod(startDateTime.AddMinutes(min), startDateTime.AddMinutes(min + 5)));
					min = min + 5;
				}
			}
		}

		[Test]
		public void ShouldTestTheSplitForTenMinResolutionWith15MinInterval()
		{
			var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
			_skillIntervalDataList = new List<ISkillIntervalData>();
			_skillIntervalDataList.Add(createSkillIntervalData(0, 30, 0.8));
			_skillIntervalDataList.Add(createSkillIntervalData(30, 60, 0.9));
			_skillIntervalDataList.Add(createSkillIntervalData(60, 90, 0.11));

			using (_mock.Playback())
			{
				var skillIntervalData = _target.SplitSkillIntervalData(_skillIntervalDataList, 10);
				Assert.AreEqual(skillIntervalData.Count(), 9);
				int min = 0;
				foreach (var si in skillIntervalData)
				{
					Assert.AreEqual(si.Period, new DateTimePeriod(startDateTime.AddMinutes(min), startDateTime.AddMinutes(min + 10)));
					min = min + 10;
				}
			}
		}

		private static SkillIntervalData createSkillIntervalData(int startMin, int endMin, double relativeDifference)
		{
			var startDateTime = new DateTime(2001, 01, 01, 8, 0, 0).ToUniversalTime();
			return new SkillIntervalData(new DateTimePeriod(startDateTime.AddMinutes(startMin), startDateTime.AddMinutes(endMin)), relativeDifference);
		}
	}
}
