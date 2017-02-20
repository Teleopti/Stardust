@RTA
@ignore
Feature: I need solid proof when I manage agent adherence
	As an adherence analyst I need to see all OoA occurrences and the exact reason why,
	so that I have solid proof when I talk to team leads and agents about bad adherence.
	and so that I can find issues in the configured setup.

Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is an activity named 'Lunch'
	And there is a site named 'Hammersmith'
	And there is a team named 'Motorhead' on site 'Hammersmith'
	And I have a role with full access
	And Mikkey Dee has a person period with
	| Field      | Value      |
	| Team       | Motorhead  |
	| Start Date | 2014-01-21 |
	And there is a rule with 
	| Field       | Value    |
	| Name        | Adhering |
	| Adherence   | In       |
	| Activity    | Phone    |
	| Phone state | Ready    |
	And there is a rule with 
	| Field       | Value        |
	| Name        | Not adhering |
	| Adherence   | Out          |
	| Activity    | Phone        |
	| Phone state | LoggedOff    |
	And there is a rule with 
	| Field       | Value    |
	| Name        | Positive |
	| Adherence   | Out      |
	| Activity    |          |
	| Phone state | Ready    |
	And there is a rule with 
	| Field       | Value     |
	| Name        | Neutral   |
	| Adherence   | Neutral   |
	| Activity    |           |
	| Phone state | LoggedOff |
	
@OnlyRunIfEnabled('RTA_SolidProofWhenManagingAgentAdherence_39351')
Scenario: See rule changes
	Given Mikkey Dee has a shift with
	| Field                    | Value            |
	| Start time               | 2016-10-11 09:00 |
	| End time                 | 2016-10-11 17:00 |
	| Activity                 | Phone            |
	| Next activity            | Lunch            |
	| Next activity start time | 2016-10-11 11:00 |
	| Next activity end time   | 2016-10-11 12:00 |
	And at '2016-10-11 08:30:00' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And the time is '2016-10-11 09:00:00'
	And the time is '2016-10-11 10:00:00'
	And the time is '2016-10-11 11:00:00'
	And the time is '2016-10-11 12:00:00'
	When I view historical adherence for 'Mikkey Dee'
	Then I should rule and state changes
	| Time                | Activity | State     | Rule         | Adherence         |
	| 2016-10-11 08:30:00 |          | LoggedOff | Neutral      | Neutral adherence |
	| 2016-10-11 09:00:00 | Phone    | LoggedOff | Not adhering | Out of adherence  |
	| 2016-10-11 11:00:00 | Lunch    | LoggedOff | Adhering     | In adherence      |

@OnlyRunIfEnabled('RTA_SolidProofWhenManagingAgentAdherence_39351')
Scenario: See state changes
	Given Mikkey Dee has a shift with
	| Field                    | Value            |
	| Start time               | 2016-10-11 09:00 |
	| End time                 | 2016-10-11 17:00 |
	| Activity                 | Phone            |
	And at '2016-10-11 08:30:00' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And at '2016-10-11 08:40:00' 'Mikkey Dee' sets his phone state to 'Ready'
	And at '2016-10-11 08:50:00' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And at '2016-10-11 09:10:00' 'Mikkey Dee' sets his phone state to 'Ready'
	And the time is '2016-10-11 12:00:00'
	When I view historical adherence for 'Mikkey Dee'
	Then I should rule and state changes
	| Time                | Activity | State     | Rule     | Adherence         |
	| 2016-10-11 08:30:00 |          | LoggedOff | Neutral  | Neutral adherence |
	| 2016-10-11 08:40:00 | Phone    | Ready     | Positive | Out of adherence  |
	| 2016-10-11 08:50:00 | Phone    | LoggedOff | Neutral  | Neutral adherence |
	| 2016-10-11 10:10:00 | Phone    | Ready     | Adhering | In adherence      |
