@RTA
Feature: Solid proof
  In order to ...
  As an adherence analyst
  I want to see all OoA occurrences and the exact reason why,
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
	And there is a rule named 'Adhering' with
	  | Field       | Value |
	  | Activity    | Phone |
	  | Phone state | Ready |
	  | Adherence   | In    |
	And there is a rule named 'Not adhering' with
	  | Field       | Value     |
	  | Activity    | Phone     |
	  | Phone state | LoggedOff |
	  | Adherence   | Out       |
	And there is a rule named 'Positive' with
	  | Field       | Value |
	  | Activity    |       |
	  | Phone state | Ready |
	  | Adherence   | Out   |
	And there is a rule named 'Neutral' with
	  | Field       | Value     |
	  | Activity    |           |
	  | Phone state | LoggedOff |
	  | Adherence   | Neutral   |
	And there is a rule named 'Positive' with
	  | Field       | Value |
	  | Activity    | Lunch |
	  | Phone state | Ready |
	  | Adherence   | Out   |
	And there is a rule named 'Adhering' with
	  | Field       | Value     |
	  | Activity    | Lunch     |
	  | Phone state | LoggedOff |
	  | Adherence   | In        |

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
	And the time is '2016-10-11 12:00:00'
	When I view historical adherence for 'Mikkey Dee'
	Then I should rule and state changes
	  | Time     | Activity | State     | Rule         | Adherence |
	  | 08:30:00 |          | LoggedOff | Neutral      | Neutral   |
	  | 09:00:00 | Phone    | LoggedOff | Not adhering | Out       |
	  | 11:00:00 | Lunch    | LoggedOff | Adhering     | In        |

  Scenario: See state changes
	Given Mikkey Dee has a shift with
	  | Field      | Value            |
	  | Start time | 2016-10-11 09:00 |
	  | End time   | 2016-10-11 17:00 |
	  | Activity   | Phone            |
	And at '2016-10-11 08:30:00' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And at '2016-10-11 08:40:00' 'Mikkey Dee' sets his phone state to 'Ready'
	And at '2016-10-11 08:50:00' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And at '2016-10-11 09:10:00' 'Mikkey Dee' sets his phone state to 'Ready'
	When I view historical adherence for 'Mikkey Dee'
	Then I should rule and state changes
	  | Time     | Activity | State     | Rule     | Adherence |
	  | 08:30:00 |          | LoggedOff | Neutral  | Neutral   |
	  | 08:40:00 |          | Ready     | Positive | Out       |
	  | 08:50:00 |          | LoggedOff | Neutral  | Neutral   |
	  | 09:10:00 | Phone    | Ready     | Adhering | In        |
