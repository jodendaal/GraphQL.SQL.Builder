using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using GraphQL.SQL.Builder;

namespace GraphQL.SQL.Tests
{
    [TestClass]
    public class SqlCommandBuilderTests
    {

        [TestMethod]
        public void Select_Join_With_Condition()
        {
            SelectQueryBuilder query = new SelectQueryBuilder("[dbo].[users]", "[UP]");
            query.Field("[UP].[UserId]", "[Id]").
                  Field("[UP].[username]", "[UN]").
                  Field("[UP].[password]", "[PWD]").
                  Join("[dbo].[Orders] [O]", JoinType.Inner, "[O].[UserId] = [UP].[UserId]").
                  Field("[O].[OrderId]", "[OrderId]").
                  Condition("[O].[OrderId]", ColumnOperator.Equals, "@OrderId");

            var expected =
@"SELECT
[UP].[UserId] AS [Id],
[UP].[username] AS [UN],
[UP].[password] AS [PWD],
[O].[OrderId] AS [OrderId]
FROM [dbo].[users] [UP]
INNER JOIN [dbo].[Orders] [O] ON [O].[UserId] = [UP].[UserId]
WHERE [O].[OrderId] = @OrderId";
            var sql = query.ToString();
            Assert.AreEqual(sql, expected);
        }


        [TestMethod]
        public void Select_ConditionSets()
        {
            SelectQueryBuilder query = new SelectQueryBuilder("[dbo].[users]", "[UP]");
            query.Field("[UP].[UserId]", "[Id]").
                  Field("[UP].[username]", "[UN]").
                  Field("[UP].[password]", "[PWD]").
                  Join("[dbo].[Orders] [O]", JoinType.Inner, "[O].[UserId] = [UP].[UserId]").
                  Field("[O].[OrderId]", "[OrderId]").
                  Condition("[O].[OrderId]", ColumnOperator.Equals, "@OrderId").
                  Condition("[UP].[password]", ColumnOperator.Equals, "@Password").
                  ConditionSet(1, SetOperator.And, (a) =>
                  {
                      a.AndCondition("[UP].[UserId]", ColumnOperator.Equals, "@UserId").
                        AndCondition("[UP].[UserId]", ColumnOperator.LessThan, "@UserId_2");
                  }).
                  ConditionSet(2, SetOperator.And, (a) =>
                  {
                      a.AndCondition("[UP].[UserId]", ColumnOperator.Equals, "@UserId_99").
                        AndCondition("[UP].[UserId]", ColumnOperator.LessThan, "@UserId_277");
                  });


            var expected =
@"SELECT
[UP].[UserId] AS [Id],
[UP].[username] AS [UN],
[UP].[password] AS [PWD],
[O].[OrderId] AS [OrderId]
FROM [dbo].[users] [UP]
INNER JOIN [dbo].[Orders] [O] ON [O].[UserId] = [UP].[UserId]
WHERE ([O].[OrderId] = @OrderId AND [UP].[password] = @Password) AND
(([UP].[UserId] = @UserId AND [UP].[UserId] < @UserId_2) AND
([UP].[UserId] = @UserId_99 AND [UP].[UserId] < @UserId_277))";
            var sql = query.ToString();
            Assert.AreEqual(sql, expected);
        }


        [TestMethod]
        public void Select_ConditionSets_Multiple()
        {
            SelectQueryBuilder query = new SelectQueryBuilder("[dbo].[users]");
            query.Field("[dbo].[users].[userId]").
                  Field("[dbo].[users].[username]").
                  Field("[dbo].[users].[password]").
                  ConditionSet(1, SetOperator.And, (a) =>
                  {
                      a.OrCondition("[dbo].[users].[userId]", ColumnOperator.Equals, "@userId").
                       OrCondition("[dbo].[users].[username]", ColumnOperator.Equals, "@username");

                  }).
                  ConditionSet(2, SetOperator.And, (a) =>
                  {
                      a.OrCondition("[dbo].[users].[userIdTwo]", ColumnOperator.Equals, "@userIdTwo").
                       OrCondition("[dbo].[users].[usernameTwo]", ColumnOperator.Equals, "@usernameTwo");
                  }).
                  ConditionSet(3, SetOperator.Or, (a) =>
                  {
                      a.AndCondition("[dbo].[users].[userIdThree]", ColumnOperator.Equals, "@userIdThree").
                       AndCondition("[dbo].[users].[usernameThree]", ColumnOperator.Equals, "@usernameThree");
                  });

            var sql = query.ToString();

            var expected =
@"SELECT
[dbo].[users].[userId],
[dbo].[users].[username],
[dbo].[users].[password]
FROM [dbo].[users]
WHERE (([dbo].[users].[userId] = @userId OR [dbo].[users].[username] = @username) AND
([dbo].[users].[userIdTwo] = @userIdTwo OR [dbo].[users].[usernameTwo] = @usernameTwo)) OR
([dbo].[users].[userIdThree] = @userIdThree AND [dbo].[users].[usernameThree] = @usernameThree)";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());
        }


