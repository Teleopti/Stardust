@RTA
@OnlyRunIfEnabled('RTA_AdjustAdherenceToNeutral_80594')
Feature: Adjust adherence to neutral
  In order to manage technical issues in the adherence states feed
  As a Manager
  I want to exclude time periods from the adherence score calculation for all agents by adjusting adherence to neutral
  
  Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'London'
	And there is a team named 'Green' on site 'London'
	And I have a role with full access
	And Ashley Andeen has a person period with
	  | Field      | Value      |
	  | Team       | Green      |
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

  Scenario: Adjust adherence to neutral
	Given there was a technical issue in the adherence states feed between '2019-01-14 09:00' and '12:00'
	And the time is '2019-01-15 08:00'
	When I view adjust adherence
	And I adjust adherence '2019-01-14 09:00' to '2019-01-14 12:00' as neutral adherence
	Then I should see adjusted period between '2019-01-14 09:00' and '2019-01-14 12:00'
	
  Scenario: Adjust adherence to neutral affecting historical adherence
	Given Ashley Andeen has a 'Phone' shift between '2019-01-14 09:00' and '17:00'
	And today is '2019-01-15'
	And at '09:00' 'Ashley Andeen' sets his phone state to 'LoggedOff'
	And at '10:00' 'Ashley Andeen' sets his phone state to 'Ready'
	And the period between '2019-01-14 09:00' and '2019-01-14 12:00' is adjusted to neutral
	When I view historical adherence for 'Ashley Andeen' on '2019-01-14'
	Then I should see adjusted period between '09:00:00' and '12:00:00'
	And I should not see any out of adherences
	And I should see neutral adherence between '09:00:00' and '12:00:00'
	And I should see adherence percentage of 100%
