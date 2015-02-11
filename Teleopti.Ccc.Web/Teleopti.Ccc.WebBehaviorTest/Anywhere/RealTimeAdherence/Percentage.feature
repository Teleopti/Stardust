Feature: Real time adherence percentage
	In order to ...
	As a real time analyst
	I want to see ...
	
@OnlyRunIfEnabled('RTA_SeePercentageAdherenceForOneAgent_30783')
Scenario: Should be able to see adherence percentage from agent state overview
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
	And the time is '2014-10-06 09:00:00'
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And the time is '2014-10-06 12:00:00'
	And I view real time adherence view for team 'Red'
	And I click on an agent state
	Then I should see historical adherence for 'Pierre Baldi' with adherence of 50%

@ignore
@OnlyRunIfEnabled('RTA_CalculatePercentageInAgentTimezone_31236')
Scenario: Should be able to calculate adherence percentage in agent's timezone
	Given there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to team                         | Red         |
	 | Access to real time adherence overview | True        |
	And I am located in '(UTC) Coordinated Universal Time'
	And there is a datasouce with id 6
	And there is an external logon named 'Pierre Baldi' with datasource 6
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2014-10-06   |
	 | External Logon | Pierre Baldi |
	And 'Pierre Baldi' is located in Hawaii
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Start time               | 2014-10-06 18:00 |
	| End time                 | 2014-10-06 20:00 |
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
	When the time is '2014-10-07 04:00:00'
	And 'Pierre Baldi' sets his phone state to 'Ready' on datasource 6
	And the time is '2014-10-07 06:00:00'
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And the time is '2014-10-07 08:00:00'
	And I view real time adherence view for team 'Red'
	And I click on an agent state
	Then I should see historical adherence for 'Pierre Baldi' with adherence of 50%