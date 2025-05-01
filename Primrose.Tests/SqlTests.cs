using Primrose.src.Sql;

namespace Primrose.Tests;

public class SqlTests {
    [Fact]
    public void CreateDatabaseTest() {
        var db = new SqlEngine(debug: true);
        db.Setup();

        db.ExecuteQuery(
            "create database test"
        );

        Assert.NotNull(db.controller.GetDatabase("test"));
    }

    [Fact]
    public void CreateAndDropDatabaseTest() {
        var db = new SqlEngine(debug: false);
        db.Setup();

        db.ExecuteQuery(
@"""
create database test
drop database test
"""
        );

        Assert.Null(db.controller.GetDatabase("test"));
    }
}