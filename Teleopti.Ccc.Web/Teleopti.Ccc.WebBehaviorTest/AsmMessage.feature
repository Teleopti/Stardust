Feature: ASM Message
	In order to communicate with supervisors
	As an agent
	I want to receive and send information

Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there is a role with
	| Field         | Value            |
	| Name          | No access to ASM |
	| Access To Asm | False            |
	
Scenario: Do not show message tab if no permission to ASM
	Given I have the role 'No access to ASM'
	When I am viewing week schedule
	Then Message tab should not be visible 

Scenario: Show message tab 
	Given I have the role 'Full access to mytime'
	When I am viewing week schedule
	Then Message tab should be visible

Scenario: Indicate when new message while logged on
	Given I have the role 'Full access to mytime'
	When I am viewing week schedule
	And I receive a new message
	Then I should be notified that I have a new message

Scenario: Indicate new message at logon
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field         | Value          |
	| Title         | New message	 |
	When I am viewing week schedule
	Then I should be notified that I have a new message
