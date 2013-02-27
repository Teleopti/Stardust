using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class PersonScheduleHubTest
	{

		private static dynamic FakeCaller<T>(string methodName, Action<T> action)
		{
			IDictionary<string, object> caller = new ExpandoObject();
			caller[methodName] = action;
			return caller;
		}

		private static void SetupHub(PersonScheduleHub target, dynamic caller)
		{
			target.Context = new HubCallerContext(null, "connection");
			target.Groups = MockRepository.GenerateMock<IGroupManager>();
			target.Clients.Caller = caller;
		}

		[Test]
		public void ShouldCreateViewModelOnSubscribe()
		{
			var viewModelFactory = MockRepository.GenerateMock<IPersonScheduleViewModelFactory>();
			var target = new PersonScheduleHub(viewModelFactory);
			SetupHub(target, FakeCaller("incomingPersonSchedule", new Action<PersonScheduleViewModel>(a => { })));
			var personId = Guid.NewGuid();


			target.SubscribePersonSchedule(personId, DateTime.Today);

			viewModelFactory.AssertWasCalled(x => x.CreateViewModel(personId, DateTime.Today));
		}

		[Test]
		public void ShouldPushDataToCallerOnSubscribe()
		{
			var viewModelFactory = MockRepository.GenerateMock<IPersonScheduleViewModelFactory>();
			var target = new PersonScheduleHub(viewModelFactory);
			object actual = null;
			SetupHub(target, FakeCaller<PersonScheduleViewModel>("incomingPersonSchedule", a => { actual = a; }));
			var personId = Guid.NewGuid();
			var data = new PersonScheduleViewModel();

			viewModelFactory.Stub(x => x.CreateViewModel(personId, DateTime.Today)).Return(data);

			target.SubscribePersonSchedule(personId, DateTime.Today);

			actual.Should().Be.SameInstanceAs(data);
		}
	}

}