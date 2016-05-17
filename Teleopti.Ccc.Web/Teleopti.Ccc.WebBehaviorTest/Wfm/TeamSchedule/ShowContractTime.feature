@OnlyRunIfEnabled('WfmTeamSchedule_ShowContractTime_38509')
Feature: ShowContractTime
	As a team leader
	I want to see agents' contract times

Background:
	Given I am american
	And there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And I have a role with
	| Field                         | Value          |
	| Name                          | Wfm Team Green |
	| Access to everyone            | True           |
	| Access to Wfm MyTeam Schedule | true           |
	| Add Activity                  | true           |
	| Remove Activity               | true           |
	| Move Activity                 | true           |
	| See Contract Time             | true           |
	And there is a shift category named 'Day'
	And there are activities
	| Name     | Color    |
	| Phone    | Green    |
	| Lunch    | Yellow   |
	| Sales    | Red      |
	| Training | Training |
	And there is a contract named 'A contract'
	And there is a contract schedule named 'A contract schedule'
	And there is a part time percentage named 'Part time percentage'
	And there is a rule set with
		| Field          | Value       |
		| Name           | A rule set  |
		| Activity       | Phone       |
		| Shift category | Day         |	
	And there is a shift bag named 'A shift bag' with rule set 'A rule set'
	And there is a skill named 'A skill' with activity 'Phone'
	And 'John Smith' has a workflow control set publishing schedules until '2016-12-01'
	And 'John Smith' has a person period with
		| Field                | Value                |
		| Shift bag            | A shift bag          |
		| Skill                | A skill              |
		| Team                 | Team green           |
		| Start date           | 2016-01-01           |
		| Contract             | A contract           |
		| Contract schedule    | A contract schedule  |
		| Part time percentage | Part time percentage |

Scenario: Should be able to see contract time
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-01-01'
	And I selected agent 'John Smith'
	And I open 'AddActivity' panel
	And I set new activity as
	| Field       | Value |
	| Activity    | Phone |
	| Start time  | 12:00 |
	| End time    | 13:00 |
	| Is next day | false |
	Then I should be able to apply my new activity
	When I apply my new activity
	Then I should see contract time of '1:00'