        [TestMethod]

        public void Select_Paging_Paramters()
        {
            SelectQueryCommand query = new SelectQueryCommand("[dbo].[users]", "");
            query.Field("[dbo].[users].[userId]").
                  Field("[dbo].[users].[username]").
                  Field("[dbo].[users].[password]").
                  Condition("[dbo].[users].[userId]", ColumnOperator.Equals, "@userId").
                  Page(query.AddParam(1, "_PageNumber"), query.AddParam(10, "_PageSize"), "[dbo].[users].[userId]");

            var sql = query.ToString();

            foreach (var param in query.Parameters)
            {
                switch (param.Key)
                {
                    case "@_PageSize":
                        Assert.AreEqual(param.Value.Value, 10, "page paramter size incorrect");
                        break;
                    case "@_PageNumber":
                        Assert.AreEqual(param.Value.Value, 1, "page number size incorrect");
                        break;
                }
            }

            var expected =
@"SELECT
[dbo].[users].[userId],
[dbo].[users].[username],
[dbo].[users].[password]
FROM [dbo].[users]
WHERE [dbo].[users].[userId] = @userId
ORDER BY [dbo].[users].[userId]
OFFSET @_PageSize * (@_PageNumber - 1)
ROWS FETCH NEXT @_PageSize ROWS ONLY";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());
        }

        [TestMethod]
        public void Select_ConditionSet_Parameters()
        {
            SelectQueryCommand query = new SelectQueryCommand("[dbo].[users]");
            query.Field("[dbo].[users].[userId]").
                  Field("[dbo].[users].[username]").
                  Field("[dbo].[users].[password]").
                  ConditionSet(1, SetOperator.Or, (a) =>
                  {
                      a.AndCondition("[dbo].[users].[userId]", ColumnOperator.Equals, query.AddParam(1, "userId")).
                       AndCondition("[dbo].[users].[username]", ColumnOperator.Equals, query.AddParam("tim", "username")).
                       AndSetAndCondition("[dbo].[users].[password]", ColumnOperator.Equals, query.AddParam("123", "password")).
                       AndSetAndCondition("[dbo].[users].[username]", ColumnOperator.Equals, query.AddParam("connor", "username_1_2")).
                       OrSetAndCondition("[dbo].[users].[password]", ColumnOperator.Equals, query.AddParam("6745645", "password_0_3")).
                       OrSetAndCondition("[dbo].[users].[username]", ColumnOperator.Equals, query.AddParam("6745645", "username_1_3"));
                  });

            var sql = query.ToString();

            foreach (var param in query.Parameters)
            {
                switch (param.Key)
                {
                    case "@password":
                        Assert.AreEqual("123", param.Value.Value, "password incorrect");
                        break;
                    case "@userId":
                        Assert.AreEqual(1, param.Value.Value, "userId incorrect");
                        break;
                    case "@username":
                        Assert.AreEqual("tim", param.Value.Value, "username incorrect");
                        break;
                    case "@username_1_2":
                        Assert.AreEqual("connor", param.Value.Value, "username_1_2 incorrect");
                        break;
                    case "@username_1_3":
                        Assert.AreEqual(param.Value.Value, "6745645", "_username_1_3 incorrect");
                        break;
                }
            }

            var expected =
@"SELECT
[dbo].[users].[userId],
[dbo].[users].[username],
[dbo].[users].[password]
FROM [dbo].[users]
WHERE (([dbo].[users].[userId] = @userId AND [dbo].[users].[username] = @username) AND
([dbo].[users].[password] = @password AND [dbo].[users].[username] = @username_1_2)) OR
([dbo].[users].[password] = @password_0_3 AND [dbo].[users].[username] = @username_1_3)";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());
        }



