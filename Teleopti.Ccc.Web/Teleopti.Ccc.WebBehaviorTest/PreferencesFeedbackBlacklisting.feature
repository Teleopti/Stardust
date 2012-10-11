Feature: Preferences feedback blacklisting
	In order to not be assigned extreme shifts without requesting it
	As an agent
	I want feedback to only give me extreme shifts when I have a preference for it
	
Background:
	Given I have a role named 'Agent'
    And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2012-10-31         |
	And I have a schedule period with 
	| Field                | Value      |
	| Start date           | 2012-10-01 |
	| Type                 | Week       |
	| Length               | 1          |
	And there is an activity named 'Phone'
	And there is a shift category named 'Day'
	And there is a rule set with
	| Field          | Value       |
	| Name           | Normal      |
	| Test           | 8:00-8:00   |
	| Activity       | Phone       |
	| Shift category | Day         |
	| Start boundry  | 8:00-8:00   |
	| End boundry    | 17:00-17:00 |
	And there is a rule set with
	| Field          | Value       |
	| Name           | Blacklisted |
	| Activity       | Phone       |
	| Shift category | Day         |
	| Start boundry  | 8:00        |
	| End boundry    | 20:00       |
	| Blacklisted    | true        |
	And there is a rule set bag with
	| Field | Value               |
	| Name  | Bag                 |
	| Sets  | Normal, Blacklisted |
	And I have a person period with 
	| Field        | Value      |
	| Start date   | 2012-10-01 |
	| Rule set bag | Bag        |

Scenario: Feedback from blacklisted shift with preference
	Given I have a preference with
	| Field             | Value      |
	| Date              | 2012-10-10 |
	| Work time minimum | 12:00      |
	When I view preferences for date '2012-10-10'
	Then I should see preference feedback with
	| Field                 | Value       |
	| Date                  | 2012-10-10  |
	| Contract time boundry | 12:00-12:00 |

Scenario: Feedback from blacklisted shift with availability
	Given there is an availability rotation with
	| Field             | Value        |
	| Name              | Availability |
	| Days              | 1            |
	| Work time minimum | 12:00        |
	And I have an availability with
	| Field      | Value        |
	| Start date | 2012-10-01   |
	| Rotation   | Availability |
	When I view preferences for date '2012-10-10'
	Then I should see preference feedback with
	| Field                 | Value       |
	| Date                  | 2012-10-10  |
	| Contract time boundry | 12:00-12:00 |

Scenario: No feedback from blacklisted shift without preference
	When I view preferences for date '2012-10-10'
	Then I should see preference feedback with
	| Field                 | Value      |
	| Date                  | 2012-10-10 |
	| Contract time boundry | 9:00-9:00  |
