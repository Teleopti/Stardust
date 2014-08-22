Feature: Session
	In order to be able to work with the application
	As an agent
	I want the application to handle my login session approprietly
	
Background:
	Given there is a role with
	| Field | Value                 |
	| Name  | Full access to mytime |

Scenario: Stay signed in after server restart
	Given I am viewing an application page
	Then I should be signed in
	When I navigate the internet
	And the server restarts
	And I navigate to an application page
	Then I should be signed in
	
Scenario: Signed out when cookie expires
	Given I am viewing an application page
	Then I should be signed in
	When my cookie expires
	And I navigate to an application page
	Then I should see the sign in page
	
Scenario: Signed out when time passes
	Given the time is '2013-09-30 16:00'
	And I am viewing an application page
	Then I should be signed in
	When the time is '2013-09-30 17:00'
	And I navigate to an application page
	Then I should see the sign in page
	
Scenario: Stay signed in when time passes with ASM open
	Given I have the role 'Full access to mytime'
	And the time is '2013-09-30 16:00'
	And I am viewing ASM
	When the time is '2013-09-30 16:29'
	And I am still viewing ASM
	And the time is '2013-09-30 16:38'
	And I navigate to an application page
	Then I should stay signed in

Scenario: Signed out when cookie expires while I browse the internet
	Given I am viewing an application page
	Then I should be signed in
	When my cookie expires
	And I navigate the internet
	And I navigate to an application page
	Then I should see the sign in page

Scenario: Signed out when navigating to next period when cookie is expired
	Given I am an agent
	And I have several virtual schedule periods
	And I am viewing preferences
	When my cookie expires
	And I click next virtual schedule period button
	Then I should see the sign in page

Scenario: Corrupt teleopti cookie due to upgrade should be overwritten by an update
	Given I am viewing an application page
	Then I should be signed in
	When My cookie gets corrupt
	And I navigate to an application page
	Then I should be signed in

Scenario: Corrupt teleopti cookie due to no longer existing database should be overwritten by an update
	Given I am viewing an application page
	Then I should be signed in
	When My cookie gets pointed to non existing database
	And I navigate to an application page
	Then I should be signed in

