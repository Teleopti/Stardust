﻿Feature: Real time adherence overview
	In order to easier find the team leader to blame
	As a real time analyst
	I want to see which parts of the organization currently not adhering to the schedule

@OnlyRunIfDisabled('RTA_RtaLastStatesOverview_27789')
Scenario: Show site
	Given the current time is '2014-01-21 13:00'
	And I have a role with
	| Field                                  | Value             |
	| Name                                   | Real time analyst |
	| Access to real time adherence overview | True              |
	And there is a site named 'London'
	And there is a team named 'Red' on site 'London'
	And Guy London has a person period with
	| Field      | Value      |
	| Team       | Red      |
	| Start Date | 2014-01-01 |
	When I view Real time adherence overview
	Then I should see the site 'London'
	And I should see the overlay 'waiting three dots'

@OnlyRunIfEnabled('RTA_RtaLastStatesOverview_27789')
Scenario: Show site without always loading status
	Given the current time is '2014-01-21 13:00'
	And I have a role with
	| Field                                  | Value             |
	| Name                                   | Real time analyst |
	| Access to real time adherence overview | True              |
	And there is a site named 'London'
	And there is a team named 'Red' on site 'London'
	And Guy London has a person period with
	| Field      | Value      |
	| Team       | Red      |
	| Start Date | 2014-01-01 |
	When I view Real time adherence overview
	Then I should see the site 'London'

@OnlyRunIfDisabled('RTA_RtaLastStatesOverview_27789')
Scenario: Show team
	Given the current time is '2014-01-21 13:00'
	And I have a role with
	| Field                                  | Value             |
	| Name                                   | Real time analyst |
	| Access to real time adherence overview | True              |
	And there is a site named 'London'
	And there is a team named 'Red' on site 'London'
	And Guy Red has a person period with
	| Field      | Value      |
	| Team       | Red      |
	| Start Date | 2014-01-01 |
	When I view Real time adherence for site 'London'
	Then I should see the team 'Red'
	And I should see the overlay 'waiting three dots'

@OnlyRunIfEnabled('RTA_RtaLastStatesOverview_27789')
Scenario: Show team without always loading status
	Given the current time is '2014-01-21 13:00'
	And I have a role with
	| Field                                  | Value             |
	| Name                                   | Real time analyst |
	| Access to real time adherence overview | True              |
	And there is a site named 'London'
	And there is a team named 'Red' on site 'London'
	And Guy Red has a person period with
	| Field      | Value      |
	| Team       | Red      |
	| Start Date | 2014-01-01 |
	When I view Real time adherence for site 'London'
	Then I should see the team 'Red'

Scenario: View updates of sum of employees not adhering to schedule for each site
	Given the current time is '2014-01-21 13:00'
	And I have a role with
	| Field                                  | Value             |
	| Name                                   | Real time analyst |
	| Access to real time adherence overview | True              |
	And there is a datasouce with id 6
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And there is a site named 'London'
	And there is a team named 'Red' on site 'London'
	And there is an external logon named 'Pierre Baldi' with datasource 6
	And Pierre Baldi has a person period with
	| Field          | Value        |
	| Team           | Green        |
	| Start Date     | 2014-01-01   |
	| External Logon | Pierre Baldi |
	And there is an external logon named 'Ashley Andeen' with datasource 6
	And Ashley Andeen has a person period with
	| Field          | Value         |
	| Team           | Red           |
	| Start Date     | 2014-01-01    |
	| External Logon | Ashley Andeen |
	And Pierre Baldi has a shift with
	| Field      | Value            |
	| Start time | 2014-01-21 12:00 |
	| End time   | 2014-01-21 19:00 |
	| Activity   | Phone            |
	And Ashley Andeen has a shift with
	| Field      | Value            |
	| Start time | 2014-01-21 12:00 |
	| End time   | 2014-01-21 19:00 |
	| Activity   | Phone            |
	And there is an alarm with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Staffing effect | 0        |
	And there is an alarm with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	 When I view Real time adherence overview
	 And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	 And 'Ashley Andeen' sets her phone state to 'Ready' on datasource 6
	 Then I should see site 'Paris' with 1 of 1 employees out of adherence
	 And I should see site 'London' with 0 of 1 employees out of adherence

