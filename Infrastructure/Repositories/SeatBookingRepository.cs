using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using NHibernate;
using NHibernate.Impl;
using NHibernate.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.SeatManagement;
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

		public IList<ISeatBooking> LoadSeatBookingsIntersectingDay(DateOnly date, Guid locationId)
		{
			return getSeatBookingsInterceptingDay (date).Where (booking => locationId == booking.Seat.Parent.Id).ToList();
		}

		public IList<ISeatBooking> LoadSeatBookingsForSeatIntersectingDay (DateOnly date, Guid seatId)
		{
			return getSeatBookingsInterceptingDay(date)
				.Where (booking => booking.Seat.Id == seatId)
				.OrderBy (booking => booking.BelongsToDate)
				.ThenBy(booking => booking.StartDateTime)
				.ToList();
		}

		private IQueryable<ISeatBooking> getSeatBookingsInterceptingDay (DateOnly date)
		{
			var requestedDate = getDateTimePeriodFromRequestedDate (date);
			return Session.Query<ISeatBooking>()
				.Where (booking =>!((requestedDate.EndDateTime < booking.StartDateTime) || (requestedDate.StartDateTime > booking.EndDateTime)));
		}


		private static DateTimePeriod getDateTimePeriodFromRequestedDate(DateOnly dateOnly)
		{
			var dateOnlyAsUTCDateTIme = new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 0, 0, 0, DateTimeKind.Utc);
			var requestedDate = new DateTimePeriod(dateOnlyAsUTCDateTIme, dateOnlyAsUTCDateTIme.AddDays(1).AddSeconds(-1));
			return requestedDate;
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

			var scheduleAndBookingQuery = getScheduleAndBookingQuery(criteria);
			return getBookingReportResults(scheduleAndBookingQuery, paging);
		}

		private static ISeatBookingReportModel getBookingReportResults(IQuery bookingQuery, Paging paging)
		{
			return paging != null
				? getBookingReportResulsWithPaging(bookingQuery, paging)
				: getBookingReportResultsWithoutPaging(bookingQuery);
		}

		private static SeatBookingReportModel getBookingReportResultsWithoutPaging(IQuery bookingQuery)
		{
			var seatBookingReportModel = new SeatBookingReportModel
			{
				SeatBookings = bookingQuery.List<PersonScheduleWithSeatBooking>()
			};

			seatBookingReportModel.RecordCount = seatBookingReportModel.SeatBookings.Count();
			return seatBookingReportModel;
		}

		private static SeatBookingReportModel getBookingReportResulsWithPaging(IQuery bookingQuery, Paging paging)
		{
			var seatBookingReportModel = new SeatBookingReportModel
			{
				SeatBookings = bookingQuery
					.SetFirstResult(paging.Skip)
					.SetMaxResults(paging.Take)
					.List<PersonScheduleWithSeatBooking>()
			};

			var firstBooking = seatBookingReportModel.SeatBookings.FirstOrDefault();
			seatBookingReportModel.RecordCount = firstBooking == null ? 0 : firstBooking.NumberOfRecords;

			return seatBookingReportModel;
		}

		private IQuery getScheduleAndBookingQuery(ISeatBookingReportCriteria reportCriteria)
		{

			var query = Session.CreateSQLQuery(
				@"exec dbo.LoadScheduleAndSeatBookingInfo @startDate=:startDate, @endDate =:endDate, 
					@teamIdList=:teamIdList, @locationIdList =:locationIdList, @businessUnitId=:businessUnitId ");

			setQueryResultTypes(query);
			setQueryParameters(reportCriteria, query);

			query.SetResultTransformer(Transformers.AliasToBean(typeof(PersonScheduleWithSeatBooking)))
				 .SetReadOnly(true);

			return query;
		}

		private static void setQueryResultTypes(ISQLQuery query)
		{
			query.AddScalar("PersonScheduleStart", NHibernateUtil.UtcDateTime)
				.AddScalar("PersonScheduleEnd", NHibernateUtil.UtcDateTime)
				.AddScalar("PersonScheduleModelSerialized", NHibernateUtil.Custom(typeof(CompressedString)))
				.AddScalar("SeatBookingStart", NHibernateUtil.UtcDateTime)
				.AddScalar("SeatBookingEnd", NHibernateUtil.UtcDateTime)
				.AddScalar("SeatId", NHibernateUtil.Guid)
				.AddScalar("SeatName", NHibernateUtil.String)
				.AddScalar("PersonId", NHibernateUtil.Guid)
				.AddScalar("FirstName", NHibernateUtil.String)
				.AddScalar("LastName", NHibernateUtil.String)
				.AddScalar("LocationId", NHibernateUtil.Guid)
				.AddScalar("LocationName", NHibernateUtil.String)
				.AddScalar("TeamId", NHibernateUtil.Guid)
				.AddScalar("TeamName", NHibernateUtil.String)
				.AddScalar("SiteId", NHibernateUtil.Guid)
				.AddScalar("SiteName", NHibernateUtil.String)
				.AddScalar("NumberOfRecords", NHibernateUtil.Int32)
				.AddScalar("IsDayOff", NHibernateUtil.Boolean)
				.AddScalar("BelongsToDateTime", NHibernateUtil.UtcDateTime);
		}

		private void setQueryParameters(ISeatBookingReportCriteria reportCriteria, ISQLQuery query)
		{
			query.SetDateOnly("startDate", reportCriteria.Period.StartDate)
				.SetDateOnly("endDate", reportCriteria.Period.EndDate)
				.SetString("teamIdList", getTeamCriteria(reportCriteria))
				.SetString("locationIdList", getLocationCriteria(reportCriteria))
				.SetGuid("businessUnitId", getBusinessUnitId());
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

	}
}