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
	And there is a shift category with
    | Field | Value |
    | Name  | Day   |
    | Color | Green |
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
	| Start time | 2013-05-06 00:00 |
	| End time   | 2013-05-06 23:59 |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-05-06'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-05-06 00:00 |
	| End time   | 2013-05-06 23:59 |

Scenario: View absence starting yesterday in list
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-05-02 00:00 |
	| End time   | 2013-05-06 15:00 |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-05-06'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-05-02 00:00 |
	| End time   | 2013-05-06 15:00 |

Scenario: View absence ending tomorrow in list
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-05-06 00:00 |
	| End time   | 2013-05-07 15:00 |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-05-06'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-05-06 00:00 |
	| End time   | 2013-05-07 15:00 |

Scenario: Remove absence with confirmation
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-05-06 00:00 |
	| End time   | 2013-05-06 23:59 |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-05-06'
	And I click 'remove' on absence named 'Vacation'
	Then I should see 1 absences in the absence list
	And I should see a shift
	When I click 'confirm removal' on absence named 'Vacation'
	And I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-05-06'
	Then I should see 0 absences in the absence list
	And I should not see any shift
	
Scenario: Remove one of two absences
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Start time     | 2013-05-06 08:00 |
	| End time       | 2013-05-06 17:00 |
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-05-06 10:00 |
	| End time   | 2013-05-06 11:00 |
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Illness			|
	| Start time | 2013-05-06 15:00 |
	| End time   | 2013-05-06 16:00 |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-05-06'
	Then I should see a scheduled activity with
	| Field      | Value |
	| Start time | 15:00 |
	| End time   | 16:00 |
	| Color      | Gray  |
	When I click 'remove' on absence named 'Illness'
	And I click 'confirm removal' on absence named 'Illness'
	And I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-05-06'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-05-06 10:00 |
	| End time   | 2013-05-06 11:00 |
	And I should see 1 absences in the absence list
	And I should not see a scheduled activity with
	| Field      | Value |
	| Start time | 15:00 |
	| End time   | 16:00 |
	| Color      | Gray  |

Scenario: Remove absence starting from day 2 on night shift
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Start time     | 2013-12-06 22:00 |
	| End time       | 2013-12-07 06:00 |
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-12-07 02:00 |
	| End time   | 2013-12-07 03:00 |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-12-06'
	Then I should see a scheduled activity with
	| Field      | Value |
	| Start time | 02:00 |
	| End time   | 03:00 |
	| Color      | Red   |
	When I click 'remove' on absence named 'Vacation'
	And I click 'confirm removal' on absence named 'Vacation'
	And I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-12-06'
	Then I should see 0 absences in the absence list
	And I should not see a scheduled activity with
	| Field      | Value |
	| Start time | 02:00 |
	| End time   | 03:00 |
	| Color      | Red   |