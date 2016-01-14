using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using AbsenceRequestConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.AbsenceRequestConfigurable;
using PersonPeriodConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.PersonPeriodConfigurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class UserStepDefinitions
	{
		[Given(@"(.*) (has|have) a schedule period with")]
		public void GivenIHaveASchedulePeriodWith(string userName, string hasHave, Table table)
		{
			var schedulePeriod = table.CreateInstance<SchedulePeriodConfigurable>();
			DataMaker.Person(userName).Apply(schedulePeriod);
		}

		[Given(@"(.*) (has|have) a person period with")]
		public void GivenIHaveAPersonPeriodWith(string userName, string hasHave, Table table)
		{
			var personPeriod = table.CreateInstance<PersonPeriodConfigurable>();
			DataMaker.Person(userName).Apply(personPeriod);
		}

		[Given(@"(.*) have a person period that starts on '(.*)'")]
		public void GivenIHaveAPersonPeriodThatStartsOn(string userName, DateTime date)
		{
			var personPeriod = new PersonPeriodConfigurable
				{
					StartDate = date,
					ShiftBag = "Common"
				};
			DataMaker.Person(userName).Apply(personPeriod);
		}

		// I am a user with
		// 'Kalle' is a user with
		// 'I' am a user with
		[Given(@"'?(I|.*)'? (am a|is a) user with")]
		public void GivenIAmAUserWith(string userName, string amAIsA, Table table)
		{
			var user = table.CreateInstance<UserConfigurable>();
			DataMaker.Person(userName).Apply(user);
		}

		[When(@"'(.*)' is updated with")]
		public void WhenIsUpdatedWith(string userName, Table table)
		{
			var user = table.CreateInstance<UserConfigurable>();
			DataMaker.Person(userName).Apply(user);
		}

		[Given(@"I have user credential with")]
		public void GivenIHaveUserCredentialWith(Table table)
		{
			var user = table.CreateInstance<UserConfigurable>();
			DataMaker.Data().Apply(user);
			DataMaker.Data().ApplyDelayed();
		}

		[Given(@"I am a user signed in with")]
		public void GivenIAmAUserSignedInWith(Table table)
		{
			var user = table.CreateInstance<UserConfigurable>();
			TestControllerMethods.LogonForSpecificUser(user.UserName, user.Password); 
		}
		








		// the ones below here does not belong here!

		[Given(@"I have a pre-scheduled meeting with")]
		public void GivenIHaveAMeetingScheduled(Table table)
		{
			var meeting = table.CreateInstance<MeetingConfigurable>();
			DataMaker.Data().Apply(meeting);
		}

		[Given(@"I have a pre-scheduled personal shift with")]
		public void GivenIHaveAPersonalShiftWith(Table table)
		{
			var personalShift = table.CreateInstance<PersonalShiftConfigurable>();
			DataMaker.Data().Apply(personalShift);
		}

		[Given(@"I have a public note with")]
		public void GivenIHaveAPublicNoteWith(Table table)
		{
			var publicNote = table.CreateInstance<PublicNoteConfigurable>();
			DataMaker.Data().Apply(publicNote);
		}

		[Given(@"I have an existing text request with")]
		public void GivenIHaveAnExistingTextRequestWith(Table table)
		{
			var textRequest = table.CreateInstance<TextRequestConfigurable>();
			DataMaker.Data().Apply(textRequest);
		}

		[Given(@"I have an existing absence request with")]
		public void GivenIHaveAnExistingAbsenceRequestWith(Table table)
		{
			var absenceRequest = table.CreateInstance<AbsenceRequestConfigurable>();
			DataMaker.Data().Apply(absenceRequest);
		}
	}
}