using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsDisplayRowColumnMapperTest
	{
		private AgentRestrictionsDisplayRowColumnMapper _mapper;

		[SetUp]
		public void Setup()
		{
			_mapper = new AgentRestrictionsDisplayRowColumnMapper();	
		}

		[Test]
		public void ShouldMapFromIndex()
		{
			Assert.AreEqual(AgentRestrictionDisplayRowColumn.AgentName, _mapper.ColumnFromIndex(0));
			Assert.AreEqual(AgentRestrictionDisplayRowColumn.Warnings, _mapper.ColumnFromIndex(1));
			Assert.AreEqual(AgentRestrictionDisplayRowColumn.Type, _mapper.ColumnFromIndex(2));
			Assert.AreEqual(AgentRestrictionDisplayRowColumn.From, _mapper.ColumnFromIndex(3));
			Assert.AreEqual(AgentRestrictionDisplayRowColumn.To, _mapper.ColumnFromIndex(4));
			Assert.AreEqual(AgentRestrictionDisplayRowColumn.ContractTargetTime, _mapper.ColumnFromIndex(5));
			Assert.AreEqual(AgentRestrictionDisplayRowColumn.DaysOff, _mapper.ColumnFromIndex(6));
			Assert.AreEqual(AgentRestrictionDisplayRowColumn.ContractTime, _mapper.ColumnFromIndex(7));
			Assert.AreEqual(AgentRestrictionDisplayRowColumn.DaysOffSchedule, _mapper.ColumnFromIndex(8));
			Assert.AreEqual(AgentRestrictionDisplayRowColumn.Min, _mapper.ColumnFromIndex(9));
			Assert.AreEqual(AgentRestrictionDisplayRowColumn.Max, _mapper.ColumnFromIndex(10));
			Assert.AreEqual(AgentRestrictionDisplayRowColumn.DaysOffScheduleRestrictions, _mapper.ColumnFromIndex(11));
			Assert.AreEqual(AgentRestrictionDisplayRowColumn.Ok, _mapper.ColumnFromIndex(12));
			Assert.AreEqual(AgentRestrictionDisplayRowColumn.None, _mapper.ColumnFromIndex(13));
		}
	}
}
