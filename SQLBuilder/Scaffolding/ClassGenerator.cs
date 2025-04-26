using System.Text;

namespace SQLBuilder.Scaffolding;
public static class ClassGenerator
{
    public static string GenerateClasses(List<TableSchema> tables, string @namespace = "GeneratedModels")
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine($"namespace {@namespace}");
        sb.AppendLine("{");

        foreach(var table in tables)
        {
            sb.AppendLine($"    public class {table.Name}");
            sb.AppendLine("    {");

            foreach(var column in table.Columns)
            {
                var csharpType = SqlTypeToCSharpType(column.DataType, column.IsNullable);
                sb.AppendLine($"        public {csharpType} {column.Name} {{ get; set; }}");
            }

            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string SqlTypeToCSharpType(string sqlType, bool isNullable)
    {
        var type = sqlType.ToLower() switch
        {
            "int" => "int",
            "bigint" => "long",
            "smallint" => "short",
            "tinyint" => "byte",
            "bit" => "bool",
            "float" => "double",
            "real" => "float",
            "decimal" => "decimal",
            "numeric" => "decimal",
            "money" => "decimal",
            "smallmoney" => "decimal",
            "varchar" => "string",
            "nvarchar" => "string",
            "char" => "string",
            "nchar" => "string",
            "text" => "string",
            "ntext" => "string",
            "datetime" => "DateTime",
            "smalldatetime" => "DateTime",
            "date" => "DateTime",
            "time" => "TimeSpan",
            "uniqueidentifier" => "Guid",
            _ => "string" // fallback
        };

        if(type != "string" && isNullable)
            type += "?";

        return type;
    }
}
