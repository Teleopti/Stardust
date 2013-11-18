@ignore
Feature: Add activity on a shift
	In order to assign work to an working agent
	As a team leader
	I want to add activity to an existing shift

Background:
	Given there is a team with
	| Field | Value            |
	| Name  | Team green       |
	And I have a role with
	| Field                      | Value               |
	| Name                       | Anywhere Team Green |
	| Access to team             | Team green          |
	| Access to Anywhere         | true                |
	| View unpublished schedules | true                |
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-11-18 |
	And there is a shift category named 'Day'
	
Scenario: Default times today
Scenario: Default times tomorrow
Scenario: Assign activity
Scenario: Assign activity late on night shift

