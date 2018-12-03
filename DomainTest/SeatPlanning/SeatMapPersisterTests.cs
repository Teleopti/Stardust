using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.DomainTest.SeatPlanning
{
	[TestFixture]
	internal class SeatMapPersisterTests
	{
		private FakeWriteSideRepository<ISeatMapLocation> _seatMapLocationRepository;
		private IBusinessUnitRepository _buRepository;
		private ICurrentBusinessUnit _currentBusinessUnit;
		private LocationInfo _childLocationInfo;
		private ISeatMapPersister _target;
		private LocationInfo _childLocation2;
		private FakeSeatBookingRepository _seatBookingRepository;
		private FakeSeatPlanRepository _seatPlanRepository;
		private IApplicationRoleRepository _applicarionRoleRepository;

		[SetUp]
		public void Setup()
		{
			_seatMapLocationRepository = new FakeWriteSideRepository<ISeatMapLocation>(null);

			var bu = new BusinessUnit("bu").WithId();
			_buRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			_currentBusinessUnit = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			_currentBusinessUnit.Stub(x => x.Current()).Return(bu);
			_buRepository.Stub(x => x.Get(_currentBusinessUnit.Current().Id.GetValueOrDefault())).Return(bu);


			_seatBookingRepository = new FakeSeatBookingRepository();
			_seatPlanRepository = new FakeSeatPlanRepository();
			_applicarionRoleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();

			_target = new SeatMapPersister(_seatMapLocationRepository,
				_buRepository, _currentBusinessUnit, _seatBookingRepository, _seatPlanRepository, _applicarionRoleRepository);
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

			var command = new SaveSeatMapCommand()
			{
				SeatMapData = dummyJsonData
			};

			_target.Save(command);
			var seatMapLocation = _seatMapLocationRepository.Single() as SeatMapLocation;

			seatMapLocation.Id.Should().Have.Value();
			seatMapLocation.SeatMapJsonData.Should().Be(dummyJsonData);
			seatMapLocation.Name.Should().Be(_currentBusinessUnit.Current().Name);

		}


		[Test]
		public void ShouldCreateChildSeatMapsForLocationsInSeatMap()
		{
			const string dummyJsonData = @"{""objects"":[{""type"":""i-text"",""originX"":""left"",""originY"":""top"",""left"":302,""top"":44,""width"":116.07,""height"":23.4,""fill"":""#000000"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":2.84,""scaleY"":2.84,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""text"":""Teleopti China"",""fontSize"":18,""fontWeight"":"""",""fontFamily"":""helvetica"",""fontStyle"":"""",""lineHeight"":1.3,""textDecoration"":"""",""textAlign"":""left"",""path"":null,""textBackgroundColor"":"""",""useNative"":true,""styles"":{}},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":18,""top"":259,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""6291c86e-3679-4df9-b2e5-a45200973fb6"",""name"":""Shenzhen"",""seatMapId"":""025e5bc6-1e04-4fe9-ab74-a45200973fb6""},""seatMapId"":""187d4c70-6158-4a38-8612-a4520098468b""},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":638,""top"":256,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""bc10076d-def3-426e-9237-a45200971952"",""name"":""Chongqing"",""seatMapId"":""09ab3b5f-2cac-4306-a03a-a45200971952""}],""background"":""""}";

			var command = new SaveSeatMapCommand()
			{
				SeatMapData = dummyJsonData,
				ChildLocations = new[] { _childLocationInfo, _childLocation2 }
			};

			_target.Save(command);

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

			var command = new SaveSeatMapCommand()
			{
				SeatMapData = dummyJsonData,
				Seats = new[]
				{
					new SeatInfo(){ Id = Guid.Parse("{d9664f22-886b-f5bf-f799-7d59765c2604}"), IsNew = true, Name = "New Seat"},
					new SeatInfo(){ Id = Guid.Parse("{8e48dd65-e68a-0834-fdc5-eae75f12065c}"), IsNew = true, Name = "New Seat 2"}
				}
			};

			_target.Save(command);

			var seatMapLocation = _seatMapLocationRepository.First() as SeatMapLocation;
			seatMapLocation.SeatCount.Should().Be(2);
			seatMapLocation.Seats.First().Name.Should().Be("New Seat");
		}

		[Test]
		public void ShouldSaveRolesForSeats()
		{
			const string dummyJsonData = @"{""objects"":[{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":370,""top"":90.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""guid"":""d9664f22-886b-f5bf-f799-7d59765c2604"",""name"":""Unnamed seat"",""priority"":1},{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":565,""top"":90.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""guid"":""8e48dd65-e68a-0834-fdc5-eae75f12065c"",""name"":""Unnamed seat"",""priority"":2}],""background"":""""}";

			var role1 = ApplicationRoleFactory.CreateRole("role1", "this is a role.");
			role1.SetId(Guid.NewGuid());
			var role2 = ApplicationRoleFactory.CreateRole("role2", "this is an ohter role.");
			role2.SetId(Guid.NewGuid());
			_applicarionRoleRepository.Stub(x => x.LoadAll()).Return(new List<IApplicationRole> {role1, role2});


			var command = new SaveSeatMapCommand()
			{
				SeatMapData = dummyJsonData,
				Seats = new[]
				{
					new SeatInfo()
					{
						Id = Guid.Parse("{d9664f22-886b-f5bf-f799-7d59765c2604}"), 
						IsNew = true, 
						Name = "New Seat", 
						RoleIdList = new []{ role1.Id.Value, role2.Id.Value }
					}
				}
			};

			_target.Save(command);

			var seatMapLocation = _seatMapLocationRepository.First() as SeatMapLocation;
			seatMapLocation.Seats.Single().Roles.Count.Should().Be(2);
			seatMapLocation.Seats.Single().Roles.First().Name.Should().Be("role1");
			seatMapLocation.Seats.Single().Roles.Second().Name.Should().Be("role2");
		}

		[Test]
		public void ShouldSavePrefixAndSuffix()
		{
			const string dummyJsonData = @"{""objects"":[{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":370,""top"":90.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""guid"":""d9664f22-886b-f5bf-f799-7d59765c2604"",""name"":""Unnamed seat"",""priority"":1},{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":565,""top"":90.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""guid"":""8e48dd65-e68a-0834-fdc5-eae75f12065c"",""name"":""Unnamed seat"",""priority"":2}],""background"":""""}";
			var seatMapLocation = new SeatMapLocation();
			seatMapLocation.SetLocation(dummyJsonData, "rootLocation");
			seatMapLocation.SetId(Guid.NewGuid());
			_seatMapLocationRepository.Add(seatMapLocation);

			var command = new SaveSeatMapCommand()
			{
				Id = seatMapLocation.Id,
				SeatMapData = dummyJsonData,
				Seats = new[]
				{
					new SeatInfo()
					{
						Id = Guid.NewGuid(),
						IsNew = true,
						Name = "New Seat"
					}
				},
				LocationPrefix = "Prefix",
				LocationSuffix = "Suffix"
			};

			_target.Save(command);
			
			seatMapLocation.LocationPrefix.Should().Be.EqualTo("Prefix");
			seatMapLocation.LocationSuffix.Should().Be.EqualTo("Suffix");
		}

		[Test]
		public void ShouldUpdateRolesForSeats()
		{
			var seatMapLocation = new SeatMapLocation();
			var role1 = ApplicationRoleFactory.CreateRole("role1", "this is a role.");
			role1.SetId(Guid.NewGuid());
			var role2 = ApplicationRoleFactory.CreateRole("role2", "this is an ohter role.");
			role2.SetId(Guid.NewGuid());
			_applicarionRoleRepository.Stub(x => x.LoadAll()).Return(new List<IApplicationRole> { role1, role2 });


			const string seatMapData = @"{""objects"":[{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":495,""top"":228.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""id"":""f19f90e8-f629-237b-2493-f2ed39e5c13b"",""name"":""Unnamed seat"",""priority"":1}],""background"":""""}";

			seatMapLocation.SetLocation(seatMapData, "rootLocation");
			seatMapLocation.SetId(Guid.NewGuid());
			var seat1 = seatMapLocation.AddSeat("Seat1", 1);
			seat1.SetRoles(new IApplicationRole[] {role1, role2});
			var seat2 = seatMapLocation.AddSeat("Seat2", 2);
			seat2.SetRoles(new IApplicationRole[] {role1, role2});
			_seatMapLocationRepository.Add(seatMapLocation);
			
			var command = new SaveSeatMapCommand()
			{
				Id = seatMapLocation.Id,
				SeatMapData = seatMapData,
				Seats = new[]
				{
					new SeatInfo
					{
						Id = seat1.Id, 
						IsNew = false, 
						Name = "Seat1", 
						RoleIdList = new []{role1.Id.Value}
					},
					new SeatInfo
					{
						Id = seat2.Id, 
						IsNew = false, 
						Name = "Seat1", 
						RoleIdList = new Guid[0]
					}
				}
			};

			_target.Save(command);

			var loadedLocation = _seatMapLocationRepository.First() as SeatMapLocation;
			loadedLocation.Seats.First().Roles.Count.Should().Be(1);
			loadedLocation.Seats.First().Roles.Single().Name.Should().Be("role1");
			loadedLocation.Seats.Second().Roles.Count.Should().Be(0);
		}

		[Test]
		public void ShouldUpdateNameAndPriorityForSeats()
		{
			var seatMapLocation = new SeatMapLocation();
			const string seatMapData = @"{""objects"":[{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":495,""top"":228.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""id"":""f19f90e8-f629-237b-2493-f2ed39e5c13b"",""name"":""Unnamed seat"",""priority"":1}],""background"":""""}";

			seatMapLocation.SetLocation(seatMapData, "rootLocation");
			seatMapLocation.SetId(Guid.NewGuid());
			var seat1 = seatMapLocation.AddSeat("1", 1);
			var seat2 = seatMapLocation.AddSeat("2", 2);
			_seatMapLocationRepository.Add(seatMapLocation);

			var command = new SaveSeatMapCommand()
			{
				Id = seatMapLocation.Id,
				SeatMapData = seatMapData,
				Seats = new[]
				{
					new SeatInfo
					{
						Id = seat1.Id,
						IsNew = false,
						Name = "3",
						Priority = 3
					},
					new SeatInfo
					{
						Id = seat2.Id,
						IsNew = false,
						Name = "4",
						Priority = 4
					}
				}
			};

			_target.Save(command);

			var loadedLocation = _seatMapLocationRepository.First() as SeatMapLocation;

			loadedLocation.Seats[0].Name.Should().Be("3");
			loadedLocation.Seats[0].Priority.Should().Be(3);
			loadedLocation.Seats[1].Name.Should().Be("4");
			loadedLocation.Seats[1].Priority.Should().Be(4);

		}


		[Test]
		public void ShouldUpdateTemporaryLocationAndSeatMapIds()
		{
			const string dummyJsonData = @"{""objects"":[{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":370,""top"":90.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""guid"":""d9664f22-886b-f5bf-f799-7d59765c2604"",""name"":""Unnamed seat"",""priority"":1},{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":565,""top"":90.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""guid"":""8e48dd65-e68a-0834-fdc5-eae75f12065c"",""name"":""Unnamed seat"",""priority"":2}],""background"":""""}";

			var command = new SaveSeatMapCommand()
			{
				SeatMapData = dummyJsonData,
				ChildLocations = new[] { _childLocationInfo, _childLocation2 },
				Seats = new[]
				{
					new SeatInfo(){ Id = Guid.Parse("{d9664f22-886b-f5bf-f799-7d59765c2604}"), IsNew = true, Name = "New Seat"},
					new SeatInfo(){ Id = Guid.Parse("{8e48dd65-e68a-0834-fdc5-eae75f12065c}"), IsNew = true, Name = "New Seat 2"}
				}
			};

			_target.Save(command);

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

			var command = new SaveSeatMapCommand()
			{
				Id = seatMapLocation.Id,
				SeatMapData = @"{""objects"":[{""type"":""i-text"",""originX"":""left"",""originY"":""top"",""left"":302,""top"":44,""width"":116.07,""height"":23.4,""fill"":""#000000"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":2.84,""scaleY"":2.84,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""text"":""Teleopti China"",""fontSize"":18,""fontWeight"":"""",""fontFamily"":""helvetica"",""fontStyle"":"""",""lineHeight"":1.3,""textDecoration"":"""",""textAlign"":""left"",""path"":null,""textBackgroundColor"":"""",""useNative"":true,""styles"":{}},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":638,""top"":256,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""bc10076d-def3-426e-9237-a45200971952"",""name"":""Chongqing"",""seatMapId"":""09ab3b5f-2cac-4306-a03a-a45200971952""}],""background"":""""}",
				ChildLocations = new[] { _childLocationInfo }
			};

			_target.Save(command);

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

			var seatBooking = new SeatBooking(new Person(), new DateOnly(2015, 03, 02), new DateTime(2015, 03, 02, 8, 0, 0, DateTimeKind.Utc), new DateTime(2015, 03, 02, 17, 0, 0, DateTimeKind.Utc));
			seatBooking.Book(seat);

			_seatBookingRepository.Add(seatBooking);

			var command = new SaveSeatMapCommand()
			{
				Id = seatMapLocation.Id,
				SeatMapData = @"{""objects"":[],""background"":""""}",
				ChildLocations = new LocationInfo[0]
			};

			_target.Save(command);

			Assert.IsTrue(!_seatMapLocationRepository.First().Seats.Any());
			Assert.IsTrue(!_seatBookingRepository.Any());

		}


		[Test]
		public void ShouldDeleteSeatPlanWhenDeletingLocationRemovesAllSeatBookingsForDay()
		{
			var seatMapLocation = new SeatMapLocation();
			var belongsToDate = new DateOnly(2015, 03, 02);

			const string seatMapData = @"{""objects"":[{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":495,""top"":228.5,""width"":36,""height"":47,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/Areas/SeatPlanner/Content/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""id"":""f19f90e8-f629-237b-2493-f2ed39e5c13b"",""name"":""Unnamed seat"",""priority"":1}],""background"":""""}";

			seatMapLocation.SetLocation(seatMapData, "rootLocation");
			seatMapLocation.SetId(Guid.NewGuid());
			var seat = seatMapLocation.AddSeat("Seat1", 1);
			_seatMapLocationRepository.Add(seatMapLocation);

			var seatBooking = new SeatBooking(new Person(), belongsToDate, new DateTime(2015, 03, 02, 8, 0, 0, DateTimeKind.Utc), new DateTime(2015, 03, 02, 17, 0, 0, DateTimeKind.Utc));
			seatBooking.Book(seat);

			_seatBookingRepository.Add(seatBooking);

			var seatPlan = new SeatPlan() { Date = belongsToDate, Status = SeatPlanStatus.Ok };
			seatPlan.SetId(Guid.NewGuid());
			_seatPlanRepository.Add(seatPlan);

			var command = new SaveSeatMapCommand()
			{
				Id = seatMapLocation.Id,
				SeatMapData = @"{""objects"":[],""background"":""""}",
				ChildLocations = new LocationInfo[0]
			};

			_target.Save(command);

			Assert.IsNull(_seatPlanRepository.GetSeatPlanForDate(belongsToDate));

		}

		[Test]
		public void ShouldNotDeleteSeatPlanWhenDeletingLocationRemovesASubsetOfSeatBookingsForDay()
		{

			var seatMapLocation = new SeatMapLocation();
			var belongsToDate = new DateOnly(2015, 03, 02);

			const string seatMapData = @"{""objects"":[{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":589.55,""top"":345.74,""width"":35.9,""height"":46.52,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/wfm/js/SeatManagement/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""id"":""09d00b0b-f366-4fa9-bd94-fa2af9025d7e"",""name"":""Unnamed seat"",""priority"":1},{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":645.45,""top"":345.74,""width"":35.9,""height"":46.52,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/wfm/js/SeatManagement/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""id"":""0af609cb-ba3e-fd55-4bc2-df0ec0dbfd77"",""name"":""Unnamed seat"",""priority"":2}],""background"":""""}";

			seatMapLocation.SetLocation(seatMapData, "rootLocation");
			seatMapLocation.SetId(Guid.NewGuid());
			var seat = seatMapLocation.AddSeat("Seat1", 1);
			seat.SetId(new Guid("09d00b0b-f366-4fa9-bd94-fa2af9025d7e"));
			var seat2 = seatMapLocation.AddSeat("Seat2", 2);
			seat2.SetId(new Guid("0af609cb-ba3e-fd55-4bc2-df0ec0dbfd77"));

			_seatMapLocationRepository.Add(seatMapLocation);

			var seatBooking = new SeatBooking(new Person(), belongsToDate, new DateTime(2015, 03, 02, 8, 0, 0, DateTimeKind.Utc), new DateTime(2015, 03, 02, 17, 0, 0, DateTimeKind.Utc));
			seatBooking.Book(seat);

			var seatBooking2 = new SeatBooking(new Person(), belongsToDate, new DateTime(2015, 03, 02, 17, 0, 1, DateTimeKind.Utc), new DateTime(2015, 03, 02, 22, 0, 0, DateTimeKind.Utc));
			seatBooking2.Book(seat2);

			_seatBookingRepository.Add(seatBooking);
			_seatBookingRepository.Add(seatBooking2);

			var seatPlan = new SeatPlan() { Date = belongsToDate, Status = SeatPlanStatus.Ok };
			seatPlan.SetId(Guid.NewGuid());
			_seatPlanRepository.Add(seatPlan);

			var command = new SaveSeatMapCommand()
			{
				Id = seatMapLocation.Id,
				SeatMapData = @"{""objects"":[{""type"":""seat"",""originX"":""left"",""originY"":""top"",""left"":589.55,""top"":345.74,""width"":35.9,""height"":46.52,""fill"":""rgb(0,0,0)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""src"":""http://localhost:52858/wfm/js/SeatManagement/Images/seat.svg"",""filters"":[],""crossOrigin"":"""",""alignX"":""none"",""alignY"":""none"",""meetOrSlice"":""meet"",""id"":""09d00b0b-f366-4fa9-bd94-fa2af9025d7e"",""name"":""Unnamed seat"",""priority"":1}],""background"":""""}",
				ChildLocations = new LocationInfo[0],
				Seats = new[]
				{
					new SeatInfo(){ Id = Guid.Parse("09d00b0b-f366-4fa9-bd94-fa2af9025d7e"), IsNew = false, Name = "New Seat"}
				}
			};

			_target.Save(command);

			Assert.IsNotNull(_seatPlanRepository.GetSeatPlanForDate(belongsToDate));

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

			var seatBooking = new SeatBooking(new Person(), new DateOnly(2015, 03, 02), new DateTime(2015, 03, 02, 8, 0, 0, DateTimeKind.Utc), new DateTime(2015, 03, 02, 17, 0, 0, DateTimeKind.Utc));
			seatBooking.Book(seat);

			_seatBookingRepository.Add(seatBooking);

			var command = new SaveSeatMapCommand()
			{
				Id = seatMapLocation.Id,
				SeatMapData = @"{""objects"":[],""background"":""""}",
				ChildLocations = new LocationInfo[0]
			};

			_target.Save(command);

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

			var command = new SaveSeatMapCommand()
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
					new SeatInfo(){ Id = Guid.Parse("{8e48dd65-e68a-0834-fdc5-eae75f12065c}"), IsNew = true, Name = "New Seat 2"}
				}
			};

			_target.Save(command);

			Assert.IsTrue(_seatMapLocationRepository.Count() == 3);
			var childSeatMapLocation = _seatMapLocationRepository.Load(childSeatMapLocation2.Id.Value);
			Assert.IsTrue(childSeatMapLocation.Seats.Count == 1);
			Assert.IsTrue(Equals(_seatMapLocationRepository.Load(childSeatMapLocation2.Id.Value).Seats[0], seat2));
		}
	}
}
