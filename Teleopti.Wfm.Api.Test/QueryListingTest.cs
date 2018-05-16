﻿using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace Teleopti.Wfm.Api.Test
{
	[ApiTest]
	public class QueryListingTest
	{
		public IApiHttpClient Client;

		[Test]
		public async Task ShouldListQueries()
		{
			Client.Authorize();

			var result = await Client.GetAsync("/query");
			result.EnsureSuccessStatusCode();

			var content = await result.Content.ReadAsStringAsync();
			var obj = JToken.Parse(content);
			obj["Result"].Children().Should().Not.Be.Empty();
		}
	}
}