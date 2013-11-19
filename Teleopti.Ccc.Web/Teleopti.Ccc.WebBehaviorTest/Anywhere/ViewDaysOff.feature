Feature: View days off
	In order to keep track of scheduled days off for a person in my team
	As a team leader
	I want to see the scheduled days off for the person
	
Background:
	Given there is a team with
	| Field | Value      |
	| Name  | Team green |
	And there is a role with
	| Field                      | Value               |
	| Name                       | Anywhere Team Green |
	| Access to team             | Team green          |
	| Access to Anywhere         | true                |
	| View unpublished schedules | true                |	
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-09-26 |
	And there is a dayoff with
    | Field | Value   |
    | Name  | Day off |
	
Scenario: View day off in team schedule
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a day off with
	| Field | Value      |
	| Name  | Day off    |
	| Date  | 2013-09-27 |
	When I view schedules for 'Team green' on '2013-09-27'
	And I click description toggle button
	Then I should see 'Pierre Baldi' with a day off named 'Day off'

Scenario: View day off in person schedule
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a day off with
	| Field | Value      |
	| Name  | Day off    |
	| Date  | 2013-09-27 |
	When I view person schedule for 'Pierre Baldi' and 'Team green' on '2013-09-27'
	And I click description toggle button
	Then I should see a day off named 'Day off'
