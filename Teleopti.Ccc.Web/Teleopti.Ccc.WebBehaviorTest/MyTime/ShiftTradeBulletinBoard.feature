@OnlyRunIfEnabled('MyTimeWeb_ShiftTradeExchangeBulletin_31296')
Feature: Shift trade bulletin board from requests
	In order to make a shift trade with someone who has the same wishs
	As an agent
	I want to be able to see and pick a shift trade from bulletin board

Scenario: Should open shift trade bulletin board
	Given I am an agent
	And I am viewing requests
	When I click to shift trade bulletin board
	Then I should see the bulletin board

