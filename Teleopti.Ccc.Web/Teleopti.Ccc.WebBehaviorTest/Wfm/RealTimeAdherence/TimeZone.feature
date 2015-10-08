@ignore
Feature: View adherence in hawaii call center
	In order to improve hawaii agents adherence 
	As a real time analyst
	I want to see adherence things
	
@OnlyRunIfEnabled('RTA_CalculatePercentageInAgentTimezone_31236')
Scenario: Should see adherence percentage when call center is in Hawaii
	Given I am located in Hawaii
	And 'Pierre Baldi' is located in Hawaii
	And there is an activity named 'Phone'
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
	| Field      | Value            |
	| Start time | 2014-10-06 11:00 |
	| End time   | 2014-10-06 19:00 |
	| Activity   | Phone            |
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
	When the utc time is '2014-10-06 21:00:00'
	And 'Pierre Baldi' sets his phone state to 'Ready' on datasource 6
	And the utc time is '2014-10-07 01:00:00'
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And the utc time is '2014-10-07 05:00:00'
	And I view real time adherence view for team 'Red'
	And I click on an agent state
	Then I should see historical adherence for 'Pierre Baldi' with adherence of 50%

@OnlyRunIfEnabled('RTA_CalculatePercentageInAgentTimezone_31236')
Scenario: Should see adherence details when call center is in Hawaii
	Given I am located in Hawaii
	And 'Pierre Baldi' is located in Hawaii
	And there is an activity named 'Phone'
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
	 | Start Date     | 2014-10-07   |
	 | External Logon | Pierre Baldi |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Start time               | 2014-10-07 11:00 |
	| End time                 | 2014-10-07 19:00 |
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
	When the utc time is '2014-10-07 21:00:00'
	And 'Pierre Baldi' sets his phone state to 'Ready' on datasource 6
	And the utc time is '2014-10-08 01:00:00'
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And the utc time is '2014-10-08 05:00:00'
	And I view manage adherence view for agent 'Pierre Baldi'
	Then I should see 'Phone' with adherence of 50%
	And I should see daily adherence for 'Pierre Baldi' is 50%
