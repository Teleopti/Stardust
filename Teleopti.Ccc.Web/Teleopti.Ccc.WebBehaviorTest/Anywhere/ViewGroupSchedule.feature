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
		| Field                | Value                |
		| Shift bag            | A shift bag          |
		| Skill                | A skill              |
		| Team                 | Team green           |
		| Start date           | 2013-10-10           |
		| Contract             | A contract           |
		| Contract schedule    | A contract schedule  |
		| Part time percentage | Part time percentage |
	And 'John King' has a person period with
         | Field                | Value                        |
         | Shift bag            | Another shift bag            |
         | Skill                | Another skill                |
         | Team                 | Team red                     |
         | Start date           | 2013-10-10                   |
         | Contract             | Another contract             |
         | Contract schedule    | Another contract schedule    |
         | Part time percentage | Another part time percentage |
	And 'John Smith' is on 'A group' of group page 'A group page'
	And 'John Smith' is on 'Another group' of group page 'A group page'
	And 'John Smith' is on 'Some other group' of group page 'Another group page'
	And I have a role with
		| Field              | Value                |
		| Access to team     | Team green, Team red |
		| Access to Anywhere | true                 |
	And I am american

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
		| Shift Bag/A shift bag                     |
		| Skill/A skill                             |
		| A group page/A group                      |
		| A group page/Another group                |
		| Another group page/Some other group       |

Scenario: View group schedule
	Given 'John Smith' have a shift with
		| Field          | Value            |
		| Shift category | Day              |
		| Activity       | Phone            |
		| Start time     | 2013-10-10 09:00 |
		| End time       | 2013-10-10 16:00 |
	And 'Pierre Baldi' has a person period with
		| Field      | Value            |
		| Team       | Team green       |
		| Start date | 2013-10-10       |
		| Contract   | Another contract |
	And 'Pierre Baldi' have a shift with
		| Field          | Value            |
		| Shift category | Day              |
		| Activity       | Phone            |
		| Start time     | 2013-10-10 09:00 |
		| End time       | 2013-10-10 16:00 |
	When I view schedules for '2013-10-10'
	And I select group 'Contract/A contract'
	Then I should see schedule for 'John Smith'
	And I should not see person 'Pierre Baldi'

Scenario: Order group pages like business heirarchy, contract, contract schedule, part time percentage, shiftbag, skill, group page names
	Given I viewing schedules for '2013-10-10'
	Then I should see group 'Business Hierarchy' before 'Contract'
	And I should see group 'Contract' before 'Contract Schedule'
	And I should see group 'Contract Schedule' before 'Part-Time Percentage'
	And I should see group 'Part-Time Percentage' before 'Shift Bag'
	And I should see group 'Shift Bag' before 'Skill'
	And I should see group 'Skill' before 'A group page'
	And I should see group 'A group page' before 'Another group page'

Scenario: Order groups in alphabetical order
	Given I viewing schedules for '2013-10-10'
	Then I should see option 'Team green' before 'Team red'
	And I should see option 'A contract' before 'Another contract'
	And I should see option 'A contract schedule' before 'Another contract schedule'
	And I should see option 'Another part time percentage' before 'Part time percentage'
	And I should see option 'A shift bag' before 'Another shift bag'
	And I should see option 'A skill' before 'Another skill'
	And I should see option 'A group' before 'Another group'

Scenario: Search groups
	Given I viewing schedules for '2013-10-10'
	When I search for group 'contract'
	Then I should see 'A contract'
	Then I should see 'Another contract'
	Then I should not see 'A skill'
	Then I should not see 'Another group'

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
	Then the group picker should have 'The site/Team green' selected	
