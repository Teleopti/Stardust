﻿Feature: Add full day absence
	In order to keep track of persons absences
	As a team leader
	I want to add absence for an person
	
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
	| Start date | 2012-12-01 |
	And there is an activity with
	| Field | Value  |
	| Name  | Lunch  |
	| Color | Yellow |
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |
	| Color | Red      |
	
Scenario: View form
	Given I have the role 'Anywhere Team Green'
	When I view person schedule for 'Pierre Baldi' on '2012-12-02'
	And I click 'add full day absence'
	Then I should see the add full day absence form

@WTFDEBUG	
Scenario: Add
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a (read model) shift with
	| Field      | Value        |
	| Date       | 2013-04-08   |
	| Start time | 08:00        |
	| End time   | 17:00        |
	| Activity   | Phone        |
	And there is a shift category named 'Day'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Start time     | 2013-04-08 08:00 |
	| End time       | 2013-04-08 17:00 |
	When I view person schedules add full day absence form for 'Pierre Baldi' on '2013-04-08'
	And I input these full day absence values
	| Field    | Value      |
	| Absence  | Vacation   |
	| End date | 2013-04-08 |
	And I click 'apply'
	Then I should see a shift layer with
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 17:00 |
	| Color      | Red   |
#	And I should see an absence in the absence list with
#	| Field      | Value            |
#	| Start time | 2013-04-08 08:00 |
#	| End time   | 2013-04-08 17:00 |
#	| Absence    | Vacation         |

Scenario: Default values
	Given I have the role 'Anywhere Team Green'
	When I view person schedules add full day absence form for 'Pierre Baldi' on '2012-12-02'
	Then I should see the add full day absence form with
	| Field      | Value      |
	| Start date | 2012-12-02 |
	| End date   | 2012-12-02 |	
	
Scenario: Invalid dates
	Given I have the role 'Anywhere Team Green'
	When I view person schedules add full day absence form for 'Pierre Baldi' on '2012-12-02'
	And I input these full day absence values
	| Field    | Value      |
	| End date | 2012-12-01 |
	Then I should see the alert 'Invalid end date'
