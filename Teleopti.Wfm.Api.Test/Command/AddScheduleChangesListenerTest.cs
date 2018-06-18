using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Api.Test.Command
{
	[ApiTest]
	public class AddScheduleChangesListenerTest
	{
		public IApiHttpClient Client;
		public FakeGlobalSettingDataRepository GlobalSettingDataRepository;

		[Test]
		public void ShouldAddNewScheduleChangesListener()
		{
			Client.Authorize();

			var result = Client.PostAsync("/command/AddScheduleChangesListener",
				new StringContent(JsonConvert.SerializeObject(new
				{
					Name = "NameOfScheduleChangeListener",
					Url = "http://endpoint/",
					DaysStartFromCurrentDate = -1,
					DaysEndFromCurrentDate = 1
				})));
			result.Result.EnsureSuccessStatusCode();

			var persistedListener = GlobalSettingDataRepository
				.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions())
				.Subscriptions()
				.First();

			persistedListener.Name.Should().Be.EqualTo("NameOfScheduleChangeListener");
			persistedListener.Uri.Should().Be.EqualTo("http://endpoint/");

		}

		[Test]
		public void ShouldRejectIfInvalidUrl()
		{
			Client.Authorize();

			var result = Client.PostAsync("/command/AddScheduleChangesListener",
				new StringContent(JsonConvert.SerializeObject(new
				{
					Name = "NameOfScheduleChangeListener",
					Url = "InvalidUrl",
					DaysStartFromCurrentDate = -1,
					DaysEndFromCurrentDate = 1
				})));
			JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)["Successful"]
				.Value<bool>().Should().Be.False();

			GlobalSettingDataRepository
				.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions())
				.Subscriptions().Any().Should().Be.False();
		}


		[Test]
		public void ShouldRejectIfEndDateIsLessThanStartDate()
		{
			Client.Authorize();

			var result = Client.PostAsync("/command/AddScheduleChangesListener",
				new StringContent(JsonConvert.SerializeObject(new
				{
					Name = "NameOfScheduleChangeListener",
					Url = "http://endpoint/",
					DaysStartFromCurrentDate = 1,
					DaysEndFromCurrentDate = -1
				})));
			JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)["Successful"]
				.Value<bool>().Should().Be.False();

			GlobalSettingDataRepository
				.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions())
				.Subscriptions().Any().Should().Be.False();
		}

		[Test]
		public void ShouldRejectIfNameIsEmpty()
		{
			Client.Authorize();

			var result = Client.PostAsync("/command/AddScheduleChangesListener",
				new StringContent(JsonConvert.SerializeObject(new
				{
					Name = "",
					Url = "http://endpoint/",
					DaysStartFromCurrentDate = -1,
					DaysEndFromCurrentDate = 1
				})));
			JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)["Successful"]
				.Value<bool>().Should().Be.False();

			GlobalSettingDataRepository
				.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions())
				.Subscriptions().Any().Should().Be.False();
		}

		[Test]
		public void ShouldRejectIfNameIsWhitespace()
		{
			Client.Authorize();

			var result = Client.PostAsync("/command/AddScheduleChangesListener",
				new StringContent(JsonConvert.SerializeObject(new
				{
					Name = " ",
					Url = "http://endpoint/",
					DaysStartFromCurrentDate = -1,
					DaysEndFromCurrentDate = 1
				})));
			JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)["Successful"]
				.Value<bool>().Should().Be.False();

			GlobalSettingDataRepository
				.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions())
				.Subscriptions().Any().Should().Be.False();
		}


		[Test, FakePermissions]
		public void ShouldRejectIfNotSufficientPermissions()
		{
			Client.Authorize(); 

			var result = Client.PostAsync("/command/AddScheduleChangesListener",
				new StringContent(JsonConvert.SerializeObject(new
				{
					Name = "Name",
					Url = "http://endpoint/",
					DaysStartFromCurrentDate = -1,
					DaysEndFromCurrentDate = 1
				})));
			JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)["Successful"]
				.Value<bool>().Should().Be.False();

			GlobalSettingDataRepository
				.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions())
				.Subscriptions().Any().Should().Be.False();
		}
	}
}
