Feature: 24394_ShifTradesBetweenAgentsInDifferentTimeZones
	In order to avoid unwanted scheduled shifts
	As an agent 
	I want to be able to swap shifts with agents in other timezones

Scenario: See shifts from colleagues in other timezone
	Given I am an agent in a team with access to the whole site
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-10-14 06:00 |
	| EndTime               | 2030-10-14 16:00 |
	| Shift category		| Day	           |
	And I have a colleague with a '+1' hour diff timezone 
	And My colleague have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-10-14 08:00 |
	| EndTime               | 2030-10-14 18:00 |
	| Shift category		| Day	           |
	When I view Add Shift Trade Request for date '2030-10-14'
	Then I should see OtherAgent in the shift trade list

Scenario: Shift trades from a colleague in another timezone should be presented in my timezone
	Given I am an agent in a team with access to the whole site
	And I have a colleague with a '+1' hour diff timezone 
	And My colleague have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-10-14 12:00 |
	| EndTime               | 2030-10-14 22:00 |
	| Shift category		| Day	           |
	And I have received a shift trade request
	| Field				| Value         |
	| From				| MyColleague	|
	| DateTo			| 2030-10-14    |
	| DateFrom			| 2030-10-14    |
	And I am viewing requests
	When I click on the request at position '1' in the list
	Then I should see a shift trade with start time '13:00'
	And I should see a shift trade with end time '23:00'

Scenario: Do not see shifts that starts on another day in my timezone
	Given I am an agent in a team with access to the whole site
	And I have a colleague with a '-16' hour diff timezone 
	And My colleague have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-10-14 08:00 |
	| EndTime               | 2030-10-14 18:00 |
	| Shift category		| Day	           |
	When I view Add Shift Trade Request for date '2030-10-14'
	Then I should not see any shifts to trade with