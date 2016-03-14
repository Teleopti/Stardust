Feature: Agents
	In order to easier find the agent to blame
	As a real time analyst
	I want to see who is currently not adhering to the schedule

Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is an activity named 'Lunch'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	 | Field          | Value        |
	 | Team           | Red          |
	 | Start Date     | 2014-01-21   |
	And Ashley Andeen has a person period with
	 | Field          | Value         |
	 | Team           | Red           |
	 | Start Date     | 2014-01-21    |
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
	And there is a rule with 
	| Field           | Value    |
	| Activity        | Phone    |
	| Phone state     | Ready    |
	| Name            | Adhering |
	| Alarm Color     | Green    |
	| Staffing effect | 0        |
	And there is a rule with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Alarm Color     | Red          |
	| Name            | Not adhering |
	| Staffing effect | -1           |

Scenario: See current states
	Given the time is '2014-01-21 12:30:00'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And 'Ashley Andeen' sets his phone state to 'Ready'
	When I view real time adherence for all agents on team 'Red'
	And the time is '2014-01-21 12:45:00'
	Then I should see agent status
		| Field                    | Value        |
		| Name                     | Pierre Baldi |
		| State                    | Pause        |
		| Activity                 | Phone        |
		| Next activity            | Lunch        |
		| Next activity start time | 13:00        |
		| Alarm                    | Not adhering |
		| Alarm Time               | 0:15:00      |
		| Alarm Color              | Red          |
	And I should see agent status
		| Field                    | Value         |
		| Name                     | Ashley Andeen |
		| State                    | Ready         |
		| Activity                 | Phone         |
		| Next activity            | Lunch         |
		| Next activity start time | 13:00         |
		| Alarm                    | Adhering      |
		| Alarm Time               | 0:15:00       |
		| Alarm Color              | Green         |

Scenario: See state updates
	Given the time is '2014-01-21 12:30:00'
	When I view real time adherence for all agents on team 'Red'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And 'Ashley Andeen' sets his phone state to 'Ready'
	And the time is '2014-01-21 12:45:00'
	Then I should see agent status
		| Field                    | Value        |
		| Name                     | Pierre Baldi |
		| State                    | Pause        |
		| Activity                 | Phone        |
		| Next activity            | Lunch        |
		| Next activity start time | 13:00        |
		| Alarm                    | Not adhering |
		| Alarm Time               | 0:15:00      |
		| Alarm Color              | Red          |
	And I should see agent status
		| Field                    | Value         |
		| Name                     | Ashley Andeen |
		| State                    | Ready         |
		| Activity                 | Phone         |
		| Next activity            | Lunch         |
		| Next activity start time | 13:00         |
		| Alarm                    | Adhering      |
		| Alarm Time               | 0:15:00       |
		| Alarm Color              | Green         |

Scenario: See all agents of the team even without state updates
	Given the time is '2014-01-21 12:30:00'
	When I view real time adherence for all agents on team 'Red'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And the time is '2014-01-21 12:45:00'
	Then I should see agent status for 'Pierre Baldi'
	Then I should see agent status for 'Ashley Andeen'

Scenario: See state updates when call center is in Istanbul
	Given I am located in Istanbul
	And 'Pierre Baldi' is located in Istanbul
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Start time               | 2015-03-24 08:00 |
	| End time                 | 2015-03-24 10:00 |
	| Activity                 | Phone            |
	| Next activity            | Lunch            |
	| Next activity start time | 2015-03-24 10:00 |
	| Next activity end time   | 2015-03-24 10:30 |
	When the utc time is '2015-03-24 06:00:00'
	And I view real time adherence for all agents on team 'Red'
	And 'Pierre Baldi' sets his phone state to 'Ready'
	And the utc time is '2015-03-24 06:15:00'
	Then I should see agent status
		| Field                    | Value        |
		| Name                     | Pierre Baldi |
		| State                    | Ready        |
		| Next activity start time | 10:00        |
		| Alarm                    | Adhering     |
		| Alarm Time               | 0:15:00      |
		| Alarm Color              | Green        |