        [TestMethod]
        public void Select_Parameter_Names_AutoGenerate()
        {
            SelectQueryCommand query = new SelectQueryCommand("[dbo].[users]");
            query.Field("[dbo].[users].[userId]").
                  Field("[dbo].[users].[username]").
                  Field("[dbo].[users].[password]").
                  ConditionSet(1, SetOperator.Or, (a) =>
                  {
                      a.AndCondition("[dbo].[users].[userId]", ColumnOperator.Equals, query.AddParam(1, "userId")).
                        AndCondition("[dbo].[users].[username]", ColumnOperator.Equals, query.AddParam(2, "userId")).
                        AndCondition("[dbo].[users].[username]", ColumnOperator.Equals, query.AddParam(3, "userId")).
                        AndCondition("[dbo].[users].[username]", ColumnOperator.Equals, query.AddParam(6, "userId"));
                  });

            var sql = query.ToString();

            foreach (var param in query.Parameters)
            {
                switch (param.Key)
                {
                    case "@userId":
                        Assert.AreEqual(1, param.Value.Value, "userId incorrect");
                        break;
                    case "@userId_1":
                        Assert.AreEqual(2, param.Value.Value, "userId_1 incorrect");
                        break;
                    case "@userId_2":
                        Assert.AreEqual(3, param.Value.Value, "userId_2 incorrect");
                        break;
                    case "@userId_3":
                        Assert.AreEqual(6, param.Value.Value, "userId_3 incorrect");
                        break;
                }
            }

            var expected =
@"SELECT
[dbo].[users].[userId],
[dbo].[users].[username],
[dbo].[users].[password]
FROM [dbo].[users]
WHERE [dbo].[users].[userId] = @userId AND [dbo].[users].[username] = @userId_1 AND [dbo].[users].[username] = @userId_2 AND [dbo].[users].[username] = @userId_3";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());
        }

        [TestMethod]
        public void Select_OrderBy()
        {
            SelectQueryCommand query = new SelectQueryCommand("[dbo].[users]");
            query.Field("[dbo].[users].[userId]").
                  OrderBy("[dbo].[users].[userId]", "ASC");
            var sql = query.ToString();
            var expected =
@"SELECT
[dbo].[users].[userId]
FROM [dbo].[users]
ORDER BY [dbo].[users].[userId] ASC
";
            Assert.AreEqual(expected.ToLower(), sql.ToLower());
        }

        [TestMethod]
        public void Select_OrderBy_With_Paging()
        {
            SelectQueryCommand query = new SelectQueryCommand("[dbo].[users]");
            query.Field("[dbo].[users].[userId]").
                  OrderBy("[dbo].[users].[userId]", "ASC").
                  Page(query.AddParam(1, "_page"), query.AddParam(10, "_pageSize"));


            var sql = query.ToString();


            var expected =
@"SELECT
[dbo].[users].[userId]
FROM [dbo].[users]
ORDER BY [dbo].[users].[userId] ASC
OFFSET @_pageSize * (@_page - 1)
ROWS FETCH NEXT @_pageSize ROWS ONLY";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());

        }

        [TestMethod]
        public void Select_Count()
        {
            SelectQueryCommand query = new SelectQueryCommand("[dbo].[users]");
            query.Count("*").
                  OrderBy("[dbo].[users].[userId]", "ASC").
                  Page(query.AddParam(1, "_page"), query.AddParam(10, "_pageSize"));


            var sql = query.ToString();


            var expected =
@"SELECT
COUNT(*)
FROM [dbo].[users]
ORDER BY [dbo].[users].[userId] ASC
OFFSET @_pageSize * (@_page - 1)
ROWS FETCH NEXT @_pageSize ROWS ONLY";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());

        }

