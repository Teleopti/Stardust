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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	internal class AddSeatMapCommandHandlerTest
	{
		private FakeWriteSideRepository<ISeatMap> _seatMapRepository;
		private IBusinessUnitRepository _buRepository;
		private ICurrentBusinessUnit _currentBusinessUnit;
		private LocationInfo _childLocation1;
		private AddSeatMapCommandHandler _target;
		private LocationInfo _childLocation2;

		[SetUp]
		public void Setup()
		{
			_seatMapRepository = new FakeWriteSideRepository<ISeatMap>();

			var bu = new BusinessUnit("bu").WithId();
			_buRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			_currentBusinessUnit = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			_currentBusinessUnit.Stub(x => x.Current()).Return(bu);
			_buRepository.Stub(x => x.Get(_currentBusinessUnit.Current().Id.GetValueOrDefault())).Return(bu);

			_target = new AddSeatMapCommandHandler(_seatMapRepository, _buRepository, _currentBusinessUnit);
			_childLocation1 = new LocationInfo()
			{
				Name = "Chongqing",
				IsNew = true,
				Id = new Guid("bc10076d-def3-426e-9237-a45200971952"),
				SeatMapId = new Guid("09ab3b5f-2cac-4306-a03a-a45200971952")
			};
			_childLocation2 = new LocationInfo()
			{
				Name = "Shenzhen",
				IsNew = true,
				Id = new Guid("6291c86e-3679-4df9-b2e5-a45200973fb6"),
				SeatMapId = new Guid("025e5bc6-1e04-4fe9-ab74-a45200973fb6")
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
			var seatMap = _seatMapRepository.Single() as SeatMap;

			seatMap.Id.Should().Have.Value();
			seatMap.SeatMapJsonData.Should().Be(dummyJsonData);
			seatMap.Location.Name.Should().Be(_currentBusinessUnit.Current().Name);
			seatMap.Location.Parent.Should().Be(seatMap);
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
				ChildLocations = new[] { _childLocation1, _childLocation2 },
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = operatedPersonId,
					TrackId = trackId
				}
			};

			_target.Handle(command);

			var seatMap = _seatMapRepository.First() as SeatMap;
			var childSeatMap = _seatMapRepository.Last() as SeatMap;

			childSeatMap.Location.Should().Not.Be.Null();
			childSeatMap.Location.Name.Should().Be("Shenzhen");
			childSeatMap.Location.ParentLocation.Should().Be(seatMap.Location);

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
				Seats=new[]
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

			var seatMap = _seatMapRepository.First() as SeatMap;
			seatMap.Location.SeatCount.Should().Be (2);
			seatMap.Location.Seats.First().Name.Should().Be ("New Seat");
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
				ChildLocations = new[] { _childLocation1, _childLocation2 },
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

			var seatMap = _seatMapRepository.First() as SeatMap;
			seatMap.SeatMapJsonData.Should().Not.Equals (dummyJsonData);
			seatMap.SeatMapJsonData.Should().Not.Contain (_childLocation1.Id.ToString());
			seatMap.SeatMapJsonData.Should().Not.Contain(_childLocation2.Id.ToString());
			seatMap.SeatMapJsonData.Should().Contain (seatMap.Location.Seats.First().Id.ToString());
			seatMap.SeatMapJsonData.Should().Contain(seatMap.Location.Seats.Last().Id.ToString());
		}


		[Test]
		public void ShouldDeleteChildSeatMaps()
		{
			var seatMap = new SeatMap();
			const string seatMapData = @"{""objects"":[{""type"":""i-text"",""originX"":""left"",""originY"":""top"",""left"":302,""top"":44,""width"":116.07,""height"":23.4,""fill"":""#000000"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":2.84,""scaleY"":2.84,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""text"":""Teleopti China"",""fontSize"":18,""fontWeight"":"""",""fontFamily"":""helvetica"",""fontStyle"":"""",""lineHeight"":1.3,""textDecoration"":"""",""textAlign"":""left"",""path"":null,""textBackgroundColor"":"""",""useNative"":true,""styles"":{}},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":18,""top"":259,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""6291c86e-3679-4df9-b2e5-a45200973fb6"",""name"":""Shenzhen"",""seatMapId"":""025e5bc6-1e04-4fe9-ab74-a45200973fb6""},""seatMapId"":""187d4c70-6158-4a38-8612-a4520098468b""},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":638,""top"":256,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""bc10076d-def3-426e-9237-a45200971952"",""name"":""Chongqing"",""seatMapId"":""09ab3b5f-2cac-4306-a03a-a45200971952""}],""background"":""""}";
			seatMap.CreateSeatMap(seatMapData, "rootLocation");
			var childSeatMap1 = seatMap.CreateChildSeatMap(_childLocation1);
			var childSeatMap2 = seatMap.CreateChildSeatMap(_childLocation2);

			seatMap.Location.SetId(Guid.NewGuid());
			childSeatMap1.Location.SetId (Guid.NewGuid());
			childSeatMap2.Location.SetId(Guid.NewGuid());
			
			_seatMapRepository.Add (seatMap);
			_seatMapRepository.Add(childSeatMap1);
			_seatMapRepository.Add(childSeatMap2);

			var lastChildSeatMap = _seatMapRepository.Last() as SeatMap;

			_childLocation1.IsNew = false;
			_childLocation1.Id = childSeatMap1.Location.Id;
			_childLocation1.SeatMapId = childSeatMap1.Id;

			var command = new AddSeatMapCommand()
			{
				Id = seatMap.Id,
				SeatMapData = @"{""objects"":[{""type"":""i-text"",""originX"":""left"",""originY"":""top"",""left"":302,""top"":44,""width"":116.07,""height"":23.4,""fill"":""#000000"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":2.84,""scaleY"":2.84,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""text"":""Teleopti China"",""fontSize"":18,""fontWeight"":"""",""fontFamily"":""helvetica"",""fontStyle"":"""",""lineHeight"":1.3,""textDecoration"":"""",""textAlign"":""left"",""path"":null,""textBackgroundColor"":"""",""useNative"":true,""styles"":{}},{""type"":""location"",""originX"":""left"",""originY"":""top"",""left"":638,""top"":256,""width"":300,""height"":200,""fill"":""rgba(255,0,0,0.5)"",""stroke"":null,""strokeWidth"":1,""strokeDashArray"":null,""strokeLineCap"":""butt"",""strokeLineJoin"":""miter"",""strokeMiterLimit"":10,""scaleX"":1,""scaleY"":1,""angle"":0,""flipX"":false,""flipY"":false,""opacity"":1,""shadow"":null,""visible"":true,""clipTo"":null,""backgroundColor"":"""",""fillRule"":""nonzero"",""globalCompositeOperation"":""source-over"",""rx"":0,""ry"":0,""id"":""bc10076d-def3-426e-9237-a45200971952"",""name"":""Chongqing"",""seatMapId"":""09ab3b5f-2cac-4306-a03a-a45200971952""}],""background"":""""}",
				ChildLocations = new[] { _childLocation1 },
				TrackedCommandInfo = new TrackedCommandInfo()
				{
					OperatedPersonId = Guid.NewGuid(),
					TrackId = Guid.NewGuid()
				}
			};

			_target.Handle(command);

			lastChildSeatMap.Location.Name.Should().Be("Shenzhen");
			var childSeatMap = _seatMapRepository.Last() as SeatMap;
			childSeatMap.Location.Name.Should().Be("Chongqing");

		}

	}
}