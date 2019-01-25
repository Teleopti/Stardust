Feature: MobileMonth
	As an agent I need to see the month view on my mobile,

Background:
Given there is a role with
	| Field                        | Value                 |
	| Name                         | Full access to mytime |
	| AccessToOvertimeAvailability | true                  |

	And there is a shift category with
	| Field | Value |
	| Name  | Early |
	| Color | Green |
	And there is an absence with
	| Field       | Value    |
	| Name        | Vacation |
	| Color       | Red      |
	| Requestable | true     |
	And there is a workflow control set with
	| Field                        | Value              |
	| Name                         | Published schedule |
	| Schedule published to date   | 2059-02-01         |
	| ReportableAbsence            | Vacation           |
	| ShiftTradeSlidingPeriodStart | 1                  |
	| ShiftTradeSlidingPeriodEnd   | 99                 |
	| AvailableAbsence             | Vacation           |
	| StaffingCheck                | intraday           |
	And I have a schedule period with
	| Field      | Value      |
	| Start date | 2013-08-19 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with
	| Field      | Value      |
	| Start date | 2013-08-19 |
	And I have the role 'Full access to mytime'
	And I am englishspeaking swede
	And I have the workflow control set 'Published schedule'

@Mobile
Scenario: View mobile month from mobile day view
	Given I have a shift with
	| Field          | Value            |
	| StartTime      | 2017-04-21 09:00 |
	| EndTime        | 2017-04-21 18:00 |
	| Shift category | Early            |
	When I am viewing mobile view for date '2017-04-21'
	And when I see mobile day view
	And I click month view icon
	Then I should see mobile month view
	And I should see schedule summary with start time '09:00' and end time '18:00' on '2017-04-21' on mobile month