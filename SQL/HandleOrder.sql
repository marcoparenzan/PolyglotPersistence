delete from SalesLT.SalesOrderHeader WHERE SalesOrderId >=71947
delete from SalesLT.SalesOrderDetail WHERE SalesOrderId >=71947

GO

select *
from SalesLT.SalesOrderHeader
where OrderDate >= '2016-12-01'

select *
from SalesLT.SalesOrderDetail d
JOIN SalesLT.SalesOrderHeader h on d.SalesOrderId = h.SalesOrderId
where OrderDate>= '2016-12-01'


