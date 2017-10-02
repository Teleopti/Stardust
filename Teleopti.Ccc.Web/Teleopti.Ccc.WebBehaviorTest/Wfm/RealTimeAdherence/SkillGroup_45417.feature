@RTA
Feature: SkillGroup
	In order to easily manage skill groups
	As a real time analyst
	I want to be able to manage skill groups

Background:
Background:
	Given there is a switch
	And I have a role with full access
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And there is a team named 'Red' on site 'Paris'
    And There is a skill to monitor called 'Skill A' with queue id '9' and queue name 'queue1' and activity 'activity1'
	And Pierre Baldi has a person period with
	| Field          | Value        |
	| Team           | Green        |
	| Start Date     | 2016-01-01   |
	And Ashley Andeen has a person period with
	| Field          | Value         |
	| Team           | Red           |
	| Start Date     | 2016-01-01    |
	And Pierre Baldi has a shift with
	| Field      | Value            |
	| Start time | 2016-08-18 12:00 |
	| End time   | 2016-08-18 19:00 |
	| Activity   | Phone            |
	And Ashley Andeen has a shift with
	| Field      | Value            |
	| Start time | 2016-08-18 12:00 |
	| End time   | 2016-08-18 19:00 |
	| Activity   | Phone            |
	And there is a rule with 
	| Field       | Value |
	| Activity    | Phone |
	| Phone state | Ready |
	| Is Alarm    | false |
	And there is a rule with 
	| Field       | Value |
	| Activity    | Phone |
	| Phone state | Pause |
	| Is Alarm    | true  |

@OnlyRunIfEnabled('WFM_Unified_Skill_Group_Management_45417')
Scenario: Create Skill Group
	Given I view real time adherence for all agents on entire Bu
		And I have a role with full access
		And I select to create a new Skill Group
		And I name the Skill Area 'my Area'
		And I select the skill 'Skill A'
	When I am done creating Skill Area 
	Then I select to monitor skill area 'my Area'
