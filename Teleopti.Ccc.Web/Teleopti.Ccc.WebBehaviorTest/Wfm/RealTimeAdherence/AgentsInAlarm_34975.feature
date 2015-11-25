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
# lets rename to "rule" ?
	And there is a rule with 
	| Field       | Value    |
	| Activity    | Phone    |
	| Phone state | Ready    |
	| Name        | Adhering |
# lets rename "alarm color" to "color"
	| Color       | Green    |
	And there is a rule with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
# lets rename "alarm color" to "color"
	| Color           | Orange       |
	| Name            | Not adhering |
# new "is alarm"
	| Alarm           | True         |
	| Alarm threshold | 00:01:00     |
# the new "alarm color"
	| Alarm color     | Red          |

@ignore
Scenario: See agents with the highest alarm time first
	Given the time is '2015-11-23 08:00:00'
# make previous scenarios "I view real time adherence for all agents"
# that toggle the screen toggle off if the feature toggle is on ;)
	And I am viewing real time adherence for agents on team 'Red'
	And 'Pierre Baldi' sets his phone state to 'Ready'
	And 'John King' sets his phone state to 'Ready'
	When the time is '2015-11-23 09:00:00'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	When the time is '2015-11-23 09:01:00'
	And 'John King' sets his phone state to 'Pause'
	When the time is '2015-11-23 09:05:00'
	Then I should see agent status
		| Name       |              |
		| Name       | Pierre Baldi |
	# rename to "color". This is display color, not "alarm color" as it says in other scenarios
		| Color      | Red          |
		| Alarm Time | 0:04:00      |
	Then I should see agent status
		| Name       |           |
		| Name       | John King |
		| Color      | Red       |
		| Alarm Time | 0:03:00   |
	And I should see agent 'Pierre Baldi' before 'John King'
