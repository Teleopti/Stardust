Feature: Overview
	In order to easier find the team leader to blame
	As a real time analyst
	I want to see which parts of the organization currently not adhering to the schedule

Background:
	Given there is a switch

Scenario: See updates of sum of employees not adhering to schedule for each site
	Given I have a role with full access
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And there is a site named 'London'
	And there is a team named 'Red' on site 'London'
	And Pierre Baldi has a person period with
	| Field          | Value        |
	| Team           | Green        |
	| Start Date     | 2014-01-01   |
	And Ashley Andeen has a person period with
	| Field          | Value         |
	| Team           | Red           |
	| Start Date     | 2014-01-01    |
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
	And there is a rule with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Staffing effect | 0        |
	And there is a rule with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	And the time is '2014-01-21 13:00'
	 When I view Real time adherence sites
	 And 'Pierre Baldi' sets his phone state to 'Pause'
	 And 'Ashley Andeen' sets her phone state to 'Ready'
	 Then I should see site 'Paris' with 1 of 1 employees out of adherence
	 And I should see site 'London' with 0 of 1 employees out of adherence

Scenario: See updates of sum of employees not adhering to schedule for each team within a site
	Given I have a role with full access
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And Pierre Baldi has a person period with
	| Field          | Value        |
	| Team           | Green        |
	| Start Date     | 2014-01-21   |
	And Ashley Andeen has a person period with
	| Field          | Value         |
	| Team           | Red           |
	| Start Date     | 2014-01-21    |
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
	And there is a rule with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Staffing effect | 0        |
	And there is a rule with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	And the time is '2014-01-21 13:00'
	 When I view Real time adherence for teams on site 'Paris'
	 And 'Pierre Baldi' sets his phone state to 'Pause'
	 And 'Ashley Andeen' sets her phone state to 'Ready'
	 Then I should see team 'Green' with 1 of 1 employees out of adherence
	 And I should see team 'Red' with 0 of 1 employees out of adherence

Scenario: See current state of sum of employees not adhering to schedule for each site
	Given I have a role with full access
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And there is a site named 'London'
	And there is a team named 'Red' on site 'London'
	And Pierre Baldi has a person period with
	| Field          | Value        |
	| Team           | Green        |
	| Start Date     | 2014-01-01   |
	And Ashley Andeen has a person period with
	| Field          | Value         |
	| Team           | Red           |
	| Start Date     | 2014-01-01    |
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
	And there is a rule with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Staffing effect | 0        |
	And there is a rule with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	And the time is '2014-01-21 13:00'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And 'Ashley Andeen' sets her phone state to 'Ready'
	When I view Real time adherence sites
	Then I should see site 'Paris' with 1 of 1 employees out of adherence
	And I should see site 'London' with 0 of 1 employees out of adherence

Scenario: See current state of sum of employees not adhering to schedule for each team within a site
	Given I have a role with full access
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And Pierre Baldi has a person period with
	| Field          | Value        |
	| Team           | Green        |
	| Start Date     | 2014-01-21   |
	And Ashley Andeen has a person period with
	| Field          | Value         |
	| Team           | Red           |
	| Start Date     | 2014-01-21    |
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
	And there is a rule with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Staffing effect | 0        |
	And there is a rule with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	And the time is '2014-01-21 13:00'
	 And 'Pierre Baldi' sets his phone state to 'Pause'
	 And 'Ashley Andeen' sets her phone state to 'Ready'
	 When I view Real time adherence for teams on site 'Paris'
	 Then I should see team 'Green' with 1 of 1 employees out of adherence
	 And I should see team 'Red' with 0 of 1 employees out of adherence
