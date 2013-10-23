Feature: View group schedule
	In order to contact agents with a specific skill
	or belonging to a specific team
	or that are part-timers
	or are students
	As a team leader
	I want to see the schedules grouped by these criteria

Background:
	Given there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And there is a team named 'Team red' on 'The site'
	And there is a contract named 'A contract'
	And there is a contract named 'Another contract'
	And there is a contract schedule named 'A contract schedule'
	And there is a contract schedule named 'Another contract schedule'
	And there is a part time percentage named 'Part time percentage'
	And there is a part time percentage named 'Another part time percentage'
	And there is an activity named 'Phone'
	And there is a shift category named 'Day'
	And there is a rule set with
		| Field          | Value       |
		| Name           | A rule set  |
		| Activity       | Phone       |
		| Shift category | Day         |
	And there is a shift bag named 'A shift bag' with rule set 'A rule set'
	And there is a shift bag named 'Another shift bag' with rule set 'A rule set'
	And there is a skill named 'A skill' with activity 'Phone'
	And there is a skill named 'Another skill' with activity 'Phone'
	And there is a group page with
		| Field | Value                  |
		| Name  | A group page           |
		| Group | A group, Another group |
	And there is a group page with
		| Field | Value              |
		| Name  | Another group page |
		| Group | Some other group   |
	And 'Pierre Baldi' has a workflow control set publishing schedules until '2013-12-01'
	And 'John Smith' has a workflow control set publishing schedules until '2013-12-01'
	And 'John Smith' has a person period with
		| Field      | Value       |
		| Shift bag  | A shift bag |
		| Skill      | A skill     |
		| Start date | 2013-10-10  |
	And 'John Smith' is on 'A group' of group page 'A group page'
	And 'John Smith' is on 'Another group' of group page 'A group page'
	And 'John Smith' is on 'Some other group' of group page 'Another group page'
	And 'Pierre Baldi' have the note 'Another note'
	And 'John Smith' have the note 'A note'
	And I have a role with
		| Field              | Value                |
		| Access to team     | Team green, Team red |
		| Access to Anywhere | true                 |
	And I am american

@ignore
Scenario: View group picker options
	Given I viewing schedules for '2013-10-10'
	Then I should be able to select groups
		| Group                                     |
		| The site/Team green                       |
		| The site/Team red                         |
		| Contract/A contract                       |
		| Contract/Another contract                 |
		| Contract Schedule/A contract schedule     |
		| Part-Time Percentage/Part time percentage |
		| Shift bag/A shift bag                     |
		| Note/A note                               |
		| Skill/A skill                             |
		| A group page/A group                      |
		| A group page/Another group                |
		| Another group page/Some other group       |

@ignore
Scenario: View group schedule
	And 'John Smith' has a shift on '2013-10-10'
	And 'John Smith' has a person period with
		| Field      | Value      |
		| Team       | Team green |
		| Start date | 2013-10-10 |
		| Contract   | A contract |
	And 'Pierre Baldi' has a shift on '2013-10-10'
	And 'Pierre Baldi' has a person period with
		| Field      | Value            |
		| Team       | Team green       |
		| Start date | 2013-10-10       |
		| Contract   | Another contract |
	Given I have a role with
		| Field              | Value                |
		| Access to team     | Team green, Team red |
		| Access to Anywhere | true                 |
	When I view schedules for '2013-10-10'
	And I select group 'Contract/A contract'
	Then I should see schedule for 'John Smith'
	Then I should see no schedule for 'Pierre Baldi'

@ignore
Scenario: Order group pages like business heirarchy, contract, contract schedule, part time percentage, notes, shiftbag, skill, group page names
	Given I viewing schedules for '2013-10-10'
	Then I should see 'Business Hierarchy' before 'Contract'
	And I should see 'Contract' before 'Contract Schedule'
	And I should see 'Contract Schedule' before 'Part-Time Percentage'
	And I should see 'Part-Time Percentage' before 'Note'
	And I should see 'Note' before 'Shift bag'
	And I should see 'Shift bag' before 'Skill'
	And I should see 'Skill' before 'A group page'
	And I should see 'A group page' before 'Another group page'

@ignore
Scenario: Order groups in alphabetical order
	Given I viewing schedules for '2013-10-10'
	Then I should see 'Team green' before 'Team red'
	And I should see 'A contract' before 'Another contract'
	And I should see 'A contract schedule' before 'Another contract schedule'
	And I should see 'Another Part-Time Percentage' before 'Part-Time Percentage'
	And I should see 'Another note' before 'A note'
	And I should see 'Another shift bag' before 'A shift bag'
	And I should see 'Another skill' before 'A skill'
	And I should see 'A group' before 'Another group'

@ignore
Scenario: Search groups
	Given I viewing schedules for '2013-10-10'
	When I search for group 'contract'
	Then I should see 'A contract'
	Then I should see 'Another contract'
	Then I should not see 'A note'
	Then I should not see 'Another note'

Scenario: Default to my team
	Given I have a person period with 
		| Field      | Value      |
		| Team       | Team red   |
		| Start date | 2013-10-10 |
	When I view schedules for '2013-10-10'
	Then the group picker should have 'The site/Team red' selected	

Scenario: Default to first option if I have no team
	Given I have no team
	When I view schedules for '2013-10-10'
	Then the group picker should have 'A group page/A group' selected	
