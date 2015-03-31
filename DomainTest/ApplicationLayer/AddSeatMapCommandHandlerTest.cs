using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	internal class AddSeatMapCommandHandlerTest
	{
		private FakeWriteSideRepository<ISeatMapLocation> _seatMapLocationRepository;
		private IBusinessUnitRepository _buRepository;
		private ICurrentBusinessUnit _currentBusinessUnit;
		private LocationInfo _childLocationInfo;
		private AddSeatMapCommandHandler _target;
		private LocationInfo _childLocation2;
		private FakeSeatBookingRepository _seatBookingRepository;

		[SetUp]
		public void Setup()
		{
			_seatMapLocationRepository = new FakeWriteSideRepository<ISeatMapLocation>();

			var bu = new BusinessUnit("bu").WithId();
			_buRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			_currentBusinessUnit = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			_currentBusinessUnit.Stub(x => x.Current()).Return(bu);
			_buRepository.Stub(x => x.Get(_currentBusinessUnit.Current().Id.GetValueOrDefault())).Return(bu);


			_seatBookingRepository = new FakeSeatBookingRepository();

			_target = new AddSeatMapCommandHandler(_seatMapLocationRepository,
				_buRepository, _currentBusinessUnit, _seatBookingRepository);
			_childLocationInfo = new LocationInfo()
			{
				Name = "Chongqing",
				IsNew = true,
				Id = new Guid("bc10076d-def3-426e-9237-a45200971952")

			};
			_childLocation2 = new LocationInfo()
			{
				Name = "Shenzhen",
				IsNew = true,
				Id = new Guid("6291c86e-3679-4df9-b2e5-a45200973fb6")
			};
		}

		[Test]
		public void ShouldSetupEntityState()
		{

			const string dummyJsonData = "{DummyData}";
			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();

			var command = new AddSeatMapCommand()
			{
				SeatMapData = dummyJsonData,
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			_target.Handle(command);
			var seatMapLocation = _seatMapLocationRepository.Single() as SeatMapLocation;

			seatMapLocation.Id.Should().Have.Value();
			seatMapLocation.SeatMapJsonData.Should().Be(dummyJsonData);
			seatMapLocation.Name.Should().Be(_currentBusinessUnit.Current().Name);

		}


		[Test]
		public void ShouldCreateChildSeatMapsForLocationsInSeatMap()
		{

			const string dummyJsonData = @"{""objects"":[{""type"":""i-text"",""originX"":""left"",""originY"":""top"",""left"":302,""top"":44,""width"":116.07,""height"":23.4,""fill"":""#000000"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":2.84,""scaleY"":2.84,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""text"":""Teleopti China"",""fontSize"":18,""fontWeight"":"""",""fontFamily"":""helvetica"",""fontStyle"":"""",""lineHeight"":1.3,""textDecoration"":"""",""textAlign"":""left"",""path"":null,""textBackgroundColor"":"""",""useNative"":true,""styles"":{}},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":18,""top"":259,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""6291c86e-3679-4df9-b2e5-a45200973fb6"",""name"":""Shenzhen"",""seatMapId"":""025e5bc6-1e04-4fe9-ab74-a45200973fb6""},""seatMapId"":""187d4c70-6158-4a38-8612-a4520098468b""},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":638,""top"":256,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""bc10076d-def3-426e-9237-a45200971952"",""name"":""Chongqing"",""seatMapId"":""09ab3b5f-2cac-4306-a03a-a45200971952""}],""background"":""""}";

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();

			var command = new AddSeatMapCommand()
			{
				SeatMapData = dummyJsonData,
				ChildLocations = new[] { _childLocationInfo, _childLocation2 },
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			_target.Handle(command);

			var seatMapLocation = _seatMapLocationRepository.First() as SeatMapLocation;
			var childSeatMapLocation = _seatMapLocationRepository.Last() as SeatMapLocation;

			childSeatMapLocation.Should().Not.Be.Null();
			childSeatMapLocation.Name.Should().Be("Shenzhen");
			childSeatMapLocation.ParentLocation.Should().Be(seatMapLocation);

		}


		[Test]
		public void ShouldCreateSeats()
		{

			const string dummyJsonData = @"{""objects"":[{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":370,""top"":90.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""guid"":""d9664f22-886b-f5bf-f799-7d59765c2604"",""name"":""Unnamed seat"",""priority"":1},{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":565,""top"":90.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""guid"":""8e48dd65-e68a-0834-fdc5-eae75f12065c"",""name"":""Unnamed seat"",""priority"":2}],""background"":""""}";

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();

			var command = new AddSeatMapCommand()
			{
				SeatMapData = dummyJsonData,
				Seats = new[]
				{
					new SeatInfo(){ Id = Guid.Parse("{d9664f22-886b-f5bf-f799-7d59765c2604}"), IsNew = true, Name = "New Seat"},
					new SeatInfo(){ Id = Guid.Parse("{8e48dd65-e68a-0834-fdc5-eae75f12065c}"), IsNew = true, Name = "New Seat 2"},
				},
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			_target.Handle(command);

			var seatMapLocation = _seatMapLocationRepository.First() as SeatMapLocation;
			seatMapLocation.SeatCount.Should().Be(2);
			seatMapLocation.Seats.First().Name.Should().Be("New Seat");
		}

		[Test]
		public void ShouldUpdateTemporaryLocationAndSeatMapIds()
		{
			const string dummyJsonData = @"{""objects"":[{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":370,""top"":90.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""guid"":""d9664f22-886b-f5bf-f799-7d59765c2604"",""name"":""Unnamed seat"",""priority"":1},{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":565,""top"":90.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""guid"":""8e48dd65-e68a-0834-fdc5-eae75f12065c"",""name"":""Unnamed seat"",""priority"":2}],""background"":""""}";

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();

			var command = new AddSeatMapCommand()
			{
				SeatMapData = dummyJsonData,
				ChildLocations = new[] { _childLocationInfo, _childLocation2 },
				Seats = new[]
				{
					new SeatInfo(){ Id = Guid.Parse("{d9664f22-886b-f5bf-f799-7d59765c2604}"), IsNew = true, Name = "New Seat"},
					new SeatInfo(){ Id = Guid.Parse("{8e48dd65-e68a-0834-fdc5-eae75f12065c}"), IsNew = true, Name = "New Seat 2"},
				},
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			_target.Handle(command);

			var seatMapLocation = _seatMapLocationRepository.First() as SeatMapLocation;
			seatMapLocation.SeatMapJsonData.Should().Not.Equals(dummyJsonData);
			seatMapLocation.SeatMapJsonData.Should().Not.Contain(_childLocationInfo.Id.ToString());
			seatMapLocation.SeatMapJsonData.Should().Not.Contain(_childLocation2.Id.ToString());
			seatMapLocation.SeatMapJsonData.Should().Contain(seatMapLocation.Seats.First().Id.ToString());
			seatMapLocation.SeatMapJsonData.Should().Contain(seatMapLocation.Seats.Last().Id.ToString());
		}


		[Test]
		public void ShouldDeleteChildSeatMaps()
		{
			var seatMapLocation = new SeatMapLocation();
			const string seatMapData = @"{""objects"":[{""type"":""i-text"",""originX"":""left"",""originY"":""top"",""left"":302,""top"":44,""width"":116.07,""height"":23.4,""fill"":""#000000"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":2.84,""scaleY"":2.84,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""text"":""Teleopti China"",""fontSize"":18,""fontWeight"":"""",""fontFamily"":""helvetica"",""fontStyle"":"""",""lineHeight"":1.3,""textDecoration"":"""",""textAlign"":""left"",""path"":null,""textBackgroundColor"":"""",""useNative"":true,""styles"":{}},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":18,""top"":259,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""6291c86e-3679-4df9-b2e5-a45200973fb6"",""name"":""Shenzhen"",""seatMapId"":""025e5bc6-1e04-4fe9-ab74-a45200973fb6""},""seatMapId"":""187d4c70-6158-4a38-8612-a4520098468b""},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":638,""top"":256,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""bc10076d-def3-426e-9237-a45200971952"",""name"":""Chongqing"",""seatMapId"":""09ab3b5f-2cac-4306-a03a-a45200971952""}],""background"":""""}";
			seatMapLocation.SetLocation(seatMapData, "rootLocation");
			var childSeatMapLocation1 = seatMapLocation.CreateChildSeatMapLocation(_childLocationInfo);
			var childSeatMapLocation2 = seatMapLocation.CreateChildSeatMapLocation(_childLocation2);

			seatMapLocation.SetId(Guid.NewGuid());
			childSeatMapLocation1.SetId(Guid.NewGuid());
			childSeatMapLocation2.SetId(Guid.NewGuid());

			_seatMapLocationRepository.Add(seatMapLocation);
			_seatMapLocationRepository.Add(childSeatMapLocation1);
			_seatMapLocationRepository.Add(childSeatMapLocation2);

			var lastChildSeatMapLocation = _seatMapLocationRepository.Last() as SeatMapLocation;

			_childLocationInfo.IsNew = false;
			_childLocationInfo.Id = childSeatMapLocation1.Id;

			var command = new AddSeatMapCommand()
			{
				Id = seatMapLocation.Id,
				SeatMapData = @"{""objects"":[{""type"":""i-text"",""originX"":""left"",""originY"":""top"",""left"":302,""top"":44,""width"":116.07,""height"":23.4,""fill"":""#000000"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":2.84,""scaleY"":2.84,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""text"":""Teleopti China"",""fontSize"":18,""fontWeight"":"""",""fontFamily"":""helvetica"",""fontStyle"":"""",""lineHeight"":1.3,""textDecoration"":"""",""textAlign"":""left"",""path"":null,""textBackgroundColor"":"""",""useNative"":true,""styles"":{}},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":638,""top"":256,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""bc10076d-def3-426e-9237-a45200971952"",""name"":""Chongqing"",""seatMapId"":""09ab3b5f-2cac-4306-a03a-a45200971952""}],""background"":""""}",
				ChildLocations = new[] { _childLocationInfo },
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			_target.Handle(command);

			lastChildSeatMapLocation.Name.Should().Be("Shenzhen");
			var childSeatMapLocation = _seatMapLocationRepository.Last() as SeatMapLocation;
			childSeatMapLocation.Name.Should().Be("Chongqing");

		}

		[Test]
		public void ShouldDeleteWhenLocationHasSeatWithBooking()
		{
			var seatMapLocation = new SeatMapLocation();

			const string seatMapData = @"{""objects"":[{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":495,""top"":228.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""id"":""f19f90e8-f629-237b-2493-f2ed39e5c13b"",""name"":""Unnamed seat"",""priority"":1}],""background"":""""}";

			seatMapLocation.SetLocation(seatMapData, "rootLocation");
			seatMapLocation.SetId(Guid.NewGuid());
			var seat = seatMapLocation.AddSeat("Seat1", 1);
			_seatMapLocationRepository.Add(seatMapLocation);

			var seatBooking = new SeatBooking(new Person(), new DateTime(2015, 03, 02, 8, 0, 0), new DateTime(2015, 03, 02, 17, 0, 0));
			seatBooking.Book(seat);

			_seatBookingRepository.Add(seatBooking);

			var command = new AddSeatMapCommand()
			{
				Id = seatMapLocation.Id,
				SeatMapData = @"{""objects"":[],""background"":""""}",
				ChildLocations = new LocationInfo[0],
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			_target.Handle(command);

			Assert.IsTrue(!_seatMapLocationRepository.First().Seats.Any());
			Assert.IsTrue(!_seatBookingRepository.Any());

		}

		[Test]
		public void ShouldDeleteWhenLocationHasChildWithSeatAndBooking()
		{
			var seatMapLocation = new SeatMapLocation();
			const string seatMapData = @"{""objects"":[{""type"":""i-text"",""originX"":""left"",""originY"":""top"",""left"":302,""top"":44,""width"":116.07,""height"":23.4,""fill"":""#000000"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":2.84,""scaleY"":2.84,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""text"":""Teleopti China"",""fontSize"":18,""fontWeight"":"""",""fontFamily"":""helvetica"",""fontStyle"":"""",""lineHeight"":1.3,""textDecoration"":"""",""textAlign"":""left"",""path"":null,""textBackgroundColor"":"""",""useNative"":true,""styles"":{}},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":18,""top"":259,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""6291c86e-3679-4df9-b2e5-a45200973fb6"",""name"":""Shenzhen"",""seatMapId"":""025e5bc6-1e04-4fe9-ab74-a45200973fb6""},""seatMapId"":""187d4c70-6158-4a38-8612-a4520098468b""},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":638,""top"":256,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""bc10076d-def3-426e-9237-a45200971952"",""name"":""Chongqing"",""seatMapId"":""09ab3b5f-2cac-4306-a03a-a45200971952""}],""background"":""""}";
			seatMapLocation.SetLocation(seatMapData, "rootLocation");
			var childSeatMapLocation1 = seatMapLocation.CreateChildSeatMapLocation(_childLocationInfo);
			var childSeatMapLocation2 = seatMapLocation.CreateChildSeatMapLocation(_childLocation2);

			seatMapLocation.SetId(Guid.NewGuid());
			childSeatMapLocation1.SetId(Guid.NewGuid());
			childSeatMapLocation2.SetId(Guid.NewGuid());

			var seat = childSeatMapLocation1.AddSeat("Seat1", 1);

			_seatMapLocationRepository.Add(seatMapLocation);
			_seatMapLocationRepository.Add(childSeatMapLocation1);
			_seatMapLocationRepository.Add(childSeatMapLocation2);

			var seatBooking = new SeatBooking(new Person(), new DateTime(2015, 03, 02, 8, 0, 0), new DateTime(2015, 03, 02, 17, 0, 0));
			seatBooking.Book(seat);

			_seatBookingRepository.Add(seatBooking);

			var command = new AddSeatMapCommand()
			{
				Id = seatMapLocation.Id,
				SeatMapData = @"{""objects"":[],""background"":""""}",
				ChildLocations = new LocationInfo[0],
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			_target.Handle(command);

			Assert.IsTrue(_seatMapLocationRepository.Count() == 1);
			Assert.IsTrue(!_seatBookingRepository.Any());
		}

		[Test]
		public void ShouldNotDeleteChildLocationSeatsWhenAddingParentSeats()
		{
			var seatMapLocation = new SeatMapLocation();
			const string seatMapData = @"{""objects"":[{""type"":""i-text"",""originX"":""left"",""originY"":""top"",""left"":302,""top"":44,""width"":116.07,""height"":23.4,""fill"":""#000000"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":2.84,""scaleY"":2.84,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""text"":""Teleopti China"",""fontSize"":18,""fontWeight"":"""",""fontFamily"":""helvetica"",""fontStyle"":"""",""lineHeight"":1.3,""textDecoration"":"""",""textAlign"":""left"",""path"":null,""textBackgroundColor"":"""",""useNative"":true,""styles"":{}},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":18,""top"":259,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""6291c86e-3679-4df9-b2e5-a45200973fb6"",""name"":""Shenzhen"",""seatMapId"":""025e5bc6-1e04-4fe9-ab74-a45200973fb6""},""seatMapId"":""187d4c70-6158-4a38-8612-a4520098468b""},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":638,""top"":256,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""bc10076d-def3-426e-9237-a45200971952"",""name"":""Chongqing"",""seatMapId"":""09ab3b5f-2cac-4306-a03a-a45200971952""}],""background"":""""}";
			seatMapLocation.SetLocation(seatMapData, "rootLocation");
			var childSeatMapLocation1 = seatMapLocation.CreateChildSeatMapLocation(_childLocationInfo);
			var childSeatMapLocation2 = seatMapLocation.CreateChildSeatMapLocation(_childLocation2);

			seatMapLocation.SetId(Guid.NewGuid());
			childSeatMapLocation1.SetId(new Guid("bc10076d-def3-426e-9237-a45200971952"));
			childSeatMapLocation2.SetId(new Guid("6291c86e-3679-4df9-b2e5-a45200973fb6"));

			var seat = childSeatMapLocation1.AddSeat("Seat1", 1);
			var seat2 = childSeatMapLocation2.AddSeat("Seat1", 1);

			_seatMapLocationRepository.Add(seatMapLocation);
			_seatMapLocationRepository.Add(childSeatMapLocation1);
			_seatMapLocationRepository.Add(childSeatMapLocation2);

			var command = new AddSeatMapCommand()
			{
				Id = seatMapLocation.Id,
				SeatMapData = @"{""objects"":[{""type"":""i-text"",""originX"":""left"",""originY"":""top"",""left"":302,""top"":44,""width"":116.07,""height"":23.4,""fill"":""#000000"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":2.84,""scaleY"":2.84,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""text"":""Teleopti China"",""fontSize"":18,""fontWeight"":"""",""fontFamily"":""helvetica"",""fontStyle"":"""",""lineHeight"":1.3,""textDecoration"":"""",""textAlign"":""left"",""path"":null,""textBackgroundColor"":"""",""useNative"":true,""styles"":{}},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":18,""top"":259,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""6291c86e-3679-4df9-b2e5-a45200973fb6"",""name"":""Shenzhen"",""seatMapId"":""025e5bc6-1e04-4fe9-ab74-a45200973fb6""},""seatMapId"":""187d4c70-6158-4a38-8612-a4520098468b""},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":638,""top"":256,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""bc10076d-def3-426e-9237-a45200971952"",""name"":""Chongqing"",""seatMapId"":""09ab3b5f-2cac-4306-a03a-a45200971952""},{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":370,""top"":90.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""guid"":""d9664f22-886b-f5bf-f799-7d59765c2604"",""name"":""Unnamed seat"",""priority"":1},{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":565,""top"":90.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""guid"":""8e48dd65-e68a-0834-fdc5-eae75f12065c"",""name"":""Unnamed seat"",""priority"":2}],""background"":""""}",
				ChildLocations = new[]{ 
					new LocationInfo()
					{
						Name = "Chongqing",
						IsNew = false,
						Id = new Guid("bc10076d-def3-426e-9237-a45200971952")
					},
					new LocationInfo()
					{
						Name = "Shenzhen",
						IsNew = false,
						Id = new Guid("6291c86e-3679-4df9-b2e5-a45200973fb6")
					}
				},

				Seats = new[]
				{
					new SeatInfo(){ Id = Guid.Parse("{d9664f22-886b-f5bf-f799-7d59765c2604}"), IsNew = true, Name = "New Seat"},
					new SeatInfo(){ Id = Guid.Parse("{8e48dd65-e68a-0834-fdc5-eae75f12065c}"), IsNew = true, Name = "New Seat 2"},
				},

				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			_target.Handle(command);

			Assert.IsTrue(_seatMapLocationRepository.Count() == 3);
			var childSeatMapLocation = _seatMapLocationRepository.Load(childSeatMapLocation2.Id.Value);
			Assert.IsTrue(childSeatMapLocation.Seats.Count == 1);
			Assert.IsTrue(Equals(_seatMapLocationRepository.Load(childSeatMapLocation2.Id.Value).Seats[0], seat2));
		}

	}
}