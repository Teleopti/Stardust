using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IPersonScheduleDayReadModelRepository
	{
		IList<PersonScheduleDayReadModel> ReadModelsOnPerson(DateOnly startDate, DateOnly toDate, Guid personId);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		void ClearPeriodForPerson(DateOnlyPeriod period, Guid personId);

		void SaveReadModel(PersonScheduleDayReadModel model);
		bool IsInitialized();
		IList<PersonScheduleDayReadModel> ForTeam(DateTimePeriod period, Guid teamId);
	}

	public class PersonScheduleDayReadModelRepository : IPersonScheduleDayReadModelRepository
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public PersonScheduleDayReadModelRepository(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public IList<PersonScheduleDayReadModel> ForTeam(DateTimePeriod period, Guid teamId)
		{
				return ((NHibernateUnitOfWork)_unitOfWorkFactory.CurrentUnitOfWork()).Session.CreateSQLQuery(
					"SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS Date, ShiftStart, ShiftEnd, Shift FROM ReadModel.PersonScheduleDay WHERE TeamId=:TeamId AND ShiftStart IS NOT NULL AND ShiftStart < :DateEnd AND ShiftEnd > :DateStart")
					.SetGuid("TeamId", teamId)
					.SetDateTime("DateStart", period.StartDateTime)
					.SetDateTime("DateEnd", period.EndDateTime)
					 .SetResultTransformer(Transformers.AliasToBean(typeof(PersonScheduleDayReadModel)))
					 .SetReadOnly(true)
					 .List<PersonScheduleDayReadModel>();
		}

		public IList<PersonScheduleDayReadModel> ReadModelsOnPerson(DateOnly startDate, DateOnly toDate, Guid personId)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
			{
				return ((NHibernateStatelessUnitOfWork)uow).Session.CreateSQLQuery(
					"SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS Date, ShiftStart, ShiftEnd, Shift FROM ReadModel.PersonScheduleDay WHERE PersonId=:personid AND BelongsToDate Between :startdate AND :enddate")
					.SetGuid("personid", personId)
					.SetDateTime("startdate",startDate)
					.SetDateTime("enddate", toDate)
					 .SetResultTransformer(Transformers.AliasToBean(typeof(PersonScheduleDayReadModel)))
					 .SetReadOnly(true)
					 .List<PersonScheduleDayReadModel>();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void ClearPeriodForPerson(DateOnlyPeriod period, Guid personId)
		{
			var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork();
			((NHibernateUnitOfWork)uow).Session.CreateSQLQuery(
				"DELETE FROM ReadModel.PersonScheduleDay WHERE PersonId=:person AND BelongsToDate BETWEEN :StartDate AND :EndDate")
				.SetGuid("person", personId)
				.SetDateTime("StartDate", period.StartDate)
				.SetDateTime("EndDate", period.EndDate)
				.ExecuteUpdate();
			uow.PersistAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SaveReadModel(PersonScheduleDayReadModel model)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				((NHibernateUnitOfWork)uow).Session.CreateSQLQuery(
				"INSERT INTO ReadModel.PersonScheduleDay (Id,PersonId,TeamId,SiteId,BusinessUnitId,ShiftStart,ShiftEnd,BelongsToDate,Shift) VALUES (:Id,:PersonId,:TeamId,:SiteId,:BusinessUnitId,:ShiftStart,:ShiftEnd,:BelongsToDate,:Shift)")
				.SetGuid("Id", Guid.NewGuid())
				.SetGuid("PersonId", model.PersonId)
				.SetGuid("TeamId", model.TeamId)
				.SetGuid("SiteId", model.SiteId)
				.SetGuid("BusinessUnitId", model.BusinessUnitId)
				.SetParameter("ShiftStart", model.ShiftStart)
				.SetParameter("ShiftEnd", model.ShiftEnd)
				.SetDateTime("BelongsToDate", model.BelongsToDate)
				.SetParameter("Shift", model.Shift,NHibernateUtil.StringClob)
				.ExecuteUpdate();

				uow.PersistAll();
			}
		}

		public bool IsInitialized()
		{
			var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork();
			var result = ((NHibernateUnitOfWork)uow).Session.CreateSQLQuery(
				"SELECT TOP 1 * FROM ReadModel.PersonScheduleDay")
				.List();
			return result.Count > 0;
		}
	}

	public class PersonScheduleDayReadModel
	{
		public Guid PersonId { get; set; }
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public DateTime Date { get; set; }
		public DateOnly BelongsToDate { get{return new DateOnly(Date);} }
		public DateTime? ShiftStart { get; set; }
		public DateTime? ShiftEnd { get; set; }
		public string Shift { get; set; }
	}
}