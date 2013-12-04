Feature: View full day absence
	In order to keep track of scheduled full day absences for a person in my team
	As a team leader
	I want to see the scheduled absences for the person
	
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
	| Start date | 2013-10-22 |
	| Team       | Team green |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |
	| Color | Red      |

Scenario: View full day absence in team schedule
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-10-22 00:00 |
	| End time   | 2013-10-22 23:59 |
	When I view schedules for '2013-10-22'
	Then I should see 'Pierre Baldi' with absence 
	| Field       | Value    |
	| Start time  | 08:00    |
	| End time    | 16:00    |
	| Color       | Red      |
	| Description | Vacation |

Scenario: View full day absence for person
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-10-22 00:00 |
	| End time   | 2013-10-22 23:59 |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-10-22'
	Then I should see a scheduled activity with
	| Field       | Value    |
	| Start time  | 08:00    |
	| End time    | 16:00    |
	| Color       | Red      |
	| Description | Vacation |
	And I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Color      | Red              |
	| Start time | 2013-10-22 00:00 |
	| End time   | 2013-10-22 23:59 |
