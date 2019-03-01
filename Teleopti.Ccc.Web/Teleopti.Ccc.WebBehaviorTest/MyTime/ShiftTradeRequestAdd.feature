@MyTimeShiftTrades
Feature: Shift Trade Request Add
	In order to avoid unwanted scheduled shifts
	As an agent
	I want to be able to trade shifts with other agents

Background:
	Given there is a site named 'The site'
	And there is a site named 'Other site'
	And there is a team named 'My team' on 'The site'
	And there is a team named 'Other team' on 'The site'
	And there is a team named 'Team in other site' on 'Other site'
	And there is a role with
	| Field          | Value                 |
	| Name           | Full access to mytime |
	| AccessToMySite | true                  |
	| AccessToSite   | Other site            |
	| AccessToTeam   | Other team            |
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
	And OtherAgent1 has a person period with
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
	And OtherAgentNotInMySite has a person period with
	| Field      | Value              |
	| Start date | 2012-06-18         |
	| Team       | Team in other site |
	| Site       | Other site         |
	And there are shift categories
	| Name  |
	| Day   |
	| Night |
	| Late  |
	And there is a dayoff with
	| Field | Value  |
	| Name  | DayOff |
	And I am englishspeaking swede


Scenario: Should trade when not break Maximum workday rule
	Given I have the role 'Full access to mytime'
	And I am american
	And there is a workflow control set with
	| Field                            | Value                    |
	| Name                             | WFC with maximum workday |
	| Schedule published to date       | 2040-06-24               |
	| Shift Trade sliding period start | 1                        |
	| Shift Trade sliding period end   | 30                       |
	| Maximum Workdays                 | 2                        |
	| ShiftTradeFlexibilityDays        | 1000                     |
	| AutoGrantShiftTradeRequest       | true                     |
	And I have the workflow control set 'WFC with maximum workday'
	And OtherAgent has the workflow control set 'WFC with maximum workday'
	And 'I' have a day off with
	| Field | Value      |
	| Name  | DayOff     |
	| Date  | 2030-01-01 |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-02 06:00 |
	| EndTime               | 2030-01-02 16:00 |
	| Shift category		| Day	           |
	And 'I' have a day off with
	| Field | Value      |
	| Name  | DayOff     |
	| Date  | 2030-01-03 |	
	And 'I' have a day off with
	| Field | Value      |
	| Name  | DayOff     |
	| Date  | 2030-01-04 |
	And OtherAgent has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-03 06:00 |
	| EndTime               | 2030-01-03 16:00 |
	| Shift category		| Day	           |
	And I have received a shift trade request
		| Field    | Value        |
		| From     | OtherAgent   |
		| DateTo   | 2030-01-03   |
		| DateFrom | 2030-01-03   |
		| Pending  | True         |
		| Subject  | trade please |
	And the time is '2029-12-27'
	And I am viewing requests
	When I click on the existing request in the list
	And I click the Approve button on the shift request
	And I wait half second and refresh the request page
	Then I should see the existing shift trade request be approved

	@NotKeyExample
Scenario: No access to make shift trade reuquests
	Given there is a role with
	| Field								| Value						|
	| Name								| No access to Shift Trade	|
	| Access To Shift Trade Requests	| False						|
	And I have the role 'No access to Shift Trade'
	When I view requests
	Then I should not be able to make a new shift trade request

	@NotKeyExample
Scenario: No workflow control set
	Given I have the role 'Full access to mytime'
	And I do not have a workflow control set
	When I view Add Shift Trade Request
	Then I should see a message text saying I am missing a workflow control set

	@NotKeyExample
Scenario: Default to first day of open shift trade period
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And the time is '2030-01-01'
	When I view Add Shift Trade Request
	Then the selected date should be '2030-01-02'

	@NotKeyExample
Scenario: Trades can not be made before the shift trade period
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And the time is '2030-01-01'
	When I view Add Shift Trade Request
	Then I cannot navigate to the previous date

	@NotKeyExample
Scenario: Trades can not be made after the shift trade period
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And the time is '2030-01-01'
	When I view Add Shift Trade Request for date '2030-01-31'
	Then I cannot navigate to the next date

	@NotKeyExample
Scenario: Show my scheduled shift
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	And the time is '2029-12-27'
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
	And the time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should see a possible schedule trade with 'OtherAgent'

	@NotKeyExample
Scenario: Should show error message for server side error
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And the time is '2079-05-20'
	# Max smalldatetime for SQL Server is 2079-06-06
	When I view Add Shift Trade Request for date '2079-06-06'
	And I click on the next date
	Then I should see an error message

	@NotKeyExample
