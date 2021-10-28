# GraphQL.SQL.Builder
[![.NET](https://github.com/jodendaal/GraphQL.SQL.Builder/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/jodendaal/GraphQL.SQL.Builder/actions/workflows/dotnet.yml)

SQL Query Builder Utility

Useful for scenarios where dynamic SQL is required. Supports multiple condition set logic.

Generate Select, Insert, Update and Delete statements with parameters.

Additional features include : Paging, Advanced Condition Sets, Auto Parameter Naming

Currently used in  [GraphQL.SQL](https://github.com/jodendaal/GraphQL.SQL) for dynamically generating SQL statements.

## Getting Started

#### Install Package
```
Install-Package GraphQL.SQL.Builder
```

## Simple Usage
   ```csharp
    public DataTable GetUser(int userId)
    {
        var query = new SelectQueryCommand("Users", "U");
        query.Field("UserId", "Id").
              Field("UserName").
              Field("Password").
              Condition("U.UserId", ColumnOperator.Equals, query.AddParam(userId,"UserId"));
        var sqlCommand = query.ToCommand();

        var table = new DataTable();
        using (var connection = new SqlConnection("connection_string"))
        {
            connection.Open();
            sqlCommand.Connection = connection;

            using (var dataAdapter = new SqlDataAdapter(sqlCommand))
            {
                dataAdapter.Fill(table);
            }
        }

        return table;
    }
   ```
##### Output
```sql
    SELECT
          UserId AS Id,
          UserName,
          Password
    FROM Users U
    WHERE U.UserId = @UserId
    
    --Parameters
    @UserId=1
```
## Paging
```csharp
    SelectQueryCommand query = new SelectQueryCommand("Users");
    query.Field("UserId").
          Field("UserName").
          Condition("UserId", ColumnOperator.Equals, query.AddParam(1,"UserId")).
          Page(query.AddParam(1, "_PageNumber"), query.AddParam(10, "_PageSize"), "UserId");
```    
##### Output
```sql
    SELECT
        UserId,
        UserName
    FROM Users
    WHERE UserId = @UserId
    ORDER BY UserId
    OFFSET @_PageSize * (@_PageNumber - 1)
    ROWS FETCH NEXT @_PageSize ROWS ONLY

    --Parameters
    @_PageNumber=1,@_PageSize=10,@UserId=1
```
## Condition Sets Usage
```csharp
    //Find users who are admins and username is either tim or connor
      SelectQueryCommand query = new SelectQueryCommand("Users");
      query.Field("UserId").
            Field("UserName").
            Field("IsAdmin").
            Condition("IsAdmin", ColumnOperator.Equals, query.AddParam(true,"IsAdmin")).
            ConditionSet(1, SetOperator.And, (set) =>
            {
                set.OrCondition("UserName", ColumnOperator.Equals, query.AddParam("tim")).
                OrCondition("UserName", ColumnOperator.Equals, query.AddParam("connor"));
            });
```    
##### Output
```sql
    SELECT
         UserId,
         UserName,
         IsAdmin
     FROM Users
     WHERE (IsAdmin = @IsAdmin) AND (UserName = @p_1 OR UserName = @p_2)

    --Parameters
    @IsAdmin=1,@p_1='tim',@p_2='connor'
```
## Multiple Condition Sets

```csharp  
    //(Find users who are admins and username is either tim or connor) and password='password'
    SelectQueryCommand query = new SelectQueryCommand("Users");
    query.Field("UserId").
            Field("UserName").
            Field("IsAdmin").
            Field("Password").
            ConditionSet(1, SetOperator.And, (set) =>
            {
                set.AndCondition("IsAdmin", ColumnOperator.Equals, query.AddParam(true, "IsAdmin")).
                OrCondition("UserName", ColumnOperator.Equals, query.AddParam("tim")).
                OrCondition("UserName", ColumnOperator.Equals, query.AddParam("connor"));
            }).
            ConditionSet(2, SetOperator.And, (set) =>
            {
                set.OrCondition("Password", ColumnOperator.Equals, query.AddParam("password")).
                    OrCondition("Password", ColumnOperator.Equals, query.AddParam("Test123")).
            });
```    
##### Output
```sql
    SELECT
        UserId,
        UserName,
        IsAdmin,
        Password
    FROM Users
    WHERE (((IsAdmin = @IsAdmin) AND (UserName = @p_1 OR UserName = @p_2))) AND (Password = @p_3 or Password=@p_4)
                
    --Parameters
    @IsAdmin=1,@p_1='tim',@p_2='connor',@p_3='password'
```
## Select
```csharp
    var query = new SelectQueryBuilder("Users", "U");
    query.Field("UserId", "Id").
          Field("UserName").
          Field("Password").
          Condition("U.UserId", ColumnOperator.Equals, "1");
```
##### Output
```sql
    SELECT
          UserId AS Id,
          UserName,
          Password
    FROM Users U
    WHERE U.UserId = 1
```
## Insert
```csharp
    var insert = new InsertQueryBuilder("Users");
        insert.Field("UserId", insert.AddParam("1")).
               Field("Password", insert.AddParam("test123"));
```
##### Output
```sql
    INSERT INTO Users
    (
        UserId,
        Password
    )
    VALUES
    (
        @p_0,
        @p_1
    )
```

## Insert From Select
```csharp
    var insert = new InsertQueryBuilder("Users");
        insert.Field("UserId").
                Field("Password").
                From("Users_Backup", "UB", (query) =>
                {
                    query.Field("UB.UserId").
                            Field("UB.Password").
                            Condition("UB.UserId", ColumnOperator.Equals, insert.AddParam(1, "UserId"));
                });
```
##### Output
```sql
    INSERT INTO Users
    (
        UserId,
        Password
    )
    SELECT
        UB.UserId,
        UB.Password
    FROM Users_Backup UB
    WHERE UB.UserId = @UserId
```

## Update
```csharp
    var update = new UpdateQueryBuilder("Users");
        update.Field("UserId", update.AddParam(10, "NewUserId")).
               Field("Password", update.AddParam("3423", "Password")).
               Condition("UserId", ColumnOperator.Equals, update.AddParam("1", "UserId"));
```
##### Output
```sql
    UPDATE Users
    SET UserId=@NewUserId,
        Password=@Password
    WHERE UserId = @UserId
```

## Update from Join
```csharp
     var update = new UpdateQueryBuilder("Users","U");
         update.Join("User_Backup UB", JoinType.Inner, "UB.UserId=U.UserId").
                Field("Password", "UB.Password").
                Condition("U.UserId", ColumnOperator.Equals, update.AddParam("1", "UserId"));
```
##### Output
```sql
    UPDATE U
        SET Password=UB.Password
    FROM Users U
    INNER JOIN User_Backup UB ON UB.UserId=U.UserId
    WHERE U.UserId = @UserId
```

## Join
```csharp
    var query = new SelectQueryBuilder("Users", "U");
    query.Field("U.UserId", "Id").
          Field("U.UserName").
          Field("U.Password").
          Join("Preferences P",JoinType.Inner,"P.UserId = U.UserId").
          Field("P.Theme").
          Condition("U.UserId", ColumnOperator.Equals, "1");
```
##### Output
```sql
    SELECT
          U.UserId AS Id,
          U.UserName,
          U.Password,
          P.Theme
    FROM Users U
    INNER JOIN Preferences P ON P.UserId = U.UserId
    WHERE U.UserId = 1
```
## Count
```csharp
    var query = new SelectQueryBuilder("Users", "U");
    query.Count("*", "[RecordCount]").
          Condition("U.UserId", ColumnOperator.Equals, "1");
```
##### Output
```sql
    SELECT
        COUNT(*) AS [RecordCount]
    FROM Users U
    WHERE U.UserId = 1
```    
