@Ignore 
Feature: Report absence on agents
	In order to keep track of agents absences
	As a team leader
	I want to add absence for an agent

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

Scenario: View full day absence form
	Given I have the role 'Anywhere Team Green'
	When I view agent schedule for 'Pierre Baldi' on '2012-12-02'
	And I click 'add full day absence'
	Then I should see the add full day absence form
	
Scenario: Add full day absence
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a (read model) shift with
	| Field            | Value        |
	| Person           | Pierre Baldi |
	| Date             | 2012-12-02   |
	| Start time       | 08:00        |
	| End time         | 17:00        |
	| Activity         | Phone        |
	When I view agent schedule for 'Pierre Baldi' on '2012-12-02'
	And I click 'add full day absence'
	And I input these full day absence values
	| Field    | Value      |
	| Absence  | Vacation   |
	| End date | 2012-12-02 |
	And I click 'apply'
	Then I should see a shift layer with
	| Field      | Value |
	| Start time | 08:00 |
	| End time   | 17:00 |

Scenario: Default full day absence values
	Given I have the role 'Anywhere Team Green'
	When I view agent schedule for 'Pierre Baldi' on '2012-12-02'
	And I click 'add full day absence'
	Then I should see the add full day absence form with
	| Field      | Value      |
	| Start date | 2012-12-02 |
	| End date   | 2012-12-02 |
	
Scenario: Adding invalid absence values
	Given I have the role 'Anywhere Team Green'
	When I view agent schedule for 'Pierre Baldi' on '2012-12-02'
	And I click 'add full day absence'
	And I input these full day absence values
	| Field    | Value      |
	| End date | 2012-12-01 |
	And I click 'apply'
	Then I should see the message 'Invalid end date'