Scenario: Show possible shift trade when victim has full day absence
	Given I have the role 'Full access to mytime'
	And there is an absence with
	| Field | Value   |
	| Name  | Vacation |
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	And OtherAgent has an absence with
	| Field		| Value            |
	| Name      | Vacation         |
	| StartTime | 2030-01-01 00:00 |
	| EndTime   | 2030-01-01 23:59 |
	And the time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should not see a possible schedule trade with 'OtherAgent'
	And I should see a message text saying that no possible shift trades could be found

	@NotKeyExample
Scenario: Show possible shift trade when victim has no schedule
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	And the time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should see a possible schedule trade with 'OtherAgent'

	@NotKeyExample
Scenario: When I only have access to my own data I can't trade shifts
	Given I have a role with
         | Field            | Value |
         | Access to my own | true  |
  And I am in a team with published schedule
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And the time is '2029-12-20'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should see a message text saying that I have no access to any teams

	@NotKeyExample
Scenario: Time line should cover my scheduled shift
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-03 06:05 |
	| EndTime               | 2030-01-03 15:55 |
	| Shift category		| Day	           |
	And the time is '2030-01-01'
	When I view Add Shift Trade Request for date '2030-01-03'
	Then I should see the time line hours span from '06:00' to '16:00'

	@NotKeyExample
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
	And the time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should see the time line hours span from '06:00' to '18:00'

	@NotKeyExample
	@OnlyRunIfDisabled('MyTimeWeb_ShiftTradeRequest_ShowMultipleShifts_74947')
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
	And the time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	And I click shift-trade-agent-name 'OtherAgent2'
	Then I should only see a possible schedule trade with 'OtherAgent2'
	
	@NotKeyExample
Scenario: Time line should cover scheduled night shift
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-03 22:00 |
	| EndTime               | 2030-01-04 07:00 |
	| Shift category		| Night	           |
	And the time is '2030-01-01'
	When I view Add Shift Trade Request for date '2030-01-03'
	Then I should see the time line hours span from '22:00' to '07:00'

	@OnlyRunIfDisabled('MyTimeWeb_ShiftTradeRequest_ShowMultipleShifts_74947')
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
	And the time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	And I click agent 'OtherAgent'
	And I enter subject 'A nice subject'
	And I enter message 'A cute little message'
	And I click send shifttrade button
	Then Add Shift Trade Request view should not be visible
	And I should see a shift trade request in the list with subject 'A nice subject'

	@NotKeyExample
	@OnlyRunIfDisabled('MyTimeWeb_ShiftTradeRequest_ShowMultipleShifts_74947')
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
	And the time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	And I click shift-trade-agent-name 'OtherAgent'
	And I click cancel button
	Then I should see a possible schedule trade with 'OtherAgent'
	And I should see a possible schedule trade with 'OtherAgent2'

	@NotKeyExample
Scenario: Show message when no agents are available for shift trade
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-02 06:00 |
	| EndTime               | 2030-01-02 16:00 |
	| Shift category		| Day	           |
	And the time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-02'
	Then I should see a message text saying that no possible shift trades could be found

	@NotKeyExample
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
	And the time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-02'
	Then I should see my schedule with
	| Field			| Value |
	| Start time	| 08:00 |
	| End time		| 16:00 |

	@NotKeyExample
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
	And the time is '2030-01-01'
	When I view Add Shift Trade Request
	Then the option 'My team' should be selected
	And I should see a possible schedule trade with 'OtherAgent'
	And I should not see a possible schedule trade with 'OtherAgentNotInMyTeam'

	@NotKeyExample
Scenario: Change site
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgentNotInMySite have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 06:00 |
	| EndTime        | 2030-01-01 16:00 |
	| Shift category | Day              |
	And OtherAgentNotInMySite has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 08:00 |
	| EndTime        | 2030-01-01 18:00 |
	| Shift category | Day              |
	And the time is '2029-12-27'
	And I view Add Shift Trade Request for date '2030-01-01'
	And I should see a message text saying that no possible shift trades could be found
	When I select the site filter 'Other site'
	Then the option for site filter 'Other site' should be selected
	And I should see a possible schedule trade with 'OtherAgentNotInMySite'

	@NotKeyExample
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
	And the time is '2029-12-27'
	And I view Add Shift Trade Request for date '2030-01-01'
	And I should see a message text saying that no possible shift trades could be found
	When I select the 'Other team'
	Then the option 'Other team' should be selected
	And I should see a possible schedule trade with 'OtherAgentNotInMyTeam'

	@NotKeyExample
