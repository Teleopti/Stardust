Feature: Monitor agents by skill
	In order to easier find the agent to blame
	As a real time analyst
	I want to see what agents are doing by skills

Background:
	Given there is a switch
	And I have a role with full access	
	And there is an activity named 'Phone'
	And there is a skill named 'Sales' with activity 'Phone'
	And there is a skill named 'Email' with activity 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Red        |
	 | Start Date | 2016-06-14 |
	 | Skill      | Sales      |
	And John King has a person period with
	 | Field      | Value      |
	 | Team       | Red        |
	 | Start Date | 2016-06-14 |
	 | Skill      | Email      |
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Red        |
	 | Start Date | 2016-06-14 |
	 | Skill      | Sales      |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2016-06-14 08:00 |
	| End time                 | 2016-06-14 17:00 |
	And John King has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2016-06-14 08:00 |
	| End time                 | 2016-06-14 17:00 |
	And Ashley Andeen has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2016-06-14 08:00 |
	| End time                 | 2016-06-14 17:00 |
	And there is a rule with 
	| Field       | Value        |
	| Activity    | Phone        |
	| Phone state | LoggedOut    |
	| Name        | Not adhering |
	| Is alarm    | true         |
	And there is a rule with 
	| Field       | Value    |
	| Activity    | Phone    |
	| Phone state | Ready    |
	| Name        | Adhering |
	| Is alarm    | false    |

@OnlyRunIfEnabled('RTA_MonitorBySkills_39081')
Scenario: Monitor agents by skill
	Given the time is '2016-06-14 08:00:00'
	And 'Pierre Baldi' sets his phone state to 'LoggedOut'
	And 'John King' sets his phone state to 'LoggedOut'
	And 'Ashley Andeen' sets his phone state to 'LoggedOut'
	When I view Real time adherence sites
	And I click 'select skill'
	And I select skill 'Sales'
	Then I should see agent status for 'Pierre Baldi'
	And I should see agent status for 'Ashley Andeen'
	And I should not see agent 'John King'
	When the time is '2016-06-14 08:10:00'
	And 'Pierre Baldi' sets his phone state to 'Ready'
	Then I should see agent status for 'Ashley Andeen'
	And I should not see agent 'Pierre Baldi'