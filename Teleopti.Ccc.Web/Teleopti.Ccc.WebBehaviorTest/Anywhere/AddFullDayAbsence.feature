Feature: Add full day absence
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
	And there is a shift category named 'Day'
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
	When I view schedules for 'Team green' on '2012-12-02'	
	And I click person name 'Pierre Baldi'
	And I click 'add full day absence' in schedule menu
	Then I should see the add full day absence form

Scenario: Add on empty day
	Given I have the role 'Anywhere Team Green'
	When I view person schedules add full day absence form for 'Pierre Baldi' in 'Team green' on '2013-04-08'
	And I input these full day absence values
	| Field    | Value      |
	| Absence  | Vacation   |
	| End date | 2013-04-08 |
	And I click 'apply'
	Then I should see a scheduled activity with
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 16:00 |
	| Color      | Red   |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-04-08'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-04-08 00:00 |
	| End time   | 2013-04-08 23:59 |

Scenario: Add on shift
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-04-08 08:00 |
	| End time       | 2013-04-08 17:00 |
	When I view person schedules add full day absence form for 'Pierre Baldi' in 'Team green' on '2013-04-08'
	And I input these full day absence values
	| Field    | Value      |
	| Absence  | Vacation   |
	| End date | 2013-04-08 |
	And I click 'apply'
	Then I should see a scheduled activity with
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 17:00 |
	| Color      | Red   |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-04-08'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-04-08 08:00 |
	| End time   | 2013-04-08 17:00 |

Scenario: Add on empty day first day
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-04-09 08:00 |
	| End time       | 2013-04-09 17:00 |
	When I view person schedules add full day absence form for 'Pierre Baldi' in 'Team green' on '2013-04-08'
	And I input these full day absence values
	| Field    | Value      |
	| Absence  | Vacation   |
	| End date | 2013-04-09 |
	And I click 'apply'
	And I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-04-08'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-04-08 00:00 |
	| End time   | 2013-04-09 17:00 |
	
Scenario: Add on empty day last day
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-04-08 08:00 |
	| End time       | 2013-04-08 17:00 |
	When I view person schedules add full day absence form for 'Pierre Baldi' in 'Team green' on '2013-04-08'
	And I input these full day absence values
	| Field    | Value      |
	| Absence  | Vacation   |
	| End date | 2013-04-09 |
	And I click 'apply'
	And I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-04-08'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-04-08 08:00 |
	| End time   | 2013-04-09 23:59 |

Scenario: Add on shifts in sequence
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-04-08 08:00 |
	| End time       | 2013-04-08 17:00 |
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-04-09 09:00 |
	| End time       | 2013-04-09 18:00 |
	When I view person schedules add full day absence form for 'Pierre Baldi' in 'Team green' on '2013-04-08'
	And I input these full day absence values
	| Field    | Value      |
	| Absence  | Vacation   |
	| End date | 2013-04-09 |
	And I click 'apply'
	And I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-04-08'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-04-08 08:00 |
	| End time   | 2013-04-09 18:00 |

Scenario: Add on shift ending tomorrow
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-05-23 22:00 |
	| End time       | 2013-05-24 07:00 |	
	When I view person schedules add full day absence form for 'Pierre Baldi' in 'Team green' on '2013-05-23'
	And I input these full day absence values
	| Field    | Value      |
	| Absence  | Vacation   |
	| End date | 2013-05-23 |
	And I click 'apply'
	Then I should see a scheduled activity with
	| Field      | Value |
	| Start time | 22:00 |
	| End time   | 07:00 |
	| Color      | Red   |
	When I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-05-23'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-05-23 22:00 |
	| End time   | 2013-05-24 07:00 |

Scenario: Add absence on a day with a night shift starting yesterday
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-05-22 22:00 |
	| End time       | 2013-05-23 07:00 |
	When I view person schedules add full day absence form for 'Pierre Baldi' in 'Team green' on '2013-05-23'
	And I input these full day absence values
	| Field    | Value      |
	| Absence  | Vacation   |
	| End date | 2013-05-23 |
	And I click 'apply'
	And I view person schedule for 'Pierre Baldi' in 'Team green' on '2013-05-23'
	Then I should see an absence in the absence list with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-05-23 07:00 |
	| End time   | 2013-05-23 23:59 |

Scenario: Default values
	Given I have the role 'Anywhere Team Green'
	When I view person schedules add full day absence form for 'Pierre Baldi' in 'Team green' on '2012-12-02'
	Then I should see the add full day absence form with
	| Field      | Value      |
	| Start date | 2012-12-02 |
	| End date   | 2012-12-02 |	
	
Scenario: Invalid dates
	Given I have the role 'Anywhere Team Green'
	When I view person schedules add full day absence form for 'Pierre Baldi' in 'Team green' on '2012-12-02'
	And I input these full day absence values
	| Field    | Value      |
	| End date | 2012-12-01 |
	Then I should see the alert 'Invalid end date'

Scenario: Back to viewing schedule after adding a full day absence
	Given I have the role 'Anywhere Team Green'
	When I view person schedules add full day absence form for 'Pierre Baldi' in 'Team green' on '2013-04-08'
	And I input these full day absence values
	| Field    | Value      |
	| Absence  | Vacation   |
	| End date | 2013-04-08 |
	And I click 'apply'
	Then I should be viewing schedules for '2013-04-08'	