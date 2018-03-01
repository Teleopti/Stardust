using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence.ApplicationLayer
{
	public class ExternalLogonReadModelPersister : IExternalLogonReader, IExternalLogonReadModelPersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public ExternalLogonReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<ExternalLogonReadModel> Read()
		{
			return _unitOfWork.Current()
				.CreateSqlQuery("SELECT * FROM ReadModel.ExternalLogon WHERE Added = 0")
				.SetResultTransformer(Transformers.AliasToBean<ExternalLogonReadModel>())
				.SetReadOnly(true)
				.List<ExternalLogonReadModel>();
		}

		public void Delete(Guid personId)
		{
			_unitOfWork.Current()
				.CreateSqlQuery("UPDATE ReadModel.ExternalLogon SET Deleted = 1 WHERE Personid = :PersonId")
				.SetParameter("PersonId", personId)
				.ExecuteUpdate();
		}

		public void Add(ExternalLogonReadModel model)
		{
			_unitOfWork.Current()
				.CreateSqlQuery("INSERT INTO ReadModel.ExternalLogon VALUES (:PersonId, :DataSourceId, :UserCode, 0, 1)")
				.SetParameter("PersonId", model.PersonId)
				.SetParameter("DataSourceId", model.DataSourceId)
				.SetParameter("UserCode", model.UserCode)
				.ExecuteUpdate();
		}

		public void Refresh()
		{
			_unitOfWork.Current()
				.CreateSqlQuery("DELETE ReadModel.ExternalLogon WHERE Deleted = 1")
				.ExecuteUpdate();
			_unitOfWork.Current()
				.CreateSqlQuery("UPDATE ReadModel.ExternalLogon SET Added = 0 WHERE Added = 1")
				.ExecuteUpdate();
		}
	}
}