Scenario: View updates of sum of employees not adhering to schedule for each team within a site
	Given the current time is '2014-01-21 13:00'
	And I have a role with
	| Field                                  | Value             |
	| Name                                   | Real time analyst |
	| Access to real time adherence overview | True              |
	And there is a datasouce with id 6
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And there is an external logon named 'Pierre Baldi' with datasource 6
	And Pierre Baldi has a person period with
	| Field          | Value        |
	| Team           | Green        |
	| Start Date     | 2014-01-21   |
	| External Logon | Pierre Baldi |
	And there is an external logon named 'Ashley Andeen' with datasource 6
	And Ashley Andeen has a person period with
	| Field          | Value         |
	| Team           | Red           |
	| Start Date     | 2014-01-21    |
	| External Logon | Ashley Andeen |
	 And Pierre Baldi has a shift with
	| Field      | Value            |
	| Start time | 2014-01-21 12:00 |
	| End time   | 2014-01-21 19:00 |
	| Activity   | Phone            |
	 And Ashley Andeen has a shift with
	| Field      | Value            |
	| Start time | 2014-01-21 12:00 |
	| End time   | 2014-01-21 19:00 |
	| Activity   | Phone            |
	And there is an alarm with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Staffing effect | 0        |
	And there is an alarm with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	 When I view Real time adherence for site 'Paris'
	 And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	 And 'Ashley Andeen' sets her phone state to 'Ready' on datasource 6
	 Then I should see team 'Green' with 1 of 1 employees out of adherence
	 And I should see team 'Red' with 0 of 1 employees out of adherence


Scenario: Should not be able to view Real time adherence overview when not permitted
	Given I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to real time adherence overview | False       |
	When I try to view Real time adherence overview
	Then I should see a message that I have no permission for this function

Scenario: Should not see Real time adherence overview in menu when not permitted
	Given I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to real time adherence overview | False       |
	When I view Anywhere
	Then I should not see Real time adherence overview in the menu
	
Scenario: Should be able to go to Real time adherence overview
	Given I have a role with
	 | Field                                  | Value              |
	 | Name                                   | Real time analyist |
	 | Access to real time adherence overview | True               |
	When I view Anywhere
	Then I should see Real time adherence overview in the menu

@OnlyRunIfEnabled('RTA_RtaLastStatesOverview_27789')
Scenario: View current state of sum of employees not adhering to schedule for each site
	Given the current time is '2014-01-21 13:00'
	And I have a role with
	| Field                                  | Value             |
	| Name                                   | Real time analyst |
	| Access to real time adherence overview | True              |
	And there is a datasouce with id 6
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And there is a site named 'London'
	And there is a team named 'Red' on site 'London'
	And there is an external logon named 'Pierre Baldi' with datasource 6
	And Pierre Baldi has a person period with
	| Field          | Value        |
	| Team           | Green        |
	| Start Date     | 2014-01-01   |
	| External Logon | Pierre Baldi |
	And there is an external logon named 'Ashley Andeen' with datasource 6
	And Ashley Andeen has a person period with
	| Field          | Value         |
	| Team           | Red           |
	| Start Date     | 2014-01-01    |
	| External Logon | Ashley Andeen |
	And Pierre Baldi has a shift with
	| Field      | Value            |
	| Start time | 2014-01-21 12:00 |
	| End time   | 2014-01-21 19:00 |
	| Activity   | Phone            |
	And Ashley Andeen has a shift with
	| Field      | Value            |
	| Start time | 2014-01-21 12:00 |
	| End time   | 2014-01-21 19:00 |
	| Activity   | Phone            |
	And there is an alarm with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Staffing effect | 0        |
	And there is an alarm with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And 'Ashley Andeen' sets her phone state to 'Ready' on datasource 6
	When I view Real time adherence overview
	Then I should see site 'Paris' with 1 of 1 employees out of adherence
	And I should see site 'London' with 0 of 1 employees out of adherence

