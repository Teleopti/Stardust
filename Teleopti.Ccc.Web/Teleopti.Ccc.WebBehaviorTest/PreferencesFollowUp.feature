@WatiN
Feature: Preferences follow up
	In order to see my schedule and my preferences side by side
	As an agent
	I can easily see my preferences on a scheduled day

Background:
	Given I have a role with
	| Field                          | Value                 |
	| Name                           | Full access to mytime |
	| Access to extended preferences | false                 |
	And there are shift categories
	| Name  |
	| Late  |
	| Night |
	And there is a dayoff with
	| Field | Value  |
	| Name  | Dayoff |
	And there is an absence with
	| Field | Value   |
	| Name  | Illness |
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2012-10-07         |
	| Available shift category   | Late               |
	| Available dayoff           | Dayoff             |
	| Available absence          | Illness            |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-10-01 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-10-01 |


Scenario: See preference on scheduled day
	Given I have a preference with
	| Field      | Value      |
	| Date       | 2012-10-02 |
	| Preference | Late       |
	And I have a shift with
	| Field          | Value            |
	| Shift category | Late             |
	| StartTime      | 2012-10-02 10:00 |
	| EndTime        | 2012-10-02 20:00 |
	When I view preferences for date '2012-10-02'
	Then I should see the day cell with
	| Field          | Value      |
	| Date           | 2012-10-02 |
	| Shift category | Late       |
	| Preference     | Late       |

Scenario: See extended indication on preference on scheduled day
	Given I have an extended preference with
	| Field            | Value      |
	| Date             | 2012-10-02 |
	| End time maximum | 20:30      |
	And I have a shift with
	| Field          | Value            |
	| Shift category | Late             |
	| StartTime      | 2012-10-02 10:00 |
	| EndTime        | 2012-10-02 20:00 |
	When I view preferences for date '2012-10-02'
	Then I should see the day cell with
	| Field               | Value      |
	| Date                | 2012-10-02 |
	| Shift category      | Late       |
	| Preference          | Extended   |
	| Extended Indication | true       |

Scenario: Display extended preference panel for preference on scheduled day
	Given I have an extended preference with
	| Field            | Value      |
	| Date             | 2012-10-02 |
	| End time maximum | 20:30      |
	And I have a shift with
	| Field          | Value            |
	| Shift category | Late             |
	| StartTime      | 2012-10-02 10:00 |
	| EndTime        | 2012-10-02 20:00 |
	When I view preferences for date '2012-10-02'
	And I click the extended preference indication on '2012-10-02'
	Then I should see extended preference with
	| Field            | Value      |
	| Date             | 2012-10-02 |
	| End time maximum | 20:30      |

Scenario: Display must have for preference on scheduled day
	Given I have a preference with
	| Field      | Value      |
	| Date       | 2012-10-02 |
	| Preference | Late       |
	| Must have  | true       |
	And I have a shift with
	| Field          | Value            |
	| Shift category | Late             |
	| StartTime      | 2012-10-02 10:00 |
	| EndTime        | 2012-10-02 20:00 |
	When I view preferences for date '2012-10-02'
	Then I should see the day cell with
	| Field     | Value      |
	| Date      | 2012-10-02 |
	| Must have | true       |
