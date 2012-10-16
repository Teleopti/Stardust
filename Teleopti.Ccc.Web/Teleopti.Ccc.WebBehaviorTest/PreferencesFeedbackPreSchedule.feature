Feature: Preference feedback for pre-scheduled meetings and personal shifts
	In order to clearly see preferences that collide with the pre scheduled personal shift or meeting.
	As an agent
	I want good feedback about personal shifts, meetings and the the preferences in collision   

Background:
	Given I have a role named 'Agent'
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2012-10-14         |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-10-01 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-10-01 |


Scenario: See indication of a pre-scheduled meeting
	Given I have a pre-scheduled meeting with
	| Field     | Value            |
	| StartTime | 2012-10-19 9:00  |
	| EndTime   | 2012-10-19 10:00 |
	| Subject   | Meeting subject  |
	| Location  | Meeting location |
	When I view preferences for date '2012-10-19'
	Then I should see that I have a pre-scheduled meeting on '2012-10-19'
	And I should have a tooltip with meeting details

Scenario: See indication of a pre-scheduled personal shift
	Given I have a pre-scheduled personal shift with
	| Field     | Value            |
	| Field     | Value            |
	| StartTime | 2012-10-19 15:00 |
	| EndTime   | 2012-10-19 17:00 |
	| Activity  | Administration   |
	When I view preferences for date '2012-10-19'
	Then I should see that I have a pre-scheduled personal shift on '2012-10-19'
	And I should have a tooltip with personal shift details

Scenario: Feedback from pre-scheduled meeting
	Given I have a preference with
	| Field           | Value            |
	| Date            | 2012-10-19       |
	| EndTime minimum | 2012-10-19 16:00 |
	And I have a pre-scheduled meeting with
	| Field     | Value            |
	| StartTime | 2012-10-19 15:00 |
	| EndTime   | 2012-10-19 17:00 |
	| Subject   | Meeting subject  |
	| Location  | Meeting location |
	When I view preferences for date '2012-10-19'
	Then I should see preference feedback with
	| Field           | Value      |
	| Date            | 2012-10-19 |
	| Endtime minimum | 17:00      |

Scenario: Feedback from pre-scheduled personal shift
	Given I have a preference with
	| Field           | Value            |
	| Date            | 2012-10-19       |
	| EndTime minimum | 2012-10-19 16:00 |
	And I have a pre-scheduled personal shift with
	| Field     | Value            |
	| StartTime | 2012-10-19 15:00 |
	| EndTime   | 2012-10-19 17:00 |
	| Activity  | Administration   |
	When I view preferences for date '2012-10-19'
	Then I should see preference feedback with
	| Field           | Value      |
	| Date            | 2012-10-19 |
	| Endtime minimum | 17:00      |