@OnlyRunIfEnabled('RTA_RtaLastStatesOverview_27789')
Scenario: View current state of sum of employees not adhering to schedule for each team within a site
	Given the current time is '2014-01-21 13:00'
	And I have a role with
	| Field                                  | Value             |
	| Name                                   | Real time analyst |
	| Access to real time adherence overview | True              |
	And there is a datasouce with id 6
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And there is an external logon named 'Pierre Baldi' with datasource 6
	And Pierre Baldi has a person period with
	| Field          | Value        |
	| Team           | Green        |
	| Start Date     | 2014-01-21   |
	| External Logon | Pierre Baldi |
	And there is an external logon named 'Ashley Andeen' with datasource 6
	And Ashley Andeen has a person period with
	| Field          | Value         |
	| Team           | Red           |
	| Start Date     | 2014-01-21    |
	| External Logon | Ashley Andeen |
	 And Pierre Baldi has a shift with
	| Field      | Value            |
	| Start time | 2014-01-21 12:00 |
	| End time   | 2014-01-21 19:00 |
	| Activity   | Phone            |
	 And Ashley Andeen has a shift with
	| Field      | Value            |
	| Start time | 2014-01-21 12:00 |
	| End time   | 2014-01-21 19:00 |
	| Activity   | Phone            |
	And there is an alarm with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Staffing effect | 0        |
	And there is an alarm with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	 And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	 And 'Ashley Andeen' sets her phone state to 'Ready' on datasource 6
	 When I view Real time adherence for site 'Paris'
	 Then I should see team 'Green' with 1 of 1 employees out of adherence
	 And I should see team 'Red' with 0 of 1 employees out of adherence

Scenario: Should not be able to see agents if not permitted
	Given I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to real time adherence overview | True		|
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	When I try to view real time adherence for team 'Red'
	Then I should see a message that I have no permission for this function

@OnlyRunIfEnabled('RTA_DrilldownToAllAgentsInOneTeam_25234')
Scenario: Should be able to see current states of all agents
	Given  the current time is '2014-01-21 12:30:00'
	And there is an activity named 'Phone'
	And there is an activity named 'Lunch'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to team                         | Red         |
	 | Access to real time adherence overview | True        |
	And there is a datasouce with id 6
	And I am located in 'London'
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
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And 'Ashley Andeen' sets his phone state to 'Ready' on datasource 6
	When I view real time adherence for team 'Red'
	And the browser time is '2014-01-21 12:45:00'
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

@OnlyRunIfEnabled('RTA_DrilldownToAllAgentsInOneTeam_25234')
Scenario: Should be able to see state updates of all agents
	Given  the current time is '2014-01-21 12:30:00'
	And there is an activity named 'Phone'
	And there is an activity named 'Lunch'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to team                         | Red         |
	 | Access to real time adherence overview | True        |
	And there is a datasouce with id 6
	And I am located in 'London'
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
	When I view real time adherence for team 'Red'
	And the browser time is '2014-01-21 12:45:00'
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And 'Ashley Andeen' sets his phone state to 'Ready' on datasource 6
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

@OnlyRunIfEnabled('RTA_DrilldownToAllAgentsInOneTeam_25234')
Scenario: Should be able to see all agents of the team with or without state updates
	Given  the current time is '2014-01-21 12:30:00'
	And there is an activity named 'Phone'
	And there is an activity named 'Lunch'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to team                         | Red         |
	 | Access to real time adherence overview | True        |
	And there is a datasouce with id 6
	And I am located in 'London'
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
	When I view real time adherence for team 'Red'
	And the browser time is '2014-01-21 12:45:00'
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And 'Ashley Andeen' sets his phone state to 'Ready' on datasource 6
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
	
@OnlyRunIfEnabled('RTA_ViewAgentsForMultipleTeams_28967')
Scenario: Should be able to see agents for multiple sites
	Given there is a site named 'Paris'
	And there is a team named 'Team Paris' on site 'Paris'
	And there is a site named 'London'
	And there is a team named 'Team London' on site 'London'
	And I have a role with
	| Field                                  | Value                  |
	| Name                                   | Real time analyst      |
	| Access to team                         | Team Paris,Team London |
	| Access to real time adherence overview | True                   |
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Team Paris |
	 | Start Date | 2014-01-21 |
	And Ashley Andeen has a person period with
	 | Field      | Value       |
	 | Team       | Team London |
	 | Start Date | 2014-01-21  |
	When I view Real time adherence overview
	And I click the site checkbox for 'London'
	And I click the site checkbox for 'Paris'
	And I click 'open'
	Then I should see real time agent name for 'Pierre Baldi'
	And I should see real time agent name for 'Ashley Andeen'

