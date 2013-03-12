using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class TeamBlockIntradayDecisionMakerTest
	{
		private MockRepository _mocks;
		private IBlockProvider _blockProvider;
		private IDataExtractorValuesForMatrixes _dataExtractorValuesForMatrixes;
		private TeamBlockIntradayDecisionMaker _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_blockProvider = _mocks.StrictMock<IBlockProvider>();
			_dataExtractorValuesForMatrixes = new DataExtractorValuesForMatrixes();
			_target = new TeamBlockIntradayDecisionMaker(_blockProvider,_dataExtractorValuesForMatrixes);
		}

		[Test]
		public void ShouldDecideABlockWithHighestSum()
		{
			var matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
			var schedulingOptions = new SchedulingOptions();
			var person = PersonFactory.CreatePerson("bill");
			var matrixes = new List<IScheduleMatrixPro> {matrix1};
			var converter1 = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
			var extractor1 = _mocks.StrictMock<IScheduleResultDataExtractor>();
			var date = new DateOnly();
			var selectedPeriod = new DateOnlyPeriod(date, date.AddDays(1));
			var blocks = new List<IBlockInfo>
				{
					new BlockInfo(new DateOnlyPeriod(date, date)),
					new BlockInfo(new DateOnlyPeriod(date.AddDays(1), date.AddDays(1)))
				};
			_dataExtractorValuesForMatrixes.Add(matrix1, new IntradayDecisionMakerComponents(converter1, extractor1));
			using (_mocks.Record())
			{
				Expect.Call(_blockProvider.Provide(selectedPeriod, new List<IPerson>{person}, matrixes, schedulingOptions)).Return(blocks);
				Expect.Call(matrix1.FullWeeksPeriodDays)
				      .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {scheduleDay1, scheduleDay2}))
				      .Repeat.Twice();
				Expect.Call(scheduleDay1.Day).Return(date).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay2.Day).Return(date.AddDays(1)).Repeat.AtLeastOnce();
				Expect.Call(extractor1.Values()).Return(new List<double?> {0.1, 0.2});
			}
			using (_mocks.Playback())
			{
				var result = _target.Decide(selectedPeriod, new List<IPerson>{person}, matrixes, schedulingOptions);

				Assert.That(result.Sum, Is.EqualTo(0.2));
			}
		}
	}
}
