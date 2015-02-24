﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{
	[TestFixture]
	public class ScheduleContractTimeCalculatorTest
	{
		private ScheduleContractTimeCalculator _scheduleContractTimeCalculator;
		private ISchedulerStateHolder _schedulerStateHolder;
		private MockRepository _mockRepository;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IPerson _person;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _scheduleRange;
		private IList<IScheduleDay> _scheduleDays;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IProjectionService _projectionService;
		private IVisualLayerCollection _visualLayerCollection;

		[SetUp]
		public void Setup()
		{	
			_mockRepository = new MockRepository();
			_schedulerStateHolder = _mockRepository.StrictMock<ISchedulerStateHolder>();
			_dateOnlyPeriod = new DateOnlyPeriod(2011, 1, 1, 2011, 1, 1);
			_person = _mockRepository.StrictMock<IPerson>();
			_scheduleContractTimeCalculator = new ScheduleContractTimeCalculator(_schedulerStateHolder, _person, _dateOnlyPeriod);
			_scheduleDictionary = _mockRepository.StrictMock<IScheduleDictionary>();
			_scheduleRange = _mockRepository.StrictMock<IScheduleRange>();
			_scheduleDay1 = _mockRepository.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mockRepository.StrictMock<IScheduleDay>();
			_scheduleDays = new List<IScheduleDay>{_scheduleDay1, _scheduleDay2};
			_projectionService = _mockRepository.StrictMock<IProjectionService>();
			_visualLayerCollection = _mockRepository.StrictMock<IVisualLayerCollection>();
		}

		[Test]
		public void ShouldCalculate()
		{
			using(_mockRepository.Record())
			{
				Expect.Call(_schedulerStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDayCollection(_dateOnlyPeriod)).Return(_scheduleDays);
				Expect.Call(_scheduleDay1.ProjectionService()).Return(_projectionService);
				Expect.Call(_scheduleDay2.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.Twice();
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(2)).Repeat.Twice();
			}
			using (_mockRepository.Playback())
			{
				Assert.AreEqual(TimeSpan.FromHours(4),_scheduleContractTimeCalculator.CalculateContractTime());
			}
		}

       
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfStateHolderIsNull()
        {
            _scheduleContractTimeCalculator = new ScheduleContractTimeCalculator(null, _person, _dateOnlyPeriod);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfPersonIsNull()
        {
            _scheduleContractTimeCalculator = new ScheduleContractTimeCalculator(_schedulerStateHolder, null, _dateOnlyPeriod);
        }
	}
}
