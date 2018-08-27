@People
@OnlyRunIfEnabled('Wfm_PeopleWeb_PrepareForRelease_76666')
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
	And I have selected person 'Anthony'
	And I have selected person 'Abraham'


Scenario: Change application logon for people
	When I navigate to application logon page
	Then The application logon page is shown
	And I can see current application logons
	| Name		| AppLogon			|
	| Anthony	| anthony@dish.com	|
	| Abraham	| abraham			|
	When I change Application logon for 'Ashley' into 'ashley'
	And I press the save button
	Then I should not the see the application logon page