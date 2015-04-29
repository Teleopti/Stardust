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
	public class ShiftCategoryPeriodRemoverTest
	{
		private ShiftCategoryPeriodRemover _target;
		private MockRepository _mock;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IScheduleDayPro _scheduleDayPro;
		private IList<IScheduleDayPro> _scheduleDayPros;
		private IScheduleMatrixValueCalculatorPro _scheduleMatrixValueCalculatorPro;
		private ITeamBlockRemoveShiftCategoryOnBestDateService _teamBlockRemoveShiftCategoryOnBestDateService;
		private DateOnly _dateOnly;
		private DateOnlyPeriod _dateOnlyPeriod;
		private ShiftCategory _shiftCategory;
		private ShiftCategoryLimitation _shiftCategoryLimitation;
		private ISchedulingOptions _schedulingOptions;


		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPros = new List<IScheduleDayPro> {_scheduleDayPro};
			_scheduleMatrixValueCalculatorPro = _mock.StrictMock<IScheduleMatrixValueCalculatorPro>();
			_teamBlockRemoveShiftCategoryOnBestDateService = _mock.StrictMock<ITeamBlockRemoveShiftCategoryOnBestDateService>();
			_dateOnly = new DateOnly(2015, 1, 1);
			_dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			_shiftCategory = new ShiftCategory("test");
			_shiftCategory.SetId(new Guid());
			_shiftCategoryLimitation = new ShiftCategoryLimitation(_shiftCategory);
			_shiftCategoryLimitation.MaxNumberOf = 0;
			_schedulingOptions = new SchedulingOptions();
			_target = new ShiftCategoryPeriodRemover(_teamBlockRemoveShiftCategoryOnBestDateService);
		}


		[Test]
		public void ShouldRemove()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleMatrixPro.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(_scheduleDayPros)).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro.Day).Return(_dateOnly).Repeat.AtLeastOnce();

				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro, _shiftCategory)).Return(true);
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.Execute(_shiftCategory, _schedulingOptions, _scheduleMatrixValueCalculatorPro, _scheduleMatrixPro, _dateOnlyPeriod)).Return(_scheduleDayPro);
				Expect.Call(_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(_scheduleDayPro, _shiftCategory)).Return(false);
			}

			using (_mock.Playback())
			{
				var result =_target.RemoveShiftCategoryOnPeriod(_shiftCategoryLimitation, _schedulingOptions, _scheduleMatrixValueCalculatorPro, _scheduleMatrixPro);
				Assert.AreEqual(result[0], _scheduleDayPro);
				Assert.AreEqual(1, _schedulingOptions.NotAllowedShiftCategories.Count);
				Assert.AreEqual(_shiftCategory, _schedulingOptions.NotAllowedShiftCategories[0]);
			}
		}
	}
}
