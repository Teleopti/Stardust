Feature: Shift Trade Requests
	In order to avoid unwanted scheduled shifts
	As an agent
	I want to be able to trade shifts with other agents

Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there is a workflow control set with
	| Field                            | Value                                     |
	| Name                             | Trade from tomorrow until 30 days forward |
	| Schedule published to date       | 2040-06-24                                |
	| Shift Trade sliding period start | 1                                         |
	| Shift Trade sliding period end   | 30                                        |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |	
	And there are shift categories
	| Name  |
	| Day   |
	| Late   |


Scenario: No access to make shift trade reuquests
	Given there is a role with
	| Field								| Value						|
	| Name								| No access to Shift Trade	|
	| Access To Shift Trade Requests	| False						|
	And I have the role 'No access to Shift Trade'
	When I view requests
	Then I should not see the New Shift Trade Request menu item
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
	Then the selected date should be '2030-01-02'

Scenario: Trades can not be made outside the shift trade period
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Current time is '2030-01-01'
	When I view Add Shift Trade Request for date '2030-02-15'
	Then the selected date should be '2030-01-31'

Scenario: Show my scheduled shift
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	And Current time is '2029-12-27'
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
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-02 06:00 |
	| EndTime               | 2030-01-02 16:00 |
	| Shift category		| Day	           |
	And Current time is '2030-01-01'
	When I view Add Shift Trade Request for date '2030-01-02'
	Then I should see a message text saying that no possible shift trades could be found

Scenario: Show my full day absence
	Given I have the role 'Full access to mytime'
	And there is an absence with
	| Field | Value   |
	| Name  | Vacation |
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a absence with
	| Field		| Value            |
	| Name      | Vacation         |
	| StartTime | 2030-01-02 00:00 |
	| EndTime   | 2030-01-02 23:59 |
	And Current time is '2030-01-01'
	When I view Add Shift Trade Request for date '2030-01-02'
	Then I should see my schedule with
	| Field			| Value |
	| Start time	| 08:00 |
	| End time		| 16:00 |

Scenario: Show my scheduled day off
	Given I have the role 'Full access to mytime'
	And there is a dayoff with
	| Field | Value  |
	| Name  | DayOff |
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And 'I' have a day off with
	| Field | Value      |
	| Name  | DayOff     |
	| Date  | 2030-01-04 |
	And Current time is '2030-01-01'
	When I view Add Shift Trade Request for date '2030-01-04'
	Then I should see my scheduled day off 'DayOff'
	And I should see the time line hours span from '8' to '17'

Scenario: View shift trade request details
	Given I have the role 'Full access to mytime'
	And I have created a shift trade request
	| Field			| Value				|
	| Subject		| swap with me		|
	And I am viewing requests
	When I click on the request
	Then I should see the shift trade request form  with subject 'swap with me'

Scenario: Close details when approving shift trade request
	Given I have the role 'Full access to mytime'
	And I have received a shift trade request from 'Some Person'
	And I am viewing requests
	When I click on the request
	And I click the Approve button on the shift request
	Then  Details should be closed

Scenario: Can not approve or deny shift trade request created by me
	Given I have the role 'Full access to mytime'
	And I have created a shift trade request to 'Some Person'
	And I am viewing requests
	When I click on the request
	Then I should not see the approve button
	And I should not see the deny button

@ignore
Scenario: Deny shift trade request
	Given I have the role 'Full access to mytime'
	And I have received a shift trade request
	And I am viewing requests
	When I click on the request
	And I click the Deny button on the shift request
	Then Details should be closed

Scenario: Should not be able to delete received shift trade request
	Given I am an agent
	And I have received a shift trade request
	| Field		| Value			|
	| From		| Ashley			|
	When I view requests
	Then I should not see a delete button on the request

Scenario: Show name of the person that created the shift trade request
	Given I have the role 'Full access to mytime'
	And I have received a shift trade request
	| Field		| Value					|
	| From		| Keil Randor			|
	And I am viewing requests
	When I click on the request
	Then I should see 'Keil Randor' as the sender of the request

Scenario: Show name of the person that recieved the shift trade request
	Given I have the role 'Full access to mytime'
	And I have created a shift trade request
	| Field	| Value					|
	| To		| Ashley Andeen		|
	And I am viewing requests
	When I click on the request
	Then I should see 'Ashley Andeen' as the receiver of the request


	

