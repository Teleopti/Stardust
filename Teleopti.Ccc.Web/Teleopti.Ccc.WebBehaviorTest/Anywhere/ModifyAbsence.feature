Feature: Modify absence
	In order to keep track of persons absences
	As a team leader
	I need to be able to record that an agent has returned to work before the end date/time of an absence

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
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |
	| Color | Red      |
	And there is an absence with
	| Field | Value   |
	| Name  | Illness |
	| Color | Gray    |
	
Scenario: Agent returns to work one day earlier than planned absence
	Given I have the role 'Anywhere Team Green'
	And I am located in Stockholm
	And 'Pierre Baldi' is located in Stockholm
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-05-05 09:00 |
	| End time       | 2013-05-05 17:00 |
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-05-06 09:00 |
	| End time       | 2013-05-06 17:00 |
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-05-01 00:00 |
	| End time   | 2013-05-06 23:59 |
	When I view schedules for 'Team green' on '2013-05-06'
	And I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-05-06'
	Then I should see 1 absences in the absence list
	And I should see a shift
	When I click 'back to work' on absence named 'Vacation'
	Then I should see the back to work form with
	| Field      | Value |
	| End time   | 2013-05-06 23:59 |
	When I set back to work to '2013-05-06'
	And I click 'save absence'
	Then I should see 'Pierre Baldi' with the scheduled activity
	| Start time | End time | Color  |
	| 09:00      | 17:00    | Green  |	
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-05-05'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-05-01 00:00 |
	| End time   | 2013-05-05 17:00 |

Scenario: Cancel return to work for an absence
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has an absence with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-05-01 00:00 |
	| End time   | 2013-05-06 23:59 |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-05-06'
	Then I should see 1 absences in the absence list
	And I should see a shift
	When I click 'back to work' on absence named 'Vacation'
	Then I should see the back to work form with
	| Field      | Value |
	| End time   | 2013-05-06 23:59 |
	When I set back to work to '2013-05-06'
	And I click 'cancel'
	And I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-05-06'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-05-01 00:00 |
	| End time   | 2013-05-06 23:59 |
