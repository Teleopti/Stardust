
@MyTimeShiftTrades
Feature: Shift trade bulletin board from requests
	In order to make a shift trade with someone who has the same wishs
	As an agent
	I want to be able to see and pick a shift trade from bulletin board

Background:
	Given there is a site named 'The site'
	And there is a team named 'My team' on 'The site'
	And there is a role with
	| Field        | Value                 |
	| Name         | Full access to mytime |
	| AccessToTeam | My team               |
	And there is a workflow control set with
	| Field                            | Value                                     |
	| Name                             | Trade from tomorrow until 30 days forward |
	| Schedule published to date       | 2040-06-24                                |
	| Shift Trade sliding period start | 1                                         |
	| Shift Trade sliding period end   | 30                                        |
	And there is a workflow control set with
	| Field                            | Value                           |
	| Name                             | Anonymous trade 30 days forward |
	| Schedule published to date       | 2040-06-24                      |
	| Shift Trade sliding period start | 1                               |
	| Shift Trade sliding period end   | 30                              |
	| Anonymous trading                | true                            |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And there are shift categories
	| Name  |
	| Day   |
	| Night |
	| Late  |
	And there is a dayoff named 'Day Off'
	
	@NotKeyExample
Scenario: Shift trade in Bulletin board should start from tomorrow
	Given I have the role 'Full access to mytime'
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And the time is '2029-10-18'
	And I am viewing requests
	When I click to shift trade bulletin board
	Then I cannot navigate to the bulletin previous date

	@NotKeyExample
Scenario: Should show my shift and other shift in bulletin board
	Given I have the role 'Full access to mytime'
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 09:00 |
	| EndTime        | 2030-01-01 17:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 08:00 |
	| EndTime        | 2030-01-01 18:00 |
	| Shift category | Day              |
	And OtherAgent has a shift exchange for bulletin
	| Field     | Value            |
	| Valid To  | 2029-12-31       |
	| StartTime | 2030-01-01 09:00 |
	| EndTime   | 2030-01-01 17:00 |
	And the time is '2029-12-27'
	When I see 'my' shift on Shift Trade Bulletin Board on date '2030-01-01'
	Then I should see my schedule with
	| Field			| Value |
	| Start time	| 09:00 |
	| End time		| 17:00 |
	And I should see a possible schedule trade with 'OtherAgent'

Scenario: Should possible make shift trade in Bulletin board
	Given I have the role 'Full access to mytime'
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 09:00 |
	| EndTime               | 2030-01-01 17:00 |
	| Shift category		| Day	           |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 08:00 |
	| EndTime        | 2030-01-01 18:00 |
	| Shift category | Day              |
	And OtherAgent has a shift exchange for bulletin
	| Field         | Value            |
	| Valid To      | 2029-12-31       |
	| StartTime     | 2030-01-01 09:00 |
	| EndTime       | 2030-01-01 17:00 |
	| WishShiftType | WorkingShift     |
	And the time is '2029-12-27'
	When I see 'the other agent' shift on Shift Trade Bulletin Board on date '2030-01-01'
	And I click agent 'OtherAgent'
	And I enter subject 'A nice subject'
	And I enter message 'A cute little message'
	And I click send button in bulletin board
	Then Shift trade bulletin board view should not be visible
	And I should see a shift trade request in the list with subject 'A nice subject'

	@NotKeyExample
Scenario: Should possible make shift trade in Bulletin board with day off
	Given I have the role 'Full access to mytime'
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a day off with
	| Field | Value      |
	| Name  | Day Off    |
	| Date  | 2030-01-01 |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 08:00 |
	| EndTime        | 2030-01-01 18:00 |
	| Shift category | Day              |
	And OtherAgent has a shift exchange for bulletin
	| Field         | Value            |
	| Valid To      | 2029-12-31       |
	| StartTime     | 2030-01-01 09:00 |
	| EndTime       | 2030-01-01 17:00 |
	| WishShiftType | Day Off          |
	And the time is '2029-12-27'
	When I see 'the other agent' shift on Shift Trade Bulletin Board on date '2030-01-01'
	And I click agent 'OtherAgent'
	And I enter subject 'A nice subject'
	And I enter message 'A cute little message'
	And I click send button in bulletin board
	Then Shift trade bulletin board view should not be visible
	And I should see a shift trade request in the list with subject 'A nice subject'

	@NotKeyExample
