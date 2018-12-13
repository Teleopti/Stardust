using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using NHibernate;
using NHibernate.Impl;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.SeatManagement;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SeatBookingRepository : Repository<ISeatBooking>, ISeatBookingRepository
	{
		public SeatBookingRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}
		
		public IList<ISeatBooking> LoadSeatBookingsForPerson(DateOnlyPeriod period, IPerson person)
		{
			return
				Session.Query<ISeatBooking>()
					.Where(booking => booking.BelongsToDate >= period.StartDate
									  && booking.BelongsToDate <= period.EndDate && booking.Person == person).ToList();
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

		public IList<ISeatBooking> LoadSeatBookingsIntersectingDateTimePeriod(DateTimePeriod date, Guid locationId)
		{
			return getSeatBookingsInterceptingDay (date).Where (booking => locationId == booking.Seat.Parent.Id).ToList();
		}


		public IList<ISeatBooking> LoadSeatBookingsIntersectingDateTimePeriod(DateTimePeriod dateTimePeriod, IList<Guid> seatIds)
		{
			return getSeatBookingsInterceptingDay(dateTimePeriod)
				.Where(booking => seatIds.Contains(booking.Seat.Id.Value))
				.OrderBy(booking => booking.BelongsToDate)
				.ThenBy(booking => booking.StartDateTime)
				.ToList();
		}
		
		private IQueryable<ISeatBooking> getSeatBookingsInterceptingDay (DateTimePeriod dateTimePeriod)
		{
			return Session.Query<ISeatBooking>()
				.Where(booking => !((dateTimePeriod.EndDateTime < booking.StartDateTime) || (dateTimePeriod.StartDateTime > booking.EndDateTime)));
		}

		public IList<ISeatBooking> GetSeatBookingsForSeat(ISeat seat)
		{
			return Session.Query<ISeatBooking>().Where(booking => booking.Seat == seat).ToList();
		}

		public void RemoveSeatBookingsForSeats(IEnumerable<ISeat> seats)
		{
			seats.ForEach(removeSeatBookingsForSeat);
		}

		private void removeSeatBookingsForSeat(ISeat seat)
		{
			GetSeatBookingsForSeat(seat)
				.ForEach(Remove);
		}

		public ISeatBookingReportModel LoadSeatBookingsReport(ISeatBookingReportCriteria criteria, Paging paging = new Paging())
		{
			var scheduleAndBookingQuery = getScheduleAndBookingQuery(criteria);
			return getBookingReportResults(scheduleAndBookingQuery, paging);
		}

		private static ISeatBookingReportModel getBookingReportResults(IQuery bookingQuery, Paging paging)
		{
			return paging.Equals(Paging.Empty)
				? getBookingReportResultsWithoutPaging(bookingQuery)
				: getBookingReportResulsWithPaging(bookingQuery, paging);
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
			seatBookingReportModel.RecordCount = firstBooking?.NumberOfRecords ?? 0;

			return seatBookingReportModel;
		}

		private IQuery getScheduleAndBookingQuery(ISeatBookingReportCriteria reportCriteria)
		{

			var query = Session.CreateSQLQuery(
				@"exec dbo.LoadScheduleAndSeatBookingInfo @startDate=:startDate, @endDate =:endDate, 
					@teamIdList=:teamIdList, @locationIdList =:locationIdList, @businessUnitId=:businessUnitId, @unseatedOnly=:unseatedOnly ");

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
				.AddScalar("LocationPrefix", NHibernateUtil.String)
				.AddScalar("LocationSuffix", NHibernateUtil.String)
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
				.SetGuid("businessUnitId", getBusinessUnitId())
				.SetBoolean("unseatedOnly", reportCriteria.ShowOnlyUnseated);
		}

		private Guid getBusinessUnitId()
		{
			var filter = (FilterImpl)Session.GetEnabledFilter("businessUnitFilter");
			object businessUnitId;
			if (!filter.Parameters.TryGetValue("businessUnitParameter", out businessUnitId))
			{
				return ((ITeleoptiIdentity)ClaimsPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault();
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