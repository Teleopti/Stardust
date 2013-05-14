using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class StandardDeviationSumCalculatorTest
	{
		private MockRepository _mocks;
		private IStandardDeviationSumCalculator _target;
		private IScheduleResultDataExtractorProvider _dataExtractorProvider;
		private OptimizationPreferences _optimizerPreferences;
		private SchedulingOptions _schedulingOptions;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dataExtractorProvider = _mocks.StrictMock<IScheduleResultDataExtractorProvider>();
			_optimizerPreferences = new OptimizationPreferences();
			_schedulingOptions = new SchedulingOptions();
			_target = new StandardDeviationSumCalculator( _dataExtractorProvider);
		}

		[Test]
		public void ShouldCalculateSumOfStandardDeviationsForOpenPeriod()
		{
			var firstDay = new DateOnly(2013, 5, 8);
			var secondDay = new DateOnly(2013, 5, 9);
			var dateOnlyPeriod = new DateOnlyPeriod(firstDay, secondDay);
			var scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var scheduleMatrixPro2 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro1, scheduleMatrixPro2 };
			var dataExtractor1 = _mocks.StrictMock<IScheduleResultDataExtractor>();
			var scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			IList<IScheduleDayPro> periodList1 = new List<IScheduleDayPro>
				{
					scheduleDayPro1
				};

			var scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			IList<IScheduleDayPro> periodList2= new List<IScheduleDayPro>
				{
					scheduleDayPro2
				};
			
			using (_mocks.Record())
			{
				Expect.Call(_dataExtractorProvider.CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(scheduleMatrixPro1,
																											 _schedulingOptions))
					  .Return(dataExtractor1);
				Expect.Call(dataExtractor1.Values()).Return(new List<double?> { 0.2});
				Expect.Call(_dataExtractorProvider.CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(scheduleMatrixPro2,
																											 _schedulingOptions))
					  .Return(dataExtractor1);
				Expect.Call(dataExtractor1.Values()).Return(new List<double?> { 0.3});
				Expect.Call(scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(periodList1));
				Expect.Call(scheduleMatrixPro2.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(periodList2));
				Expect.Call(scheduleDayPro1.Day).Return(firstDay).Repeat.AtLeastOnce();
				Expect.Call(scheduleDayPro2.Day).Return(secondDay).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				var result = _target.Calculate(dateOnlyPeriod, matrixList, _optimizerPreferences, _schedulingOptions);

				Assert.That(result, Is.EqualTo(0.5));
			}
		}
	}
}
