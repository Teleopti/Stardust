using System;
using NUnit.Framework;
using PerformanceTests.Database;

namespace PerformanceTests
{
	[TestFixture]
	public class StardustPerformanceTests 
	{
		private DatabaseHelper _databaseHelper;
		private HttpSender _httpSender;

		[TestFixtureSetUp]
		public void TextFixtureSetUp()
		{
			_httpSender = new HttpSender();

			//create DB
			_databaseHelper = new DatabaseHelper();
			_databaseHelper.Create();

			//Run App

		}

		[Test]
		public async void StardustLoadPerformance()
		{
			var resp = await _httpSender.TryGetAsync(new Uri("http://localhost:9000/stardustdashboard/job"));
			
			Assert.IsTrue(resp);
		}

	}
}
