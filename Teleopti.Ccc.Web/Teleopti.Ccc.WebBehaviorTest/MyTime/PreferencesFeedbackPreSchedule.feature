@MyTimePreferences
Feature: Preferences feedback pre scheduled
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
	And there is a shift category named 'Day'
	And there is an activity with
	| Field        | Value |
	| Name         | Phone |
	| AllowMeeting | true  |
	And there is an activity with
	| Field        | Value          |
	| Name         | Administration |
	| AllowMeeting | true           |
	And there is a rule set with
	| Field          | Value       |
	| Name           | Common      |
	| Activity       | Phone       |
	| Shift category | Day         |
	| Start boundry  | 8:00-9:00   |
	| End boundry    | 17:00-18:00 |
	And there is a shift bag with
	| Field    | Value  |
	| Name     | Common |
	| Rule set | Common |

Scenario: See indication of a pre-scheduled meeting
	Given I have a person period that starts on '2012-10-01'
	And I have a pre-scheduled meeting with
	| Field     | Value            |
	| StartTime | 2012-10-19 9:00  |
	| EndTime   | 2012-10-19 10:00 |
	| Subject   | Meeting subject  |
	| Location  | Meeting location |
	When I view preferences for date '2012-10-19'
	Then I should see that I have a pre-scheduled meeting on '2012-10-19'

Scenario: Tooltip of a pre-scheduled meeting
	Given I have a person period that starts on '2012-10-01'
	And I have a pre-scheduled meeting with
	| Field     | Value            |
	| StartTime | 2012-10-19 9:00  |
	| EndTime   | 2012-10-19 10:00 |
	| Subject   | Meeting subject  |
	| Location  | Meeting location |
	When I view preferences for date '2012-10-19'
	Then I should have a tooltip for meeting details with
	| Field     | Value            |
	| StartTime | 2012-10-19 9:00  |
	| EndTime   | 2012-10-19 10:00 |
	| Subject   | Meeting subject  |

Scenario: See indication of a pre-scheduled personal shift
	Given I have a person period that starts on '2012-10-01'
	And I have a pre-scheduled personal shift with
	| Field     | Value            |
	| StartTime | 2012-10-19 9:00  |
	| EndTime   | 2012-10-19 10:00 |
	| Activity  | Administration   |
	When I view preferences for date '2012-10-19'
	Then I should see that I have a pre-scheduled personal shift on '2012-10-19'

Scenario: Tooltip of a pre-scheduled personal shift
	Given I have a person period that starts on '2012-10-01'
	And I have a pre-scheduled personal shift with
	| Field     | Value            |
	| StartTime | 2012-10-19 9:00  |
	| EndTime   | 2012-10-19 10:00 |
	| Activity  | Administration   |
	When I view preferences for date '2012-10-19'
	Then I should have a tooltip for personal shift details with
	| Field     | Value            |
	| StartTime | 2012-10-19 9:00  |
	| EndTime   | 2012-10-19 10:00 |
	| Activity  | Administration   |

Scenario: Feedback from pre-scheduled meeting
	Given I have a person period that starts on '2012-10-01'
	And I have a pre-scheduled meeting with
	| Field     | Value            |
	| StartTime | 2012-10-19 17:00 |
	| EndTime   | 2012-10-19 18:00 |
	| Subject   | Meeting subject  |
	| Location  | Meeting location |
	When I view preferences for date '2012-10-19'
	Then I should see preference feedback with
	| Field           | Value       |
	| Date            | 2012-10-19  |
	| EndTime boundry | 18:00-18:00 |

Scenario: Feedback from a pre-scheduled meeting outside of possible shifts
	Given I have a person period that starts on '2012-10-01'
	And I have a pre-scheduled meeting with
	| Field     | Value            |
	| StartTime | 2012-10-19 18:00 |
	| EndTime   | 2012-10-19 19:00 |
	| Subject   | Meeting subject  |
	| Location  | Meeting location |
	When I view preferences for date '2012-10-19'
	Then I should see preference feedback with
	| Field          | Value              |
	| Date           | 2012-10-19         |
	| Feedback error | No available shift |

Scenario: Feedback from pre-scheduled personal shift
	Given I have a person period that starts on '2012-10-01'
	And I have a pre-scheduled personal shift with
	| Field     | Value            |
	| StartTime | 2012-10-19 17:00 |
	| EndTime   | 2012-10-19 18:00 |
	| Activity  | Administration   |
	When I view preferences for date '2012-10-19'
	Then I should see preference feedback with
	| Field           | Value       |
	| Date            | 2012-10-19  |
	| EndTime boundry | 18:00-18:00 |
