@RTA
Feature: Alarm context
	In order to have an idea why an agent is out of adherence or in alarm
	As a real time analyst
	I do want to see parts of the agents shift to give me some context

Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is an activity named 'Lunch'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Red        |
	 | Start Date | 2016-05-19 |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2016-05-20 08:00 |
	| End time                 | 2016-05-20 17:00 |
	| Next activity            | Lunch            |
	| Next activity start time | 2016-05-20 12:00 |
	| Next activity end time   | 2016-05-20 13:00 |
	And there is a rule with 
	| Field       | Value        |
	| Activity    | Phone        |
	| Phone state | LoggedOut    |

Scenario: Late for work
	Given the time is '2016-05-19 17:00:00'
	And 'Pierre Baldi' sets his phone state to 'LoggedOut'
	Given the time is '2016-05-20 8:15:00'
	When I view real time adherence for all agents on team 'Red'
	Then I should see agent status
	| Field             | Value        |
	| Name              | Pierre Baldi |
	| Previous activity | <none>       |
	| Activity          | Phone        |

Scenario: Late back from lunch
	Given the time is '2016-05-19 12:00:00'
	And 'Pierre Baldi' sets his phone state to 'LoggedOut'
	Given the time is '2016-05-20 13:15:00'
	When I view real time adherence for all agents on team 'Red'
	Then I should see agent status
	| Field             | Value        |
	| Name              | Pierre Baldi |
	| Previous activity | Lunch        |
	| Activity          | Phone        |

Scenario: Early lunch
	Given the time is '2016-05-20 11:45:00'
	And 'Pierre Baldi' sets his phone state to 'LoggedOut'
	When I view real time adherence for all agents on team 'Red'
	Then I should see agent status
	| Field         | Value        |
	| Name          | Pierre Baldi |
	| Activity      | Phone        |
	| Next Activity | Lunch        |
