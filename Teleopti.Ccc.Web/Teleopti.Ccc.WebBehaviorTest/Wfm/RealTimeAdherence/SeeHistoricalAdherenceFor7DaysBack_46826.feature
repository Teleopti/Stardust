@RTA
Feature: See historical adherence for 7 days back
  In order to ...
  As an adherence analyst
  I want to see all OoA occurrences and the exact reason why for up to 7 days back,
  so that I have solid proof when I talk to team leads and agents about bad adherence.

  Background:
	Given there is a switch
	And there is an activity named 'Phone'
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

  Scenario: See historical adherence for 7 days back
	Given Mikkey Dee has a 'Phone' shift between '2018-01-02 09:00' and '17:00'
	Given Mikkey Dee has a 'Phone' shift between '2018-01-03 09:00' and '17:00'
	Given Mikkey Dee has a 'Phone' shift between '2018-01-04 09:00' and '17:00'
	Given Mikkey Dee has a 'Phone' shift between '2018-01-05 09:00' and '17:00'
	Given Mikkey Dee has a 'Phone' shift between '2018-01-06 09:00' and '17:00'
	Given Mikkey Dee has a 'Phone' shift between '2018-01-07 09:00' and '17:00'
	Given Mikkey Dee has a 'Phone' shift between '2018-01-08 09:00' and '17:00'
	And at '2018-01-02 09:00:00' 'Mikkey Dee' sets his phone state to 'Ready'
	And at '2018-01-03 10:00:00' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And at '2018-01-04 11:00:00' 'Mikkey Dee' sets his phone state to 'Ready'
	And at '2018-01-05 12:00:00' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And at '2018-01-06 13:00:00' 'Mikkey Dee' sets his phone state to 'Ready'
	And at '2018-01-07 14:00:00' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And at '2018-01-08 15:00:00' 'Mikkey Dee' sets his phone state to 'Ready'
	And the time is '2018-01-08 16:00:00'
	When I view historical adherence for 'Mikkey Dee' on '2018-01-02'
	Then I should see rule change 'Adhering' at '09:00:00'
	When I view historical adherence for 'Mikkey Dee' on '2018-01-03'
	Then I should see rule change 'Not adhering' at '10:00:00'
	When I view historical adherence for 'Mikkey Dee' on '2018-01-04'
	Then I should see rule change 'Adhering' at '11:00:00'
	When I view historical adherence for 'Mikkey Dee' on '2018-01-05'
	Then I should see rule change 'Not adhering' at '12:00:00'
	When I view historical adherence for 'Mikkey Dee' on '2018-01-06'
	Then I should see rule change 'Adhering' at '13:00:00'
	When I view historical adherence for 'Mikkey Dee' on '2018-01-07'
	Then I should see rule change 'Not adhering' at '14:00:00'
	When I view historical adherence for 'Mikkey Dee' on '2018-01-08'
	Then I should see rule change 'Adhering' at '15:00:00'

