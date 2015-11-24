@ignore
Feature: Night shifts
	In order to improve night shift agents adherence 
	As a real time analyst
	I want to see adherence things
	
Background:
	Given there is a switch

@ignore
@OnlyRunIfEnabled('RTA_CalculatePercentageInAgentTimezone_31236')
Scenario: See adherence percentage for night shift agents
	Given there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2014-10-06   |
	And Pierre Baldi has a shift with
	| Field      | Value            |
	| Start time | 2014-10-06 21:00 |
	| End time   | 2014-10-07 05:00 |
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
	When the time is '2014-10-06 21:00:00'
	And 'Pierre Baldi' sets his phone state to 'Ready'
	And the time is '2014-10-07 01:00:00'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And the time is '2014-10-07 05:00:00'
	And I view real time adherence view for team 'Red'
	And I click on an agent state
	Then I should see historical adherence for 'Pierre Baldi' with adherence of 50%

@ignore
@OnlyRunIfEnabled('RTA_CalculatePercentageInAgentTimezone_31236')
Scenario: See adherence details for night shift agents
	Given there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2014-10-06   |
	And Pierre Baldi has a shift with
	| Field      | Value            |
	| Start time | 2014-10-06 21:00 |
	| End time   | 2014-10-07 05:00 |
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
	When the time is '2014-10-06 21:00:00'
	And 'Pierre Baldi' sets his phone state to 'Ready'
	And the time is '2014-10-07 01:00:00'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And the time is '2014-10-07 05:00:00'
	And I view manage adherence view for agent 'Pierre Baldi'
	Then I should see 'Phone' with adherence of 50%
	And I should see daily adherence for 'Pierre Baldi' is 50%
