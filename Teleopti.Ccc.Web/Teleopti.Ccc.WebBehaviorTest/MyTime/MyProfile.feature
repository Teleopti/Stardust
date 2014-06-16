
Feature: My Profile
	In order to view and change my profile
	As an agent
	I want to 
		be able to view my language
		be able to change culture
		be able to change language
		be able to change my password


Scenario: See my profile
	Given I am an agent
	When I view my regional settings
	Then I should see my culture
	And I should see my language

Scenario: Change my culture
	Given I am an agent
	And I am swedish
	When I view my regional settings
	And I change culture to US
	Then I should see US date format

Scenario: Change my language
	Given I am an agent
	And I am swedish
	When I view my regional settings
	And I change language to english
	Then I should see english text

Scenario: Change my password
	Given I am an agent
	When I view my password
	And I change my password to 'newP@ssw0rd'
	And my cookie expires
	And I navigate to an application page
	And I sign in using my new password 'newP@ssw0rd'
	Then I should be signed in

Scenario: Incorrect current password when changing password
	Given I am an agent
	When I view my password
	And I change my password using incorrect current password
	Then I should see a message saying the password is incorrect
	
Scenario: Incorrect confirming password when changing password
	Given I am an agent
	When I view my password
	And I am changing password using incorrect confirm password
	Then I should see a message saying the password is not confirmed correctly
	And Confirm button should be disabled
