Feature: Shift Trade Request Add
	In order to avoid unwanted scheduled shifts
	As an agent
	I want to be able to trade shifts with other agents

Background:
	Given there is a site named 'The site'
	And there is a team named 'My team' on 'The site'
	And there is a team named 'Other team' on 'The site'
	And there is a role with
	| Field        | Value                 |
	| Name         | Full access to mytime |
	| AccessToTeam | Other team            |
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
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And OtherAgent2 has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And OtherAgentNotInMyTeam has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | Other team |
	And there are shift categories
	| Name  |
	| Day   |
	| Night |
	| Late  |

Scenario: No access to make shift trade reuquests
	Given there is a role with
	| Field								| Value						|
	| Name								| No access to Shift Trade	|
	| Access To Shift Trade Requests	| False						|
	And I have the role 'No access to Shift Trade'
	When I view requests
	Then I should not be able to make a new shift trade request

Scenario: No workflow control set
	Given I have the role 'Full access to mytime'
	And I do not have a workflow control set
	When I view Add Shift Trade Request
	Then I should see a message text saying I am missing a workflow control set

Scenario: Default to first day of open shift trade period
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And the current time is '2030-01-01'
	When I view Add Shift Trade Request
	Then the selected date should be '2030-01-02'

Scenario: Trades can not be made before the shift trade period
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And the current time is '2030-01-01'
	When I view Add Shift Trade Request
	Then I cannot navigate to the previous date

Scenario: Trades can not be made after the shift trade period
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And the current time is '2030-01-01'
	When I view Add Shift Trade Request for date '2030-01-31'
	Then I cannot navigate to the next date

Scenario: Show my scheduled shift
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	And the current time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should see my schedule with
	| Field			| Value |
	| Start time	| 06:00 |
	| End time		| 16:00 |

Scenario: Show possible shift trades
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	And OtherAgent has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 08:00 |
	| EndTime               | 2030-01-01 18:00 |
	| Shift category		| Day	           |
	And the current time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should see a possible schedule trade with 'OtherAgent'

Scenario: Show possible shift trade when victim has no schedule
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	And the current time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should not see a possible schedule trade with 'OtherAgent'
	And I should see a message text saying that no possible shift trades could be found

Scenario: When I only have access to my own data I can't trade shifts
	Given I am an agent in a team with access only to my own data
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And the current time is '2029-12-20'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should see a message text saying that I have no access to any teams

Scenario: Time line should cover my scheduled shift
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-03 06:05 |
	| EndTime               | 2030-01-03 15:55 |
	| Shift category		| Day	           |
	And the current time is '2030-01-01'
	When I view Add Shift Trade Request for date '2030-01-03'
	Then I should see the time line hours span from '06:00' to '16:00'

Scenario: Time line should cover all scheduled shifts
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	And OtherAgent has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 08:00 |
	| EndTime               | 2030-01-01 18:00 |
	| Shift category		| Day	           |
	And the current time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should see the time line hours span from '06:00' to '18:00'

Scenario: When clicking an agent in shift trade list, the other agent's should be hidden
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent2 have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent2 has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	And OtherAgent2 has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 07:00 |
	| EndTime               | 2030-01-01 20:00 |
	| Shift category		| Day	           |
	And OtherAgent has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 08:00 |
	| EndTime               | 2030-01-01 18:00 |
	| Shift category		| Day	           |
	And the current time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	And I click shift-trade-agent-name 'OtherAgent2'
	Then I should only see a possible schedule trade with 'OtherAgent2'
	
Scenario: Time line should cover scheduled night shift
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-03 22:00 |
	| EndTime               | 2030-01-04 07:00 |
	| Shift category		| Night	           |
	And the current time is '2030-01-01'
	When I view Add Shift Trade Request for date '2030-01-03'
	Then I should see the time line hours span from '22:00' to '07:00'

Scenario: Sending shift trade request closes the Add Shift Trade Request view
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	And OtherAgent has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 08:00 |
	| EndTime               | 2030-01-01 18:00 |
	| Shift category		| Day	           |
	And the current time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	And I click agent 'OtherAgent'
	And I enter subject 'A nice subject'
	And I enter message 'A cute little message'
	And I click send shifttrade button
	Then Add Shift Trade Request view should not be visible
	And I should see a shift trade request in the list with subject 'A nice subject'


Scenario: Cancel a shift trade request before sending 
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent2 have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent2 has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	And OtherAgent2 has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 07:00 |
	| EndTime               | 2030-01-01 20:00 |
	| Shift category		| Day	           |
	And OtherAgent has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 08:00 |
	| EndTime               | 2030-01-01 18:00 |
	| Shift category		| Day	           |
	And the current time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	And I click shift-trade-agent-name 'OtherAgent'
	And I click cancel button
	Then I should see a possible schedule trade with 'OtherAgent'
	And I should see a possible schedule trade with 'OtherAgent2'

