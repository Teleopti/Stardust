using NHibernate;
using NHibernate.Transform;
using System;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Schedule
{
	public class ASMScheduleChangeTimeRepository : IASMScheduleChangeTimeRepository
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		public ASMScheduleChangeTimeRepository(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public ASMScheduleChangeTime GetScheduleChangeTime(Guid personId)
		{
			using (var session = openSession())
			{
				return session.CreateSQLQuery($@"
						SELECT [PersonId], [TimeStamp] FROM [ReadModel].ASMScheduleChangeTime 
						WHERE PersonId = :{nameof(ASMScheduleChangeTime.PersonId)}
					")
				.SetGuid(nameof(ASMScheduleChangeTime.PersonId), personId)
				.SetResultTransformer(Transformers.AliasToBean<ASMScheduleChangeTime>())
				.UniqueResult<ASMScheduleChangeTime>();
			}
		}

		public void Save(params ASMScheduleChangeTime[] times)
		{
			using (var session = openSession())
			{
				using (var transaction = session.BeginTransaction())
				{
					foreach (var time in times)
					{
						session.CreateSQLQuery($@"
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
					transaction.Commit();
				}
			}

		}

		private IStatelessSession openSession()
		{
			return _unitOfWork.Current().Session().SessionFactory.OpenStatelessSession();
		}
	}
}
