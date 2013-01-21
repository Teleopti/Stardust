﻿
Feature: Shift Trade Requests
	In order to avoid unwanted scheduled shifts
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
	#And there is a dayoff with
	#| Field | Value  |
	#| Name  | DayOff |
	#And there is an absence with
	#| Field | Value   |
	#| Name  | Illness |
	#And there is a skill with
	#| Field | Value   |
	#| Name  | Skill 1 |
	And there is a workflow control set with
	| Field                            | Value                                     |
	| Name                             | Trade from tomorrow until 30 days forward |
	| Schedule published to date       | 2040-06-24                                |
	| Shift Trade sliding period start | 2                                         |
	| Shift Trade sliding period end   | 30                                        |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |	
	#And I have a shift with
	#| Field                 | Value            |
	#| StartTime             | 2030-01-01 06:00 |
	#| EndTime               | 2030-01-01 16:00 |
	#| Shift category		| Day	           |
	#| Lunch3HoursAfterStart | true             |
	#And an agent has a workflow control set with
	#| Field                      | Value              |
	#| Agent name                 | Other agent 1      |
	#| Name                       | Published schedule |
	#| Schedule published to date | 2040-06-24         |
	#And an agent has a schedule period with 
	#| Field      | Value         |
	#| Agent name | Other agent 1 |
	#| Start date | 2012-06-18    |
	#| Type       | Week          |
	#| Length     | 1		     |
	#And an agent has a person period with 
	#| Field      | Value         |
	#| Agent name | Other agent 1 |
	#| Start date | 2012-06-18    |
	#| Skill      | Skill 1       |
	
Scenario: No access to make shift trade reuquests
	Given I have the role 'No access to Shift Trade'
	When I view requests
	Then I should not see the Create Shift Trade Request button
	And I should not see the Requests button

Scenario: No workflow control set
	Given I have the role 'Full access to mytime'
	And I do not have a workflow control set
	When I view Add Shift Trade Request
	Then I should see a message text saying I am missing a workflow control set
	And I should not see the datepicker

Scenario: Default to first day of open shift trade period
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Current time is '2030-01-01'
	When I view Add Shift Trade Request
	Then the selected date should be '2030-01-03'

Scenario: Trades can not be made outside the shift trade period
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Current time is '2030-01-01'
	When I view Add Shift Trade Request for date '2030-02-15'
	Then the selected date should be '2030-01-31'

Scenario: Show my scheduled shift
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Current time is '2029-12-27'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should see my schedule with
	| Field			| Value |
	| Start time	| 06:00 |
	| End time		| 16:00 |

Scenario: Time line should cover my scheduled shift
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-03 06:05 |
	| EndTime               | 2030-01-03 15:55 |
	| Shift category		| Day	           |
	And Current time is '2030-01-01'
	When I view Add Shift Trade Request for date '2030-01-03'
	Then I should see the time line hours span from '6' to '16'

Scenario: Show message when no agents are available for shift trade
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Current time is '2013-01-01'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-02 06:00 |
	| EndTime               | 2030-01-02 16:00 |
	| Shift category		| Day	           |
	When I view Add Shift Trade Request for date '2030-01-02'
	Then I should see a message text saying that no possible shift trades could be found

Scenario: Show my scheduled day off
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a day off with
	| Field | Value      |
	| Date  | 2030-01-04 |
	| Name  | DayOff     |
	And Current time is '2030-01-01'
	When I view Add Shift Trade Request for date '2030-01-04'
	Then I should see my scheduled day off 'DayOff'
	And I should see the time line hours span from '8' to '17'

Scenario: Show my full-day absence
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have absence with
	| Field		| Value      |
	| Date		| 2030-01-05 |
	| Name		| Illness	 |
	| Full Day  | True		 |
	And Current time is '2030-01-01'
	When I view Add Shift Trade Request for date '2030-01-05'
	Then I should see my scheduled absence 'Illness'
	And I should see the time line hours span from '8' to '17'

@ignore
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

@ignore
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

@ignore
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

@ignore
Scenario: View shift trade request details
	Given I have the role 'Full access to mytime'
	And I have an existing shift trade request
	And I am viewing requests
	When I click on the request
	Then I should see the shift trade request form 
	
@ignore
Scenario: Approve shift trade request
	Given I have the role 'Full access to mytime'
	And I have created a shift trade request
	And I am viewing requests
	When I click on the request
	And I click the Approve button on the shift request
	Then I should not see the shift trade request in the list

@ignore
Scenario: Deny shift trade request
	Given I have the role 'Full access to mytime'
	And I have received a shift trade request
	And I am viewing requests
	When I click on the request
	And I click the Deny button on the shift request
	Then I should not see the shift trade request in the list

@ignore
Scenario: Delete created shift trade request
	Given I have the role 'Full access to mytime'
	And I have created a shift trade request
	And I am viewing requests
	When I click the shift trade request's delete button
	Then I should not see the shift trade request in the list

@ignore
Scenario: Should not be able to delete received shift trade request
	Given I am an agent
	And I have received a shift trade request from 'Ashley'
	When I view requests
	Then I should not see any delete button on my existing shift trade request

@ignore
Scenario: Approve shift trade on same day request should update shift in schedule
	Given I have the role 'Full access to mytime'
	And Current time is '2012-01-14'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2012-01-15 10:00 |
	| EndTime               | 2012-01-15 15:00 |
	| Shift category			| Night	          |
	And an agent has a shift with
	| Field                 | Value            |
	| Agent name            | Other agent 1    |
	| StartTime             | 2012-01-15 11:00 |
	| EndTime               | 2012-01-15 16:00 |
	| Shift category        | Late             |
	And I have an existing shift trade request for '2012-01-15' with 'Other agent 1'
	And I am viewing requests
	When I click on the request
	And I click the Approve button on the shift request
	And I navigate to week schedule page for date '2012-01-15'
	When I view my week schedule for date '2012-01-15'
	Then I should see activities on date '2012-01-15' with:
	| Field                 | Value         |
	| First activity times  | 11:00 - 16:00 |
