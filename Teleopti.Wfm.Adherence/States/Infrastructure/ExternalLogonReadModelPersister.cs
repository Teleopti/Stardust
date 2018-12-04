using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;

namespace Teleopti.Wfm.Adherence.States.Infrastructure
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
				.CreateSqlQuery("SELECT PersonId, DataSourceId, UserCode, TimeZone, Deleted, Added FROM ReadModel.ExternalLogon WHERE Added = 0")
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
				.CreateSqlQuery("INSERT INTO ReadModel.ExternalLogon (PersonId, DataSourceId, UserCode, TimeZone, Deleted, Added) VALUES (:PersonId, :DataSourceId, :UserCode, :TimeZone, 0, 1)")
				.SetParameter("PersonId", model.PersonId)
				.SetParameter("DataSourceId", model.DataSourceId)
				.SetParameter("UserCode", model.UserCode)
				.SetParameter("TimeZone", model.TimeZone)
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