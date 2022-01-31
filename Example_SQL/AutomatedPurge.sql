declare @tableCursor cursor,
@tableName varchar(100),
@amountOfDaysToKeep varchar(50),
@sql nvarchar(500),
@amountOfRecordsDeleted int

set @tableCursor = cursor for select tableName from [Ctrl_PurgeControl]

open @tableCursor
fetch next from @tableCursor into @tableName
while(@@FETCH_STATUS = 0)
begin

-- delete from tables
set @amountOfDaysToKeep = cast((select AmountOfDaysToKeep from [Ctrl_PurgeControl] where TableName = @tableName) as varchar(50))
set @sql = 'delete from '+ @tableName+' where DATEDIFF(day,LoadDateTime,GETDATE()) > '+@amountOfDaysToKeep

--execute @sql
exec sp_executesql @sql
set @amountOfRecordsDeleted = @@ROWCOUNT

--Record how many records were deleted
if @amountOfRecordsDeleted > 0
insert into [Ctrl_PurgeHistory] (TableName, PurgeDate, AmountOfRecordsDeleted) values (@tableName, cast(GETDATE() as datetime2), @amountOfRecordsDeleted)

fetch next from @tableCursor into @tableName
end
close @tableCursor
deallocate @tableCursor