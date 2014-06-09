Feature: Move activity
	As a team leader
	I want to move an activity for one agent

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
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And there is an activity with
	| Field | Value  |
	| Name  | Lunch  |
	| Color | Yellow |

Scenario: The team leader should be able to move an activity some minutes later 

Scenario: The shift manager (?) should still see the different layers of schedules 

Scenario: If there is an activity behind, it becomes visible

Scenario: The team leader has to save after each change

Scenario:  

Questions : 
Should the app notify the shift manager of the change ?
Have we got a tablet for test ?

