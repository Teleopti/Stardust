using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SeatBookingRepository : Repository<ISeatBooking>, ISeatBookingRepository
	{
		public SeatBookingRepository (IUnitOfWork unitOfWork)
			: base (unitOfWork)
		{
		}

		public SeatBookingRepository (IUnitOfWorkFactory unitOfWorkFactory)
			: base (unitOfWorkFactory)
		{
		}

		public SeatBookingRepository (ICurrentUnitOfWork currentUnitOfWork)
			: base (currentUnitOfWork)
		{
		}

		public ISeatBooking LoadAggregate (Guid id)
		{
			return Session.Query<ISeatBooking>()
				.FirstOrDefault (booking => booking.Id == id);
		}

		public ISeatBooking LoadSeatBookingForPerson (DateOnly date, IPerson person)
		{
			return
				Session.Query<ISeatBooking>()
					.SingleOrDefault (booking => (booking.BelongsToDate == date && booking.Person == person));
		}

		public IList<ISeatBooking> LoadSeatBookingsForDateOnlyPeriod (DateOnlyPeriod period)
		{
			return Session.Query<ISeatBooking>()
				.Where (booking => booking.BelongsToDate >= period.StartDate
				                   && booking.BelongsToDate <= period.EndDate).ToList();
		}

		public IList<ISeatBooking> LoadSeatBookingsForDay (DateOnly date)
		{
			return Session.Query<ISeatBooking>()
				.Where (booking => booking.BelongsToDate == date).ToList();
		}

		public IList<ISeatBooking> GetSeatBookingsForSeat (ISeat seat)
		{
			return Session.Query<ISeatBooking>().Where (booking => booking.Seat == seat).ToList();
		}

		public void RemoveSeatBookingsForSeats (IEnumerable<ISeat> seats)
		{
			seats.ForEach (RemoveSeatBookingsForSeat);
		}

		public void RemoveSeatBookingsForSeat (ISeat seat)
		{
			GetSeatBookingsForSeat (seat)
				.ForEach (booking => Session.Delete (booking));
		}

		public ISeatBookingReportModel LoadSeatBookingsReport (ISeatBookingReportCriteria criteria, Paging paging = null)
		{
			var rowCountCriteria = createRowCountCriteria (criteria);
			var bookingCriteria = createSeatBookingCriteria (criteria);

			return getResult (bookingCriteria, rowCountCriteria, paging);
		}

		private ICriteria createSeatBookingCriteria (ISeatBookingReportCriteria criteria)
		{
			var seatBookingCriteria = applyBasicReportCriteria (criteria);
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
		
		private static void applyAdditionalCriteria (ISeatBookingReportCriteria criteria, ICriteria seatBookingCriteria)
		{
			if (!criteria.Locations.IsNullOrEmpty())
			{
				applyLocationFilter (criteria, seatBookingCriteria);
			}

			if (!criteria.Teams.IsNullOrEmpty())
			{
				applyTeamFilter (criteria, seatBookingCriteria);
			}
		}


		private static void applyLocationFilter (ISeatBookingReportCriteria criteria, ICriteria seatBookingCriteria)
		{
			seatBookingCriteria.CreateCriteria ("Seat")
				.Add (Restrictions.In ("Parent", criteria.Locations.ToList()));
		}

		private static void applyTeamFilter (ISeatBookingReportCriteria criteria, ICriteria seatBookingCriteria)
		{
			seatBookingCriteria
				.CreateAlias ("Person", "person", JoinType.InnerJoin)
				.Add (Subqueries.Exists (applyPersonPeriodByTeamFilter (criteria.Teams.ToArray(), criteria.Period)));
		}

		private static DetachedCriteria applyPersonPeriodByTeamFilter (ICollection teams, DateOnlyPeriod dateOnlyPeriod)
		{
			return DetachedCriteria.For (typeof (PersonPeriod), "first")
				.SetProjection (Projections.Id())
				.Add (Restrictions.Le ("StartDate", dateOnlyPeriod.EndDate))
				.Add (Restrictions.EqProperty ("first.Parent", "person.Id"))
				.Add (Restrictions.In ("Team", teams))
				.Add (Subqueries.NotExists (DetachedCriteria.For<PersonPeriod>()
					.SetProjection (Projections.Id())
					.Add (Restrictions.EqProperty ("Parent", "first.Parent"))
					.Add (Restrictions.Le ("StartDate", dateOnlyPeriod.EndDate))
					.Add (Restrictions.GtProperty ("StartDate", "first.StartDate"))
					.Add (Restrictions.Le ("StartDate", dateOnlyPeriod.StartDate))));
		}

		

		private static ISeatBookingReportModel getResult (ICriteria bookingCriteria, IFutureValue<int> rowCount, Paging paging)
		{
			// if you change anything, then check sql profiler to ensure that nhibernate only sends one request
			return paging != null
				? getResultWithPaging (paging, bookingCriteria, rowCount)
				: getResultWithoutPaging (bookingCriteria);
		}

		private static SeatBookingReportModel getResultWithoutPaging (ICriteria bookingCriteria)
		{
			var seatBookingReportModel = new SeatBookingReportModel {SeatBookings = bookingCriteria.List<SeatBooking>()};
			seatBookingReportModel.RecordCount = seatBookingReportModel.SeatBookings.Count();
			return seatBookingReportModel;
		}

		private static SeatBookingReportModel getResultWithPaging (Paging paging, ICriteria bookingCriteria,
			IFutureValue<int> rowCount)
		{
			var seatBookingReportModel = new SeatBookingReportModel();

			var bookingQuery = bookingCriteria
				.SetFirstResult (paging.Skip)
				.SetMaxResults (paging.Take)
				.Future<SeatBooking>();

			seatBookingReportModel.RecordCount = rowCount.Value;
			seatBookingReportModel.SeatBookings = bookingQuery.ToList();

			return seatBookingReportModel;
		}
	}

	public class SeatBookingReportModel : ISeatBookingReportModel
	{
		public IEnumerable<ISeatBooking> SeatBookings { get; set; }
		public int RecordCount { get; set; }
	}


	public class SeatBookingReportCriteria : ISeatBookingReportCriteria
	{

		public IEnumerable<ISeatMapLocation> Locations { get; set; }
		public IEnumerable<ITeam> Teams { get; set; }
		public DateOnlyPeriod Period { get; set; }

		public SeatBookingReportCriteria()
		{
		}
		public SeatBookingReportCriteria (IEnumerable<ISeatMapLocation> locations, IEnumerable<ITeam> teams,
			DateOnlyPeriod period)
		{
			Locations = locations;
			Teams = teams;
			Period = period;
		}
		
	}
}