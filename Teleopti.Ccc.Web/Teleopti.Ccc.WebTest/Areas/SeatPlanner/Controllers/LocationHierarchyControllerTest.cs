using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;

namespace Teleopti.Ccc.WebTest.Areas.SeatPlanner.Controllers
{
	internal class LocationHierarchyControllerTest
	{
		private LocationHierarchyController _locationHierarchyController;
		private LocationHierarchyProvider _locationHierarchyProvider;
		private ISeatMapLocationRepository _seatMapLocationRepository;

		[SetUp]
		public void Setup()
		{
			_seatMapLocationRepository = MockRepository.GenerateMock<ISeatMapLocationRepository>();
			_locationHierarchyProvider = new LocationHierarchyProvider(_seatMapLocationRepository);
			_locationHierarchyController = new LocationHierarchyController(_locationHierarchyProvider);
		}

		[Test]
		public void ShouldGetLocationHierarchy()
		{
			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetLocation("{}", "Location1");
			seatMapLocation.SetId(Guid.NewGuid());
			var childLocation = new SeatMapLocation() {Name = "ChildLocation", SeatMapJsonData = "{}"};
			childLocation.SetId (Guid.NewGuid());
			childLocation.AddSeat ("TestSeat", 0);

			seatMapLocation.AddChild (childLocation);
			_seatMapLocationRepository.Stub(x => x.LoadRootSeatMap()).Return(seatMapLocation);
			var result = _locationHierarchyController.Get() as LocationViewModel;

			Assert.True(result.GetType() == typeof(LocationViewModel));
			Assert.True(result.Id == seatMapLocation.Id);
			Assert.True(result.Name == seatMapLocation.Name);
			Assert.True (result.Children.Count == 1);
			Assert.True(result.Children.First().Id == childLocation.Id);
			Assert.True(result.Children.First().Seats.Count()==1);
		}
	}
}	

