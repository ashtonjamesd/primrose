using Primrose.src.Sql.Models;

namespace Primrose.src.Auth;

internal sealed class AuthService {
    public readonly List<SqlUser> Users = [];
    
    public SqlUser? Login(string name, string pass) {
        var user = GetUser(name);
        if (user is null) return null;

        if (user.Password == pass) {
            return user;
        }

        return null;
    }

    public SqlUser? GetUser(string name) {
        var user = Users
            .FirstOrDefault(x => x.Name == name);

        return user;
    }
}