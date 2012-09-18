using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class TeamSteadyStateDictionaryCreatorTest
	{
		private TeamSteadyStateDictionaryCreator _target;
		private MockRepository _mocks;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new TeamSteadyStateDictionaryCreator();	
		}

		[Test]
		public void ShouldCreateDictionary()
		{
			using(_mocks.Record())
			{
				
			}

			using(_mocks.Playback())
			{
				_target.Create();
			}
		}
	}
}
