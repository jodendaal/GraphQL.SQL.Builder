using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using GraphQL.SQL.Builder;
using System.Data.SqlClient;
using System.Data;

namespace GraphQL.SQL.Tests
{
    [TestClass]
    public class InsertQueryBuilderTests
    {
        [TestMethod]
        public void Insert()
        {
            var insert = new InsertQueryBuilder("Users");
            insert.Field("UserId", "'1'").
                   Field("Password", "'test123'");

            var sql = insert.ToString();

            var expected =
@"INSERT INTO Users
(
UserId,
Password
)
VALUES
(
'1',
'test123'
)
";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());
        }

        [TestMethod]
        public void Insert_Parameters()
        {
            var insert = new InsertQueryBuilder("Users");
            insert.Field("UserId", insert.AddParam("1")).
                   Field("Password", insert.AddParam("test123"));

            var sql = insert.ToString();


            foreach (var param in insert.Parameters)
            {
                switch (param.Key)
                {
                    case "@p_0":
                        Assert.AreEqual(param.Value.Value, "1", "userid incorrect");
                        break;
                    case "@p_1":
                        Assert.AreEqual(param.Value.Value, "test123", "password incorrect");
                        break;
                }
            }
            var expected =
@"INSERT INTO Users
(
UserId,
Password
)
VALUES
(
@p_0,
@p_1
)
";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());
        }


        [TestMethod]
        public void Insert_From_Select()
        {
            var insert = new InsertQueryBuilder("Users");
            insert.Field("UserId").
                   Field("Password").
                   From("Users_Backup", "UB", (query) =>
                   {
                       query.Field("UB.UserId").
                             Field("UB.Password").
                             Condition("UB.UserId", ColumnOperator.Equals, insert.AddParam(1, "UserId"));
                   });

            var sql = insert.ToString();
            var sqlCommand = insert.ToCommand();
            Assert.AreEqual(1, sqlCommand.Parameters[0].Value);
            Assert.AreEqual("@UserId", sqlCommand.Parameters[0].ParameterName);
            var expected =
@"INSERT INTO Users
(
UserId,
Password
)
SELECT
UB.UserId,
UB.Password
FROM Users_Backup UB
WHERE UB.UserId = @UserId
";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());
        }


        [TestMethod]
        public void Insert_From_Select_NoFields_Specified()
        {
            var insert = new InsertQueryBuilder("Users");
            insert.From("Users_Backup", "UB", (query) =>
                   {
                       query.Field("UB.UserId").
                             Field("UB.Password").
                             Condition("UB.UserId", ColumnOperator.Equals, insert.AddParam(1, "UserId"));
                   });

            var sql = insert.ToString();
            var sqlCommand = insert.ToCommand();
            Assert.AreEqual(1, sqlCommand.Parameters[0].Value);
            Assert.AreEqual("@UserId", sqlCommand.Parameters[0].ParameterName);
            var expected =
@"INSERT INTO Users
SELECT
UB.UserId,
UB.Password
FROM Users_Backup UB
WHERE UB.UserId = @UserId
";

            Assert.AreEqual(expected.ToLower(), sql.ToLower());
        }
    }
}