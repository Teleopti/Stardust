using System.Linq;
using NHibernate;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Rta
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

		public void Enqueue(BatchInputModel model)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"INSERT INTO RTA.StateQueue VALUES (:model)")
				.SetParameter("model", _serializer.SerializeObject(model), NHibernateUtil.StringClob)
				.ExecuteUpdate();
		}

		public BatchInputModel Dequeue()
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"DELETE TOP (1) FROM RTA.StateQueue OUTPUT DELETED.Model")
				.List<string>()
				.Select(x => _deserializer.DeserializeObject<BatchInputModel>(x))
				.SingleOrDefault();
		}
	}
}