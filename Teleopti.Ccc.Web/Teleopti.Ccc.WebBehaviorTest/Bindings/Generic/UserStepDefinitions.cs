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
		[Given(@"I have a schedule period with")]
		public void GivenIHaveASchedulePeriodWith(Table table)
		{
			var schedulePeriod = table.CreateInstance<SchedulePeriodConfigurable>();
			UserFactory.User().Setup(schedulePeriod);
		}

		[Given(@"I have a person period with")]
		public void GivenIHaveAPersonPeriodWith(Table table)
		{
			var personPeriod = table.CreateInstance<PersonPeriodConfigurable>();
			UserFactory.User().Setup(personPeriod);
		}

		[Given(@"I have a person period that starts on '(.*)'")]
		public void GivenIHaveAPersonPeriodThatStartsOn(DateTime date)
		{
			var personPeriod = new PersonPeriodConfigurable
				{
					StartDate = date,
					RuleSetBag = "Common"
				};
			UserFactory.User().Setup(personPeriod);
		}

		[Given(@"I am a user with")]
		public void GivenIAmAUserWith(Table table)
		{
			var user = table.CreateInstance<UserConfigurable>();
			UserFactory.User().Setup(user);
		}

		[Given(@"I have user credential with")]
		public void GivenIHaveUserCredentialWith(Table table)
		{
			var user = table.CreateInstance<UserConfigurable>();
			UserFactory.User().MakeUser(user.UserName, user.UserName, user.Password);
		}


		[Given(@"I am a user signed in with")]
		public void GivenIAmAUserSignedInWith(Table table)
		{
			var user = table.CreateInstance<UserConfigurable>();
			TestControllerMethods.LogonForSpecificUser(user.UserName, user.Password); 
		}

		[Given(@"There is an user called '(.*)'")]
		public void GivenThereIsAnUserCalled(string userName)
		{
			UserFactory.User().AddColleague(userName);
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