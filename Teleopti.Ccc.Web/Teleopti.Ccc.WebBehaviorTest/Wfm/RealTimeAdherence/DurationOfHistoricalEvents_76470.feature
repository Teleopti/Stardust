@RTA
Feature: Duration of historical events
  In order to ...
  As an adherence analyst
  I want to see duration spent in one event so that I don't have to calculate it myself

  Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'London'
	And there is a team named 'Team Preferences' on site 'London'
	And I have a role with full access
	And John Smith has a person period with
	  | Field      | Value            |
	  | Team       | Team Preferences |
	  | Start Date | 2014-01-21       |
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

  Scenario: See duration of historical events
	Given John Smith has a 'Phone' shift between '2018-07-06 09:00' and '17:00'
	And at '2018-07-06 09:00:00' 'John Smith' sets his phone state to 'Ready'
	And at '2018-07-06 15:00:00' 'John Smith' sets his phone state to 'LoggedOff'
	And the time is '2018-07-06 18:00:00'
	When I view historical adherence for 'John Smith' on '2018-07-06'
	Then I should see duration for historical events
	  | Time     | Duration |
	  | 09:00:00 | 06:00:00 |
	  | 15:00:00 | 02:00:00 |