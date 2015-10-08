@ignore
Feature: Real time adherence agents
	In order to easier find the agent to blame
	As a real time analyst
	I want to see who is currently not adhering to the schedule

Scenario: Should not be able to see agents if not permitted
	Given I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to real time adherence overview | True		|
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	When I try to view real time adherence for team 'Red'
	Then I should see a message that I have no permission for this function

Scenario: Should be able to see current states of all agents
	Given there is an activity named 'Phone'
	And there is an activity named 'Lunch'
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
	 | Start Date     | 2014-01-21   |
	 | External Logon | Pierre Baldi |
	And there is an external logon named 'Ashley Andeen' with datasource 6
	And Ashley Andeen has a person period with
	 | Field          | Value         |
	 | Team           | Red           |
	 | Start Date     | 2014-01-21    |
	 | External Logon | Ashley Andeen |
	And Pierre Baldi has a shift with
	| Field			| Value            |
	| Start time	| 2014-01-21 12:00 |
	| End time		| 2014-01-21 13:00 |
	| Activity		| Phone            |
	| Next activity	| Lunch            |
	| Next activity start time | 2014-01-21 13:00 |
	| Next activity end time | 2014-01-21 13:30 |
	And Ashley Andeen has a shift with
	| Field			| Value            |
	| Start time	| 2014-01-21 12:00 |
	| End time		| 2014-01-21 13:00 |
	| Activity		| Phone            |
	| Next activity	| Lunch            |
	| Next activity start time | 2014-01-21 13:00 |
	| Next activity end time | 2014-01-21 13:30 |
	And there is an alarm with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Alarm Color           | Green	|
	| Staffing effect | 0        |
	And there is an alarm with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Alarm Color           | Red	|
	| Name            | Not adhering |
	| Staffing effect | -1           |
	And the time is '2014-01-21 12:30:00'
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And 'Ashley Andeen' sets his phone state to 'Ready' on datasource 6
	When I view real time adherence for team 'Red'
	And the time is '2014-01-21 12:45:00'
	Then I should see real time agent details for 'Pierre Baldi'
		| Name                     |                  |
		| Name                     | Pierre Baldi     |
		| State                    | Pause            |
		| Activity                 | Phone            |
		| Next activity            | Lunch            |
		| Next activity start time | 13:00 |
		| Alarm                    | Not adhering     |
		| Alarm Time               | 0:15:00 |
		| Alarm Color              | Red              |
	And I should see real time agent details for 'Ashley Andeen'
		| Field				| Value				|
		| Name				| Ashley Andeen		|
		| State				| Ready		|
		| Activity			| Phone		|
		| Next activity		| Lunch		|
		| Next activity start time	| 13:00	|
		| Alarm	| Adhering	|
		| Alarm Time	| 0:15:00 |
		| Alarm Color              | Green              |

Scenario: Should be able to see state updates of all agents
	Given there is an activity named 'Phone'
	And there is an activity named 'Lunch'
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
	 | Start Date     | 2014-01-21   |
	 | External Logon | Pierre Baldi |
	And there is an external logon named 'Ashley Andeen' with datasource 6
	And Ashley Andeen has a person period with
	 | Field          | Value         |
	 | Team           | Red           |
	 | Start Date     | 2014-01-21    |
	 | External Logon | Ashley Andeen |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Start time               | 2014-01-21 12:00 |
	| End time                 | 2014-01-21 13:00 |
	| Activity                 | Phone            |
	| Next activity            | Lunch            |
	| Next activity start time | 2014-01-21 13:00 |
	| Next activity end time   | 2014-01-21 13:30 |
	And Ashley Andeen has a shift with
	| Field			| Value            |
	| Start time	| 2014-01-21 12:00 |
	| End time		| 2014-01-21 13:00 |
	| Activity		| Phone            |
	| Next activity	| Lunch            |
	| Next activity start time | 2014-01-21 13:00 |
	| Next activity end time | 2014-01-21 13:30 |
	And there is an alarm with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Alarm Color     | Green    |
	| Staffing effect | 0        |
	And there is an alarm with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Alarm Color     | Red          |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	And the time is '2014-01-21 12:30:00'
	When I view real time adherence for team 'Red'
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And 'Ashley Andeen' sets his phone state to 'Ready' on datasource 6
	And the time is '2014-01-21 12:45:00'
	Then I should see real time agent details for 'Pierre Baldi'
		| Name                     |              |
		| Name                     | Pierre Baldi |
		| State                    | Pause        |
		| Activity                 | Phone        |
		| Next activity            | Lunch        |
		| Next activity start time | 13:00        |
		| Alarm                    | Not adhering |
		| Alarm Time               | 0:15:00      |
		| Alarm Color              | Red          |
	And I should see real time agent details for 'Ashley Andeen'
		| Field                    | Value         |
		| Name                     | Ashley Andeen |
		| State                    | Ready         |
		| Activity                 | Phone         |
		| Next activity            | Lunch         |
		| Next activity start time | 13:00         |
		| Alarm                    | Adhering      |
		| Alarm Time               | 0:15:00       |
		| Alarm Color              | Green         |

