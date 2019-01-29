using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;

namespace Teleopti.Ccc.WebTest.Areas.SeatPlanner.Controllers
{
	
	internal class SeatMapControllerTest
	{
		private ISeatMapLocationRepository _seatMapLocationRepository ;
		private ISeatBookingRepository _seatBookingRepository;
		private SeatMapPersister _seatMapPersister;
		private SeatMapController _seatMapController;
		private SeatMapProvider _seatMapProvider;
		private IUserTimeZone _userTimeZone;
		private TimeZoneInfo _timeZone;

		[SetUp]
		public void Setup()
		{

			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));  //GMT +1
			_userTimeZone.Stub(x => x.TimeZone()).Return(_timeZone);

			_seatMapLocationRepository = MockRepository.GenerateMock<ISeatMapLocationRepository>();
			_seatBookingRepository = MockRepository.GenerateMock<ISeatBookingRepository>();

			_seatMapPersister = new SeatMapPersister(_seatMapLocationRepository, new FakeBusinessUnitRepository(null),
				new FakeCurrentBusinessUnit(), _seatBookingRepository, new FakeSeatPlanRepository(), new FakeApplicationRoleRepository());
			_seatMapProvider = new SeatMapProvider(_seatMapLocationRepository, _seatBookingRepository, _userTimeZone);

			_seatMapController = new SeatMapController(_seatMapProvider, _seatMapPersister);
		}

		[Test]
		public void ShouldGetSeatMapLocation()
		{
			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetLocation("{}", "Location1");
			seatMapLocation.SetId(Guid.NewGuid());

			_seatMapLocationRepository.Stub(x => x.LoadAggregate(seatMapLocation.Id.Value)).Return(seatMapLocation);

			var result = _seatMapController.Get(seatMapLocation.Id);

			Assert.True(result.GetType() == typeof(LocationViewModel));
			Assert.True(result.Id == seatMapLocation.Id);
			Assert.True(result.Name == seatMapLocation.Name);
			Assert.True(result.LocationPrefix == seatMapLocation.LocationPrefix);
			Assert.True(result.LocationSuffix == seatMapLocation.LocationSuffix);
		}
	}
}
