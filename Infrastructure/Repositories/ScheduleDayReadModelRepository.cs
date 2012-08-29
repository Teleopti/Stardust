using System;
using System.Collections.Generic;
using System.Drawing;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ScheduleDayReadModelRepository
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public ScheduleDayReadModelRepository(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}


		public IList<ScheduleDayReadModel> ReadModelsOnPerson(DateOnly startDate, DateOnly toDate, Guid personId)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
			{
				return ((NHibernateStatelessUnitOfWork)uow).Session.CreateSQLQuery(
					"SELECT PersonId, BelongsToDate AS Date, StartDateTime, EndDateTime, WorkDay, Label, DisplayColor AS ColorCode, WorkTime AS WorkTimeTicks, ContractTime AS ContractTimeTicks FROM ReadModel.ScheduleDay WHERE PersonId=:personid AND BelongsToDate Between :startdate AND :enddate")
					.SetGuid("personid", personId)
					.SetDateTime("startdate",startDate)
					.SetDateTime("enddate", toDate)
					 .SetResultTransformer(Transformers.AliasToBean(typeof(ScheduleDayReadModel)))
					 .SetReadOnly(true)
					 .List<ScheduleDayReadModel>();


			}
		}

		public void SaveReadModel(ScheduleDayReadModel model)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				((NHibernateUnitOfWork) uow).Session.CreateSQLQuery(
					"INSERT INTO ReadModel.ScheduleDay (PersonId,BelongsToDate,StartDateTime,EndDateTime,WorkDay,WorkTime,ContractTime,Label,DisplayColor,InsertedOn) VALUES (:PersonId,:Date,:StartDateTime,:EndDateTime,:WorkDay,:WorkTime,:ContractTime,:Label,:DisplayColor,:InsertedOn)")
					.SetGuid("PersonId", model.PersonId)
					.SetDateTime("StartDateTime", model.StartDateTime)
					.SetDateTime("EndDateTime", model.EndDateTime)
					.SetInt64("WorkTime", model.WorkTime.Ticks)
					.SetInt64("ContractTime", model.ContractTime.Ticks)
					.SetBoolean("WorkDay", model.WorkDay)
					.SetString("Label", model.Label)
					.SetInt32("DisplayColor", model.DisplayColor.ToArgb())
					.SetDateTime("Date", model.BelongsToDate)
					.SetDateTime("InsertedOn", DateTime.UtcNow)
					.ExecuteUpdate();
				uow.PersistAll();
			}
		}
	}

	public class ScheduleDayReadModel
	{
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public DateOnly BelongsToDate { get{return new DateOnly(Date);} }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public bool WorkDay { get; set; }
		public string Label { get; set; }
		public int ColorCode { get; set; }
		public Color DisplayColor { get { return Color.FromArgb(ColorCode); } }
		public long WorkTimeTicks { get; set; }
		public TimeSpan WorkTime { get { return TimeSpan.FromTicks(WorkTimeTicks); } }
		public long ContractTimeTicks { get; set; }
		public TimeSpan ContractTime { get { return TimeSpan.FromTicks(ContractTimeTicks); } }
	}
}