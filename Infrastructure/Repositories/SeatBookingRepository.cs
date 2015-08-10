using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Impl;
using NHibernate.Linq;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SeatBookingRepository : Repository<ISeatBooking>, ISeatBookingRepository
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public SeatBookingRepository(IUnitOfWork unitOfWork)
			: base(unitOfWork)
		{
			_unitOfWork = new FixedCurrentUnitOfWork(unitOfWork);
		}

		public SeatBookingRepository(IUnitOfWorkFactory unitOfWorkFactory)
			: base(unitOfWorkFactory)
		{
		}

		public SeatBookingRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
			_unitOfWork = currentUnitOfWork;
		}

		public ISeatBooking LoadAggregate(Guid id)
		{
			return Session.Query<ISeatBooking>()
				.FirstOrDefault(booking => booking.Id == id);
		}

		public ISeatBooking LoadSeatBookingForPerson(DateOnly date, IPerson person)
		{
			return
				Session.Query<ISeatBooking>()
					.SingleOrDefault(booking => (booking.BelongsToDate == date && booking.Person == person));
		}

		public IList<ISeatBooking> LoadSeatBookingsForDateOnlyPeriod(DateOnlyPeriod period)
		{
			return Session.Query<ISeatBooking>()
				.Where(booking => booking.BelongsToDate >= period.StartDate
								   && booking.BelongsToDate <= period.EndDate).ToList();
		}

		public IList<ISeatBooking> LoadSeatBookingsForDay(DateOnly date)
		{
			return Session.Query<ISeatBooking>()
				.Where(booking => booking.BelongsToDate == date).ToList();
		}

		public IList<ISeatBooking> GetSeatBookingsForSeat(ISeat seat)
		{
			return Session.Query<ISeatBooking>().Where(booking => booking.Seat == seat).ToList();
		}

		public void RemoveSeatBookingsForSeats(IEnumerable<ISeat> seats)
		{
			seats.ForEach(RemoveSeatBookingsForSeat);
		}

		public void RemoveSeatBookingsForSeat(ISeat seat)
		{
			GetSeatBookingsForSeat(seat)
				.ForEach(booking => Session.Delete(booking));
		}

		public ISeatBookingReportModel LoadSeatBookingsReport(ISeatBookingReportCriteria criteria, Paging paging = null)
		{

			var scheduleAndBookingQuery = getScheduleAndBookingInformation(criteria);
			return getResultNew(scheduleAndBookingQuery, paging);
		}

		private static ISeatBookingReportModel getResultNew(IQuery bookingQuery, Paging paging)
		{
			return paging != null
				? getResultWithPaging(bookingQuery, paging)
				: getResultWithoutPaging(bookingQuery);
		}

		private static SeatBookingReportModel getResultWithoutPaging(IQuery bookingQuery)
		{
			var seatBookingReportModel = new SeatBookingReportModel
			{
				SeatBookings = bookingQuery.List<PersonScheduleWithSeatBooking>()
			};
			
			seatBookingReportModel.RecordCount = seatBookingReportModel.SeatBookings.Count();
			return seatBookingReportModel;
		}

		private static SeatBookingReportModel getResultWithPaging(IQuery bookingCriteria, Paging paging)
		{
			var seatBookingReportModel = new SeatBookingReportModel();

			seatBookingReportModel.SeatBookings = bookingCriteria
				.SetFirstResult(paging.Skip)
				.SetMaxResults(paging.Take)
				.List<PersonScheduleWithSeatBooking>();

			var firstBooking = seatBookingReportModel.SeatBookings.FirstOrDefault();

			seatBookingReportModel.RecordCount = firstBooking == null ? 0 : firstBooking.NumberOfRecords; 

			return seatBookingReportModel;
		}

		private IQuery getScheduleAndBookingInformation(ISeatBookingReportCriteria reportCriteria)
		{

			var query =  Session.CreateSQLQuery(
				@"exec dbo.LoadScheduleAndSeatBookingInfo @startDate=:startDate, @endDate =:endDate, 
					@teamIdList=:teamIdList, @locationIdList =:locationIdList, @businessUnitId=:businessUnitId ")
				.SetDateOnly("startDate", reportCriteria.Period.StartDate)
				.SetDateOnly("endDate", reportCriteria.Period.EndDate)
				.SetString("teamIdList", getTeamCriteria(reportCriteria))
				.SetString("locationIdList", getLocationCriteria(reportCriteria))
				.SetGuid("businessUnitId", getBusinessUnitId())
				.SetResultTransformer(Transformers.AliasToBean(typeof(PersonScheduleWithSeatBooking)))
				.SetReadOnly(true);

			return query;

		}

		private Guid getBusinessUnitId()
		{
			var filter = (FilterImpl)_unitOfWork.Session().GetEnabledFilter("businessUnitFilter");
			object businessUnitId;
			if (!filter.Parameters.TryGetValue("businessUnitParameter", out businessUnitId))
			{
				businessUnitId = ((ITeleoptiIdentity)ClaimsPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault();
			}
			return Guid.Parse(businessUnitId.ToString());
		}

		private static string getLocationCriteria(ISeatBookingReportCriteria reportCriteria)
		{
			return reportCriteria.Locations.IsNullOrEmpty() ? null : string.Join(",", reportCriteria.Locations.Select(location => location.Id));
		}

		private static string getTeamCriteria(ISeatBookingReportCriteria reportCriteria)
		{
			return reportCriteria.Teams.IsNullOrEmpty() ? null : string.Join(",", reportCriteria.Teams.Select(team => team.Id));
		}

		#region old code


		private ICriteria createSeatBookingCriteria(ISeatBookingReportCriteria criteria)
		{
			var seatBookingCriteria = applyBasicReportCriteria(criteria);
			applyAdditionalCriteria(criteria, seatBookingCriteria);
			return seatBookingCriteria;
		}

		private ICriteria applyBasicReportCriteria(ISeatBookingReportCriteria criteria)
		{
			var seatBookingCriteria = Session.CreateCriteria<SeatBooking>()
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.Add(Restrictions.Between("BelongsToDate", criteria.Period.StartDate, criteria.Period.EndDate))
				.AddOrder(Order.Asc("StartDateTime"));

			return seatBookingCriteria;
		}

		private IFutureValue<int> createRowCountCriteria(ISeatBookingReportCriteria criteria)
		{
			var rowCountCritiera = applyBasicRowCountCriteria(criteria);
			applyAdditionalCriteria(criteria, rowCountCritiera);
			return rowCountCritiera.FutureValue<int>();
		}

		private ICriteria applyBasicRowCountCriteria(ISeatBookingReportCriteria criteria)
		{
			return Session.CreateCriteria<SeatBooking>()
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.Add(Restrictions.Between("BelongsToDate", criteria.Period.StartDate, criteria.Period.EndDate))
				.SetProjection(Projections.RowCount());
		}

		private static void applyAdditionalCriteria(ISeatBookingReportCriteria criteria, ICriteria seatBookingCriteria)
		{
			if (!criteria.Locations.IsNullOrEmpty())
			{
				applyLocationFilter(criteria, seatBookingCriteria);
			}

			if (!criteria.Teams.IsNullOrEmpty())
			{
				applyTeamFilter(criteria, seatBookingCriteria);
			}
		}


		private static void applyLocationFilter(ISeatBookingReportCriteria criteria, ICriteria seatBookingCriteria)
		{
			seatBookingCriteria.CreateCriteria("Seat")
				.Add(Restrictions.In("Parent", criteria.Locations.ToList()));
		}

		private static void applyTeamFilter(ISeatBookingReportCriteria criteria, ICriteria seatBookingCriteria)
		{
			seatBookingCriteria
				.CreateAlias("Person", "person", JoinType.InnerJoin)
				.Add(Subqueries.Exists(applyPersonPeriodByTeamFilter(criteria.Teams.ToArray(), criteria.Period)));
		}

		private static DetachedCriteria applyPersonPeriodByTeamFilter(ICollection teams, DateOnlyPeriod dateOnlyPeriod)
		{
			return DetachedCriteria.For(typeof(PersonPeriod), "first")
				.SetProjection(Projections.Id())
				.Add(Restrictions.Le("StartDate", dateOnlyPeriod.EndDate))
				.Add(Restrictions.EqProperty("first.Parent", "person.Id"))
				.Add(Restrictions.In("Team", teams))
				.Add(Subqueries.NotExists(DetachedCriteria.For<PersonPeriod>()
					.SetProjection(Projections.Id())
					.Add(Restrictions.EqProperty("Parent", "first.Parent"))
					.Add(Restrictions.Le("StartDate", dateOnlyPeriod.EndDate))
					.Add(Restrictions.GtProperty("StartDate", "first.StartDate"))
					.Add(Restrictions.Le("StartDate", dateOnlyPeriod.StartDate))));
		}

		#endregion

	}

	public class SeatBookingReportModel : ISeatBookingReportModel
	{
		public IEnumerable<IPersonScheduleWithSeatBooking> SeatBookings { get; set; }
		public int RecordCount { get; set; }
	}


	public class PersonScheduleWithSeatBooking : IPersonScheduleWithSeatBooking
	{
		private DateTime _belongsToDateTime;
		public DateTime PersonScheduleStart { get; set; }
		public DateTime PersonScheduleEnd { get; set; }
		public DateTime? SeatBookingStart { get; set; }
		public DateTime? SeatBookingEnd { get; set; }
		public DateOnly BelongsToDate { get; set; }
		public Guid SeatId { get; set; }
		public String SeatName { get; set; }
		public Guid PersonId { get; set; }
		public String FirstName { get; set; }
		public String LastName { get; set; }
		public Guid LocationId { get; set; }
		public String LocationName { get; set; }
		public Guid TeamId { get; set; }
		public String TeamName { get; set; }
		public int NumberOfRecords{ get; set; }

		// map database return value to date only
		public DateTime BelongsToDateTime
		{
			get { return _belongsToDateTime; }
			set
			{
				_belongsToDateTime = value;
				BelongsToDate = new DateOnly(value);
			}
		}
		
	}
	
	public class SeatBookingReportCriteria : ISeatBookingReportCriteria
	{

		public IEnumerable<ISeatMapLocation> Locations { get; set; }
		public IEnumerable<ITeam> Teams { get; set; }
		public DateOnlyPeriod Period { get; set; }

		public SeatBookingReportCriteria()
		{
		}
		public SeatBookingReportCriteria(IEnumerable<ISeatMapLocation> locations, IEnumerable<ITeam> teams,
			DateOnlyPeriod period)
		{
			Locations = locations;
			Teams = teams;
			Period = period;
		}

	}
}