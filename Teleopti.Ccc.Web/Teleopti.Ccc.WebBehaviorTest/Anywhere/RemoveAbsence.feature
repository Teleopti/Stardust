﻿@ignore
Feature: Remove absences
	In order to keep track of persons absences
	As a team leader
	I need to be able to remove incorrect absences on persons

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
	| Start date | 2013-03-01 |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |
	| Color | Red      |
	And there is an absence with
	| Field | Value   |
	| Name  | Illness |
	| Color | Gray    |
	
Scenario: View absence form with one full day absence
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-03-10 00:00 |
	| End time   | 2013-03-10 23:59 |
	When I view person schedule for 'Pierre Baldi' on '2013-03-10'
	And I click 'absence layer' 
	Then I should see the absence form with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-03-10 00:00 |
	| End time   | 2013-03-10 23:59 |

Scenario: View absence form with multiple full days absences
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-03-09 00:00 |
	| End time   | 2013-03-10 23:59 |
	When I view person schedule for 'Pierre Baldi' on '2013-03-10'
	And I click 'absence layer' 
	Then I should see the absence form with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-03-09 00:00 |
	| End time   | 2013-03-10 23:59 |

Scenario: Show confirmation button
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-03-10 00:00 |
	| End time   | 2013-03-10 23:59 |
	When I view person schedule absence form for 'Pierre Baldi' on '2013-03-10'
	And I click 'Remove'
	Then I should see the button 'Confirm removal'

Scenario: No absence removal without confirmation
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-03-10 00:00 |
	| End time   | 2013-03-10 23:59 |
	When I view person schedule absence form for 'Pierre Baldi' on '2013-03-10'
	And I click 'Remove'
	Then I should see one absence for 'Pierre Baldi' on '2013-03-10'

Scenario: Remove one full day absence
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-03-10 00:00 |
	| End time   | 2013-03-10 23:59 |
	When I view person schedule absence form for 'Pierre Baldi' on '2013-03-10'
	And I click 'Remove'
	And I click 'Confirm removal'
	Then I should not see any absence for 'Pierre Baldi' on '2013-03-10'

Scenario: Remove multiple full days absences
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-03-09 00:00 |
	| End time   | 2013-03-10 23:59 |
	When I view person schedule absence form for 'Pierre Baldi' on '2013-03-10'
	And I click 'Remove'
	And I click 'Confirm removal'
	Then I should not see any absence for 'Pierre Baldi' on '2013-03-09'
	And I should not see any absence for 'Pierre Baldi' on '2013-03-10'

Scenario: Remove one of two absences
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-03-10 10:00 |
	| End time   | 2013-03-10 11:00 |
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Illness			|
	| Start time | 2013-03-10 15:00 |
	| End time   | 2013-03-10 23:59 |
	When I view person schedule absence form for 'Pierre Baldi' on '2013-03-10'
	And I click 'Remove'
	And I click 'Confirm removal'	
	Then I should only see absence 'Illness' for 'Pierre Baldi' on '2013-03-10'
	