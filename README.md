# GraphQL.SQL.Builder
[![.NET](https://github.com/jodendaal/GraphQL.SQL.Builder/actions/workflows/dotnet.yml/badge.svg)](https://github.com/jodendaal/GraphQL.SQL.Builder/actions/workflows/dotnet.yml)

SQL Query Builder Utility

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

## Example Usage

### Select

    var query = new SelectQueryBuilder("Users", "U");
    query.Field("UserId", "Id").
          Field("UserName").
          Field("Password").
          Condition("U.UserId", ColumnOperator.Equals, "1");

##### Output
    SELECT
          UserId AS Id,
          UserName,
          Password
    FROM Users U
    WHERE U.UserId = 1

### Join

    var query = new SelectQueryBuilder("Users", "U");
    query.Field("U.UserId", "Id").
          Field("U.UserName").
          Field("U.Password").
          Join("Preferences P",JoinType.Inner,"P.UserId = U.UserId").
          Field("P.Theme").
          Condition("U.UserId", ColumnOperator.Equals, "1");

##### Output
    SELECT
          U.UserId AS Id,
          U.UserName,
          U.Password,
          P.Theme
    FROM Users U
    INNER JOIN Preferences P ON P.UserId = U.UserId
    WHERE U.UserId = 1

### Count

    var query = new SelectQueryBuilder("Users", "U");
    query.Count("*", "[RecordCount]").
          Condition("U.UserId", ColumnOperator.Equals, "1");

##### Output
    SELECT
        COUNT(*) AS [RecordCount]
    FROM Users U
    WHERE U.UserId = 1