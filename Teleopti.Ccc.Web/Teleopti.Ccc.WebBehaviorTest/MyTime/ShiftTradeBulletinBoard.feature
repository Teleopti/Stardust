﻿@OnlyRunIfEnabled('MyTimeWeb_ShiftTradeExchangeBulletin_31296')
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
	And there are shift categories
	| Name  |
	| Day   |
	| Night |
	| Late  |


Scenario: Shift trade in Bulletin board should start from tomorrow
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And the current time is '2029-10-18'
	And I am viewing requests
	When I click to shift trade bulletin board
	Then I cannot navigate to the bulletin previous date	

Scenario: Should show my shift and other shift in bulletin board
	Given I have the role 'Full access to mytime'
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
	| Field          | Value      |
	| Offer end date | 2029-12-31 |
	| Start time     | 9:00       |
	| End time       | 17:00      |
	And the current time is '2029-12-27'
	When I view Shift Trade Bulletin Board for date '2030-01-01'
	Then I should see my schedule with
	| Field			| Value |
	| Start time	| 09:00 |
	| End time		| 17:00 |
	And I should see a possible schedule trade with 'OtherAgent'

Scenario: Should possible make shift trade in Bulletin board
	Given I have the role 'Full access to mytime'
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
	| Field          | Value      |
	| Offer end date | 2029-12-31 |
	| Start time     | 9:00       |
	| End time       | 17:00      |
	And the current time is '2029-12-27'
	When I view Shift Trade Bulletin Board for date '2030-01-01'
	And I click agent 'OtherAgent'
	And I enter subject 'A nice subject'
	And I enter message 'A cute little message'
	And I click send button in bulletin board
	Then Shift trade bulletin board view should not be visible
	And I should see a shift trade request in the list with subject 'A nice subject'
