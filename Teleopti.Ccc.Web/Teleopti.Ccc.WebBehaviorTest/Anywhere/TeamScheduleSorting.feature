Feature: Team schedule sorting
	In order to easily find who is working at a specific time
	As a team leader
	I want to get the persons sorted in order of the schedules
	
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
	And 'Ashley Andeen' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-09-26 |
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-09-26 |
	And there are shift categories
	| Name  |
	| Day   |	
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |
	| Color | Red      |
	And there is a dayoff with
    | Field | Value   |
    | Name  | Day off |
	
Scenario: Order by shift start
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-09-27 08:00 |
	| End time       | 2013-09-27 16:00 |
	And 'Ashley Andeen' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-09-27 09:00 |
	| End time       | 2013-09-27 17:00 |
	When I view schedules for '2013-09-27'
	Then I should see 'Pierre Baldi' before 'Ashley Andeen'

@ignore
Scenario: Order full day absences after shifts in team schedule
	Given I have the role 'Anywhere Team Green'
	And 'Ashley Andeen' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-09-27 00:00 |
	| End time   | 2013-09-27 23:59 |
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-09-27 08:00 |
	| End time       | 2013-09-27 17:00 |
	When I view schedules for '2013-09-27'
	Then I should see 'Pierre Baldi' before 'Ashley Andeen'

@ignore
Scenario: Order days off after full day absences in team schedule
	Given I have the role 'Anywhere Team Green'
	And 'Ashley Andeen' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-09-27 00:00 |
	| End time   | 2013-09-27 23:59 |
	And 'Pierre Baldi' have a day off with
	| Field | Value      |
	| Name  | Day off    |
	| Date  | 2013-09-27 |
	When I view schedules for '2013-09-27'
	Then I should see 'Ashley Andeen' before 'Pierre Baldi'

@ignore
Scenario: Order no shifts after day off
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a day off with
	| Field | Value      |
	| Name  | Day off    |
	| Date  | 2013-09-27 |
	And 'Ashley Andeen' has no shift
	When I view schedules for '2013-09-27'
	Then I should see 'Pierre Baldi' before 'Ashley Andeen'