        [TestMethod]
        public void Select_Avg()
        {
            var query = new SelectQueryCommand("Orders");
            query.Avg("OrderId", "AvgOrderId").
                  Field("customerId").
                  OrderBy("CustomerId", "ASC").
                  GroupBy("CustomerId").
                  Page(query.AddParam(1, "_page"), query.AddParam(10, "_pageSize"));


            var sql = query.ToString();


            var expected =
@"SELECT
AVG(OrderId) AS AvgOrderId,
customerId
FROM Orders
GROUP BY CustomerId
ORDER BY CustomerId ASC
OFFSET @_pageSize * (@_page - 1)
ROWS FETCH NEXT @_pageSize ROWS ONLY";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());

        }

        [TestMethod]
        public void Select_Min()
        {
            SelectQueryCommand query = new SelectQueryCommand("Orders");
            query.Min("OrderId", "MinOrderId").
                  Field("customerId").
                  OrderBy("CustomerId", "ASC").
                  GroupBy("CustomerId").
                  Page(query.AddParam(1, "_page"), query.AddParam(10, "_pageSize"));


            var sql = query.ToString();


            var expected =
@"SELECT
MIN(OrderId) AS MinOrderId,
customerId
FROM Orders
GROUP BY CustomerId
ORDER BY CustomerId ASC
OFFSET @_pageSize * (@_page - 1)
ROWS FETCH NEXT @_pageSize ROWS ONLY";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());

        }

        [TestMethod]
        public void Select_Max()
        {
            SelectQueryCommand query = new SelectQueryCommand("Orders");
            query.Max("OrderId", "MaxOrderId").
                  Field("customerId").
                  OrderBy("CustomerId", "ASC").
                  GroupBy("CustomerId").
                  Page(query.AddParam(1, "_page"), query.AddParam(10, "_pageSize"));


            var sql = query.ToString();


            var expected =
@"SELECT
MAX(OrderId) AS MaxOrderId,
customerId
FROM Orders
GROUP BY CustomerId
ORDER BY CustomerId ASC
OFFSET @_pageSize * (@_page - 1)
ROWS FETCH NEXT @_pageSize ROWS ONLY";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());

        }

        [TestMethod]
        public void Select_Sum()
        {
            SelectQueryCommand query = new SelectQueryCommand("Orders");
            query.Sum("OrderId", "SumOrderId").
                  Field("customerId").
                  OrderBy("CustomerId", "ASC").
                  GroupBy("CustomerId").
                  Page(query.AddParam(1, "_page"), query.AddParam(10, "_pageSize"));


            var sql = query.ToString();


            var expected =
@"SELECT
SUM(OrderId) AS SumOrderId,
customerId
FROM Orders
GROUP BY CustomerId
ORDER BY CustomerId ASC
OFFSET @_pageSize * (@_page - 1)
ROWS FETCH NEXT @_pageSize ROWS ONLY";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());

        }

        [TestMethod]
        public void Select_Paging()
        {
            SelectQueryCommand query = new SelectQueryCommand("[dbo].[users]", "");
            query.Field("[dbo].[users].[userId]").
                  Field("[dbo].[users].[username]").
                  Field("[dbo].[users].[password]").
                  Condition("[dbo].[users].[userId]", ColumnOperator.Equals, "@userId").
                  Page(query.AddParam(1, "_PageNumber"), query.AddParam(10, "_PageSize"), "[dbo].[users].[userId]");

            var sql = query.ToString();

            foreach (var param in query.Parameters)
            {
                switch (param.Key)
                {
                    case "@_PageSize":
                        Assert.AreEqual(param.Value.Value, 10, "page paramter size incorrect");
                        break;
                    case "@_PageNumber":
                        Assert.AreEqual(param.Value.Value, 1, "page number size incorrect");
                        break;
                }
            }

            var expected =
@"SELECT
[dbo].[users].[userId],
[dbo].[users].[username],
[dbo].[users].[password]
FROM [dbo].[users]
WHERE [dbo].[users].[userId] = @userId
ORDER BY [dbo].[users].[userId]
OFFSET @_PageSize * (@_PageNumber - 1)
ROWS FETCH NEXT @_PageSize ROWS ONLY";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());
        }
    }
}
