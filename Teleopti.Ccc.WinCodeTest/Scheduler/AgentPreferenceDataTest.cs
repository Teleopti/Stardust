using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentPreferenceDataTest
	{
		private AgentPreferenceData _target;

		[SetUp]
		public void Setup()
		{
			_target = new AgentPreferenceData();	
		}

		[Test]
		public void ShouldGetSet()
		{
			var shiftCategory = new ShiftCategory("shiftCategory");
			var absence = new Absence();
			var dayOff = new DayOffTemplate(new Description("dayOff"));
			var activity = new Activity("actvity");

			_target.ShiftCategory = shiftCategory;
			_target.Absence = absence;
			_target.DayOffTemplate = dayOff;
			_target.Activity = activity;

			_target.MinStart = TimeSpan.FromHours(1);
			_target.MaxStart = TimeSpan.FromHours(2);
			_target.MinEnd = TimeSpan.FromHours(3);
			_target.MaxEnd = TimeSpan.FromHours(4);
			_target.MinLength = TimeSpan.FromHours(5);
			_target.MaxLength = TimeSpan.FromHours(6);

			_target.MinStartActivity = TimeSpan.FromHours(7);
			_target.MaxStartActivity = TimeSpan.FromHours(8);
			_target.MinEndActivity = TimeSpan.FromHours(9);
			_target.MaxEndActivity = TimeSpan.FromHours(10);
			_target.MinLengthActivity = TimeSpan.FromHours(11);
			_target.MaxLengthActivity = TimeSpan.FromHours(12);

			_target.MustHave = true;

			Assert.AreEqual(shiftCategory, _target.ShiftCategory);
			Assert.AreEqual(absence, _target.Absence);
			Assert.AreEqual(dayOff, _target.DayOffTemplate);
			Assert.AreEqual(activity, _target.Activity);

			Assert.AreEqual(TimeSpan.FromHours(1), _target.MinStart);
			Assert.AreEqual(TimeSpan.FromHours(2),_target.MaxStart);
			Assert.AreEqual(TimeSpan.FromHours(3),_target.MinEnd);
			Assert.AreEqual(TimeSpan.FromHours(4),_target.MaxEnd);
			Assert.AreEqual(TimeSpan.FromHours(5),_target.MinLength);
			Assert.AreEqual(TimeSpan.FromHours(6),_target.MaxLength);

			Assert.AreEqual(TimeSpan.FromHours(7),_target.MinStartActivity);
			Assert.AreEqual(TimeSpan.FromHours(8),_target.MaxStartActivity);
			Assert.AreEqual(TimeSpan.FromHours(9),_target.MinEndActivity);
			Assert.AreEqual(TimeSpan.FromHours(10),_target.MaxEndActivity);
			Assert.AreEqual(TimeSpan.FromHours(11),_target.MinLengthActivity);
			Assert.AreEqual(TimeSpan.FromHours(12), _target.MaxLengthActivity);

			Assert.IsTrue(_target.MustHave);
		}
	}
}
