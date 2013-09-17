using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class UserStepDefinitions
	{
		[Given(@"(.*) (has|have) a schedule period with")]
		public void GivenIHaveASchedulePeriodWith(string userName, string hasHave, Table table)
		{
			var schedulePeriod = table.CreateInstance<SchedulePeriodConfigurable>();
			UserFactory.User(userName).Setup(schedulePeriod);
		}

		[Given(@"(.*) (has|have) a person period with")]
		public void GivenIHaveAPersonPeriodWith(string userName, string hasHave, Table table)
		{
			var personPeriod = table.CreateInstance<PersonPeriodConfigurable>();
			UserFactory.User(userName).Setup(personPeriod);
		}

		[Given(@"(.*) have a person period that starts on '(.*)'")]
		public void GivenIHaveAPersonPeriodThatStartsOn(string userName, DateTime date)
		{
			var personPeriod = new PersonPeriodConfigurable
				{
					StartDate = date,
					RuleSetBag = "Common"
				};
			UserFactory.User(userName).Setup(personPeriod);
		}

		// I am a user with
		// 'Kalle' is a user with
		// 'I' am a user with
		[Given(@"'?(I|.*)'? (am a|is a) user with")]
		public void GivenIAmAUserWith(string userName, string amAIsA, Table table)
		{
			var user = table.CreateInstance<UserConfigurable>();
			UserFactory.User(userName).Setup(user);
		}

		[Given(@"I have user credential with")]
		public void GivenIHaveUserCredentialWith(Table table)
		{
			var user = table.CreateInstance<UserConfigurable>();
			UserFactory.User().Setup(user);
			UserFactory.User().MakeUser();
		}

		[Given(@"I am a user signed in with")]
		public void GivenIAmAUserSignedInWith(Table table)
		{
			var user = table.CreateInstance<UserConfigurable>();
			TestControllerMethods.LogonForSpecificUser(user.UserName, user.Password); 
		}













		// the ones below here does not belong here!

		[Given(@"I have a pre-scheduled meeting with")]
		[Given(@"I have a meeting scheduled")]
		public void GivenIHaveAMeetingScheduled(Table table)
		{
			var meeting = table.CreateInstance<MeetingConfigurable>();
			UserFactory.User().Setup(meeting);
		}

		[Given(@"I have a pre-scheduled personal shift with")]
		public void GivenIHaveAPersonalShiftWith(Table table)
		{
			var personalShift = table.CreateInstance<PersonalShiftConfigurable>();
			UserFactory.User().Setup(personalShift);
		}

		[Given(@"I have a public note with")]
		public void GivenIHaveAPublicNoteWith(Table table)
		{
			var publicNote = table.CreateInstance<PublicNoteConfigurable>();
			UserFactory.User().Setup(publicNote);
		}

		[Given(@"I have an existing text request with")]
		public void GivenIHaveAnExistingTextRequestWith(Table table)
		{
			var textRequest = table.CreateInstance<TextRequestConfigurable>();
			UserFactory.User().Setup(textRequest);
		}

		[Given(@"I have an existing absence request with")]
		public void GivenIHaveAnExistingAbsenceRequestWith(Table table)
		{
			var absenceRequest = table.CreateInstance<AbsenceRequestConfigurable>();
			UserFactory.User().Setup(absenceRequest);
		}
	}
}