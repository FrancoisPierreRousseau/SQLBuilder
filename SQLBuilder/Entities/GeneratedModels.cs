using SQLBuilder.Attributs;

namespace SQLBuilder.Entities;

public class __EFMigrationsHistory
{
    public string MigrationId { get; set; }
    public string ProductVersion { get; set; }
}

public class Agencies
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Adresse1 { get; set; }
    public string Addresse2 { get; set; }
    public string ZipCode { get; set; }
    public string City { get; set; }
    public string Phone { get; set; }
    public string Fax { get; set; }
}

public class Users
{
    [IgnoreInsert]
    public int Id { get; set; }
    public int? AgencyId { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public DateTime? CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    public string FirstName { get; set; }
}

public class Tickets
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string CustomerId { get; set; }
    public int Priority { get; set; }
    public int State { get; set; }
    public int Status { get; set; }
    public int Origin { get; set; }
    public string Object { get; set; }
    public DateTime? ClotureAt { get; set; }
    public string Description { get; set; }
    public string Solution { get; set; }
    public DateTime? CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
}

public class Attachments
{
    public int Id { get; set; }
    public int? TicketId { get; set; }
    public string Source { get; set; }
    public string SourceWindows { get; set; }
}

public class FollowUpSheets
{
    public int Id { get; set; }
    public int? TicketId { get; set; }
    public int Type { get; set; }
    public DateTime Time { get; set; }
    public string Comment { get; set; }
    public DateTime? CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
}

public class sysdiagrams
{
    public string name { get; set; }
    public int principal_id { get; set; }
    public int diagram_id { get; set; }
    public int? version { get; set; }
    public string definition { get; set; }
}


