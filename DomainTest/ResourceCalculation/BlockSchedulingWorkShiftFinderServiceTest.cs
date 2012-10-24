﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class BlockSchedulingWorkShiftFinderServiceTest
    {
        private IBlockSchedulingWorkShiftFinderService _target;
        private MockRepository _mocks;
        private IWorkShiftCalculator _calculator;
        private IFairnessValueCalculator _fairnessValueCalculator;
        private IActivity _activity;
        private IWorkShift _workShift1;
        private IWorkShift _workShift2;
        private IWorkShift _workShift3;
        private IShiftCategory _category;
    	private ISchedulingOptions _schedulingOptions;
    	private IShiftCategoryFairnessShiftValueCalculator _shiftCategoryFairnessShiftValueCalculator;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _calculator = _mocks.StrictMock<IWorkShiftCalculator>();
            _fairnessValueCalculator = _mocks.StrictMock<IFairnessValueCalculator>();
			_schedulingOptions = new SchedulingOptions();
        	_shiftCategoryFairnessShiftValueCalculator = _mocks.StrictMock<IShiftCategoryFairnessShiftValueCalculator>();
			_target = new BlockSchedulingWorkShiftFinderService(_calculator, _fairnessValueCalculator, _shiftCategoryFairnessShiftValueCalculator);
        }

        [Test]
        public void CanFindHighestShiftValue()
        {
            DateOnly dateOnly = new DateOnly(2009,2,2);
            var shifts = GetCashes();
            ICccTimeZoneInfo cccTimeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));
            foreach (IShiftProjectionCache shiftProjectionCache in shifts)
            {
                shiftProjectionCache.SetDate(dateOnly, cccTimeZoneInfo);
            }
            IFairnessValueResult fairnessValueResult = new FairnessValueResult();
            using (_mocks.Record())
            {
                Expect.Call(_calculator.CalculateShiftValue(shifts[0].MainShiftProjection, null, 1,true, true)).Return(10);
                Expect.Call(_calculator.CalculateShiftValue(shifts[1].MainShiftProjection, null, 1, true, true)).Return(5);
                Expect.Call(_calculator.CalculateShiftValue(shifts[2].MainShiftProjection, null, 1, true, true)).Return(15);
                
                Expect.Call(_fairnessValueCalculator.CalculateFairnessValue(10, 0, 5, fairnessValueResult.FairnessPoints,
																			fairnessValueResult, 15, _schedulingOptions)).Return(10);

                Expect.Call(_fairnessValueCalculator.CalculateFairnessValue(5, 0, 5, fairnessValueResult.FairnessPoints,
																			fairnessValueResult, 15, _schedulingOptions)).Return(5);

                Expect.Call(_fairnessValueCalculator.CalculateFairnessValue(15, 0, 5, fairnessValueResult.FairnessPoints,
																			fairnessValueResult, 15, _schedulingOptions)).Return(15);
            }
           
            using (_mocks.Playback())
            {
				var ret = _target.BestShiftValue(dateOnly, shifts, null, fairnessValueResult, fairnessValueResult, 5, TimeSpan.FromHours(48), false, null, 1, true, true, _schedulingOptions);
                Assert.AreEqual(15,ret.Value );
            }
            
        }

		[Test]
		public void CanFindHighestShiftValueWithFairness()
		{
			DateOnly dateOnly = new DateOnly(2009, 2, 2);
			var shifts = GetCashes();
			ICccTimeZoneInfo cccTimeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));
			foreach (IShiftProjectionCache shiftProjectionCache in shifts)
			{
				shiftProjectionCache.SetDate(dateOnly, cccTimeZoneInfo);
			}
			IFairnessValueResult fairnessValueResult = new FairnessValueResult();
			var shiftCategoryFairnessFactors = _mocks.DynamicMock<IShiftCategoryFairnessFactors>();
			using (_mocks.Record())
			{
				Expect.Call(_calculator.CalculateShiftValue(shifts[0].MainShiftProjection, null, 1, true, true)).Return(10);
				Expect.Call(_calculator.CalculateShiftValue(shifts[1].MainShiftProjection, null, 1, true, true)).Return(5);
				Expect.Call(_calculator.CalculateShiftValue(shifts[2].MainShiftProjection, null, 1, true, true)).Return(15);
				
				Expect.Call(_shiftCategoryFairnessShiftValueCalculator.ModifiedShiftValue(10, 0, 15, _schedulingOptions)).Return(10);
				Expect.Call(_shiftCategoryFairnessShiftValueCalculator.ModifiedShiftValue(5, 0, 15, _schedulingOptions)).Return(5);
				Expect.Call(_shiftCategoryFairnessShiftValueCalculator.ModifiedShiftValue(15, 0, 15, _schedulingOptions)).Return(15);
			}

			using (_mocks.Playback())
			{
				var ret = _target.BestShiftValue(dateOnly, shifts, null, fairnessValueResult, fairnessValueResult, 5, TimeSpan.FromHours(48), true, shiftCategoryFairnessFactors, 1, true, true, _schedulingOptions);
				Assert.AreEqual(15, ret.Value);
			}

		}

        private IList<IShiftProjectionCache> GetCashes()
        {
            var tmpList = GetWorkShifts();
            var retList = new List<IShiftProjectionCache>();
            foreach (IWorkShift shift in tmpList)
            {
                retList.Add(new ShiftProjectionCache(shift));
            }
            return retList;
        }
        private IList<IWorkShift> GetWorkShifts()
        {
            _activity = ActivityFactory.CreateActivity("sd");
            _category = ShiftCategoryFactory.CreateShiftCategory("dv");
            _workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
                                                          _activity, _category);
            _workShift2 = WorkShiftFactory.CreateWorkShift(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0),
                                                          _activity, _category);
            _workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
                                                                      _activity, _category);

            _schedulingOptions.UseCommonActivity = true;
            _schedulingOptions.CommonActivity = _activity;

            return new List<IWorkShift> { _workShift1, _workShift2, _workShift3 };
        }
    }
}
