@RTA
Feature: Easily spot late for work
  In order to spot late for work
  As a real-time analyst
  I want to be informed when the agent goes in adherence late (after alarm) in the first activity of the day
  and how late he was

  Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And I have a role with full access
	And Ashley Andeen has a person period with
	  | Field      | Value      |
	  | Team       | Green      |
	  | Start Date | 2018-05-10 |
	And there is a rule named 'Adhering' with
	  | Field       | Value |
	  | Activity    | Phone |
	  | Phone state | Ready |
	  | Adherence   | In    |
	And there is a rule named 'Not adhering' with
	  | Field         | Value     |
	  | Activity      | Phone     |
	  | Phone state   | LoggedOff |
	  | Adherence     | Out       |
	  | IsLogOutState | true      |

  Scenario: See late for work during first activity
	Given Ashley Andeen has a 'Phone' shift between '2018-05-17 08:00' and '17:00'
	And Ashley Andeen has a 'Phone' shift between '2018-05-18 08:00' and '17:00'
	And at '2018-05-17 17:00:00' 'Ashley Andeen' sets his phone state to 'LoggedOff'
	And at '2018-05-18 08:30:00' 'Ashley Andeen' sets his phone state to 'Ready'
	And the time is '2018-05-18 17:30:00'
	When I view historical adherence for 'Ashley Andeen' on '2018-05-18'
	Then I should be informed she is 30 minutes late for work

  Scenario: Late for work within 1 minute threshold
	Given Ashley Andeen has a 'Phone' shift between '2018-05-17 08:00' and '17:00'
	And Ashley Andeen has a 'Phone' shift between '2018-05-18 08:00' and '17:00'
	And at '2018-05-17 17:00:00' 'Ashley Andeen' sets his phone state to 'LoggedOff'
	And at '2018-05-18 08:00:59' 'Ashley Andeen' sets his phone state to 'Ready'
	And the time is '2018-05-18 17:30:00'
	When I view historical adherence for 'Ashley Andeen' on '2018-05-18'
	Then I should not be informed she is late for work
