using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class LogCreateIslandsCallback : ICreateIslandsCallback
	{
		private readonly Stopwatch _stopwatch;
		
		public LogCreateIslandsCallback()
		{
			_stopwatch = new Stopwatch();
			_stopwatch.Start();
		}
		
		void ICreateIslandsCallback.BasicIslandsCreated(IEnumerable<IEnumerable<SkillSet>> basicIslands, IDictionary<ISkill, int> noAgentsKnowingSkill)
		{
			if (IslandsBasic == null)
			{
				var islands = basicIslands.Select(skillSetInIsland => new Island(skillSetInIsland, noAgentsKnowingSkill));
				IslandsBasic = new LogCreateIslandInfo(islands, _stopwatch.Elapsed);
			}
		}

		void ICreateIslandsCallback.AfterExtendingDueToReducing(IEnumerable<Island> islands)
		{
			var timeToReduce = _stopwatch.Elapsed - IslandsBasic.TimeToGenerate;
			IslandsAfterReducing = new LogCreateIslandInfo(islands, timeToReduce);
		}
		
		public LogCreateIslandInfo IslandsBasic { get; private set; }
		public LogCreateIslandInfo IslandsAfterReducing { get; private set; }
	//	public LogCreateIslandInfo IslandsComplete { get; private set; }
	}
}