using System;
using System.Linq;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldCreateViewModelOnSubscribe()
		{
			var viewModelFactory = MockRepository.GenerateMock<IPersonScheduleViewModelFactory>();
			var target = new PersonScheduleHub(viewModelFactory);
			var hubBuilder = new TestHubBuilder();
			hubBuilder.SetupHub(target, hubBuilder.FakeCaller("incomingPersonSchedule", new Action<PersonScheduleViewModel>(a => { })));
			var personId = Guid.NewGuid();

			target.PersonSchedule(personId, DateTime.Today);

			viewModelFactory.AssertWasCalled(x => x.CreateViewModel(personId, DateTime.Today));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldPushDataToCallerOnSubscribe()
		{
			var viewModelFactory = MockRepository.GenerateMock<IPersonScheduleViewModelFactory>();
			var target = new PersonScheduleHub(viewModelFactory);
			object actual = null;
			var hubBuilder = new TestHubBuilder();
			hubBuilder.SetupHub(target, hubBuilder.FakeCaller("incomingPersonSchedule", new Action<PersonScheduleViewModel>(a => { actual = a; })));
			var personId = Guid.NewGuid();
			var data = new PersonScheduleViewModel();

			viewModelFactory.Stub(x => x.CreateViewModel(personId, DateTime.Today)).Return(data);

			target.PersonSchedule(personId, DateTime.Today);

			actual.Should().Be.SameInstanceAs(data);
		}

	}

}