Scenario: Should be able to see all agents of the team with or without state updates
	Given there is an activity named 'Phone'
	And there is an activity named 'Lunch'
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
	 | Start Date     | 2014-01-21   |
	 | External Logon | Pierre Baldi |
	And there is an external logon named 'Ashley Andeen' with datasource 6
	And Ashley Andeen has a person period with
	 | Field          | Value         |
	 | Team           | Red           |
	 | Start Date     | 2014-01-21    |
	 | External Logon | Ashley Andeen |
	 And John Smith has a person period with
	 | Field      | Value      |
	 | Team       | Red        |
	 | Start Date | 2014-01-21 |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Start time               | 2014-01-21 12:00 |
	| End time                 | 2014-01-21 13:00 |
	| Activity                 | Phone            |
	| Next activity            | Lunch            |
	| Next activity start time | 2014-01-21 13:00 |
	| Next activity end time   | 2014-01-21 13:30 |
	And Ashley Andeen has a shift with
	| Field                    | Value            |
	| Start time               | 2014-01-21 12:00 |
	| End time                 | 2014-01-21 13:00 |
	| Activity                 | Phone            |
	| Next activity            | Lunch            |
	| Next activity start time | 2014-01-21 13:00 |
	| Next activity end time   | 2014-01-21 13:30 |
	And John Smith has a shift with
	| Field                    | Value            |
	| Start time               | 2014-01-21 12:00 |
	| End time                 | 2014-01-21 13:00 |
	| Activity                 | Phone            |
	| Next activity            | Lunch            |
	| Next activity start time | 2014-01-21 13:00 |
	| Next activity end time   | 2014-01-21 13:30 |
	And there is an alarm with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Alarm Color     | Green    |
	| Staffing effect | 0        |
	And there is an alarm with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Alarm Color     | Red          |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	And the time is '2014-01-21 12:30:00'
	When I view real time adherence for team 'Red'
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And 'Ashley Andeen' sets his phone state to 'Ready' on datasource 6
	And the time is '2014-01-21 12:45:00'
	Then I should see real time agent details for 'Pierre Baldi'
		| Name                     |                  |
		| Name                     | Pierre Baldi     |
		| State                    | Pause            |
		| Activity                 | Phone            |
		| Next activity            | Lunch            |
		| Next activity start time | 2014-01-21 13:00 |
		| Alarm                    | Not adhering     |
		| Alarm Time               | 0:15:00          |
		| Alarm Color              | Red              |
	And I should see real time agent details for 'Ashley Andeen'
		| Field                    | Value            |
		| Name                     | Ashley Andeen    |
		| State                    | Ready            |
		| Activity                 | Phone            |
		| Next activity            | Lunch            |
		| Next activity start time | 2014-01-21 13:00 |
		| Alarm                    | Adhering         |
		| Alarm Time               | 0:15:00          |
		| Alarm Color              | Green            |
	And I should see real time agent name for 'John Smith'

Scenario: Should see adherence status when call center is in Istanbul
	Given I am located in Istanbul
	And 'Pierre Baldi' is located in Istanbul
	And there is an activity named 'Phone'
	And there is a site named 'Istanbul'
	And there is a team named 'Red' on site 'Istanbul'
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
	 | Start Date     | 2015-03-24   |
	 | External Logon | Pierre Baldi |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Start time               | 2015-03-24 08:00 |
	| End time                 | 2015-03-24 10:00 |
	| Activity                 | Phone            |
	And there is an alarm with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Alarm Color     | Green    |
	| Staffing effect | 0        |
	When the utc time is '2015-03-24 06:00:00'
	And 'Pierre Baldi' sets his phone state to 'Ready' on datasource 6
	And the utc time is '2015-03-24 07:00:00'
	And I view real time adherence view for team 'Red'
	And I click on an agent state
	Then I should see real time agent details for 'Pierre Baldi'
		| Field       | Value        |
		| Name        | Pierre Baldi |
		| State       | Ready        |
		| Alarm       | Adhering     |
		| Alarm Time  | 1:00:00      |
		| Alarm Color | Green        |