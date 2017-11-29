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
						SELECT [PersonId], [TimeStamp] FROM [dbo].ASMScheduleChangeTime 
						WHERE PersonId = :{nameof(ASMScheduleChangeTime.PersonId)}
					")
				.SetGuid(nameof(ASMScheduleChangeTime.PersonId), personId)
				.SetResultTransformer(Transformers.AliasToBean<ASMScheduleChangeTime>())
				.UniqueResult<ASMScheduleChangeTime>();
		}

		public void Add(ASMScheduleChangeTime time)
		{
			_unitOfWork.Current().Session().CreateSQLQuery($@"
						INSERT INTO [dbo].ASMScheduleChangeTime (
					[{nameof(ASMScheduleChangeTime.PersonId)}]
					 ,[{nameof(ASMScheduleChangeTime.TimeStamp)}])
					VALUES 
					(:{nameof(ASMScheduleChangeTime.PersonId)}, 
					:{nameof(ASMScheduleChangeTime.TimeStamp)})")
				.SetGuid(nameof(ASMScheduleChangeTime.PersonId), time.PersonId)
				.SetDateTime(nameof(ASMScheduleChangeTime.TimeStamp), time.TimeStamp)
				.ExecuteUpdate();

		}

		public void Update(ASMScheduleChangeTime time) {
			_unitOfWork.Current().Session().CreateSQLQuery($@"
					UPDATE [dbo].ASMScheduleChangeTime 
						SET TimeStamp = :{nameof(ASMScheduleChangeTime.TimeStamp)}
					WHERE PersonId = :{nameof(ASMScheduleChangeTime.PersonId)}")
				.SetGuid(nameof(ASMScheduleChangeTime.PersonId), time.PersonId)
				.SetParameter(nameof(ASMScheduleChangeTime.TimeStamp), time.TimeStamp)
				.ExecuteUpdate();
		}
	}
}
