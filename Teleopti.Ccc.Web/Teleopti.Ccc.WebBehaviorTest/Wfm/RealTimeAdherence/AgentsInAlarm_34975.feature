Feature: Agents in alarm
	In order to easier find the agent to blame
	As a real time analyst
	I want to see who is currently not adhering to the schedule

Background:
	Given there is a switch

@ignore
Scenario: See agents in alarm
	Given there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2015-11-23   |
	And Ashley Andeen has a person period with
	 | Field          | Value         |
	 | Team           | Red           |
	 | Start Date     | 2015-11-23   |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2015-11-23 08:00 |
	| End time                 | 2015-11-23 17:00 |
	And Ashley Andeen has a shift with
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
	And the time is '2015-11-23 12:30:00'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And 'Ashley Andeen' sets his phone state to 'Ready'
# make "real time adherence for agents on..." and "real time adherence for ALL agents on..." steps, change old scenarios ?
# OR make "real time adherence for agents IN ALARM on..." and "real time adherence for agents on..." steps, change old scenarios ?
	When I view real time adherence for agents on team 'Red'
	And the time is '2015-11-23 12:35:00'
	Then I should see agent details for 'Pierre Baldi'
		| Name       |              |
		| Name       | Pierre Baldi |
# rename to "color". This is display color, not "alarm color" as it says in other scenarios
		| Color      | Red          |
		| Alarm Time | 0:04:00      |
	And I should not see agent 'Ashley Andeen'
