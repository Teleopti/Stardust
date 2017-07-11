using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
	public class OvertimeRequestTest
	{
		private DateTimePeriod _period;

		[SetUp]
		public void Setup()
		{
			_period = new DateTimePeriod(new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc),
										new DateTime(2008, 7, 19, 0, 0, 0, DateTimeKind.Utc));
		}

		[Test]
		public void ShouldGetDetails()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var definiationSet = new MultiplicatorDefinitionSet("overtime paid", MultiplicatorType.Overtime);
			var overtimeRequest = new PersonRequest(person, new OvertimeRequest(definiationSet, _period)).Request;

			var text = overtimeRequest.GetDetails(new CultureInfo("en-US"));
			Assert.AreEqual("overtime paid, 2:00 AM - 2:00 AM", text);

			text = overtimeRequest.GetDetails(new CultureInfo("ko-KR"));
			Assert.AreEqual("overtime paid, 오전 2:00 - 오전 2:00", text);

			text = overtimeRequest.GetDetails(new CultureInfo("zh-TW"));
			Assert.AreEqual("overtime paid, 上午 02:00 - 上午 02:00", text);
		}
	}
}
