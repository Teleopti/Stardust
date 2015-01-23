﻿Feature: Real time adherence overview
	In order to easier find the team leader to blame
	As a real time analyst
	I want to see which parts of the organization currently not adhering to the schedule

@OnlyRunIfDisabled('RTA_RtaLastStatesOverview_27789')
Scenario: Show site
	Given the time is '2014-01-21 13:00'
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
	Given the time is '2014-01-21 13:00'
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
	Given the time is '2014-01-21 13:00'
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
	Given the time is '2014-01-21 13:00'
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
	Given I have a role with
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
	And the time is '2014-01-21 13:00'
	 When I view Real time adherence overview
	 And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	 And 'Ashley Andeen' sets her phone state to 'Ready' on datasource 6
	 Then I should see site 'Paris' with 1 of 1 employees out of adherence
	 And I should see site 'London' with 0 of 1 employees out of adherence

Scenario: View updates of sum of employees not adhering to schedule for each team within a site
	Given I have a role with
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
	And the time is '2014-01-21 13:00'
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
	Given I have a role with
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
	And the time is '2014-01-21 13:00'
	And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	And 'Ashley Andeen' sets her phone state to 'Ready' on datasource 6
	When I view Real time adherence overview
	Then I should see site 'Paris' with 1 of 1 employees out of adherence
	And I should see site 'London' with 0 of 1 employees out of adherence

@OnlyRunIfEnabled('RTA_RtaLastStatesOverview_27789')
Scenario: View current state of sum of employees not adhering to schedule for each team within a site
	Given I have a role with
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
	And the time is '2014-01-21 13:00'
	 And 'Pierre Baldi' sets his phone state to 'Pause' on datasource 6
	 And 'Ashley Andeen' sets her phone state to 'Ready' on datasource 6
	 When I view Real time adherence for site 'Paris'
	 Then I should see team 'Green' with 1 of 1 employees out of adherence
	 And I should see team 'Red' with 0 of 1 employees out of adherence
