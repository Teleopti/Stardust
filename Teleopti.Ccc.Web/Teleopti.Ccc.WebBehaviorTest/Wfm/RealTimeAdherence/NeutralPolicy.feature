Feature: Neutral adherence policy
	In order to improve adherence with neutral alarm occured
	As a real time analyst
	I want to see correct adherence value

Background:
	Given there is a switch

@OnlyRunIfEnabled('RTA_NeutralAdherence_30930')
Scenario: See adherence percentage with neutral adherence
	Given there is an activity named 'Phone'
	And there is an activity named 'Administration'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2015-03-02   |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2015-03-02 08:00 |
	| End time                 | 2015-03-02 09:00 |
	| Next activity            | Administration   |
	| Next activity start time | 2015-03-02 09:00 |
	| Next activity end time   | 2015-03-02 10:00 |
	And there is a rule with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Adherence       | In       |
	| Staffing effect | 0        |
	And there is a rule with 
	| Field           | Value          |
	| Activity        | Administration |
	| Phone state     | SomeCode       |
	| Name            | Unknown        |
	| Adherence       | Neutral        |
	| Staffing effect | -1             |
	When the time is '2015-03-02 08:00:00'
	And 'Pierre Baldi' sets his phone state to 'Ready'
	And the time is '2015-03-02 09:00:00'
	And 'Pierre Baldi' sets his phone state to 'SomeCode'
	And the time is '2015-03-02 10:00:00'
	And I view real time adherence for agents on team 'Red'
	And I click on an agent state
	Then I should see historical adherence for 'Pierre Baldi' with adherence of 100%

@OnlyRunIfEnabled('RTA_NeutralAdherence_30930')
Scenario: See adherence details with neutral adherence
	Given there is an activity named 'Phone'
	And there is an activity named 'Administration'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2015-03-02   |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2015-03-02 08:00 |
	| End time                 | 2015-03-02 09:00 |
	| Next activity            | Administration   |
	| Next activity start time | 2015-03-02 09:00 |
	| Next activity end time   | 2015-03-02 10:00 |
	And there is a rule with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Adherence       | In       |
	| Staffing effect | 0        |
	And there is a rule with 
	| Field           | Value          |
	| Activity        | Administration |
	| Phone state     | SomeCode       |
	| Name            | Unknown        |
	| Adherence       | Neutral        |
	| Staffing effect | -1             |
	When the time is '2015-03-02 08:00:00'
	And 'Pierre Baldi' sets his phone state to 'Ready'
	And the time is '2015-03-02 09:00:00'
	And 'Pierre Baldi' sets his phone state to 'SomeCode'
	And the time is '2015-03-02 10:00:00'
	And I view agent details view for agent 'Pierre Baldi'
	Then I should see 'Administration' without adherence
	And I should see daily adherence for 'Pierre Baldi' is 100%
