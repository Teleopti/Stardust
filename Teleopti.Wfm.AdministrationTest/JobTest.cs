﻿using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Job = Teleopti.Ccc.Infrastructure.Repositories.Stardust.Job;

namespace Teleopti.Wfm.AdministrationTest
{
	[TestFixture]
	public class JobTest
	{
		[Test]
		public void ShouldReturnDataSource()
		{
			const string datasource = "DataSourceTest";
			var e = new UpdateStaffingLevelReadModelEvent {LogOnDatasource = datasource};
			var ser = JsonConvert.SerializeObject(e);
			var job = new Job {Serialized = ser};
			job.DataSource.Should().Be.EqualTo(datasource);
		}
	}
}
