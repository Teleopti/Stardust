@OnlyRunIfEnabled('MyTimeWeb_ShiftTradeExchangeBulletin_31296')
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
	| AccessToTeam | My team            |
	And there is a workflow control set with
	| Field                            | Value                                     |
	| Name                             | Trade from tomorrow until 30 days forward |
	| Schedule published to date       | 2040-06-24                                |
	| Shift Trade sliding period start | 1                                         |
	| Shift Trade sliding period end   | 30                                        |


Scenario: Shift trade in Bulletin board should start from tomorrow
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And the current time is '2029-10-18'
	And I am viewing requests
	When I click to shift trade bulletin board
	Then I cannot navigate to the bulletin previous date	