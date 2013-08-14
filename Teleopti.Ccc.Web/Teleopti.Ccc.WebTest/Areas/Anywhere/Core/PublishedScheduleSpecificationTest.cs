using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Core
{
	[TestFixture]
	public class PublishedScheduleSpecificationTest
	{
		private PublishedScheduleSpecification _target;
		private ISchedulePersonProvider _schedulePersonProvider;
		private Guid _teamId;
		private DateTime _date;

		private MockRepository _mock; 

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_schedulePersonProvider = _mock.Stub<ISchedulePersonProvider>();
			_teamId = Guid.NewGuid();
			_date = new DateTime(2013, 02, 01, 0, 0, 0, DateTimeKind.Utc);
			_target = new PublishedScheduleSpecification(_schedulePersonProvider, _teamId, _date);
		}

		[Test]
		public void CanInstanciate()
		{
			Assert.IsNotNull(_target);
		}


	}
}
