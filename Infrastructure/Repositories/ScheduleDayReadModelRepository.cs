using System;
using System.Collections.Generic;
using System.Drawing;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ScheduleDayReadModelRepository : IScheduleDayReadModelRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public ScheduleDayReadModelRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IList<ScheduleDayReadModel> ReadModelsOnPerson(DateOnly startDate, DateOnly toDate, Guid personId)
		{
			return ((NHibernateUnitOfWork) _currentUnitOfWork.Current()).Session.CreateSQLQuery(
				"SELECT PersonId, BelongsToDate AS Date, StartDateTime, EndDateTime, Workday, Label, DisplayColor AS ColorCode, WorkTime AS WorkTimeTicks, ContractTime AS ContractTimeTicks FROM ReadModel.ScheduleDay WHERE PersonId=:personid AND BelongsToDate Between :startdate AND :enddate")
				.SetGuid("personid", personId)
				.SetDateTime("startdate", startDate)
				.SetDateTime("enddate", toDate)
				.SetResultTransformer(Transformers.AliasToBean(typeof (ScheduleDayReadModel)))
				.SetReadOnly(true)
				.List<ScheduleDayReadModel>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void ClearPeriodForPerson(DateOnlyPeriod period, Guid personId)
		{
			((NHibernateUnitOfWork)_currentUnitOfWork.Current()).Session.CreateSQLQuery(
				"DELETE FROM ReadModel.ScheduleDay WHERE PersonId=:person AND BelongsToDate BETWEEN :StartDate AND :EndDate")
				.SetGuid("person", personId)
				.SetDateTime("StartDate", period.StartDate)
				.SetDateTime("EndDate", period.EndDate)
				.ExecuteUpdate();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SaveReadModel(ScheduleDayReadModel model)
		{
			((NHibernateUnitOfWork)_currentUnitOfWork.Current()).Session.CreateSQLQuery(
					"INSERT INTO ReadModel.ScheduleDay (PersonId,BelongsToDate,StartDateTime,EndDateTime,Workday,WorkTime,ContractTime,Label,DisplayColor,NotScheduled) VALUES (:PersonId,:Date,:StartDateTime,:EndDateTime,:Workday,:WorkTime,:ContractTime,:Label,:DisplayColor,:NotScheduled)")
					.SetGuid("PersonId", model.PersonId)
					.SetDateTime("StartDateTime", model.StartDateTime)
					.SetDateTime("EndDateTime", model.EndDateTime)
					.SetInt64("WorkTime", model.WorkTime.Ticks)
					.SetInt64("ContractTime", model.ContractTime.Ticks)
					.SetBoolean("Workday", model.Workday)
					.SetString("Label", model.Label)
					.SetInt32("DisplayColor", model.DisplayColor.ToArgb())
					.SetDateTime("Date", model.BelongsToDate)
					.SetBoolean("NotScheduled", model.NotScheduled)
					.ExecuteUpdate();
		}

		public bool IsInitialized()
		{
			var result = ((NHibernateUnitOfWork)_currentUnitOfWork.Current()).Session.CreateSQLQuery(
				"SELECT TOP 1 * FROM ReadModel.ScheduleDay")
				.List();
			return result.Count > 0;
		}
	}

	public class ScheduleDayReadModel
	{
		private string _label;

		public ScheduleDayReadModel()
		{
			StartDateTime = new DateTime(1900,1,1);
			EndDateTime = new DateTime(1900,1,1);
		}
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public DateOnly BelongsToDate { get{return new DateOnly(Date);} }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public bool Workday { get; set; }
		public string Label
		{
			get
			{
				if (_label == null)
					return "";
				return _label;
			}
			set { _label = value; }
		}

		public int ColorCode { get; set; }
		public Color DisplayColor { get { return Color.FromArgb(ColorCode); } }
		public long WorkTimeTicks { get; set; }
		public TimeSpan WorkTime { get { return TimeSpan.FromTicks(WorkTimeTicks); } }
		public long ContractTimeTicks { get; set; }
		public TimeSpan ContractTime { get { return TimeSpan.FromTicks(ContractTimeTicks); } }

		public bool NotScheduled { get; set; }
	}

}