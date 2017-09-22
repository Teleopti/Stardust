using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class RequeueHangfireRepository : IRequeueHangfireRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _currentAnalyticsUnitOfWork;
		private readonly INow _now;

		public RequeueHangfireRepository(ICurrentAnalyticsUnitOfWork currentAnalyticsUnitOfWork, INow now)
		{
			_currentAnalyticsUnitOfWork = currentAnalyticsUnitOfWork;
			_now = now;
		}

		public IList<RequeueCommand> GetUnhandledRequeueCommands()
		{
			return _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery($@"SELECT 
					 [id]  {nameof(RequeueCommand.Id)}
					,[event_name] {nameof(RequeueCommand.EventName)}
					,[handler_name] {nameof(RequeueCommand.HandlerName)}
					,[handled] {nameof(RequeueCommand.Handled)}
					,[timestamp]  {nameof(RequeueCommand.Timestamp)}
				FROM [dbo].[hangfire_requeue] WHERE [handled]=0")
				.SetResultTransformer(Transformers.AliasToBean(typeof(RequeueCommand)))
				.List<RequeueCommand>();
		}

		public void MarkAsCompleted(RequeueCommand command)
		{
			var now = _now.UtcDateTime();
			_currentAnalyticsUnitOfWork.Current().Session()
				.CreateSQLQuery($@"UPDATE [dbo].[hangfire_requeue]
			SET 
				[handled] = :{nameof(command.Handled)}, 
				[timestamp] = :{nameof(now)}
			WHERE 
				[id] = :{nameof(command.Id)}")
				.SetParameter(nameof(now), now)
				.SetParameter(nameof(command.Handled), true)
				.SetParameter(nameof(command.Id), command.Id)
				.ExecuteUpdate();
		}
	}

	
}