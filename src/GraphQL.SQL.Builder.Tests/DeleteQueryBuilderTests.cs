using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using GraphQL.SQL.Builder;
using System.Data.SqlClient;
using System.Data;

namespace GraphQL.SQL.Tests
{
    [TestClass]
    public class DeleteQueryBuilderTests
    {
        [TestMethod]
        public void Delete()
        {
            var delete = new DeleteQueryBuilder("Users");
            delete.Condition("UserId", ColumnOperator.Equals, delete.AddParam("1", "UserId"));


            var sql = delete.ToString();

            var expected =
@"DELETE FROM Users
WHERE UserId = @UserId";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());
        }

        [TestMethod]
        public void Delete_Join_Condition()
        {
            var delete = new DeleteQueryBuilder("Users","U");
            delete.Join("User_Backup UB", JoinType.Inner, "UB.UserId=U.UserId").
                   Condition("U.UserId", ColumnOperator.Equals, delete.AddParam("1", "UserId"));

            var sql = delete.ToString();

            var expected =
@"DELETE U FROM Users U
INNER JOIN User_Backup UB ON UB.UserId=U.UserId
WHERE U.UserId = @UserId";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());
        }


    }
}