Feature: My Profile
	In order to 
	to see if I have given a specific language or default
	see date formats I can understand
	to be able to read text on the site
	keep my account safe

	As an agent

	I want to 
	be able to view my language
	be able to change culture
	be able to change language
	be able to change my password

Scenario: See my profile
	Given I am an agent
	When I view my profile
	Then I should see my culture
	And I should see my language



Scenario: Change my culture
	Given I am an agent
	And I am swedish
	When I sign in
	Then I should see swedish date format
	When I change culture to US
	Then I should see US date format

Scenario: Browser's default culture
	Given I am an agent without culture
	When I sign in
	Then I should see the browser's language's date format

Scenario: Change my culture to computer's default
	Given I am an agent
	And I am swedish
	When I change culture to browser's default
	Then I should see the browser's language's date format



Scenario: Change my language
	Given I am an agent
	And I am swedish
	When I sign in
	Then I should see swedish text
	When I change language to english
	Then I should see english text

Scenario: Browser's default language
	Given I am an agent without language
	When I sign in
	Then I should see text in the the browser's language 

Scenario: Change my language to computer's default
	Given I am an agent
	And I am swedish
	When I change language to browser's default
	Then I should see text in the the browser's language 



Scenario: Change my password
	Given I am an agent
	When I change my password
	And I sign out
	And I sign in using my new password
	Then I should be signed in
	
Scenario: Continue browsing after changing password
	Given I am an agent
	When I change my password
	Then I should be able to continue browsing the site

Scenario: Incorrect current password when changing password
	Given I am an agent
	When I am changing password
	And I give incorrect current password
	Then I should see a message saying the password is incorrect
	
Scenario: Incorrect confirming password when changing password
	Given I am an agent
	When I am changing password
	And I give incorrect confirming password
	Then I should see a message saying the password is not confirmed correctly

