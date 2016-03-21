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
		[Category("StardustLoadPerformance")]
		public void StardustLoadPersomance()
		{
			_httpSender.GetAsync(new Uri("http://localhost:9000/stardustdashboard/job"));
			
			Assert.IsTrue(true);
		}

	}
}
