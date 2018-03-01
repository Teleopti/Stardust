using System.Linq;
using NHibernate;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain.Service
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
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"DELETE TOP (1) FROM RTA.StateQueue OUTPUT DELETED.Model")
				.List<string>()
				.Select(x => _deserializer.DeserializeObject<BatchInputModel>(x))
				.SingleOrDefault();
		}
	}
}