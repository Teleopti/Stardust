using NHibernate.Transform;
using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Schedule
{
	public class ASMScheduleChangeTimeRepository : IASMScheduleChangeTimeRepository
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly INow _now;
		public ASMScheduleChangeTimeRepository(ICurrentUnitOfWork unitOfWork, INow now)
		{
			_unitOfWork = unitOfWork;
			_now = now;
		}
		public ASMScheduleChangeTime GetScheduleChangeTime(Guid personId)
		{
			return _unitOfWork.Current().Session().CreateSQLQuery($@"
						SELECT [PersonId], [TimeStamp] FROM [ReadModel].ASMScheduleChangeTime 
						WHERE PersonId = :{nameof(ASMScheduleChangeTime.PersonId)}
					")
				.SetGuid(nameof(ASMScheduleChangeTime.PersonId), personId)
				.SetResultTransformer(Transformers.AliasToBean<ASMScheduleChangeTime>())
				.UniqueResult<ASMScheduleChangeTime>();
		}

		public void Save(ASMScheduleChangeTime time) {

			_unitOfWork.Current().Session().CreateSQLQuery($@"
			MERGE INTO[ReadModel].[ASMScheduleChangeTime] AS T
				USING ( VALUES (:{nameof(ASMScheduleChangeTime.PersonId)}))
				AS S({nameof(ASMScheduleChangeTime.PersonId)})
				ON T.PersonId = S.PersonId
				WHEN NOT MATCHED THEN
					INSERT
					(
						{nameof(ASMScheduleChangeTime.PersonId)},
						{nameof(ASMScheduleChangeTime.TimeStamp)}
					) VALUES (
						S.{nameof(ASMScheduleChangeTime.PersonId)},
							:{nameof(ASMScheduleChangeTime.TimeStamp)}
					)
				WHEN MATCHED THEN
					UPDATE SET
						{nameof(ASMScheduleChangeTime.TimeStamp)} = :{nameof(ASMScheduleChangeTime.TimeStamp)}
					;")
				.SetGuid(nameof(ASMScheduleChangeTime.PersonId), time.PersonId)
				.SetDateTime(nameof(ASMScheduleChangeTime.TimeStamp), time.TimeStamp)
				.ExecuteUpdate();
			
	}
	}
}