Scenario: Should possibly make my empty day trade with main shift day in Bulletin board
	Given I have the role 'Full access to mytime'
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have an empty day with
	| Field | Value      |
	| Date  | 2030-01-01 |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 08:00 |
	| EndTime        | 2030-01-01 18:00 |
	| Shift category | Day              |
	And OtherAgent has a shift exchange for bulletin
	| Field         | Value            |
	| Valid To      | 2029-12-31       |
	| StartTime     | 2030-01-01 09:00 |
	| EndTime       | 2030-01-01 17:00 |
	| WishShiftType | EmptyDay       |
	And the time is '2029-12-27'
	When I see 'the other agent' shift on Shift Trade Bulletin Board on date '2030-01-01'
	And I click agent 'OtherAgent'
	And I enter subject 'A nice subject'
	And I enter message 'A cute little message'
	And I click send button in bulletin board
	Then Shift trade bulletin board view should not be visible
	And I should see a shift trade request in the list with subject 'A nice subject'

	@NotKeyExample
Scenario: Should possible make shift trade with empty day in Bulletin board
	Given I have the role 'Full access to mytime'
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have an empty day with
	| Field | Value      |
	| Date  | 2030-01-01 |
	And OtherAgent has an empty day with
	| Field | Value      |
	| Date  | 2030-01-01 |
	And OtherAgent has a shift exchange for bulletin
	| Field         | Value            |
	| Valid To      | 2029-12-31       |
	| StartTime     | 2030-01-01 09:00 |
	| EndTime       | 2030-01-01 17:00 |
	| WishShiftType | EmptyDay         |
	And the time is '2029-12-27'
	When I see 'the other agent' shift on Shift Trade Bulletin Board on date '2030-01-01'
	And I click agent 'OtherAgent'
	And I enter subject 'A nice subject'
	And I enter message 'A cute little message'
	And I click send button in bulletin board
	Then Shift trade bulletin board view should not be visible
	And I should see a shift trade request in the list with subject 'A nice subject'

	@NotKeyExample
Scenario: Should see the anonymous name when viewing empty day in Bulletin board
	Given I have the role 'Full access to mytime'
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And I have the workflow control set 'Anonymous trade 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have an empty day with
	| Field | Value      |
	| Date  | 2030-01-01 |
	And OtherAgent has an empty day with
	| Field | Value      |
	| Date  | 2030-01-01 |
	And OtherAgent has a shift exchange for bulletin
	| Field         | Value            |
	| Valid To      | 2029-12-31       |
	| StartTime     | 2030-01-01 09:00 |
	| EndTime       | 2030-01-01 17:00 |
	| WishShiftType | EmptyDay         |
	And the time is '2029-12-27'
	When I see anonym shift on Shift Trade Bulletin Board on date '2030-01-01'
	Then I should see agent name as Anonym
	
Scenario: Should list announced shift
	Given  I have the role 'Full access to mytime'
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I am american
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 09:00 |
	| EndTime               | 2030-01-01 17:00 |
	| Shift category		| Day	           |
	And I have a shift exchange for bulletin
	| Field     | Value            |
	| Valid To  | 2029-12-31       |
	| StartTime | 2030-01-01 06:00 |
	| EndTime   | 2030-01-01 14:00 |
	When I am viewing requests
	Then I should see the request of type 'Shift Trade Post' in the list

Scenario: Delete shift exchange offer in request list
	Given  I have the role 'Full access to mytime'
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 09:00 |
	| EndTime               | 2030-01-01 17:00 |
	| Shift category		| Day	           |
	And I have a shift exchange for bulletin
	| Field     | Value            |
	| Valid To  | 2029-12-31       |
	| StartTime | 2030-01-01 06:00 |
	| EndTime   | 2030-01-01 14:00 |
	And I am viewing requests
	When I delete the existing request in the list
	Then I should not see any requests in the list

	@NotKeyExample
Scenario: View shift trade post details
	Given  I have the role 'Full access to mytime'
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 09:00 |
	| EndTime               | 2030-01-01 17:00 |
	| Shift category		| Day	           |
	And I have a shift exchange for bulletin
	| Field     | Value            |
	| Valid To  | 2029-12-31       |
	| StartTime | 2030-01-01 06:00 |
	| EndTime   | 2030-01-01 14:00 |
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see the detail form for the existing request in the list
	And I should see the values of the shift trade post
	| Field          | Value      |
	| Offer end date | 2029-12-31 |
	| Start time     | 6:00       |
	| End time       | 14:00      |

