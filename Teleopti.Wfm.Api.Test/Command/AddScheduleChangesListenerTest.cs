using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Api.Test.Command
{
	[ApiTest]
	[LoggedOnAppDomain]
	public class AddScheduleChangesListenerTest
	{
		public IApiHttpClient Client;
		public FakeGlobalSettingDataRepository GlobalSettingDataRepository;

		[Test]
		public async Task ShouldAddNewScheduleChangesListener()
		{
			Client.Authorize();

			var result = await Client.PostAsync("/command/AddScheduleChangesListener",
				new StringContent(JsonConvert.SerializeObject(new
				{
					Name = "NameOfScheduleChangeListener",
					Url = "http://endpoint/",
					DaysStartFromCurrentDate = -1,
					DaysEndFromCurrentDate = 1
				}), Encoding.UTF8, "application/json"));
			result.EnsureSuccessStatusCode();

			var persistedListener = GlobalSettingDataRepository
				.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions())
				.Subscriptions()
				.First();

			persistedListener.Name.Should().Be.EqualTo("NameOfScheduleChangeListener");
			persistedListener.Uri.Should().Be.EqualTo("http://endpoint/");

		}

		[Test]
		public async Task ShouldRejectIfInvalidUrl()
		{
			Client.Authorize();

			var result = await Client.PostAsync("/command/AddScheduleChangesListener",
				new StringContent(JsonConvert.SerializeObject(new
				{
					Name = "NameOfScheduleChangeListener",
					Url = "InvalidUrl",
					DaysStartFromCurrentDate = -1,
					DaysEndFromCurrentDate = 1
				}), Encoding.UTF8, "application/json"));
			JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync())["Successful"]
				.Value<bool>().Should().Be.False();

			GlobalSettingDataRepository
				.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions())
				.Subscriptions().Any().Should().Be.False();
		}


		[Test]
		public async Task ShouldRejectIfEndDateIsLessThanStartDate()
		{
			Client.Authorize();

			var result = await Client.PostAsync("/command/AddScheduleChangesListener",
				new StringContent(JsonConvert.SerializeObject(new
				{
					Name = "NameOfScheduleChangeListener",
					Url = "http://endpoint/",
					DaysStartFromCurrentDate = 1,
					DaysEndFromCurrentDate = -1
				}), Encoding.UTF8, "application/json"));
			JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync())["Successful"]
				.Value<bool>().Should().Be.False();

			GlobalSettingDataRepository
				.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions())
				.Subscriptions().Any().Should().Be.False();
		}

		[Test]
		public async Task ShouldRejectIfNameIsEmpty()
		{
			Client.Authorize();

			var result = await Client.PostAsync("/command/AddScheduleChangesListener",
				new StringContent(JsonConvert.SerializeObject(new
				{
					Name = "",
					Url = "http://endpoint/",
					DaysStartFromCurrentDate = -1,
					DaysEndFromCurrentDate = 1
				}), Encoding.UTF8, "application/json"));
			JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync())["Successful"]
				.Value<bool>().Should().Be.False();

			GlobalSettingDataRepository
				.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions())
				.Subscriptions().Any().Should().Be.False();
		}

		[Test]
		public async Task ShouldRejectIfNameIsWhitespace()
		{
			Client.Authorize();

			var result = await Client.PostAsync("/command/AddScheduleChangesListener",
				new StringContent(JsonConvert.SerializeObject(new
				{
					Name = " ",
					Url = "http://endpoint/",
					DaysStartFromCurrentDate = -1,
					DaysEndFromCurrentDate = 1
				}), Encoding.UTF8, "application/json"));
			JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync())["Successful"]
				.Value<bool>().Should().Be.False();

			GlobalSettingDataRepository
				.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions())
				.Subscriptions().Any().Should().Be.False();
		}


		[Test, FakePermissions]
		public async Task ShouldRejectIfNotSufficientPermissions()
		{
			Client.Authorize(); 

			var result = await Client.PostAsync("/command/AddScheduleChangesListener",
				new StringContent(JsonConvert.SerializeObject(new
				{
					Name = "Name",
					Url = "http://endpoint/",
					DaysStartFromCurrentDate = -1,
					DaysEndFromCurrentDate = 1
				})));
			JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync())["Successful"]
				.Value<bool>().Should().Be.False();

			GlobalSettingDataRepository
				.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions())
				.Subscriptions().Any().Should().Be.False();
		}
	}
}
