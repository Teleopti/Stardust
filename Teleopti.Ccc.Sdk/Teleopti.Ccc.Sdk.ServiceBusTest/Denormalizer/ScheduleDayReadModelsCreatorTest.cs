using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class ScheduleDayReadModelsCreatorTest
	{
		private MockRepository _mocks;
		private ScheduleDayReadModelsCreator _target;
		private IPersonRepository _personRepository;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_personRepository = _mocks.DynamicMock<IPersonRepository>();
			
			_target = new ScheduleDayReadModelsCreator(_personRepository);
		}

		[Test]
		public void ShouldGetSchedulesAndCallCreator()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

			Expect.Call(_personRepository.Get(person.Id.GetValueOrDefault())).Return(person);

			_mocks.ReplayAll();

			var ret =
				_target.GetReadModels(new DenormalizedSchedule
				                      	{
				                      		ContractTime = TimeSpan.FromHours(8),
				                      		WorkTime = TimeSpan.FromHours(7),
				                      		Date = new DateTime(2012, 12, 2),
											PersonId = person.Id.GetValueOrDefault(),
											StartDateTime = new DateTime(2012,12,1,8,0,0,DateTimeKind.Utc),
											EndDateTime = new DateTime(2012,12,1,17,0,0,DateTimeKind.Utc),
				                      	});

			ret.ContractTime.Should().Be.EqualTo(TimeSpan.FromHours(8));
			ret.WorkTime.Should().Be.EqualTo(TimeSpan.FromHours(7));
			ret.StartDateTime.Should().Be.EqualTo(new DateTime(2012, 12, 1, 9, 0, 0));
			ret.EndDateTime.Should().Be.EqualTo(new DateTime(2012, 12, 1, 18, 0, 0));
			
			_mocks.VerifyAll();
		}
	}


}