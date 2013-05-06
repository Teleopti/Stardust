﻿Feature: Remove absences
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
	And there is a shift category with
    | Field | Value |
    | Name  | Day   |
    | Color | Green |
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Start time     | 2013-04-16 08:00 |
	| End time       | 2013-04-16 17:00 |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |
	| Color | Red      |
	And there is an absence with
	| Field | Value   |
	| Name  | Illness |
	| Color | Gray    |
	
Scenario: View absence on this day in list
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-04-16 00:00 |
	| End time   | 2013-04-16 23:59 |
	When I view person schedule for 'Pierre Baldi' on '2013-04-16'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-04-16 00:00 |
	| End time   | 2013-04-16 23:59 |

Scenario: View absence starting yesterday in list
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-04-15 00:00 |
	| End time   | 2013-04-16 15:00 |
	When I view person schedule for 'Pierre Baldi' on '2013-04-16'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-04-15 00:00 |
	| End time   | 2013-04-16 15:00 |

Scenario: View absence ending tomorrow in list
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-04-16 00:00 |
	| End time   | 2013-04-17 15:00 |
	When I view person schedule for 'Pierre Baldi' on '2013-04-16'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-04-16 00:00 |
	| End time   | 2013-04-17 15:00 |

Scenario: Remove absence with confirmation
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-03-10 00:00 |
	| End time   | 2013-03-10 23:59 |
	When I view person schedule for 'Pierre Baldi' on '2013-03-10'
	And I click 'remove' on absence named 'Vacation'
	Then I should see 1 absences in the absence list
	And I should see a shift layer with
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 17:00 |
	| Color      | Red   |
	When I click 'confirm removal' on absence named 'Vacation'
	Then I should see 0 absences in the absence list
	And I should see a shift layer with
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 17:00 |
	| Color      | Green |

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
	When I view person schedule for 'Pierre Baldi' on '2013-03-10'
	Then I should see 1 absences in the absence list
	And I should see a shift layer with
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 17:00 |
	| Color      | Gray  |
	When I click 'remove' on absence named 'Illness'
	And I click 'confirm removal' on absence named 'Illness'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-03-10 10:00 |
	| End time   | 2013-03-10 11:00 |
	And I should see 1 absences in the absence list
	And I should see a shift layer with
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 17:00 |
	| Color      | Red   |
