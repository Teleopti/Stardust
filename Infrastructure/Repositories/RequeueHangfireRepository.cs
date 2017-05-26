using System;
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

		public IList<RequeueCommand> GetRequeueCommands()
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

	public interface IRequeueHangfireRepository
	{
		IList<RequeueCommand> GetRequeueCommands();
		void MarkAsCompleted(RequeueCommand command);
	}

	public class RequeueCommand
	{
		public virtual Guid Id { get; set; }
		public virtual string EventName { get; set; }
		public virtual string HandlerName { get; set; }
		public virtual bool Handled { get; set; }
		public virtual DateTime Timestamp { get; set; }
	}
}