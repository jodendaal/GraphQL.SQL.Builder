using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using GraphQL.SQL.Builder;
using System.Data.SqlClient;
using System.Data;

namespace GraphQL.SQL.Tests
{
    [TestClass]
    public class UpdateQueryBuilderTests
    {
        [TestMethod]
        public void Update()
        {
            var update = new UpdateQueryBuilder("Users");
            update.Field("UserId", update.AddParam(10, "NewUserId")).
                   Field("Password", update.AddParam("3423", "Password")).
                   Condition("UserId", ColumnOperator.Equals, update.AddParam("1", "UserId"));


            var sql = update.ToString();

            var expected =
@"UPDATE Users
SET
UserId=@NewUserId,
Password=@Password
WHERE UserId = @UserId";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());
        }

        [TestMethod]
        public void Update_From_Select()
        {
            var update = new UpdateQueryBuilder("Users","U");
            update.Join("User_Backup UB", JoinType.Inner, "UB.UserId=U.UserId").
                   Field("Password", "UB.Password").
                   Condition("U.UserId", ColumnOperator.Equals, update.AddParam("1", "UserId"));

            var sql = update.ToString();

            var expected =
@"UPDATE U
SET
Password=UB.Password
FROM Users U
INNER JOIN User_Backup UB ON UB.UserId=U.UserId
WHERE U.UserId = @UserId";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());
        }


    }
}