using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.WinCodeTest.Common
{
	[TestFixture]
	public class ScheduleContractTimeCalculatorTest
	{
		private ScheduleContractTimeCalculator _scheduleContractTimeCalculator;
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
			_dateOnlyPeriod = new DateOnlyPeriod(2011, 1, 1, 2011, 1, 1);
			_person = _mockRepository.StrictMock<IPerson>();
			_scheduleDictionary = _mockRepository.StrictMock<IScheduleDictionary>();
			_scheduleContractTimeCalculator = new ScheduleContractTimeCalculator(_scheduleDictionary, _person, _dateOnlyPeriod);
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
	}
}
