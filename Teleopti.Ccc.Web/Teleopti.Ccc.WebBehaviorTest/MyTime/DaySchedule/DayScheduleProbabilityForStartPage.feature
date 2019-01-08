Feature: Day Schedule Probability For Start Page
	I need to see probability for both absence and overtime for a specific day

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
	| Field                                 | Value              |
	| Name                                  | Published schedule |
	| Schedule published to date            | 2059-02-01         |
	| ReportableAbsence                     | Vacation           |
	| ShiftTradeSlidingPeriodStart          | 1                  |
	| ShiftTradeSlidingPeriodEnd            | 99                 |
	| AvailableAbsence                      | Vacation           |
	| StaffingCheck                         | intraday           |
	| AbsenceProbabilityEnabled             | True               |
	| OvertimeProbabilityEnabled            | True               |
	| OvertimeRequestOpenPeriodRollingStart | 0                  |
	| OvertimeRequestOpenPeriodRollingEnd   | 13                 |

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
	And There is a skill to monitor called 'Phone' with queue id '9' and queue name 'queue1' and activity 'activity1'
	And there is queue statistics for the skill 'Phone' up until '19:00'
	And there is forecast data for skill 'Phone' for date 'today'
	And there are scheduled agents for 'Phone' for date 'today'
	And 'I' have a person period with
		| Field                | Value                |
		| Skill                | Phone                |
		| Start date           | 2017-01-01           |
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 09:00 |
	| EndTime        | 18:00 |
	| Shift category | Early            |
	| Activity       | activity1        |

Scenario: Should see the absence probability
	When I am viewing mobile view for today
	And I click show probability toggle
	And I click show absence probability
	Then I should see the probability in schedule

Scenario: Should see the overtime probability
	When I am viewing mobile view for today
	And I click show probability toggle
	And I click show overtime probability
	Then I should see the probability in schedule
	
Scenario: Should hide staffing probability
	When I am viewing mobile view for today
	And I click show probability toggle
	And I click show overtime probability
	And I click show probability toggle
	And I click hide probability
	Then I should not see the probability in schedule
