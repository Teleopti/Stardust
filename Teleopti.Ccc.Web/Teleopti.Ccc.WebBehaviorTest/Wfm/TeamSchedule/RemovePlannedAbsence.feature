@WFM
@OnlyRunIfEnabled('WfmTeamSchedule_RemoveAbsence_36705')
Feature: Remove Planned Absence
	As a team leader
	I need to remove or shorten absences for multiple reasons.
	I want an easy way to find the agents
	I need to change and make the changes easily. 

Background: 
	Given I am american
	And there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And there is a contract named 'A contract'
	And there is a contract schedule named 'A contract schedule'
	And there is a part time percentage named 'Part time percentage'
	And there is an activity named 'Phone'
	And there is a shift category named 'Day'
	And there is a rule set with
		| Field          | Value       |
		| Name           | A rule set  |
		| Activity       | Phone       |
		| Shift category | Day         |
	And there are absences
		| Name     | Color    |
		| Name     | Vacation |
		| Vacation | Pink     |
		| Illness  | Blue     |
	And there is a shift bag named 'A shift bag' with rule set 'A rule set'
	And there is a skill named 'A skill' with activity 'Phone'
	And I have a person period with
	| Field      | Value      |
	| Start date | 2016-01-01 |
	| Team       | Team green |
	And I have a role with
		| Field                         | Value      |
		| Access to team                | Team green |
		| Access to Wfm MyTeam Schedule | true       |
	And 'John Smith' has a workflow control set publishing schedules until '2016-12-01'
	And 'Bill Gates' has a workflow control set publishing schedules until '2016-12-01'
	And 'Candy Mamer' has a workflow control set publishing schedules until '2016-12-01'
	And 'John Smith' has a person period with
		| Field                | Value                |
		| Shift bag            | A shift bag          |
		| Skill                | A skill              |
		| Team                 | Team green           |
		| Start date           | 2016-01-01           |
		| Contract             | A contract           |
		| Contract schedule    | A contract schedule  |
		| Part time percentage | Part time percentage |
	And 'Bill Gates' has a person period with
		| Field                | Value                |
		| Shift bag            | A shift bag          |
		| Skill                | A skill              |
		| Team                 | Team green           |
		| Start date           | 2016-01-01           |
		| Contract             | A contract           |
		| Contract schedule    | A contract schedule  |
		| Part time percentage | Part time percentage |
	And 'Candy Mamer' has a person period with
		| Field                | Value                |
		| Shift bag            | A shift bag          |
		| Skill                | A skill              |
		| Team                 | Team green           |
		| Start date           | 2016-01-01           |
		| Contract             | A contract           |
		| Contract schedule    | A contract schedule  |
		| Part time percentage | Part time percentage |
	And 'John Smith' has a shift with
		| Field          | Value            |
		| Shift category | Day              |
		| Activity       | Phone            |
		| Start time     | 2016-10-10 09:00 |
		| End time       | 2016-10-10 16:00 |
	And 'Bill Gates' has a shift with
		| Field          | Value            |
		| Shift category | Day              |
		| Activity       | Phone            |
		| Start time     | 2016-10-10 09:00 |
		| End time       | 2016-10-10 16:00 |

Scenario: Could delete absences for an agent
	Given 'John Smith' has an absence with
		| Field      | Value            |
		| Name       | Vacation         |
		| Start time | 2016-10-10 10:00 |
		| End time   | 2016-10-10 11:00 |
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I select a site "The site"
	And I searched schedule with keyword 'John'
	And I click button to search for schedules
	Then I should see schedule with absence 'Vacation' for 'John Smith' displayed
	When I selected agent 'John Smith'
	And I open menu in team schedule
	And I click menu item 'RemoveAbsence' in team schedule
	Then I should see a successful notice
	And I should see schedule with no absence for 'John Smith' displayed

Scenario: Absence deletion should only be enabled when when absence selected
	Given 'John Smith' has an absence with
		| Field      | Value            |
		| Name       | Vacation         |
		| Start time | 2016-10-10 10:00 |
		| End time   | 2016-10-10 11:00 |
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I select a site "The site"
	And I searched schedule with keyword 'John'
	And I click button to search for schedules
	And I should see schedule with absence 'Vacation' for 'John Smith' displayed
	And I open menu in team schedule
	Then I should see 'RemoveAbsence' menu item is disabled

Scenario: Full day absence should be able to delete
	Given 'John Smith' has a full day absence named 'Vacation' on '2016-10-10'	
	And 'Bill Gates' has a full day absence named 'Illness' on '2016-10-10'
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I select a site "The site"
	And I searched schedule with keyword 'Team green'
	And I click button to search for schedules
	And I selected agent 'John Smith'
	And I selected agent 'Bill Gates'
	And I open menu in team schedule
	And I click menu item 'RemoveAbsence' in team schedule
	Then I should see a successful notice

