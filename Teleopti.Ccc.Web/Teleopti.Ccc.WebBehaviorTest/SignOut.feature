Feature: Sign Out
	In order to avoid others from seeing my data
	As an agent
	I want to sign out

Scenario: Sign out from week schedule
	Given I am an agent
	And My schedule is published
	When I view my week schedule
	And I sign out
	And I press back in the web browser
	Then I should be redirected to the sign in page
