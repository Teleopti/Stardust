update [dbo].[ApplicationFunction]
  set [IsDeleted] = 1
  where  [FunctionCode] = 'Permission'