delete o from [Order Details] o
inner join Orders OO on OO.OrderID = O.OrderID
where o.OrderID = 9999