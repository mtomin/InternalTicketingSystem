# InternalTicketingSystem

This is a small WPF app allowing users to create tickets describing a certain problem and admins to close them. The user personal information, user IT info (username, password etc.) and tickets are saved in a SQL database.

## Login

The passwords are salted and hashed using PBKDF2. The current database cotains one admin and two users with login data of admin/admin, user/user and user2/user2 as well as one test ticket issued by user. Logging out returns the user to the Login window.

![alt text](https://i.imgur.com/bJ3C6dr.jpg "Login window")

## User screen
![alt text](https://i.imgur.com/vRrHwPQ.png "User window")
Once logged in, user can:
  - See his currently opened tickets
  - Submit another ticket
  - Log out

Opened tickets are displayed along with some basic information in the lower left corner, while ticket submission adds amother ticket to the database. 

## Admin screen

![alt text](https://i.imgur.com/9li2YNY.jpg "Admin window")

Admin can:
- See open tickets by all users
- Add new users
- Access the list of all registered users (not admins!) by clicking the "Load users" button
- Remove a user (and choose to keep the "orphaned" tickets opened by that user or purge them too)
- Log out

### Dependencies

InternalTicketingSystem uses [Dapper](https://github.com/StackExchange/Dapper) for database access.
Password hashing uses built-in cryptographic services.

### Todos

 - Add the ability to inspect closed tickets
 - Add messaging between admins and users for reminders/followup

License
----

MIT

**Free Software, Hell Yeah!**