Scenario: Show message when no agents are available for shift trade
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-02 06:00 |
	| EndTime               | 2030-01-02 16:00 |
	| Shift category		| Day	           |
	And the current time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-02'
	Then I should see a message text saying that no possible shift trades could be found

Scenario: Show my full day absence
	Given I have the role 'Full access to mytime'
	And there is an absence with
	| Field | Value   |
	| Name  | Vacation |
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have an absence with
	| Field		| Value            |
	| Name      | Vacation         |
	| StartTime | 2030-01-02 00:00 |
	| EndTime   | 2030-01-02 23:59 |
	And the current time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-02'
	Then I should see my schedule with
	| Field			| Value |
	| Start time	| 08:00 |
	| End time		| 16:00 |

Scenario: Default to my team
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgentNotInMyTeam have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-02 06:00 |
	| EndTime               | 2030-01-02 16:00 |
	| Shift category		| Day	           |
	And OtherAgent has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-02 10:00 |
	| EndTime               | 2030-01-02 20:00 |
	| Shift category		| Late	           |
	And OtherAgentNotInMyTeam has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-02 08:00 |
	| EndTime               | 2030-01-02 18:00 |
	| Shift category		| Day	           |
	And the current time is '2030-01-01'
	When I view Add Shift Trade Request
	Then the option 'The site/My team' should be selected
	And I should see a possible schedule trade with 'OtherAgent'
	And I should not see a possible schedule trade with 'OtherAgentNotInMyTeam'

Scenario: Change team
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgentNotInMyTeam have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	And OtherAgentNotInMyTeam has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 08:00 |
	| EndTime               | 2030-01-01 18:00 |
	| Shift category		| Day	           |
	And the current time is '2029-12-27'
	And I view Add Shift Trade Request for date '2030-01-01'
	And I should see a message text saying that no possible shift trades could be found
	When I select the 'Other team'
	Then the option 'The site/Other team' should be selected
	And I should see a possible schedule trade with 'OtherAgentNotInMyTeam'

Scenario: Change date and keep selected team
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And the current time is '2029-12-27'
	And I view Add Shift Trade Request for date '2030-01-01'
	When I select the 'Other team'
	And I click on the next date
	Then the option 'The site/Other team' should be selected

Scenario: Cannot trade shifts when teamless
	Given I have the role 'Full access to mytime'
	And my last working date as an agent in the organisation is '2030-01-01'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-02 06:00 |
	| EndTime               | 2030-01-02 16:00 |
	| Shift category		| Day	           |
	And OtherAgent has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-02 08:00 |
	| EndTime               | 2030-01-02 18:00 |
	| Shift category		| Day	           |
	And the current time is '2030-01-01'
	When I view Add Shift Trade Request
	Then I should see a message text saying that I have no access to any teams
	And I should not see a possible schedule trade with 'OtherAgent'

Scenario: Show possible shift trades from my team
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgentNotInMyTeam have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	And OtherAgent has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 10:00 |
	| EndTime               | 2030-01-01 20:00 |
	| Shift category		| Late	           |
	And OtherAgentNotInMyTeam has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 08:00 |
	| EndTime               | 2030-01-01 18:00 |
	| Shift category		| Day	           |
	And the current time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should see a possible schedule trade with 'OtherAgent'
	And I should not see a possible schedule trade with 'OtherAgentNotInMyTeam'

@ignore
Scenario: Paging possible shifts
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have possible shift trades with
	| Field                              | Value                                     |
	| Date                               | 2030-01-01                                |
	| Possible trade count               | 60                                        |
	| Workflow control set     | Trade from tomorrow until 30 days forward |
	| Person period start date | 2012-06-18                                |
	| Shift category            | Day                                       |
	| Team                      | My team                                   |
	And the current time is '2029-12-27'
	And I view Add Shift Trade Request for date '2030-01-01'
	And I can see '50' possible shift trades
	When I scroll down to the bottom of the shift trade section
	Then I can see '60' possible shift trades

Scenario: Sort possible shift trades by starttime
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent2 have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	And OtherAgent has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 09:00 |
	| EndTime               | 2030-01-01 18:00 |
	| Shift category		| Day	           |
	And OtherAgent2 has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 08:00 |
	| EndTime               | 2030-01-01 18:00 |
	| Shift category		| Day	           |
	And the current time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should see 'OtherAgent2' first in the list
	And I should see 'OtherAgent' last in the list





