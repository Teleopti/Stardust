﻿@WFM
Feature: Staffing
	In order to manage staffing
	As an admin
	I want to see staffing for skills

Background:
Given I have a role with
| Field              | Value |
| Access to Staffing | True  |
And I have a role with full access
And There is a skill to monitor called 'Skill A' with queue id '1' and queue name 'queue1' and activity 'Phone'
And There is a skill to monitor called 'Skill B' with queue id '2' and queue name 'queue2' and activity 'Chat'
And There is a compensation with name 'Comparera'


Scenario: View staffing for one skillgroup
	Given the time is '2017-09-25 08:00'
	And there is queue statistics for skill 'Skill A' until '2017-09-27 17:00'
	And there is staffing data for skills 'Skill A' for date '2017-09-25'
	And there is queue statistics for skill 'Skill B' until '2017-09-27 17:00'
	And there is staffing data for skills 'Skill B' for date '2017-09-25'
	And there is a skill group with name 'Group A' with skills 'Skill A, Skill B'
	When I am viewing staffing page
	And I select skill group 'Group A'
	And I change staffing date to '2017-09-25'
	Then I should see the selected skill group 'Group A' 
	
Scenario: Show bpo exchange page
	Given I am viewing staffing page
	When I press the BPO exchange button 
	Then I should see the bpo exchange page

Scenario: Show manage skill group page
	Given I am viewing staffing page
	When I press the create skill button 
	Then I should see the manage skill group page

#Shrinkage 
