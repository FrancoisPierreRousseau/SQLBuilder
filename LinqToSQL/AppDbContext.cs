using LinqToSQL.Context;
using LinqToSQL.Entities;
using LinqToSQL.Query;

namespace LinqToSQL;


public class AppDbContext : DbContext
{
    public Query<Users> Users => Set<Users>();
    public Query<Tickets> Tickets => Set<Tickets>();
}

