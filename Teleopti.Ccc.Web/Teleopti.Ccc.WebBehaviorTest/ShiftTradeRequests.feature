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
	And there is a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 08:00 |
	| EndTime               | 2030-01-01 17:00 |
	| Lunch3HoursAfterStart | true             |

@ignore
Scenario: Do not show shift trade request tab if no permission
	Given I have the role 'No access to Shift Trade'
	When I sign in
	Then shift trade tab should not be visible

@ignore
Scenario: Show Shift trade reuqest tab
	Given I have the role 'Full access to mytime'
	When I sign in
	Then shift trade tab should be visible

@ignore
Scenario: Default to today when viewing shift trade
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01'
	When I navigate to shift trade page
	Then the selected date should be '2030-01-01'

@ignore
Scenario: Default time line when I am not scheduled
	Given I have the role 'Full access to mytime'
	And Current time is '2020-10-24'
	When I navigate to shift trade page

@ignore
Scenario: Show my scheduled day at the top
	Given I have the role 'Full access to mytime'
	And Current time is '2030-01-01'
	When I navigate to shift trade page
	Then I should see my schedule with
	| Field | Value |
	| Start time	| 08:00 |
	| End time		| 17:00 |

@ignore
Scenario: 