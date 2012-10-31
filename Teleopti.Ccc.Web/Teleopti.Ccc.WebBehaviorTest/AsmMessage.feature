﻿Feature: ASM Message
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
	And I am viewing week schedule
	When I receive message number '1' while not viewing message page
	Then I should be notified that I have '1' unread message(s)

Scenario: Indicate another new message while logged on
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field         | Value        |
	| Title         | New message	 |
	And I am viewing week schedule
	And I should be notified that I have '1' unread message(s)
	When I receive message number '2' while not viewing message page
	Then I should be notified that I have '2' unread message(s)

Scenario: Indicate new message at logon
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field         | Value        |
	| Title         | New message	 |
	When I am viewing week schedule
	Then I should be notified that I have '1' unread message(s)

Scenario: Navigate to message tab
	Given I have the role 'Full access to mytime'
	And I have no unread messages
	And I am viewing week schedule
	When I navigate to messages
	Then I should not see any messages
	And I should see a user-friendly message explaining I dont have any messages

Scenario: View unread messages
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field         | Value        |
	| Title         | New message	 |
	When I am viewing messages
	Then I should see '1' message(s) in the list

Scenario: Open unread message
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field		| Value				|
	| Title		| New message		|
	| Message	| Text in message	|	
	When I am viewing messages
	And I click on the message at position '1' in the list
	Then I should see the message details form with
	| Field		| Value				|
	| Title		| New message		|
	| Message	| Text in message	|	

Scenario: Confirm message is read
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field         | Value          |
	| Title         | New message	 |
	When I am viewing messages
	And I click on the message at position '1' in the list
	When I click the confirm button
	Then I should not see any messages
	And I should see a user-friendly message explaining I dont have any messages

Scenario: Sort messages in list by latest message
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field				| Value			|
	| Title				| Message		|
	| Is oldest message	| True			|
	And I have an unread message with
	| Field         | Value				|
	| Title         | Latest message	|
	When I am viewing messages
	Then I should see the message with title 'Latest message' at position '1' in the list
	And I should see the message with title 'Message' at position '2' in the list

Scenario: Reduce number of unread messages in message tab title
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field         | Value			|
	| Title         | New message	|
	And I have an unread message with
	| Field         | Value					|
	| Title         | Another new message	|
	And I am viewing week schedule
	And I should be notified that I have '2' unread message(s)
	When I navigate to messages
	And I confirm reading the message at position '1' of '2' in the list
	Then I should be notified that I have '1' unread message(s)

Scenario: Receive a new message when viewing message page
	Given I have the role 'Full access to mytime'
	And I am viewing messages
	When I receive message number '1'
	Then I should see '1' message(s) in the list

Scenario: Open unread message where text reply is allowed
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field					| Value				|
	| Title					| New message		|
	| Message				| Text in message	|
	| Text reply allowed	| True				|
	And I am viewing messages
	When I click on the message at position '1' in the list
	Then I should see the message details form with an editable text box

Scenario: See reply dialogue in message text
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field					| Value						|
	| Title					| Work late					|
	| Message				| Can u work late today?	|
	| Text reply allowed	| True						|
	| My reply				| Ok if you buy me dinner?  |
	| Senders reply			| It´s a deal!				|
	And I am viewing messages
	When I click on the message at position '1' in the list
	Then I should see this conversation
	| Messages					|
	| Ok if you buy me dinner?  |
	| It´s a deal!				|

Scenario: Do not allow empty reply
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field					| Value				|
	| Title					| New message		|
	| Message				| Text in message	|
	| Text reply allowed	| True				|
	And I am viewing messages
	When I click on the message at position '1' in the list
	Then the send button should be disabled

Scenario: Send text reply message
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field					| Value				|
	| Title					| New message		|
	| Message				| Text in message	|
	| Text reply allowed	| True				|
	And I am viewing messages
	And I click on the message at position '1' in the list
	When I enter the text reply 'my reply'
	And I click the confirm button
	Then I should not see any messages
	And I should see a user-friendly message explaining I dont have any messages
 
Scenario: Show replyoptions for message with multiple options
Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field					| Value				|
	| Title					| New message		|
	| Message				| Text in message	|
	| Text reply allowed	| False				|
	| ReplyOption1			| Yes				|
	| ReplyOption2			| No				|
	And I am viewing messages
	When I click on the message at position '1' in the list
	Then I should see radiobuttons with 
	| Captions	|
	| Yes		|
	| No		|

Scenario: Confirm message with multiple replyoptions
Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field        | Value					|
	| Title        | Ashley is ill			|
	| Message      | Can you work tomorrow?	|
	| ReplyOption1 | Probably				|
	| ReplyOption2 | Probably not			|
	| ReplyOption3 | Defenitly not			|
	And I am viewing messages
	When I click on the message at position '1' in the list
	And I click the radiobutton with caption 'Probably not'
	And I click the confirm button
	Then I should not see any messages

Scenario: Enable confirmbutton when user has selected a replyoption
Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field					| Value				|
	| Title					| New message		|
	| Message				| Text in message	|
	| Text reply allowed	| False				|
	| ReplyOption1			| Yes				|
	| ReplyOption2			| No				|
	And I am viewing messages
	When I click on the message at position '1' in the list
	And I click the radiobutton with caption 'No'
	Then the send button should be enabled
 
Scenario: Confirmbutton should be disabled when user hasnt selected a replyoption
Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field					| Value				|
	| Title					| New message		|
	| Message				| Text in message	|
	| Text reply allowed	| False				|
	| ReplyOption1			| Yes				|
	| ReplyOption2			| No				|
	And I am viewing messages
	When I click on the message at position '1' in the list
	Then the send button should be disabled

Scenario: User can only select one replyoption
Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field					| Value				|
	| Title					| New message		|
	| Message				| Text in message	|
	| Text reply allowed	| False				|
	| ReplyOption1			| Yes				|
	| ReplyOption2			| No				|
	And I am viewing messages
	When I click on the message at position '1' in the list
	And I click the radiobutton with caption 'No'
	And I click the radiobutton with caption 'Yes'
	Then the radiobutton with caption 'No' should not be checked

Scenario: Do not show replyoptions if there is only one option
Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field					| Value				|
	| Title					| New message		|
	| Message				| Text in message	|
	| Text reply allowed	| False				|
	| ReplyOption1			| Ok					|
	And I am viewing messages
	When I click on the message at position '1' in the list
	Then I should not see any options
	And the send button should be enabled
	