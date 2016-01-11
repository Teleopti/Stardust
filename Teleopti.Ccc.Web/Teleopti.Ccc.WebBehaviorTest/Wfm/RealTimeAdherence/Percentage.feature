Feature: Adherence percentage
	In order to ...
	As a real time analyst
	I want to see ...
	
Background:
	Given there is a switch

@OnlyRunIfEnabled('RTA_NewEventHangfireRTA_34333')
Scenario: See adherence percentage from agent state overview
	Given there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2014-10-06   |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Start time               | 2014-10-06 08:00 |
	| End time                 | 2014-10-06 10:00 |
	| Activity                 | Phone            |
	And there is a rule with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Alarm Color     | Red          |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	And there is a rule with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Alarm Color     | Green    |
	| Staffing effect | 0        |
	When the time is '2014-10-06 08:00:00'
	And 'Pierre Baldi' sets his phone state to 'Ready'
	And the time is '2014-10-06 09:00:00'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And the time is '2014-10-06 12:00:00'
	And I view real time adherence for all agents on team 'Red'
	And I click on an agent state
	Then I should see historical adherence for 'Pierre Baldi' with adherence of 50%