@OnlyRunIfEnabled('RTA_ViewAgentsForMultipleTeams_28967')
Scenario: Should be able to see agents for multiple teams
	Given there is a site named 'Stockholm'
	And there is a team named 'Täby' on site 'Stockholm'
	And there is a team named 'Garnisonen' on site 'Stockholm'
	And I have a role with
	| Field                                  | Value             |
	| Name                                   | Real time analyst |
	| Access to team                         | Täby, Garnisonen  |
	| Access to real time adherence overview | True              |
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Täby       |
	 | Start Date | 2014-01-21 |
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Garnisonen |
	 | Start Date | 2014-01-21 |
	 When I view Real time adherence for site 'Stockholm'
	And I click the team checkbox for 'Täby'
	And I click the team checkbox for 'Garnisonen'
	And I click 'open'
	Then I should see real time agent name for 'Pierre Baldi'
	And I should see real time agent name for 'Ashley Andeen'

@OnlyRunIfEnabled('RTA_MonitorMultipleBusinessUnits_28348')
Scenario: Should display sites of a selected business unit
	Given the current time is '2014-08-01 13:00'
	And I have a role with
	| Field                                  | Value             |
	| Name                                   | Real time analyst |	
	| Access to everyone					 | True              |
	| Access to real time adherence overview | True              |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 1 |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 2 |
	And there is a site 'Paris' on business unit 'Business Unit 1'
	And there is a site 'London' on business unit 'Business Unit 2'
	And there is a team named 'Red' on site 'Paris'
	And there is a team named 'Green' on site 'London'
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Red |
	 | Start Date | 2014-01-21 |
	And Ashley Andeen has a person period with
	 | Field      | Value       |
	 | Team       | Green |
	 | Start Date | 2014-01-21  |
	When I view Real time adherence overview
	And I choose business unit 'Business Unit 1'
	Then I should see the site 'Paris'

@OnlyRunIfEnabled('RTA_MonitorMultipleBusinessUnits_28348')
Scenario: Should be able to see all agents state updates of a team within a specific business unit
	Given  the current time is '2014-01-21 12:30:00'
	And I have a role with
	| Field                                  | Value             |
	| Name                                   | Real time analyst |	
	| Access to everyone					 | True              |
	| Access to real time adherence overview | True              |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 1 |
	And there is a business unit with
	| Field | Value           |
	| Name  | Business Unit 2 |
	And there is a site 'Paris' on business unit 'Business Unit 1'
	And there is a site 'London' on business unit 'Business Unit 2'
	And there is a team named 'Red' on site 'Paris'
	And there is an activity in business unit 'Business Unit 1' named 'Phone'
	And there is an activity in business unit 'Business Unit 1' named 'Lunch'
	And there is a datasouce with id 6
	And I am located in 'London'
	And there is an external logon named 'Pierre Baldi' with datasource 6
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2014-01-21   |
	 | External Logon | Pierre Baldi |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Start time               | 2014-01-21 12:00 |
	| End time                 | 2014-01-21 13:00 |
	| Activity                 | Phone            |
	| Next activity            | Lunch            |
	| Next activity start time | 2014-01-21 13:00 |
	| Next activity end time   | 2014-01-21 13:30 |
	And there is an alarm with 
	| Field           | Value    |
	| Business Unit   | Business Unit 1    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Alarm Color     | Green    |
	| Staffing effect | 0        |
	And there is an alarm with 
	| Field           | Value        |
	| Business Unit   | Business Unit 1    |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Alarm Color     | Red          |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	When I view Real time adherence overview
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And I choose business unit 'Business Unit 1'
	And I click the site 'Paris'
	And I view real time adherence for team 'Red'
	And the browser time is '2014-01-21 12:45:00'
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

@OnlyRunIfEnabled('RTA_ChangeScheduleInAgentStateView_29934')
Scenario: Should be able to change schedule from agent state overview
	Given  the current time is '2014-09-09 12:30:00'
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with
	| Field                                  | Value       |
	| Name                                   | Team leader |
	| Access to team                         | Red         |
	| Access to real time adherence overview | True        |
	| Access to Anywhere         | true                |
	And there is a workflow control set with
	| Field                      | Value                      |
	| Name                       | Schedule published to 0909 |
	| Schedule published to date | 2014-09-09                 |
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Team       | Red |
	| Start date | 2014-01-01 |
	And 'Pierre Baldi' has the workflow control set 'Schedule published to 0909'
	And there are shift categories
	| Name  |
	| Day   |
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2014-09-09 08:00 |
	| End time       | 2014-09-09 17:00 |
	When I view real time adherence for team 'Red'
	And the browser time is '2014-09-09 12:45:00'
	And I click agent state of 'Pierre Baldi'
	And I click 'change schedule' in agent menu
	Then I should see schedule for 'Pierre Baldi'
	And I should see schedule menu
