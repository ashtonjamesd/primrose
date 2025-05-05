using Primrose.src.Sql;

namespace Primrose.Tests.src;

public class TableTests {
    [Fact]
    public void CreateAndDropTableTest() {
        var db = new SqlEngine()
            .Setup();
        Assert.NotNull(db.controller.Database);
        Assert.NotNull(db.controller.GetDatabase("master"));
        Assert.NotNull(db.controller.GetTable("primrose_master"));
        Assert.NotNull(db.controller.GetUser("primrose"));

        db.ExecuteQuery("create table test (x int)");
        Assert.NotNull(db.controller.GetTable("test"));

        db.ExecuteQuery("drop table test");
        Assert.Null(db.controller.GetTable("test"));
    }
}