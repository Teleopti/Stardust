@ignore
Feature: View full day absence on day off
	In order to see that the person is on holiday and that the day is counted as a day off
	As a team leader
	I want to see the scheduled absences and day off for the person

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
	| Start date | 2013-10-25 |
	| Team       | Team green |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |
	| Color | Red      |
	And there is a day off named 'Day off'

Scenario: View full day absence on day off in team schedule
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a day off named 'Day off' on '2013-10-25'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-10-25 00:00 |
	| End time   | 2013-10-25 23:59 |
	When I view schedules for '2013-10-25'
	Then I should see 'Pierre Baldi' with a day off named 'Day off'
	And I should see 'Pierre Baldi' with absence 
	| Field       | Value    |
	| Start time  | 08:00    |
	| End time    | 16:00    |
	| Color       | Red      |
	| Description | Vacation |

Scenario: View full day absence on day off for person
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a day off named 'Day off' on '2013-10-25'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-10-25 00:00 |
	| End time   | 2013-10-25 23:59 |
	When I view person schedule for 'Pierre Baldi' on '2013-10-25'
	Then I should see a day off named 'Day off'
	And I should see a shift layer with
	| Field       | Value    |
	| Start time  | 08:00    |
	| End time    | 16:00    |
	| Color       | Red      |
	| Description | Vacation |
	And I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Color      | Red              |
	| Start time | 2013-10-25 00:00 |
	| End time   | 2013-10-25 23:59 |

Scenario: Order as full day absence when on day off
	Given I have the role 'Anywhere Team Green'
	And 'Ashley Andeen' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-10-28 02:00 |
	| End time       | 2013-10-28 10:00 |
	And 'Ashley Andeen' has a full day absence named 'Vacation' on '2013-10-28'
	And 'Pierre Baldi' has a day off named 'Day off' on '2013-10-28'
	And 'Pierre Baldi' has a full day absence named 'Vacation' on '2013-10-28'
	And 'John King' has a day off named 'Day off' on '2013-10-28'
	When I view schedules for '2013-10-28'
	Then I should see 'Ashley Andeen' before 'Pierre Baldi'
	Then I should see 'Pierre Baldi' before 'John King'
