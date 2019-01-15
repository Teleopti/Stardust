@RTA
@OnlyRunIfEnabled('RTA_ReviewHistoricalAdherence_74770')
Feature: Review historical adherence
  In order to ...
  As a Manager or Team Lead
  I want to see historical adherence (%) for my team/teams,
  so that I can track historical adherence and find where it has been poor.

  In order to...
  As a Manager or Team Lead
  I want to see #Lates and duration for my team/teams,
  so that I can manage late arrivals

  Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'London'
	And there is a team named 'Red' on site 'London'
	And I have a role with full access
	And John Smith has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2018-08-01 |
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

  Scenario: Review historical adherence
	Given John Smith has a 'Phone' shift between '2018-08-02 10:00' and '20:00'
	And John Smith has a 'Phone' shift between '2018-08-03 10:00' and '20:00'
	And John Smith has a 'Phone' shift between '2018-08-04 10:00' and '20:00'
	And John Smith has a 'Phone' shift between '2018-08-05 10:00' and '20:00'
	And John Smith has a 'Phone' shift between '2018-08-06 10:00' and '20:00'
	And John Smith has a 'Phone' shift between '2018-08-07 10:00' and '20:00'
	And John Smith has a 'Phone' shift between '2018-08-08 10:00' and '20:00'

	And at '2018-08-01 17:00:00' 'John Smith' sets his phone state to 'LoggedOff'

	And at '2018-08-02 10:05:00' 'John Smith' sets his phone state to 'Ready'
	And at '2018-08-02 11:05:00' 'John Smith' sets his phone state to 'LoggedOff'

	And at '2018-08-03 10:00:00' 'John Smith' sets his phone state to 'Ready'
	And at '2018-08-03 12:00:00' 'John Smith' sets his phone state to 'LoggedOff'

	And at '2018-08-04 10:10:00' 'John Smith' sets his phone state to 'Ready'
	And at '2018-08-04 13:10:00' 'John Smith' sets his phone state to 'LoggedOff'

	And at '2018-08-05 10:00:00' 'John Smith' sets his phone state to 'Ready'
	And at '2018-08-05 14:00:00' 'John Smith' sets his phone state to 'LoggedOff'

	And at '2018-08-06 10:20:00' 'John Smith' sets his phone state to 'Ready'
	And at '2018-08-06 15:20:00' 'John Smith' sets his phone state to 'LoggedOff'

	And at '2018-08-07 10:00:00' 'John Smith' sets his phone state to 'Ready'
	And at '2018-08-07 16:00:00' 'John Smith' sets his phone state to 'LoggedOff'

	And at '2018-08-08 10:00:00' 'John Smith' sets his phone state to 'Ready'
	And at '2018-08-08 17:00:00' 'John Smith' sets his phone state to 'LoggedOff'

	And the time is '2018-08-09 08:00:00'
	When I review historical adherence for team 'Red'

	Then I should see 'John Smith' having adherence percent of '10' on '08/02'
	And I should see 'John Smith' having adherence percent of '20' on '08/03'
	And I should see 'John Smith' having adherence percent of '30' on '08/04'
	And I should see 'John Smith' having adherence percent of '40' on '08/05'
	And I should see 'John Smith' having adherence percent of '50' on '08/06'
	And I should see 'John Smith' having adherence percent of '60' on '08/07'
	And I should see 'John Smith' having adherence percent of '70' on '08/08'

	And I should see 'John Smith' having adherence percent of '40' for period
	And I should see 'John Smith' is late for work in total of '35' minutes for '3' times during period  

	