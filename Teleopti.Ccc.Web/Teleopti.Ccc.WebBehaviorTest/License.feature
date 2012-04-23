Feature: License
	In order to enforce application license
	As Teleopti
	I want restrict usage based on license

Scenario: Restrict usage based on license
	Given I am a user that does not have license to web mytime
	When I open the sign in page
	Then I should get a message telling me I dont have a license
	And I should not see the sign in page

Scenario: Show licensed to information in portal
	Given I am an agent
	And My schedule is published
	When I view my week schedule
	Then I Should see licensed to information