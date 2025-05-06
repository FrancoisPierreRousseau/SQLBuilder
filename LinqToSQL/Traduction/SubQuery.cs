namespace LinqToSQL.Traduction;
public class SubQuery
{
    public string Sql { get; }
    public SubQuery(string sql) => Sql = sql;

    public override string ToString() => $"({Sql})";
}

