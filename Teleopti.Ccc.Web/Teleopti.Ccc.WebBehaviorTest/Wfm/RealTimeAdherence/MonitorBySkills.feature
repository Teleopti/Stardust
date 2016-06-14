Feature: Monitor agents by skills
	In order to easier find the agent to blame
	As a real time analyst
	I want to see what agents are doing by skills

Background:
	Given there is a switch
	And I have a role with full access
	And Pierre Baldi has a person period with
	| Field      | Value      |
	| Start Date | 2016-06-14 |
	| Skill      | Sales      |
	And Pierre Baldi has a shift with
	| Field      | Value            |
	| Activity   | Phone            |
	| Start time | 2016-06-14 08:00 |
	| End time   | 2016-06-14 17:00 |
	And there is a rule with 
	| Field       | Value |
	| Activity    | Phone |
	| Phone state | Ready |
	
@OnlyRunIfEnabled('RTA_MonitorBySkills_39081')
Scenario: Monitor agents by skills
	Given the time is '2016-06-14 08:00:00'
    And 'Pierre Baldi' sets his phone state to 'Ready'
	When I view real time adherence for skill 'Sales'
	Then I should see agent status
	| Field         | Value        |
	| Name          | Pierre Baldi |
