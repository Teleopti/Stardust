using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class TeamBlockIntradayDecisionMakerTest
	{
		private MockRepository _mocks;
		private IBlockProvider _blockProvider;
		private ILockableData _lockableData;
		private TeamBlockIntradayDecisionMaker _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_blockProvider = _mocks.StrictMock<IBlockProvider>();
			_lockableData = new LockableData();
			_target = new TeamBlockIntradayDecisionMaker(_blockProvider,_lockableData);
		}

		[Test]
		public void ShouldDecideOneBlock()
		{
			var matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
			var schedulingOptions = new SchedulingOptions();
			var matrixes = new List<IScheduleMatrixPro> {matrix1};
			var converter1 = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
			var extractor1 = _mocks.StrictMock<IScheduleResultDataExtractor>();
			var date = new DateOnly();
			var blocks = new List<IIntradayBlock>
				{
					new IntradayBlock {BlockDays = new List<DateOnly> {date}},
					new IntradayBlock {BlockDays = new List<DateOnly> {date.AddDays(1)}}
				};
			_lockableData.Add(matrix1, new IntradayDecisionMakerComponents(converter1, extractor1));
			using (_mocks.Record())
			{
				Expect.Call(_blockProvider.Provide(matrixes)).Return(blocks);
				Expect.Call(matrix1.FullWeeksPeriodDays)
				      .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {scheduleDay1, scheduleDay2}))
				      .Repeat.Twice();
				Expect.Call(scheduleDay1.Day).Return(date).Repeat.AtLeastOnce();
				Expect.Call(scheduleDay2.Day).Return(date.AddDays(1)).Repeat.AtLeastOnce();
				Expect.Call(extractor1.Values()).Return(new List<double?> {0.1, 0.2});
			}
			using (_mocks.Playback())
			{
				var result = _target.Decide(matrixes, schedulingOptions);

				Assert.That(result.Sum, Is.EqualTo(0.2));
			}
		}
	}
}
