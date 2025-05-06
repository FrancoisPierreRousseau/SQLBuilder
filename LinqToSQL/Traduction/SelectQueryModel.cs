namespace LinqToSQL.Traduction;
public class SelectQueryModel
{
    public string TableName { get; init; } = string.Empty;
    public List<string> Columns { get; } = new();
    public List<string> WhereClauses { get; } = new();
    public List<string> OrderByClauses { get; } = new();
    public List<JoinModel> Joins { get; } = new();

    public List<string> GroupByColumns { get; } = new();
    public string? HavingClause { get; set; }

    public bool IsDistinct { get; set; } = false;
    public int? Skip { get; set; }
    public int? Take { get; set; }
}
