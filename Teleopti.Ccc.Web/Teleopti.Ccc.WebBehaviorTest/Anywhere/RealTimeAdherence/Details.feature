Feature: Real time adherence details
	In order to easier check realtime/historical adherence details for agents
	As a real time analyst
	I want to see whom and when was adhered and not adhered to the schedule

@OnlyRunIfEnabled('RTA_AdherenceDetails_34267')
Scenario: Should be able to see adherence details for one agent within working hour
	Given there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to team                         | Red         |
	 | Access to real time adherence overview | True        |
	And there is a datasouce with id 6
	And there is an external logon named 'Pierre Baldi' with datasource 6
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2014-10-06   |
	 | External Logon | Pierre Baldi |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Start time               | 2014-10-06 08:00 |
	| End time                 | 2014-10-06 10:00 |
	| Activity                 | Phone            |
	And there is an alarm with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Alarm Color     | Red          |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	And there is an alarm with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Alarm Color     | Green    |
	| Staffing effect | 0        |
	When the time is '2014-10-06 08:00:00'
	And 'Pierre Baldi' sets his phone state to 'Ready' on datasource 6
	And the time is '2014-10-06 08:30:00'
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And the time is '2014-10-06 10:00:00'
	And I view manage adherence view for agent 'Pierre Baldi'
	Then I should see 'Phone' with adherence of 25%
	And I should see daily adherence for 'Pierre Baldi' is 25%

@OnlyRunIfEnabled('RTA_AdherenceDetails_34267')
Scenario: Should be able to see adherence details for one agent outside of working hour
	Given there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to team                         | Red         |
	 | Access to real time adherence overview | True        |
	And there is a datasouce with id 6
	And there is an external logon named 'Pierre Baldi' with datasource 6
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2014-10-06   |
	 | External Logon | Pierre Baldi |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Start time               | 2014-10-06 08:00 |
	| End time                 | 2014-10-06 10:00 |
	| Activity                 | Phone            |
	And there is an alarm with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Alarm Color     | Red          |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	And there is an alarm with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Alarm Color     | Green    |
	| Staffing effect | 0        |
	When the time is '2014-10-06 08:00:00'
	And 'Pierre Baldi' sets his phone state to 'Ready' on datasource 6
	And the time is '2014-10-06 08:30:00'
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And the time is '2014-10-06 10:15:00'
	And I view manage adherence view for agent 'Pierre Baldi'
	Then I should see 'Phone' with adherence of 25%
	And I should see daily adherence for 'Pierre Baldi' is 25%
