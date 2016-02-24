Feature: Publish shift exchange offer
	In order to trade for a shift that I rather want
	As an agent 
	I want to announce my current shift to trade with others

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
	And I add new shift exchange offer for current day
	Then I should see add shift exchange offer form with
	| Field          | Value      |
	| Offer end date | 2022-08-19 |
	| Start time     | 8:00       |
	| End time       | 17:00      |

	