Scenario: Change date and keep selected team
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And the time is '2029-12-27'
	And I view Add Shift Trade Request for date '2030-01-01'
	When I select the 'Other team'
	And I click on the next date
	Then the option 'Other team' should be selected

	@NotKeyExample
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
	And the time is '2030-01-01'
	When I view Add Shift Trade Request
	Then I should not see a possible schedule trade with 'OtherAgent'

	@NotKeyExample
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
	And the time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should see a possible schedule trade with 'OtherAgent'
	And I should not see a possible schedule trade with 'OtherAgentNotInMyTeam'

	@NotKeyExample
Scenario: Sort possible shift trades by starttime for default
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
	And the time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should see 'OtherAgent2' first in the list
	And I should see 'OtherAgent' last in the list

	@NotKeyExample
Scenario: Show possible shift trades with day off
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 06:00 |
	| EndTime        | 2030-01-01 16:00 |
	| Shift category | Day              |
	And 'OtherAgent' has a day off with
	| Field | Value      |
	| Name  | DayOff     |
	| Date  | 2030-01-01 |
	And the time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should see 'OtherAgent' last in the list

	@NotKeyExample
Scenario: Show MySchedule when it is day off
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And 'I' has a day off with
	| Field | Value      |
	| Name  | DayOff     |
	| Date  | 2030-01-01 |
	And 'OtherAgent' has a day off with
	| Field | Value      |
	| Name  | DayOff     |
	| Date  | 2030-01-01 |
	And the time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	Then I should see MySchedule is dayoff

	@OnlyRunIfDisabled('MyTimeWeb_ShiftTradeRequest_ShowMultipleShifts_74947')
Scenario: Should be able to add a day to a shift trade
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-03-01 06:00 |
	| EndTime        | 2030-03-01 16:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-03-01 09:00 |
	| EndTime        | 2030-03-01 18:00 |
	| Shift category | Day              |
	And the time is '2030-02-27'
	When I view Add Shift Trade Request for date '2030-03-01'
	And I choose 'OtherAgent' to make a shift trade
	Then I should see 'OtherAgent' in my shift trade list for date '2030-03-01'

	@OnlyRunIfDisabled('MyTimeWeb_ShiftTradeRequest_ShowMultipleShifts_74947')
Scenario: Should be able to remove a day from a shift trade
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 06:00 |
	| EndTime        | 2030-01-01 16:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 09:00 |
	| EndTime        | 2030-01-01 18:00 |
	| Shift category | Day              |
	And the time is '2029-12-27'
	And I view Add Shift Trade Request for date '2030-01-01'
	And I choose 'OtherAgent' to make a shift trade
	When I remove the selected day from the shift trade list
	Then I should not see schedule on date '2030-01-01' in my shift trade list with 'OtherAgent'

	@NotKeyExample
	@OnlyRunIfDisabled('MyTimeWeb_ShiftTradeRequest_ShowMultipleShifts_74947')
Scenario: Should navigate to next day for the agent I am going to trade with
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 06:00 |
	| EndTime        | 2030-01-01 16:00 |
	| Shift category | Day              |
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-02 06:00 |
	| EndTime        | 2030-01-02 16:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 09:00 |
	| EndTime        | 2030-01-01 18:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-02 09:00 |
	| EndTime        | 2030-01-02 18:00 |
	| Shift category | Day              |
	And the time is '2029-12-27'
	And I view Add Shift Trade Request for date '2030-01-01'
	And I choose 'OtherAgent' to make a shift trade
	When I click on the next date
	Then I should see 'OtherAgent' in my shift trade list for date '2030-01-01'
	And I should see 'OtherAgent' can be added for date '2030-01-02'

	@NotKeyExample
	@OnlyRunIfDisabled('MyTimeWeb_ShiftTradeRequest_ShowMultipleShifts_74947')
Scenario: Should navigate to previous day for the agent I am going to trade with
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 06:00 |
	| EndTime        | 2030-01-01 16:00 |
	| Shift category | Day              |
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-02 06:00 |
	| EndTime        | 2030-01-02 16:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 09:00 |
	| EndTime        | 2030-01-01 18:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-02 09:00 |
	| EndTime        | 2030-01-02 18:00 |
	| Shift category | Day              |
	And the time is '2029-12-27'
	And I view Add Shift Trade Request for date '2030-01-02'
	And I choose 'OtherAgent' to make a shift trade
	When I click on the previous date
	Then I should see 'OtherAgent' in my shift trade list for date '2030-01-02'
	And I should see 'OtherAgent' can be added for date '2030-01-01'

	@NotKeyExample
	@OnlyRunIfDisabled('MyTimeWeb_ShiftTradeRequest_ShowMultipleShifts_74947')
