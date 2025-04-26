namespace SQLBuilder.Entities;
internal class Ticket
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string CustomerId { get; set; }

    public int Priority { get; set; }

    public int State { get; set; }

    public int Status { get; set; }

    public int Origin { get; set; }

    public string Object { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }
}
