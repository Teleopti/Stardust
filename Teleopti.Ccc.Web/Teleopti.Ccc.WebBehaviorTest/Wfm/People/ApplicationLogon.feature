@People
@OnlyRunIfEnabled('Wfm_PeopleWeb_PrepareForRelease_47766')
Feature: AppLogon
	In order to change application logon for a person
	As a teamleader
	I want to be able to see current application logon and easily change it for multiple people

Background:
	Given I have a role with full access
	And WA People exists
	| Name		|
	| Ashley	|
	| Anthony	|
	| Abraham	|
	
	And Person 'Ashley' has app logon name ''
	And Person 'Anthony' has app logon name 'anthony@dish.com'
	And Person 'Abraham' has app logon name 'abraham'

	And I view people
	And I searched for 'a'
	And I have selected person 'Ashley'
	And I have selected person 'I'
	And I have selected person 'Abraham'

@ignore
Scenario: Change application logon for people
	When I navigate to application logon page
	Then The application logon page is shown
	And I can see current application logons
	| Name		| AppLogon			|
	| Ashley	| 					|
	| Anthony	| anthony@dish.com	|
	| Abraham	| abraham			|
	When I change Application logon for 'Ashley' into 'ashley'
	And I press the save button
	Then I should not the see the application logon page

@ignore
Scenario: Feedback from server when entering already taken logon
	When I navigate to application logon page
	Then The application logon page is shown
	When I change Application logon for 'Ashley' into 'abraham'
	Then I should see a validation error on 'Ashley'
	And The save button should be disabled

