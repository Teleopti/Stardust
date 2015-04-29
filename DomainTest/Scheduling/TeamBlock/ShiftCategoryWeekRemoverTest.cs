using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class ShiftCategoryWeekRemoverTest
	{
		private MockRepository _mock;
		private ITeamBlockRemoveShiftCategoryOnBestDateService _teamBlockRemoveShiftCategoryOnBestDateService;
		private IShiftCategoryLimitation _shiftCategoryLimitation;
		private IShiftCategory _shiftCategory;
		private ISchedulingOptions _schedulingOptions;
		private IScheduleMatrixValueCalculatorPro _scheduleMatrixValueCalculatorPro;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayPro2;
		private IScheduleDayPro _scheduleDayPro3;
		private IScheduleDayPro _scheduleDayPro4;
		private IScheduleDayPro _scheduleDayPro5;
		private IScheduleDayPro _scheduleDayPro6;
		private IScheduleDayPro _scheduleDayPro7;
		private IList<IScheduleDayPro> _scheduleDayPros;
		private DateOnly _dateOnly;
		private DateOnlyPeriod _dateOnlyPeriod;
		private ShiftCategoryWeekRemover _target;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamBlockRemoveShiftCategoryOnBestDateService = _mock.StrictMock<ITeamBlockRemoveShiftCategoryOnBestDateService>();
			_shiftCategory = new ShiftCategory("shiftCategory");
			_shiftCategory.SetId(Guid.NewGuid());
			_shiftCategoryLimitation = new ShiftCategoryLimitation(_shiftCategory);
			_schedulingOptions = new SchedulingOptions();
			_scheduleMatrixValueCalculatorPro = _mock.StrictMock<IScheduleMatrixValueCalculatorPro>();
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro3 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro4 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro5 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro6 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro7 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPros = new List<IScheduleDayPro> { _scheduleDayPro1, _scheduleDayPro2, _scheduleDayPro3, _scheduleDayPro4, _scheduleDayPro5, _scheduleDayPro6, _scheduleDayPro7 };
			_dateOnly = new DateOnly(2015, 1, 1);
			_dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(6));
			_target = new ShiftCategoryWeekRemover(_teamBlockRemoveShiftCategoryOnBestDateService);
		}

		[Test]
		public void ShouldRemove()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleMatrixPro.FullWeeksPeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(_scheduleDayPros));
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro1, _shiftCategory)).Return(true);
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro2, _shiftCategory)).Return(false);
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro3, _shiftCategory)).Return(false);
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro4, _shiftCategory)).Return(false);
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro5, _shiftCategory)).Return(false);
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro6, _shiftCategory)).Return(false);
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro7, _shiftCategory)).Return(false);

				Expect.Call(_scheduleDayPro1.Day).Return(_dateOnly).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.Execute(_shiftCategory, _schedulingOptions, _scheduleMatrixValueCalculatorPro, _scheduleMatrixPro, _dateOnlyPeriod)).Return(_scheduleDayPro1);

				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro1, _shiftCategory)).Return(false);
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro2, _shiftCategory)).Return(false);
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro3, _shiftCategory)).Return(false);
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro4, _shiftCategory)).Return(false);
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro5, _shiftCategory)).Return(false);
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro6, _shiftCategory)).Return(false);
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro7, _shiftCategory)).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _target.Remove(_shiftCategoryLimitation, _schedulingOptions, _scheduleMatrixValueCalculatorPro, _scheduleMatrixPro);
				Assert.AreEqual(result[0], _scheduleDayPro1);
				Assert.AreEqual(1,_schedulingOptions.NotAllowedShiftCategories.Count);
				Assert.AreEqual(_shiftCategory, _schedulingOptions.NotAllowedShiftCategories[0]);
			}
		}
	}
}
