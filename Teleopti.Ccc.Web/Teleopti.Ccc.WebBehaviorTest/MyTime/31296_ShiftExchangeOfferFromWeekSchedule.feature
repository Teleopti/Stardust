@OnlyRunIfEnabled('MyTimeWeb_ShiftTradeExchangeBulletin_31296')
Feature: Publish shift exchange offer
	In order to trade a shift I don't want
	As an agent 
	I want to publish it for others to grab

Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there are shift categories
	| Name  |
	| Day   |
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2023-08-28         |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2022-08-19 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2022-08-19 |

Scenario: Add shift trade exchange offer for a shift
	Given I have the role 'Full access to mytime'
	And I have a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2022-08-20 09:00 |
	| End time         | 2022-08-20 18:00 |
	And I view my week schedule for date '2022-08-20'
	When I click on the day summary for date '2022-08-20'
	And I click add new shift exchange offer
	Then I should see add shift exchange offer form with
	| Field          | Value      |
	| Offer end date | 2022-08-13 |
	| Start time     | 8:00       |
	| End time       | 17:00      |

	@ignore
Scenario: Default shift exchange offer values on dayoff
	Given I have the role 'Full access to mytime'
	And I have a day off with
	| Field | Value      |
	| Name  | DayOff     |
	| Date  | 2022-08-20 |
	And I view my week schedule for date '2022-08-20'
	When I click on the day summary for date '2022-08-20'
	And I click add new shift exchange offer
	Then I should see add shift exchange offer form with
	| Field      | Value      |
	| Offer end date | 2022-08-18 |
	| Start time | 08:00      |
	| End time   | 17:00      |

	@ignore
Scenario: Submit shift exchange offer
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2022-08-20'
	When I click on the day summary for date '2022-08-20'
	And I click add new shift exchange offer
	And I input shift exchange offer with
	| Field             | Value |
	| Start time        | 16:30 |
	| End time          | 03:00 |
	| End time next day | true  |
	And I click submit button
	Then I should see an shift exchange offer symbol

	@ignore
Scenario: Add invalid shift exchange offer
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2022-08-20'
	When I click on the day summary for date '2022-08-20'
	And I click add new shift exchange offer
	And I input shift exchange offer with
	| Field      | Value |
	| Start time | 13:30 |
	| End time   | 11:00 |
	And I click submit button
	Then I should see the 'shift exchange offer error' 'End time'
	And I should not see an shift exchange offer symbol for date '2022-08-20'

