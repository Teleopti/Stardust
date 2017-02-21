--deny all new requests if they are stuck when removing NewAbsenceRequestHandler
update dbo.PersonRequest set RequestStatus = 1 where RequestStatus = 3
