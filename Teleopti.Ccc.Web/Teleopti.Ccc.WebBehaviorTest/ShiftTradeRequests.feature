Feature: Shift Trade Requests
	In order to avoid an unwanted scheduled shifts
	As an agent
	I want to be able to trade shifts with other agents

Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there is a role with
	| Field								| Value						|
	| Name								| No access to Shift Trade	|
	| Access To Shift Trade Requests	| False						|
	And there are shift categories
	| Name  |
	| Day   |
	| Late   |
	And there is a dayoff with
	| Field | Value  |
	| Name  | DayOff |
	And there is an absence with
	| Field | Value   |
	| Name  | Illness |
	And there is a skill with
	| Field | Value   |
	| Name  | Skill 1 |
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2040-06-24         |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |	
	| Start date | 2012-06-18 |
	| Skill      | Skill 1    |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	| Lunch3HoursAfterStart | true             |
	And an agent has a workflow control set with
	| Field                      | Value              |
	| Agent name                 | Other agent 1      |
	| Name                       | Published schedule |
	| Schedule published to date | 2040-06-24         |
	And an agent has a schedule period with 
	| Field      | Value         |
	| Agent name | Other agent 1 |
	| Start date | 2012-06-18    |
	| Type       | Week          |
	| Length     | 1		     |
	And an agent has a person period with 
	| Field      | Value         |
	| Agent name | Other agent 1 |
	| Start date | 2012-06-18    |
	| Skill      | Skill 1       |


Scenario: Do not show shift trade request tab if no permission
	Given I have the role 'No access to Shift Trade'
	When I sign in
	Then shift trade tab should not be visible

Scenario: Default to first day of open period when viewing shift trade
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01'
	And I can do shift trades between '2030-01-03' and '2030-01-17'
	When I navigate to shift trade page
	Then the selected date should be '2030-01-03'

Scenario: Default time line when I am not scheduled
	Given I have the role 'Full access to mytime'
	And Current time is '2020-10-24'
	When I navigate to shift trade page
	Then I should see the time line span from '7:45' to '17:15'

Scenario: Time line when I have a scheduled shift
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01'
	When I navigate to shift trade page
	Then I should see the time line span from '5:45' to '16:15'

Scenario: Show my scheduled shift
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01'
	When I navigate to shift trade page
	Then I should see my schedule with
	| Field			| Value |
	| Start time	| 06:00 |
	| End time		| 16:00 |

Scenario: Show my scheduled day off
	Given I have the role 'Full access to mytime'
	And I have a day off with
	| Field | Value      |
	| Date  | 2030-01-03 |
	| Name  | DayOff     |
	And Current time is '2030-01-03'
	When I navigate to shift trade page
	Then I should see my scheduled day off 'DayOff'

Scenario: Show my full-day absence
	Given I have the role 'Full access to mytime'
	And I have absence with
	| Field		| Value      |
	| Date		| 2030-01-05 |
	| Name		| Illness	 |
	| Full Day  | True		 |
	And Current time is '2030-01-05'
	When I navigate to shift trade page
	Then I should see my scheduled absence 'Illness'

Scenario: Show message when no possible shift trades
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-05'
	And I can do shift trades between '2030-01-06' and '2030-01-17'
	When I navigate to shift trade page for date '2030-01-05'
	Then I should see a user-friendly message explaining that shift trades cannot be made

Scenario: One possible shift to trade with because shift trade periods match
	Given I have the role 'Full access to mytime'
	And Current time is '2029-12-29'
	And I can do shift trades between '2030-01-01' and '2030-01-17'
	And another agent named 'Other agent 1' can do shift trades between '2030-01-01' and '2030-01-17'
	And an agent has a shift with
	| Field                 | Value            |
	| Agent name            | Other agent 1    |
	| StartTime             | 2030-01-01 10:00 |
	| EndTime               | 2030-01-01 20:00 |
	| Shift category		| Late	           |
	| Lunch3HoursAfterStart | true             |
	When I navigate to shift trade page
	Then I should have one possible shift to trade with

Scenario: Not possible to trade shift because no matching skills
	Given I have the role 'Full access to mytime'
	And Current time is '2029-12-29'
	And I can do shift trades between '2030-01-01' and '2030-01-17'
	And another agent named 'Other agent 1' can do shift trades between '2030-01-01' and '2030-01-17'
	And an agent has a shift with
	| Field                 | Value            |
	| Agent name            | Other agent 1    |
	| StartTime             | 2030-01-01 10:00 |
	| EndTime               | 2030-01-01 20:00 |
	| Shift category		| Late	           |
	| Lunch3HoursAfterStart | true             |
	And I have a updated workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Shift trade matching skill | Skill 1            |
	When I navigate to shift trade page
	Then I should see a user-friendly message explaining that shift trades cannot be made

Scenario: One possible shift to trade with because shift trade periods and skills are matching
	Given I have the role 'Full access to mytime'
	And Current time is '2029-12-29'
	And I can do shift trades between '2030-01-01' and '2030-01-17'
	And another agent named 'Other agent 1' can do shift trades between '2030-01-01' and '2030-01-17'
	And an agent has a shift with
	| Field                 | Value            |
	| Agent name            | Other agent 1    |
	| StartTime             | 2030-01-01 10:00 |
	| EndTime               | 2030-01-01 20:00 |
	| Shift category		| Late	           |
	| Lunch3HoursAfterStart | true             |
	And I have a updated workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Shift trade matching skill | Skill 1            |
	And an agent has a updated workflow control set with
	| Field                      | Value              |
	| Agent name                 | Other agent 1      |
	| Name                       | Published schedule |
	| Shift trade matching skill | Skill 1            |
	When I navigate to shift trade page
	Then I should have one possible shift to trade with