using System.Linq;
using NHibernate;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Wfm.Adherence.States.Infrastructure
{
	public class StateQueue : IStateQueueWriter, IStateQueueReader
	{
		private readonly ICurrentAnalyticsUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _serializer;
		private readonly IJsonDeserializer _deserializer;

		public StateQueue(ICurrentAnalyticsUnitOfWork unitOfWork,
			IJsonSerializer serializer,
			IJsonDeserializer deserializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
			_deserializer = deserializer;
		}

		[LogInfo]
		public virtual void Enqueue(BatchInputModel model)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"INSERT INTO RTA.StateQueue VALUES (:model)")
				.SetParameter("model", _serializer.SerializeObject(model), NHibernateUtil.StringClob)
				.ExecuteUpdate();
		}
		
		[LogInfo]
		public virtual BatchInputModel Dequeue()
		{
			//https://www.mssqltips.com/sqlservertip/1257/processing-data-queues-in-sql-server-with-readpast-and-updlock/
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
DELETE TOP (1) 
FROM RTA.StateQueue WITH (READPAST, UPDLOCK, ROWLOCK)
OUTPUT DELETED.Model
")
				.List<string>()
				.Select(x => _deserializer.DeserializeObject<BatchInputModel>(x))
				.SingleOrDefault();
		}

		[LogInfo]
		public virtual int Count()
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"SELECT COUNT(*) FROM RTA.StateQueue WITH (NOLOCK)")
				.UniqueResult<int>();
		}
	}
}