Scenario: Could delete absences for multiple agents
	Given 'John Smith' has an absence with
		| Field      | Value            |
		| Name       | Vacation         |
		| Start time | 2016-10-10 10:00 |
		| End time   | 2016-10-10 11:00 |
	And 'Bill Gates' has an absence with
		| Field      | Value            |
		| Name       | Illness          |
		| Start time | 2016-10-10 10:00 |
		| End time   | 2016-10-10 11:00 |
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I select a site "The site"
	And I searched schedule with keyword 'green'
	And I click button to search for schedules
	Then I should see schedule with absence 'Vacation' for 'John Smith' displayed
	When I selected agent 'John Smith'
	And I selected agent 'Bill Gates'
	And I open menu in team schedule
	And I click menu item 'RemoveAbsence' in team schedule
	Then I should see a successful notice

@ignore
Scenario: Could remove one intraday absence out of 2
	Given I am a team leader
	And I viewing schedule of my team members on date '2016-2-1'
	And 'Ashley Andeen' has intraday absence in date '2016-2-1 9:00-10:00'
	And 'Ashley Andeen' has intraday absence in date '2016-2-1 13:00-14:00'
	When I remove absence '2016-2-1 9:00-10:00' for 'Ashley Andeen'
	Then I should see absence '2016-2-1 13:00-14:00' for 'Ashley Andeen'

@ignore
Scenario: Could remove all intraday absences of one agent
	Given I am a team leader
	And I viewing schedule of my team members on date '2016-2-1'
	And 'Ashley Andeen' has intraday absence in date '2016-2-1 9:00-10:00'
	And 'Ashley Andeen' has intraday absence in date '2016-2-1 13:00-14:00'
	When I remove all absneces for 'Ashley Andeen'
	Then I should see there is no absence for 'Ashley Andeen'

@ignore
Scenario: Delete absence in top layer by default
	Given I am a team leader
	And I viewing schedule of my team members on date '2016-2-1'
	And 'Ashley Andeen' has full day absence 'Holiday' on date '2016-2-1'
	And 'Ashley Andeen' has full day absence 'Illness' on date '2016-2-1'
	When I remove absnece 'Illness' for 'Ashley Andeen' on date '2016-2-1'
	Then I should see absence 'Holiday' for 'Ashley Andeen' on date '2016-2-1'

@ignore
Scenario: Delete absence for today by default when agent has cross days absence
	Given I am a team leader
	And I viewing schedule of my team members on date '2016-2-2'
	And 'Ashley Andeen' has absence for date '2016-2-1 to 2016-2-3'
	When I remove absence for 'Ashley Andeen' on date '2016-2-2'
	Then I should not see absence for 'Ashley Andeen' on date '2016-2-2'
	And I should see absence for 'Ashley Andeen' on date '2016-2-1'
	And I should see absence for 'Ashley Andeen' on date '2016-2-3'

@ignore
Scenario: Can delete absence cross days
	Given I am a team leader
	And I viewing schedule of my team members on date '2016-2-1'
	And 'Ashley Andeen' has absence for date '2016-2-1 to 2016-2-3'
	When I remove whole absence for 'Ashley Andeen'
	Then I should not see any absence for 'Ashley Andeen' during date '2016-2-1 to 2016-2-3'

@ignore
Scenario: Need user confirm before remove absence
	Given I am a team leader
	And I viewing schedule of my team members on date '2016-2-1'
	And 'Ashley Andeen' has absence for date '2016-2-1'
	When I remove absence for 'Ashley Andeen' on date '2016-2-1'
	Then I should get a confirm message before my remove action

@ignore
Scenario: Can filter schedule with absences only
	Given I am a team leader
	And I viewing schedule of my team members on date '2016-2-1'
	And 'Ashley Andeen' has absence for date '2016-2-1'
	And 'John Smith' has no absence for date '2016-2-1'
	And 'Pierre Baldi' has absence from '2016-2-1 13:00-14:00'
	When I select only to see absence
	Then I should see 'Ashley Andeen' and 'Pierre Baldi' in schedule board
	And I should not see 'John Smith' in schedule board

@ignore
Scenario: Add absence and swap shifts should be disabled when schedule part selected
	Given I am a team leader
	And I viewing schedule of my team members on date '2016-2-1'
	And 'Ashley Andeen' has intraday absence for date '2016-2-1'
	When I click intraday absence part on 'Ashley Andeen' schedule
	And I open the action menu
	Then I should see 'Add Absence' be disabled 
	And I should see 'Swap Shifts' be disabled

@ignore
Scenario: Should refresh schedule after absence deleted
	Given I am a team leader
	When I viewing schedule of my team members
	And I selected absence of agent "Ashley Andeen"
	And I delete selected absences
	Then I will see 'Ashley Andeen' schedule without the absence.

@ignore
Scenario: Should notify the agent that the schedule has been changed after absence delete
	Given I am a team leader
	When I viewing schedule of my team members
	And Ashley Andeen is view her own schedule in MyTimeWeb
	And I selected absence of agent "Ashley Andeen"
	And I delete selected absences
	Then Ashley Andeen will receive a message that her schedule has been changed

@ignore
Scenario: Should notify other team leaders the schedule has been changed after absence delete

@ignore
Scenario: PA should be updated after absence has been deleted.

@ignore
Scenario: I should be informed about the success/failure of my command

@ignore
Scenario: I can remove absences from agents (on the same date) on different pages at the same time.
