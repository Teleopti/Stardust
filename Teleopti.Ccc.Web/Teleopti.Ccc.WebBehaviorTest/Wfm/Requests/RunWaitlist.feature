@WFM
@OnlyRunIfEnabled('Wfm_Requests_Run_waitlist_38071')
Feature: RunWaitlist
	As a manger
	I want to be able to run through
	and approve waitlisted requests after something has changed in the system.

@ignore
Scenario: Should notify user that waitlist begin to run in backend
   Given Given I have a role with
		 | Field               | Value |
		 | Run Request Wailist | True  |
	When I view wfm requests
	 And I triggered run request waitlist
	Then I should see a notification that run request wailist task started

@ignore
#Function description only
Scenario: Should notify user that run waitlist task finished
   Given Given I have a role with
		 | Field               | Value |
		 | Run Request Wailist | True  |
	When I view wfm requests
	 And I triggered run request waitlist
	 And I wait until run waitlist task finished
	Then I should see a instant notification that run request wailist task started by me finished
