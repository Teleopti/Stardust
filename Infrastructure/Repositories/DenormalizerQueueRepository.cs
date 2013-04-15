using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class DenormalizerQueueRepository : IDenormalizerQueueRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public DenormalizerQueueRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IEnumerable<DenormalizerQueueItem> DequeueDenormalizerMessages(IBusinessUnit businessUnit)
		{
			var unitOfWork = _currentUnitOfWork.Current();
			return ((NHibernateUnitOfWork) unitOfWork).Session.CreateSQLQuery(
				"exec [dbo].[DequeueNormalizeMessages] @BusinessUnit=:BusinessUnit")
				.SetGuid("BusinessUnit", businessUnit.Id.GetValueOrDefault())
				.SetResultTransformer(Transformers.AliasToBean(typeof (DenormalizerQueueItem)))
				.SetReadOnly(true)
				.List<DenormalizerQueueItem>();
		}
	}
}