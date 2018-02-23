﻿@RTA
@Ignore

Feature: Remove approved out of adherences
  As a Manager I need to remove approved hours of Out of Adherence for a certain day and agent,
  so that the daily percentage adherence number is adjusted.

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
	
  Scenario: Remove approved out of adherences
	Given Mikkey Dee has a 'Phone' shift between '2018-02-22 10:00' and '20:00'
	And at '2018-02-22 10:00:00' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And at '2018-02-22 11:00:00' 'Mikkey Dee' sets his phone state to 'Ready'
	Given the time is '2018-02-22 21:00:00'
	When I view historical adherence for 'Mikkey Dee' on '2018-02-22'
	And I approve out of adherence starting at '10:00:00' as in adherence
	Then I should see approved period between '10:00:00' and '11:00:00'
	Given the time is '2018-02-22 21:05:00'
	When I remove approved period between '10:00:00' and '11:00:00'
	Then I should see adherence percentage of 90%
	And I should see recorded out of adherence between '09:00:00' and '10:00:00'