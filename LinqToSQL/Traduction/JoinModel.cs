namespace LinqToSQL.Traduction;
public class JoinModel
{
    public string JoinType { get; init; } = "INNER"; // INNER, LEFT, RIGHT
    public string TableName { get; init; }
    public string OnClause { get; init; } = string.Empty;
}
