@mobilesignin
Feature: Mobile Sign in
	In order to access the site
	As a mobile user that is not signed in
	I want to be able to sign in

Background:
	Given I browse with a mobile

Scenario: Sign in with a user with multiple business units by user name
	Given I am a user with multiple business units
	When I sign in by user name
	And I select a business unit
	Then I should be signed in

Scenario: Sign in with a user with one business unit by user name and I should be directed into that business unit direct without having to select it
	Given I am a mobileuser with a single business unit
	When I sign in by user name
	Then I should be signed in

Scenario: Sign in with wrong password should give me an informative error
	Given I am a user with a single business unit
	When I sign in by user name and wrong password
	Then I should see an log on error

Scenario: Sign in without permission
	Given I dont have permission to sign in
	When I sign in by user name
	Then I am not signed in

@IgnoreBeforeScenario
Scenario: Enter signin page with page preference 
	Given I Navigate to the signin page with subpage preference
	Then I Should see the login page