using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Interfaces.Domain;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.DomainTest.Notification
{
	[TestFixture, DomainTest]
	public class ScheduleChangeCheckerTest : ISetup
	{
		public ScheduleChangeChecker Target;
		public void Setup(ISystem system, IIocConfiguration configuration)
		{

		}

		//[Test]
		//public void ShouldReturnTrueWhenScheduleChanged()
		//{
		//	var period = new DateOnlyPeriod(new DateOnly(2017, 11, 16), new DateOnly(2017, 11, 17));
		//	Target.Check(period).Should().Be.EqualTo(true);
		//}

		//[Test]
		//public void ShouldReturnFalseWhenUnpublishedScheduleChanged()
		//{
		//	var period = new DateOnlyPeriod(new DateOnly(2017, 11, 16), new DateOnly(2017, 11, 17));
		//	var wfc = WorkflowControlSetFactory.CreateWorkFlowControlSet(AbsenceFactory.CreateAbsence("sick"), new GrantAbsenceRequest(), false);
		//	wfc.SchedulePublishedToDate = new DateTime(2017, 11, 1);
		//	var person = PersonFactory.CreatePerson(wfc);

		
			
		//	Target.Check(period).Should().Be.EqualTo(false);
		//}
	}
}
