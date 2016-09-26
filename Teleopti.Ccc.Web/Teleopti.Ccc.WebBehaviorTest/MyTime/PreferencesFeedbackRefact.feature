@MyTimePreferences
Feature: Preferences feedback - Scenario refact
	In order to know at which times I might work
	As an agent
	I want feedback for my preferences
	
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
	And there is a dayoff named 'Day off'
	And there is an absence with
	| Field            | Value    |
	| Name             | Vacation |
	| In contract time | true     |
	And there is a rule set with
	| Field          | Value       |
	| Name           | Rule        |
	| Activity       | Phone       |
	| Shift category | Day         |
	| Start boundry  | 8:00-9:00   |
	| End boundry    | 16:00-17:00 |
	And there is a shift bag with
	| Field    | Value |
	| Name     | Bag   |
	| Rule set | Rule  |
	And there is a contract schedule with
	| Field              | Value     |
	| Name               | Full week |
	| Monday work day    | true      |
	| Tuesday work day   | true      |
	| Wednesday work day | true      |
	| Thursday work day  | true      |
	| Friday work day    | true      |
	| Saturday work day  | true      |
	| Sunday work day    | true      |
	And there is a contract with
	| Field                     | Value         |
	| Name                      | 8 hours a day |
	| Average work time per day | 8:00          |

Scenario: Feedback for a day without restrictions
	Given I have a person period with 
	| Field      | Value      |
	| Start date | 2012-10-01 |
	| Shift bag  | Bag        |
	When I view preferences for date '2012-10-13'
	Then I should see preference feedback with
	| Field                 | Value       |
	| Date                  | 2012-10-13  |
	| Start time boundry    | 08:00-09:00   |
	| End time boundry      | 16:00-17:00 |
	| Contract time boundry | 7:00-9:00   |

Scenario: Feedback for a day with day off preference
	Given I have a person period with 
	| Field      | Value      |
	| Start date | 2012-10-01 |
	| Shift bag  | Bag        |
	And I have a preference with
	| Field  | Value      |
	| Date   | 2012-10-13 |
	| Dayoff | Day off    |
	When I view preferences for date '2012-10-13'
	Then I should see no preference feedback on '2012-10-13'

Scenario: Feedback for a day with absence preference
	Given I have a person period with 
	| Field             | Value         |
	| Start date        | 2012-10-01    |
	| Shift bag         | Bag           |
	| Contract schedule | Full week     |
	| Contract          | 8 hours a day |
	And I have a preference with
	| Field   | Value      |
	| Date    | 2012-10-13 |
	| Absence | Vacation   |
	When I view preferences for date '2012-10-13'
	Then I should see preference feedback with
	| Field                 | Value       |
	| Date                  | 2012-10-13  |
	| Contract time boundry | 8:00-8:00   |
