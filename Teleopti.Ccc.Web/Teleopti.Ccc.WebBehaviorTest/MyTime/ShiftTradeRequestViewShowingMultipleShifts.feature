Feature: ShiftTradeRequestViewShowingMultipleShifts
	As an agent who intends to trade shifts with a colleague,
	I want to view more than one shift on one page.

Background:
	Given there is a role with
	| Field | Value                 |
	| Name  | Full access to mytime |
	And there is a workflow control set with
	| Field                            | Value                                     |
	| Name                             | Trade from tomorrow until 30 days forward |
	| Schedule published to date       | 2040-06-24                                |
	| Shift Trade sliding period start | 1                                         |
	| Shift Trade sliding period end   | 30                                        |
	And there is a site named 'The site'
	And there is a team named 'The team' on site 'The site'
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | The team   |
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Team       | The team   |
	And there are shift categories
	| Name  |
	| Day   |
	| Night |

Scenario: See the shift trade view
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And OtherAgent have the workflow control set 'Trade from tomorrow until 30 days forward'
	And the time is '2030-01-01'
	And I am american
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
	When I view Add Shift Trade Request
	And I choose 'OtherAgent' to make a shift trade
	Then I should see a list for schedule for both me and OtherAgent start from date '1/2/2030'
