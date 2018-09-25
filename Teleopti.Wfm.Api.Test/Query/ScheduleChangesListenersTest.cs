using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Api.Test.Query
{
	[ApiTest]
	public class ScheduleChangesListenersTest
	{
		public IApiHttpClient Client;
		public FakeGlobalSettingDataRepository GlobalSettingDataRepository;

		[Test]
		public void ShouldGetScheduleChangesListeners()
		{
			const string name = "NameOfScheduleChangeListener";
			const string url = "http://endpoint/";
			const int daysStartFromCurrentDate = -1;
			const int daysEndFromCurrentDate = 1;

			Client.Authorize();
			Client.PostAsync("/command/AddScheduleChangesListener",
				new StringContent(JsonConvert.SerializeObject(new
				{
					Name = name,
					Url = url,
					DaysStartFromCurrentDate = daysStartFromCurrentDate,
					DaysEndFromCurrentDate = daysEndFromCurrentDate
				}), Encoding.UTF8, "application/json")).Result.EnsureSuccessStatusCode();

			var result =
				Client.PostAsync("/query/ScheduleChangesListenerSubscription/AllScheduleChangesListenerSubscription",
					new StringContent("{}"));

			var obj =
				JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)["Result"][0];

			var listener = obj["Listeners"].First();
			listener["Name"].Value<string>().Should().Be.EqualTo(name);
			listener["Url"].Value<string>().Should().Be.EqualTo(url);
			listener["DaysStartFromCurrentDate"].Value<int>().Should().Be.EqualTo(daysStartFromCurrentDate);
			listener["DaysEndFromCurrentDate"].Value<int>().Should().Be.EqualTo(daysEndFromCurrentDate);
		}

		[Test]
		public void ShouldIncludePublicKeyForSignatureValidation()
		{
			Client.Authorize();
			var result =
				Client.PostAsync("/query/ScheduleChangesListenerSubscription/AllScheduleChangesListenerSubscription",
					new StringContent("{}", Encoding.UTF8, "application/json"));

			var obj =
				JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result)["Result"][0];

			obj["Modulus"].Value<string>().Should().Be.EqualTo(
				"tcQWMgdpQeCd8+gzB3rYQAehHXF5mBGdyFMkJMEmcQmTlkpg22xLNz/kNYXZ7j2Cuhls+PBORzZkfBsNoL1vErT+N9Es4EEWOt6ntNe7wujqQqktUT/QOWEMJ8zJQM3bn7Oj9H5StBr7DWSRzgEjOc7knDcb4KCQL3ceXqmqwSonPfP1hp+bE8rZuxDISYiZVEkm417YzUHBk3ppV30Q9zvfL9IZX0q/ebCTRnLFockl7yOVucomvo8j4ssFPCAYgASoNvzWq+s5UTzYELl1I7F3hQnFwx0bIpQFmGbZ5BbNczc6rVYtCX5KDMsVaJSUcXBAnqGd20hq/ICkBR658w==");
			obj["Exponent"].Value<string>().Should().Be.EqualTo("AQAB");
		}

		[Test, FakePermissions]
		public void ShouldRejectIfNotSufficientPermissions()
		{
			Client.Authorize();

			var result =
				Client.PostAsync("/query/ScheduleChangesListenerSubscription/AllScheduleChangesListenerSubscription",
					new StringContent("{}", Encoding.UTF8, "application/json"));

			var obj = JObject.Parse(result.Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result);

			obj["Successful"].Value<bool>().Should().Be.False();
			obj["Result"].Value<string>().Should().Be.Null();
		}
	}
}
