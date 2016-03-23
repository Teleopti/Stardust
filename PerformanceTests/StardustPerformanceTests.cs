using System;
using System.Configuration;
using System.Diagnostics;
using NUnit.Framework;
using Performance.Test.Database;
using PerformanceTests;
using PerformanceTests.Database;

namespace Performance.Test
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
			Process.Start(ConfigurationManager.AppSettings["ManagerExeLocation"]);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			Process[] managerProcesses = Process.GetProcessesByName("ManagerConsoleHost");
			foreach (Process process in managerProcesses)
			{
				process.Kill();
			}

			Process[] NodeProcesses = Process.GetProcessesByName("NodeConsoleHost");
			foreach (Process process in NodeProcesses)
			{
				process.Kill();
			}
		}

		[Test]
		public async void StardustLoadPerformance()
		{
			var resp =  await _httpSender.GetAsync(new Uri("http://localhost:9000/stardustdashboard/job"));
			resp.EnsureSuccessStatusCode();
			Assert.IsTrue(true);
		}
	}
}
