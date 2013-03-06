using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IPersonScheduleDayReadModelRepository
	{
		IEnumerable<PersonScheduleDayReadModel> ForPerson(DateOnly startDate, DateOnly endDate, Guid personId);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date")]
		PersonScheduleDayReadModel ForPerson(DateOnly date, Guid personId);
		IEnumerable<PersonScheduleDayReadModel> ForTeam(DateTimePeriod period, Guid teamId);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		void ClearPeriodForPerson(DateOnlyPeriod period, Guid personId);

		void SaveReadModel(PersonScheduleDayReadModel model);
		bool IsInitialized();
	}

	public class PersonScheduleDayReadModelRepository : IPersonScheduleDayReadModelRepository
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public PersonScheduleDayReadModelRepository(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPerson(DateOnly startDate, DateOnly endDate, Guid personId)
		{
			return _unitOfWork.Session().CreateSQLQuery(
				"SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS Date, ShiftStart, ShiftEnd, Shift FROM ReadModel.PersonScheduleDay WHERE PersonId=:personid AND BelongsToDate Between :startdate AND :enddate")
			                  .SetGuid("personid", personId)
			                  .SetDateTime("startdate", startDate)
			                  .SetDateTime("enddate", endDate)
			                  .SetResultTransformer(Transformers.AliasToBean(typeof (PersonScheduleDayReadModel)))
			                  .SetReadOnly(true)
			                  .List<PersonScheduleDayReadModel>()
				;
		}

		public PersonScheduleDayReadModel ForPerson(DateOnly date, Guid personId)
		{
			return ForPerson(date, date, personId).FirstOrDefault();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForTeam(DateTimePeriod period, Guid teamId)
		{
			return _unitOfWork.Session().CreateSQLQuery(
				"SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS Date, ShiftStart, ShiftEnd, Shift FROM ReadModel.PersonScheduleDay WHERE TeamId=:TeamId AND ShiftStart IS NOT NULL AND ShiftStart < :DateEnd AND ShiftEnd > :DateStart")
				.SetGuid("TeamId", teamId)
				.SetDateTime("DateStart", period.StartDateTime)
				.SetDateTime("DateEnd", period.EndDateTime)
				 .SetResultTransformer(Transformers.AliasToBean(typeof(PersonScheduleDayReadModel)))
				 .SetReadOnly(true)
				 .List<PersonScheduleDayReadModel>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void ClearPeriodForPerson(DateOnlyPeriod period, Guid personId)
		{
			_unitOfWork.Session().CreateSQLQuery(
				"DELETE FROM ReadModel.PersonScheduleDay WHERE PersonId=:person AND BelongsToDate BETWEEN :StartDate AND :EndDate")
				.SetGuid("person", personId)
				.SetDateTime("StartDate", period.StartDate)
				.SetDateTime("EndDate", period.EndDate)
				.ExecuteUpdate();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SaveReadModel(PersonScheduleDayReadModel model)
		{
			_unitOfWork.Session().CreateSQLQuery(
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
		}

		public bool IsInitialized()
		{
			var result = _unitOfWork.Session().CreateSQLQuery(
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