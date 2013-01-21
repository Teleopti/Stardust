Feature: RefactorCreateOtherUsersData
	In order to make it easier to create other agents data
	We need to find a better way structuring state setup

Background:
	Given there are shift categories
	| Name  |
	| Night |
	| Day   |
	And There is an user called 'Arne Anka'

Scenario: Replacing I to a dynamic name
	#this is the old way
	Given I am an agent
	And I have a shift with 
	| Field                 | Value            |
	| StartTime             | 2012-01-15 10:00 |
	| EndTime               | 2012-01-15 15:00 |
	| Shift category			| Night	          |
	#instead adding the user name (the key to the correct UserFactory)
	And 'Arne Anka' have a shift with
	| Field                 | Value            |
	| StartTime             | 2012-01-15 10:00 |
	| EndTime               | 2012-01-15 15:00 |
	| Shift category			| Day	          |
	#and remove all "I" methods everywhere to accept a string instead
	#that way, everything "I" can have, Arne Anka can have as well
	When I am viewing week schedule
	Then ASM link should not be visible 

