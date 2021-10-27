# GraphQL.SQL.Builder
[![.NET](https://github.com/jodendaal/GraphQL.SQL.Builder/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/jodendaal/GraphQL.SQL.Builder/actions/workflows/dotnet.yml)

SQL Query Builder Utility

Usefull for scenarious where dynamic SQL is required. Supports complex set logic.

## Supported Queries
-   [x] Select
-   [x] Insert
-   [x] Update
-   [x] Delete
-   [x] Paging

## Aggregates
-   [x] Count 
-   [x] Min
-   [x] Max
-   [x] Sum
-   [x] Avg

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
