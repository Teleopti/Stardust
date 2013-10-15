@ignore
Feature: View group schedule
	In order to contact agents with a specific skill
	or beloning to specific team
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
	And there is a part time percentage named 'Part time percentage'
	And there is an activity named 'Phone'
	And there is a shift category named 'Day'
	And there is a rule set with
		| Field          | Value       |
		| Name           | A rule set  |
		| Activity       | Phone       |
		| Shift category | Day         |
		| Start boundry  | 8:00-8:00   |
		| End boundry    | 17:00-17:00 |
	And there is a shift bag with
		| Field    | Value       |
		| Name     | A shift bag |
		| Rule set | A rule set  |
	And there is a skill with
		| Field    | Value   |
		| Name     | A skill |
		| Activity | Phone   |
	And 'Pierre Baldi' has a workflow control set publishing schedules until '2013-12-01'
	And 'John Smith' has a workflow control set publishing schedules until '2013-12-01'
	# Might be required for the shift bag and skill to display
	#And 'John Smith' has a person period with
	#	| Field     | Value       |
	#	| Shift bag | A shift bag |
	#	| Skill     | A skill     |
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
		| Shift bag/A shift bag                     |
		| Skill/A skill                             |

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

Scenario: Order groupings like business heirarchy, contract, contract schedule, part time percentage, shiftbag, skill
	Given I viewing schedules for '2013-10-10'
	Then I should see 'Business Hierarchy' before 'Contract'
	And I should see 'Contract' before 'Contract Schedule'
	And I should see 'Contract Schedule' before 'Part-Time Percentage'
	And I should see 'Part-Time Percentage' before 'Shift Bag'
	And I should see 'Shift bag' before 'Skill'

Scenario: Order groups in alphabetical order
	Given I viewing schedules for '2013-10-10'
	Then I should see 'Team green' before 'Team red'
	And I should see 'A contract' before 'Another contract'

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
