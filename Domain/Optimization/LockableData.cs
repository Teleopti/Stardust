using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface ILockableData
	{
		IDictionary<IScheduleMatrixPro, IIntradayDecisionMakerComponents> Data { get; }
		void Add(IScheduleMatrixPro scheduleMatrixPro, IntradayDecisionMakerComponents intradayIntradayDecisionMakerComponents);
	}
	
	public class LockableData : ILockableData
	{
		public IDictionary<IScheduleMatrixPro, IIntradayDecisionMakerComponents> Data { get; private set; }

		public LockableData()
		{
			Data = new Dictionary<IScheduleMatrixPro, IIntradayDecisionMakerComponents>();
		}

		public void Add(IScheduleMatrixPro scheduleMatrixPro, IntradayDecisionMakerComponents intradayIntradayDecisionMakerComponents)
		{
			Data.Add(scheduleMatrixPro, intradayIntradayDecisionMakerComponents);
		}
	}
}