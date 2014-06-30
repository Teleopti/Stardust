@ignore
Feature: Schedule change actions under permission control
	In order to let team leaders only manage their own staff
	As a manager
	I want to restrict what schedule change actions team leaders are allowed to do


Background:
	Given there is a team with
	| Field | Value            |
	| Name  | Team green       |
	And I have a role with
	| Field                      | Value                          |
	| Name                       | Green without Full day absence |
	| Access to team             | Team green                     |
	| Access to Anywhere         | true                           |
	| View unpublished schedules | true                           |
	| Add full day absence       | false                          |
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2014-06-30 |
	And there is a shift category named 'Day'
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |

Scenario: No permission to add full day absence 
	Given I have the role 'Green without Full day absence'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2014-06-30 09:00 |
	| End time       | 2014-06-30 16:00 |
	When I view schedules for 'Team green' on '2014-06-30'
	And I click person name 'Pierre Baldi'
	Then I should not see 'Add full day absence' option

Scenario: Cannot navigate to add full day absence 
	Given I have the role 'Green without Full day absence'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2014-06-30 09:00 |
	| End time       | 2014-06-30 16:00 |
	When I view person schedules add full day absence form for 'Pierre Baldi' in 'Team green' on '2014-06-30'
	Then I should see an error message

