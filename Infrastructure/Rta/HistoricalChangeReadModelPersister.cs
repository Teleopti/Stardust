using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class HistoricalChangeReadModelPersister : IHistoricalChangeReadModelPersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public HistoricalChangeReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Persist(HistoricalChangeReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(@"
INSERT INTO [ReadModel].[HistoricalChange] (
	PersonId,
	BelongsToDate,
	Timestamp,
	StateName,
	StateGroupId,
	ActivityName,
	ActivityColor,
	RuleName,
	RuleColor,
	Adherence
) VALUES (
	:PersonId,
	:BelongsToDate,
	:Timestamp,
	:StateName,
	:StateGroupId,
	:ActivityName,
	:ActivityColor,
	:RuleName,
	:RuleColor,
	:Adherence
)")
				.SetParameter("PersonId", model.PersonId)
				.SetParameter("BelongsToDate", model.BelongsToDate?.Date)
				.SetParameter("Timestamp", model.Timestamp)
				.SetParameter("StateName", model.StateName)
				.SetParameter("StateGroupId", model.StateGroupId)
				.SetParameter("ActivityName", model.ActivityName)
				.SetParameter("ActivityColor", model.ActivityColor)
				.SetParameter("RuleName", model.RuleName)
				.SetParameter("RuleColor", model.RuleColor)
				.SetParameter("Adherence", (int?)model.Adherence)
				.ExecuteUpdate();
		}

		public void Remove(DateTime until)
		{
			_unitOfWork.Current()
				   .CreateSqlQuery(@"
DELETE FROM [ReadModel].[HistoricalChange] WHERE Timestamp < :ts")
				   .SetParameter("ts", until)
				   .ExecuteUpdate();

		}
	}
}