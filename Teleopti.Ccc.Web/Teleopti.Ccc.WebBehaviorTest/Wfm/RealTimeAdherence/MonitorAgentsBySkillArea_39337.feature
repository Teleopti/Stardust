@RTA
Feature: Monitor agents by skill area
	In order to monitor and drive adherence
	As an intraday analyst
	I want to see what agents are doing based on skill area

Background:
	Given there is a switch
	And I have a role with full access	
	And there is an activity named 'Phone'
	And there is a skill named 'Sales' with activity 'Phone'
	And there is a skill named 'Support' with activity 'Phone'
	And there is a skill named 'Invoice' with activity 'Phone'
	And there is a Skill Area called 'Phone skills' that monitors skills 'Sales, Support'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Red        |
	 | Start Date | 2016-06-14 |
	 | Skill      | Support    |
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Red        |
	 | Start Date | 2016-06-14 |
	 | Skill      | Sales      |
	 And John King has a person period with
	 | Field      | Value      |
	 | Team       | Red        |
	 | Start Date | 2016-06-14 |
	 | Skill      | Invoice    |
	And Ashley Andeen has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2016-06-14 08:00 |
	| End time                 | 2016-06-14 17:00 |
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
	And there is a rule with 
	| Field       | Value    |
	| Name        | Adhering |
	| Activity    | Phone    |
	| Phone state | Ready    |
	| Is alarm    | false    |
	And there is a rule with 
	| Field       | Value        |
	| Name        | Not adhering |
	| Activity    | Phone        |
	| Phone state | LoggedOut    |
	| Is alarm    | true         |

Scenario: Monitor agents by skill area
	Given the time is '2016-06-14 08:00:00'
	And 'Ashley Andeen' sets his phone state to 'LoggedOut'
	And 'Pierre Baldi' sets his phone state to 'LoggedOut'
	And 'John King' sets his phone state to 'LoggedOut'
	And  I am viewing real time adherence for skill area 'Phone skills' on sites 'Paris'
	Then I should see agent 'Pierre Baldi' with state 'LoggedOut'
	And I should see agent 'Ashley Andeen' with state 'LoggedOut'
	And I should not see agent 'John King'
	Given the time is '2016-06-14 08:05:00'
	And 'Pierre Baldi' sets his phone state to 'Ready'
	Given the time is '2016-06-14 08:10:00'
	Then I should see agent 'Ashley Andeen' with state 'LoggedOut'
	And I should not see agent 'Pierre Baldi'