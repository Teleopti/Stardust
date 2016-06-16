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
		 And Ashley Andeen has a person period with
         | Field      | Value      |
         | Start Date | 2016-06-14 |
         | Skill      | Email      |
       
@OnlyRunIfEnabled('RTA_MonitorBySkills_39081')
Scenario: Monitor agents by skills
		Given the time is '2016-06-14 08:00:00'
		When I view real time adherence for agents with skills 'Sales'
		Then I should see agent 'Pierre Baldi'
		And I should not se agent 'Ashley Andeen'