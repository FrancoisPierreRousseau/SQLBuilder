// CRUD Mais probléme, elles n'utilise pas la même connections.

using SQLBuilder;
using SQLBuilder.Entities;

var connectionString = "Server=(local);Database=Callfollower;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;";

//Origin = @Origin And State = @State", new { Origin = 1, State = 3
/* var users = new Query<Ticket>("Tickets")
    .Where(t => t.Origin == 1 && t.State == 3)
    .OrderBy("State")
    .Execute(connectionString);


new InsertBuilder<Ticket>("Tickets")
    .Values(() => new Ticket { Object = "Un objet", State = 3, Priority = 2, Origin = 1, Status = 1 })
    .Execute(connectionString);


var newUsers = new Query<Ticket>("Tickets")
    .Where(t => t.Origin == 1 && t.State == 3)
    .OrderBy("State")
    .Execute(connectionString);


new UpdateBuilder<Ticket>("Tickets")
    .Set(u => new { u.Status, u.Object }, new { Status = 2, Object = "Objet mis à jour" })
    .Where(u => u.Id == 7046)
    .Execute(connectionString);

var usersUpdate = new Query<Ticket>("Tickets")
    .Where(t => t.Origin == 1 && t.State == 3)
    .OrderBy("State")
    .Execute(connectionString);


new DeleteBuilder<Ticket>("Tickets")
    .Where(u => u.Id == 1)
    .Execute(connectionString);


var usersDelete = new Query<Ticket>("Tickets")
    .Where(t => t.Origin == 1 && t.State == 3)
    .OrderBy("State")
    .Execute(connectionString); 
*/




/*
 * Utilisation d'un systéme pour les transaction (même pool de connection, possibilité de rollback, de commit ...)
 * Important pour l'atomicité des données
 */


/* using var transactionManager = new TransactionManager(connectionString);

try
{
    var users = new Query<Ticket>("Tickets")
    .Where(t => t.Origin == 1 && t.State == 3)
    .OrderBy("State")
    .Execute(transactionManager);


    new InsertBuilder<Ticket>("Tickets")
        .Values(() => new Ticket { Object = "Un objet", State = 3, Priority = 2, Origin = 1, Status = 1 })
        .Execute(transactionManager);


    new UpdateBuilder<Ticket>("Tickets")
        .Set(u => new { u.Status, u.Object }, new { Status = 2, Object = "Objet mis à jour" })
        .Where(u => u.Id == 7046)
        .Execute(transactionManager);

    new DeleteBuilder<Ticket>("Tickets")
        .Where(u => u.Id == 1)
        .Execute(transactionManager);

    Batch.Insert(new List<Ticket>
    {
        new() { Object = "Un objet", State = 3, Priority = 2, Origin = 1, Status = 1 },
        new() { Object = "Un objet", State = 3, Priority = 2, Origin = 1, Status = 1 },
        new() { Object = "Un objet", State = 3, Priority = 2, Origin = 1, Status = 1 }
     }, "Tickets", transactionManager);

    transactionManager.Commit();

}
catch
{
    // transactionManager.Rollback();
    // automatique rollback car pas commit
    throw;
} */

/* 
 * Batch Insert / Update
 */

Batch.Insert(new List<Ticket>
    {
        new() { Object = "Un objet", State = 3, Priority = 2, Origin = 1, Status = 1 },
        new() { Object = "Un objet", State = 3, Priority = 2, Origin = 1, Status = 1 },
        new() { Object = "Un objet", State = 3, Priority = 2, Origin = 1, Status = 1 }
     }, "Tickets", connectionString);




