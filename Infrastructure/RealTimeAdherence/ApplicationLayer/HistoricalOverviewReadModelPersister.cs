using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence.ApplicationLayer
{
	public class HistoricalOverviewReadModelPersister : IHistoricalOverviewReadModelPersister, IHistoricalOverviewReadModelReader
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public HistoricalOverviewReadModelPersister(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Upsert(HistoricalOverviewReadModel model)
		{
			var date = model.Date.Date == DateTime.MinValue ? new DateTime(1900, 1, 1) : model.Date.Date;
			var updated = _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
UPDATE [ReadModel].[HistoricalOverview] 
	SET 
		Adherence = :Adherence, 
		WasLateForWork = :WasLateForWork, 
		MinutesLateForWork = :MinutesLateForWork, 
		ShiftLength = :ShiftLength
	WHERE 
		PersonId = :PersonId AND 
		[Date] = :Date 
")
				.SetParameter("PersonId", model.PersonId)
				.SetParameter("Adherence", model.Adherence)
				.SetParameter("WasLateForWork", model.WasLateForWork)
				.SetParameter("MinutesLateForWork", model.MinutesLateForWork)
				.SetParameter("ShiftLength", model.ShiftLength)
				.SetParameter("Date", date)
				.ExecuteUpdate();
			if (updated == 0)
			{
				_unitOfWork.Current().Session()
					.CreateSQLQuery(@"
INSERT INTO [ReadModel].[HistoricalOverview] 
	(
		[PersonId], 
		[Date], 
		[Adherence], 
		[WasLateForWork], 
		[MinutesLateForWork], 
		[ShiftLength]  
	) 
VALUES 
	(
		:PersonId, 
		:Date, 
		:Adherence, 
		:WasLateForWork, 
		:MinutesLateForWork, 
		:ShiftLength
	)
")
				.SetParameter("PersonId", model.PersonId)
				.SetParameter("Adherence", model.Adherence)
				.SetParameter("WasLateForWork", model.WasLateForWork)
				.SetParameter("MinutesLateForWork", model.MinutesLateForWork)
				.SetParameter("ShiftLength", model.ShiftLength)
				.SetParameter("Date", date)
				.ExecuteUpdate();
			}
		}

		public IEnumerable<HistoricalOverviewReadModel> Read(IEnumerable<Guid> personIds)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
SELECT 
	[PersonId], 
	[Date], 
	[Adherence], 
	[WasLateForWork], 
	[MinutesLateForWork], 
	[ShiftLength]
FROM [ReadModel].[HistoricalOverview]
WHERE PersonId IN (:PersonId)
")
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetParameterList("PersonId", personIds)
				.List()
				.Cast<HistoricalOverviewReadModel>();
		}

		class internalModel : HistoricalOverviewReadModel
		{
			public new DateTime Date
			{
				set { base.Date = new DateOnly(value); }
			}
		}
	}
}