Feature: Session Anywhere
	In order to be able to work with the application
	As an agent
	I want the application to handle my login session approprietly

Scenario: Signed out when time passes
	Given I have a role with
	| Field              | Value              |
	| Name               | Access to Anywhere |
	| Access to Anywhere | true               |
	And the time is '2013-09-30 16:00'
	And I am viewing Anywhere
	Then I should be signed in for Anywhere
	When the time is '2013-09-30 17:00'
	And I navigate to Anywhere
	Then I should see the sign in page