Scenario: Should be able to choose a date in calender and show the shift for the agent I am going to trade with
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 06:00 |
	| EndTime        | 2030-01-01 16:00 |
	| Shift category | Day              |
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-05 06:00 |
	| EndTime        | 2030-01-05 16:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 09:00 |
	| EndTime        | 2030-01-01 18:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-05 09:00 |
	| EndTime        | 2030-01-05 18:00 |
	| Shift category | Day              |
	And the time is '2029-12-27'
	And I view Add Shift Trade Request for date '2030-01-01'
	And I choose 'OtherAgent' to make a shift trade
	When I select date '2030-01-05' by calender
	Then I should see 'OtherAgent' in my shift trade list for date '2030-01-01'
	And I should see 'OtherAgent' can be added for date '2030-01-05'

	@NotKeyExample
	@OnlyRunIfDisabled('MyTimeWeb_ShiftTradeRequest_ShowMultipleShifts_74947')
Scenario: Should be able to cancel a not sent shift trade and go back to overview
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 06:00 |
	| EndTime        | 2030-01-01 16:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 09:00 |
	| EndTime        | 2030-01-01 18:00 |
	| Shift category | Day              |
	And the time is '2029-12-27'
	And I view Add Shift Trade Request for date '2030-01-01'
	And I choose 'OtherAgent' to make a shift trade
	When I click cancel button
	Then I should see 'OtherAgent' last in the list

	@NotKeyExample
	@OnlyRunIfDisabled('MyTimeWeb_ShiftTradeRequest_ShowMultipleShifts_74947')
Scenario: The added days should be sorted by date
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-03 06:00 |
	| EndTime        | 2030-01-03 16:00 |
	| Shift category | Day              |
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-05 06:00 |
	| EndTime        | 2030-01-05 16:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-03 09:00 |
	| EndTime        | 2030-01-03 18:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-05 09:00 |
	| EndTime        | 2030-01-05 18:00 |
	| Shift category | Day              |
	And the time is '2030-01-01'
	And I view Add Shift Trade Request for date '2030-01-05'
	And I choose 'OtherAgent' to make a shift trade
	When I select date '2030-01-03' by calender
	And I add 'OtherAgent' to my shift trade list
	Then I should see 'OtherAgent' for date '2030-01-03' at top of my shift trade list

	@NotKeyExample
	@OnlyRunIfDisabled('MyTimeWeb_ShiftTradeRequest_ShowMultipleShifts_74947')
Scenario: Should cancel the current shift trade when switch to another team to trade
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgentNotInMyTeam have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 06:00 |
	| EndTime        | 2030-01-01 16:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 09:00 |
	| EndTime        | 2030-01-01 18:00 |
	| Shift category | Day              |
	And OtherAgentNotInMyTeam has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 09:00 |
	| EndTime        | 2030-01-01 18:00 |
	| Shift category | Day              |
	And the time is '2029-12-27'
	And I view Add Shift Trade Request for date '2030-01-01'
	And I choose 'OtherAgent' to make a shift trade
	When I select the 'Other team'
	And I clear the name search box
	Then I should see 'OtherAgentNotInMyTeam' last in the list
	And I choose 'OtherAgentNotInMyTeam' to make a shift trade
	And I should not see schedule on date '2030-01-01' in my shift trade list with 'OtherAgent'
	And I should see 'OtherAgentNotInMyTeam' in my shift trade list for date '2030-01-01'

	@NotKeyExample
Scenario: Show possible shift trades from All
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
	And the time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	And I select the 'All'
	Then I should see a possible schedule trade with 'OtherAgent'
	And I should see a possible schedule trade with 'OtherAgentNotInMyTeam' 

	@NotKeyExample
Scenario: Show possible shift trades with name filter
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent1 has the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent2 has the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category		| Day	           |
	And OtherAgent1 has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 10:00 |
	| EndTime               | 2030-01-01 20:00 |
	| Shift category		| Late	           |
	And OtherAgent2 has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 10:00 |
	| EndTime        | 2030-01-01 20:00 |
	| Shift category | Late             |
	And the time is '2029-12-27'
	When I view Add Shift Trade Request for date '2030-01-01'
	And I type '2' in the name search box 
	Then I should see a possible schedule trade with 'OtherAgent2'
	And I should not see a possible schedule trade with 'OtherAgent1' 

@OnlyRunIfEnabled('MyTimeWeb_Request_CleanUpRequestHisotry_77776')
@NotKeyExample
Scenario: Should not see request list in shift trade view
	Given I have the role 'Full access to mytime'
	And I have an existing text request
	When I view Add Shift Trade Request
	Then I should not see any request in current view
