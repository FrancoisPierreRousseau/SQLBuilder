using SQLBuilder;
using SQLBuilder.Entities;

var users = new Query<Ticket>("Tickets")
    .Where("Origin = @Origin And State = @State", new { Origin = 1, State = 3 })
    .OrderBy("State")
    .Execute("Server=(local);Database=Callfollower;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;");


Console.WriteLine(users);