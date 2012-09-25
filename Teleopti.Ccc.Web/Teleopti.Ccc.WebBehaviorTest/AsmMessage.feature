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

Scenario: Indicate new message while logged on
	Given I have the role 'Full access to mytime'
	When I am viewing week schedule
	And I receive a new message
	Then I should be notified that I have '1' new message(s)

Scenario: Indicate another new message while logged on
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field         | Value          |
	| Title         | New message	 |
	And I am viewing week schedule
	And I should be notified that I have '1' new message(s)
	When I receive a new message
	Then I should be notified that I have '2' new message(s)

Scenario: Indicate new message at logon
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field         | Value          |
	| Title         | New message	 |
	When I am viewing week schedule
	Then I should be notified that I have '1' new message(s)



Scenario: Navigate to message tab
	Given I have the role 'Full access to mytime'
	And I have no unread messages
	When I am viewing messages
	Then I should not see any messages

Scenario: Navigate to message tab with an unread message
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field         | Value          |
	| Title         | New message	 |
	When I am viewing messages
	Then I should see a message in the list

Scenario: Open unread message
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field         | Value          |
	| Title         | New message	 |
	And I am viewing messages
	When I click on the message
	Then I should see the message details

Scenario: Confirm message is read
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field         | Value          |
	| Title         | New message	 |
	And I am viewing messages
	And I click on the message at position '1' in the list
	When I click the confirm button
	Then I should not see any messages
	And message tab indicates 'no' new message(s)

Scenario: Order messages in list by date
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field         | Value			|
	| Title         | New message	|
	| Date			| 2030-10-03	|
	And I have an unread message with
	| Field         | Value					|
	| Title         | Another new message	|
	| Date			| 2030-10-05			|
	When I am viewing messages
	Then I should see the message with date '2030-10-05' at position '1' in the list
	And I should see the message with date '2030-10-03' at position '2' in the list

Scenario: Reduce number of unread messages in message tab title
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field         | Value			|
	| Title         | New message	|
	And I have an unread message with
	| Field         | Value					|
	| Title         | Another new message	|
	And message tab indicates '2' new message(s)
	When I am viewing messages
	And I confirm reading the message at position '1' in the list
	Then message tab indicates '1' new message(s)
	



	


