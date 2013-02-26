using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
	public class TeamScheduleHubTest
	{
		private IPersonScheduleDayReadModelRepository personScheduleDayReadModelRepository;
		private TeamScheduleHub hub;

		[SetUp]
		public void Setup()
		{
			personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelRepository>();
			hub = new TeamScheduleHub(personScheduleDayReadModelRepository);
		}

		[Test]
		public void ShouldGetSchedulesForDateAndTeam()
		{
			var period = new DateTimePeriod(2012, 12, 01,2012,12,01);
			var teamId = Guid.NewGuid();
			var model = new PersonScheduleDayReadModel
			            	{
			            		Shift =
			            			"{\"FirstName\":\"Jon\",\"LastName\":\"Kleinsmith\",\"EmploymentNumber\":\"137577\",\"Id\":\"b46a2588-8861-42e3-ab03-9b5e015b257c\",\"Date\":\"2011-09-01T00:00:00\",\"WorkTimeMinutes\":480,\"ContractTimeMinutes\":480,\"Projection\":[{\"Color\":\"#00FF00\",\"Start\":\"2011-09-01T07:30:00Z\",\"End\":\"2011-09-01T09:45:00Z\",\"Minutes\":135,\"Title\":\"Phone\"},{\"Color\":\"#FF0000\",\"Start\":\"2011-09-01T09:45:00Z\",\"End\":\"2011-09-01T10:00:00Z\",\"Minutes\":15,\"Title\":\"Short break\"},{\"Color\":\"#00FF00\",\"Start\":\"2011-09-01T10:00:00Z\",\"End\":\"2011-09-01T12:00:00Z\",\"Minutes\":120,\"Title\":\"Phone\"},{\"Color\":\"#FFFF00\",\"Start\":\"2011-09-01T12:00:00Z\",\"End\":\"2011-09-01T13:00:00Z\",\"Minutes\":60,\"Title\":\"Lunch\"},{\"Color\":\"#00FF00\",\"Start\":\"2011-09-01T13:00:00Z\",\"End\":\"2011-09-01T14:45:00Z\",\"Minutes\":105,\"Title\":\"Phone\"},{\"Color\":\"#FF0000\",\"Start\":\"2011-09-01T14:45:00Z\",\"End\":\"2011-09-01T15:00:00Z\",\"Minutes\":15,\"Title\":\"Short break\"},{\"Color\":\"#00FF00\",\"Start\":\"2011-09-01T15:00:00Z\",\"End\":\"2011-09-01T16:30:00Z\",\"Minutes\":90,\"Title\":\"Phone\"}]}"
			            	};

			personScheduleDayReadModelRepository.Stub(x => x.ForTeam(period.ChangeEndTime(TimeSpan.FromHours(25)), teamId)).Return(new[] { model });

			bool called = false;
			dynamic o = new ExpandoObject();
			o.incomingTeamSchedule = new Action<IEnumerable<object>>(result =>
				{
					called = true;
					JObject shift = (JObject)result.Single();
					shift["EmploymentNumber"].Value<string>().Should().Be.EqualTo("137577");
					((JArray)shift["Projection"])[0]["Title"].Value<string>().Should().Be.EqualTo("Phone");
				});
			hub.Groups = MockRepository.GenerateMock<IGroupManager>();
			hub.Context = new HubCallerContext(MockRepository.GenerateMock<Microsoft.AspNet.SignalR.IRequest>(),"connectionid");
			hub.Clients.Caller = o;
			hub.SubscribeTeamSchedule(teamId,period.StartDateTime);

			called.Should().Be.True();
		}

		[TearDown]
		public void Teardown()
		{
			hub.Dispose();
		}
	}
}