using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	[Toggle(Toggles.ETL_EventbasedDate_39562)]
	public class AnalyticsDateRepositoryWithCreationTest
	{
		public IAnalyticsDateRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;

		[Test]
		public void ShouldCreateMissingDatesWhenLoading()
		{
			var targetDate = new DateTime(2000, 01, 05);
			var date = WithAnalyticsUnitOfWork.Get(() => Target.Date(targetDate));
			date.DateDate.Date.Should().Be.EqualTo(targetDate.Date);
			date.DateId.Should().Be.EqualTo(6);
		}

		[Test]
		public async void ShouldHandleMultipleRequestsAtTheSameTime()
		{
			var tasks = new List<Task>();

			for (var i = 0; i < 10; i++)
			{
				tasks.Add(new Task(() =>
				{
					var targetDate = new DateTime(2000, 01, 05);
					var date = WithAnalyticsUnitOfWork.Get(() => Target.Date(targetDate));
					date.DateDate.Date.Should().Be.EqualTo(targetDate.Date);
					date.DateId.Should().Be.EqualTo(6);
				}));
			}
			foreach(var task in tasks)
				task.Start();

			await Task.WhenAll(tasks.ToArray());
		}

		[Test]
		public void ShouldLoadMaxDate()
		{
			var targetDate = new DateTime(2000, 01, 05);
			WithAnalyticsUnitOfWork.Get(() => Target.Date(targetDate));

			var date = WithAnalyticsUnitOfWork.Get(() => Target.MaxDate());
			date.DateDate.Date.Should().Be.EqualTo(targetDate.Date);
			date.DateId.Should().Be.EqualTo(6);
		}

		[Test]
		public void ShouldLoadMinDate()
		{
			var targetDate = new DateTime(2000, 01, 05);
			WithAnalyticsUnitOfWork.Get(() => Target.Date(targetDate));

			var date = WithAnalyticsUnitOfWork.Get(() => Target.MinDate());
			date.DateDate.Should().Be.EqualTo(new DateTime(1999, 12, 31));
			date.DateId.Should().Be.EqualTo(1);
		}
	}
}