Scenario: Should modify shift trade post
	Given  I have the role 'Full access to mytime'
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 09:00 |
	| EndTime               | 2030-01-01 17:00 |
	| Shift category		| Day	           |
	And I have a shift exchange for bulletin
	| Field     | Value            |
	| Valid To  | 2029-12-31       |
	| StartTime | 2030-01-01 06:00 |
	| EndTime   | 2030-01-01 14:00 |
	And I am viewing requests
	When I click on the existing request in the list
	And I change the shift trade post value with
	| Field          | Value      |
	| Offer end date | 2029-12-31 |
	| Start time     | 8:00       |
	| End time       | 16:00      |
	And I submit my changes for the existing shift trade post
	And I click on the existing request in the list
	Then I should see the updated values of the shift trade post
	| Field          | Value      |
	| Offer end date | 2029-12-31 |
	| Start time     | 8:00       |
	| End time       | 16:00      |

	@NotKeyExample
Scenario: Should not show agent name in shift trade board list
	Given I have the role 'Full access to mytime'
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And I have the workflow control set 'Anonymous trade 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 09:00 |
	| EndTime        | 2030-01-01 17:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 08:00 |
	| EndTime        | 2030-01-01 18:00 |
	| Shift category | Day              |
	And OtherAgent has a shift exchange for bulletin
	| Field     | Value            |
	| Valid To  | 2029-12-31       |
	| StartTime | 2030-01-01 09:00 |
	| EndTime   | 2030-01-01 17:00 |
	And the time is '2029-12-27'
	When I see 'the other agent' shift on Shift Trade Bulletin Board on date '2030-01-01'
	Then I should not see agent name in the possible schedule trade list

	@NotKeyExample
Scenario: Should not show subject and message box
	Given I have the role 'Full access to mytime'
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And I have the workflow control set 'Anonymous trade 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 09:00 |
	| EndTime        | 2030-01-01 17:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 08:00 |
	| EndTime        | 2030-01-01 18:00 |
	| Shift category | Day              |
	And OtherAgent has a shift exchange for bulletin
	| Field     | Value            |
	| Valid To  | 2029-12-31       |
	| StartTime | 2030-01-01 09:00 |
	| EndTime   | 2030-01-01 17:00 |
	And the time is '2029-12-27'
	When I see 'the other agent' shift on Shift Trade Bulletin Board on date '2030-01-01'
	And I click OtherAgent shift
	Then I should see a confirm message on bulletin trade board

	@NotKeyExample
Scenario: Should not show agent name in shift trade request detail view
	Given I have the role 'Full access to mytime'
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And I have the workflow control set 'Anonymous trade 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 09:00 |
	| EndTime               | 2030-01-01 17:00 |
	| Shift category		| Day	           |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 08:00 |
	| EndTime        | 2030-01-01 18:00 |
	| Shift category | Day              |
	And OtherAgent has a shift exchange for bulletin
	| Field         | Value            |
	| Valid To      | 2029-12-31       |
	| StartTime     | 2030-01-01 09:00 |
	| EndTime       | 2030-01-01 17:00 |
	| WishShiftType | WorkingShift     |
	And the time is '2029-12-27'
	When I see 'the other agent' shift on Shift Trade Bulletin Board on date '2030-01-01'
	And I click OtherAgent shift
	And I click send button in bulletin board
	And I am viewing requests
	And I click on the existing request in the list
	Then I should not see the agent name in detail view

Scenario: Should show my shift and other shifts filtered by site open hours in bulletin board
	Given I have the role 'Full access to mytime'
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team		 | My team |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | My team    |
	And There are open hours '08:00-17:00' for 'Monday,Tuesday,Wednesday,Thursday,Friday', in site 'The site'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 08:00 |
	| EndTime        | 2030-01-01 17:00 |
	| Shift category | Day              |
	And OtherAgent has a shift with
	| Field          | Value            |
	| StartTime      | 2030-01-01 08:00 |
	| EndTime        | 2030-01-01 16:00 |
	| Shift category | Day              |
	And OtherAgent has a shift exchange for bulletin
	| Field     | Value            |
	| Valid To  | 2029-12-31       |
	| StartTime | 2030-01-01 08:00 |
	| EndTime   | 2030-01-01 17:00 |
	And the time is '2029-12-27'
	When I see 'my' shift on Shift Trade Bulletin Board on date '2030-01-01'
	Then I should see my schedule with
	| Field			| Value |
	| Start time	| 08:00 |
	| End time		| 17:00 |
	And I should see a possible schedule trade with 'OtherAgent'

Scenario: Should not see request list in bulletin board view
	Given I have the role 'Full access to mytime'
	And I have an existing text request
	When I view Bulletin Board
	Then I should not see any request in current view