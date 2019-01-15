@RTA
Feature: Agents in alarm
	In order to easier find the agent to blame
	As a real time analyst
	I want to see who is currently not adhering to the schedule

Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Red        |
	 | Start Date | 2015-11-23 |
	And John King has a person period with
	 | Field      | Value      |
	 | Team       | Red        |
	 | Start Date | 2015-11-23 |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2015-11-23 08:00 |
	| End time                 | 2015-11-23 17:00 |
	And John King has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2015-11-23 08:00 |
	| End time                 | 2015-11-23 17:00 |
	And there is a rule with 
	| Field       | Value |
	| Adherence   | In    |
	| Activity    | Phone |
	| Phone state | Ready |
	| Is alarm    | False |
	And there is a rule with 
	| Field           | Value        |
	| Adherence       | Out          |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Is alarm        | True         |
	| Alarm threshold | 00:01:00     |

Scenario: See agents with the highest alarm time first
	Given the time is '2015-11-23 08:00:00'
	And I am viewing real time adherence for agents on team 'Red'
	And 'Pierre Baldi' sets his phone state to 'Ready'
	And 'John King' sets his phone state to 'Ready'
	When the time is '2015-11-23 08:10:00'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	When the time is '2015-11-23 08:11:00'
	And 'John King' sets his phone state to 'Pause'
	When the time is '2015-11-23 08:15:00'
	Then I should see agent 'Pierre Baldi' before 'John King'
