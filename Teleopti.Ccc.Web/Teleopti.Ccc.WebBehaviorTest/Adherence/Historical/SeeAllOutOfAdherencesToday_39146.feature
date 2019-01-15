@RTA
Feature: See all out of adherences for today
  In order to ...
  As a real time analyst
  I want to see all OoA that has happened today for one agent
  so that I can act upon it eventhough I was in a meeting when it happened and was obviously not looking at the screen,
  so that I free up time and don't have to look at the screen all day,
  so that I can act on OoA today not only shortly after it occured.

  Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is an activity named 'Lunch'
	And there is a site named 'Paris'
	And there is a team named 'Justin' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	  | Field      | Value      |
	  | Team       | Justin     |
	  | Start Date | 2014-01-21 |
	And Pierre Baldi has a shift with
	  | Field                    | Value            |
	  | Start time               | 2016-10-11 09:00 |
	  | End time                 | 2016-10-11 17:00 |
	  | Activity                 | Phone            |
	  | Next activity            | Lunch            |
	  | Next activity start time | 2016-10-11 11:00 |
	  | Next activity end time   | 2016-10-11 12:00 |
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
	  | Phone state | Pause        |
	And there is a rule with
	  | Field       | Value    |
	  | Name        | Positive |
	  | Adherence   | Out      |
	  | Activity    |          |
	  | Phone state | Ready    |


  Scenario: See out of adherences
	Given the time is '2016-10-11 08:30:00'
	And 'Pierre Baldi' sets his phone state to 'Ready'
	And at '2016-10-11 08:45:00' 'Pierre Baldi' sets his phone state to 'Pause'
	And at '2016-10-11 09:00:00' 'Pierre Baldi' sets his phone state to 'Ready'
	And at '2016-10-11 10:00:00' 'Pierre Baldi' sets his phone state to 'Pause'
	And at '2016-10-11 10:20:00' 'Pierre Baldi' sets his phone state to 'Ready'
	When I view historical adherence for 'Pierre Baldi' on '2016-10-11'
	Then I should see out of adherences
	  | Start time | End time |
	  | 08:30:00   | 08:45:00 |
	  | 10:00:00   | 10:20:00 |
	And I should see activities
	  | Start time          | End time            |
	  | 2016-10-11 09:00:00 | 2016-10-11 11:00:00 |
	  | 2016-10-11 11:00:00 | 2016-10-11 12:00:00 |
	  | 2016-10-11 12:00:00 | 2016-10-11 17:00:00 |