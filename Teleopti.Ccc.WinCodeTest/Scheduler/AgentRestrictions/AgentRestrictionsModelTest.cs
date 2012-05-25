using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsModelTest
	{
		private AgentRestrictionsModel _model;
		private AgentRestrictionsDisplayRow _agentRestrictionsDisplayRow;
		private MockRepository _mocks;
		private IScheduleMatrixPro _scheduleMatrixPro;
		//private IList<IPerson> _persons;
		//private IPerson _person;
		//private IPerson _anotherPerson;
		//private ISchedulerStateHolder _stateHolder;
		private IAgentRestrictionsDisplayRowCreator _agentRestrictionsDisplayRowCreator;

		[SetUp]
		public void Setup()
		{
			_model = new AgentRestrictionsModel();
			_mocks = new MockRepository();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_agentRestrictionsDisplayRow = new AgentRestrictionsDisplayRow(_scheduleMatrixPro);
			//_person = _mocks.StrictMock<IPerson>();
			//_anotherPerson = _mocks.StrictMock<IPerson>();
			//_persons = new List<IPerson>{_person, _anotherPerson};
			//_stateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
			_agentRestrictionsDisplayRowCreator = _mocks.StrictMock<IAgentRestrictionsDisplayRowCreator>();
		}

		[Test]
		public void ShouldReturnDisplayRows()
		{
			Assert.IsNotNull(_model.DisplayRows);	
		}

		[Test]
		public void ShouldLoadData()
		{
			using(_mocks.Record())
			{
				Expect.Call(() => _agentRestrictionsDisplayRowCreator.Create());
			}

			using(_mocks.Playback())
			{
				_model.LoadData(_agentRestrictionsDisplayRowCreator);	
			}	 
		}

		[Test]
		public void ShouldGetDisplayRowFromIndex()
		{
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow);
			Assert.AreEqual(_agentRestrictionsDisplayRow, _model.DisplayRowFromRowIndex(2));
		}
	}
}
