using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TeamScheduleTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<AgentScheduleViewModelReworkedMapper>().As<IAgentScheduleViewModelReworkedMapper>().SingleInstance();
			builder.RegisterInstance(new FakePermissionProvider()).As<IPermissionProvider>().SingleInstance();
			builder.RegisterInstance(new FakeLayerViewModelReworkedMapper()).As<ILayerViewModelReworkedMapper>().SingleInstance();
			builder.RegisterInstance(new FakePersonNameProvider()).As<IPersonNameProvider>().SingleInstance();
		}
	}

	[TestFixture]
	[TeamScheduleTestAttribute]
	public class AgentScheduleViewModelReworkedMapperTest
	{
		public IAgentScheduleViewModelReworkedMapper Mapper;

		[Test]
		public void ShouldMap()
		{
			var personSchedule = new PersonSchedule()
			{
				Person = PersonFactory.CreatePerson(),
				Schedule = new PersonScheduleDayReadModel()
			};
			var target = Mapper.Map(personSchedule);
			target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMapName()
		{
			var personSchedule = new PersonSchedule()
			{
				Person = PersonFactory.CreatePerson(),
				Schedule = new PersonScheduleDayReadModel()
				{
					FirstName = "AA",LastName = "BB"
				}
			};
			var target = Mapper.Map(personSchedule);
			target.Name.Should().Be.EqualTo("BB AA");
		}

		[Test]
		public void ShoudMapPersonId()
		{
			var personId = Guid.NewGuid();
			var personSchedule = new PersonSchedule()
			{
				Person = PersonFactory.CreatePerson(),
				Schedule = new PersonScheduleDayReadModel()
				{
					PersonId = personId
				}
			};
			var target = Mapper.Map(personSchedule);
			target.PersonId.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldMapStartTimeUtc()
		{
			var model = new Model()
			{
				Shift = new Shift() {Projection = new List<SimpleLayer>(){new SimpleLayer()}}
			};
			var personSchedule = new PersonSchedule()
			{
				Person = PersonFactory.CreatePerson(),
				Schedule = new PersonScheduleDayReadModel()
				{
					Start = new DateTime(2015,5,11),
					Model = JsonConvert.SerializeObject(model)
				}
			};
			var target = Mapper.Map(personSchedule);
			target.StartTimeUtc.Should().Be.EqualTo(new DateTime(2015,5,11)); 
		}

		[Test]
		public void ShouldMapMinStart()
		{
			var model = new Model()
			{
				Shift = new Shift() { Projection = new List<SimpleLayer>() { new SimpleLayer() } }
			};
			var personSchedule = new PersonSchedule()
			{
				Person = PersonFactory.CreatePerson(),
				Schedule = new PersonScheduleDayReadModel()
				{
					MinStart = new DateTime(2015, 5, 11),
					Model = JsonConvert.SerializeObject(model)
				}
			};
			var target = Mapper.Map(personSchedule);
			target.MinStart.Should().Be.EqualTo(new DateTime(2015, 5, 11));
		}

		[Test]
		public void ShoudMapIsDayOff()
		{
			var model = new Model()
			{
				Shift = new Shift() { Projection = new List<SimpleLayer>() { new SimpleLayer() } },
				DayOff = new DayOff() { Start = new DateTime(2015,5,11),End = new DateTime(2015,5,11).AddHours(11)}
			};
			var personSchedule = new PersonSchedule()
			{
				Person = PersonFactory.CreatePerson(),
				Schedule = new PersonScheduleDayReadModel()
				{	
					Model = JsonConvert.SerializeObject(model)
				}
			};
			var target = Mapper.Map(personSchedule);
			target.IsDayOff.Should().Be.EqualTo(true);
		}
		
		[Test]
		public void ShouldMapTotal()
		{
			var personSchedule = new PersonSchedule()
			{
				Person = PersonFactory.CreatePerson(),
				Schedule = new PersonScheduleDayReadModel()
				{	
					Total = 1
				}
			};
			var target = Mapper.Map(personSchedule);
			target.Total.Should().Be.EqualTo(1);
		}
		
		[Test]
		public void ShouldMapEmptyAgentScheduleWhenScheduleNotPublished()
		{
			var personSchedule = new PersonSchedule()
			{
				Person = PersonFactory.CreatePerson(),
				Schedule = new PersonScheduleDayReadModel()
				{	
					Total = 1
				}
			};
			Mapper = new AgentScheduleViewModelReworkedMapper
				(
				new FakeLayerViewModelReworkedMapper(),
				new FakePersonNameProvider(),
				new FakePermissionProvider()
					{
						isPersonSchedulePublished = false
					});

			var target = Mapper.Map(personSchedule);
			target.MinStart.Should().Be.EqualTo(null);
		}
	}

	public class FakePersonNameProvider : IPersonNameProvider
	{
		public string BuildNameFromSetting(Name name)
		{
			throw new System.NotImplementedException();
		}

		public string BuildNameFromSetting(string firstName, string lastName)
		{
			return lastName + " " + firstName;
		}
	}

	public class FakeLayerViewModelReworkedMapper : ILayerViewModelReworkedMapper
	{
		public IEnumerable<LayerViewModelReworked> Map(IEnumerable<SimpleLayer> sourceLayers)
		{
			return sourceLayers.Select(Map);
		}

		public LayerViewModelReworked Map(SimpleLayer sourceLayer)
		{
			throw new System.NotImplementedException();
		}
	}
}
