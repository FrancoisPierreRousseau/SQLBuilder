// Deux partie, SQLBuilder et Orm

/*
 *  SQLBuilder (la plus flexible et facile à utiliser) 
 */

// CRUD Mais probléme, elles n'utilise pas la même connections.

using Microsoft.Data.SqlClient;
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
 * Batch Insert / Update (pour l'instant non présent)
 */

/* Batch.Insert(new List<Ticket>
 {
        new() { Object = "Un objet", State = 3, Priority = 2, Origin = 1, Status = 1 },
        new() { Object = "Un objet", State = 3, Priority = 2, Origin = 1, Status = 1 },
        new() { Object = "Un objet", State = 3, Priority = 2, Origin = 1, Status = 1 }
 }, "Tickets", connectionString); */






/*
 * ORM (peut être sympa, y a des truc à prendre, maais pas totalement finis)  
 * 
 * */



/*
 * Début Scaffolding 
 */

// Génération des classes
/* var reader = new DatabaseSchemaReader(connectionString);
var tables = reader.GetTables();

var generatedCode = ClassGenerator.GenerateClasses(tables, "MyApp.Models");

File.WriteAllText("GeneratedModels.cs", generatedCode); */

// CRUD
/* var connection = new SqlConnection(connectionString);
connection.Open();
using var command = new SqlCommand("SELECT * FROM Tickets", connection);
using var reader = command.ExecuteReader();
var tickets = SimpleMapper.MapToList<Tickets>(reader); */

// Avantage, je récupére l'id
/* var connection = new SqlConnection(connectionString);
connection.Open();

var user = new Users
{
    Name = "François",
    Password = "password",
    FirstName = "firstName"
};

var newId = InsertBuilder.ExecuteInsert(connection, user, "Users");

Console.WriteLine($"Inserted user with Id = {newId}"); */



/* Repostories */

/* var connection = new SqlConnection(connectionString);
connection.Open();

var userRepository = new Repository<Users>(connection);
var newUser = new Users
{
    Name = "Gégé",
    Password = "gégé",
    FirstName = "rare"
};
var newId = userRepository.Insert(newUser);   */

// Get by Id
// var user = userRepository.GetById(newId);

// Update (non encore implémenté)
/* user.Name = "Alice Updated";
userRepository.Update(user); */

// Get all
// var allUsers = userRepository.GetAll();

// Delete
// userRepository.Delete(newId); */


/* Optimisation du QueryBuilder */
/* var whereClause = ExpressionToSqlTranslator.Translate2<Tickets>(u => u.Status == 3);
Console.WriteLine(whereClause); */

// Select All
var connection = new SqlConnection(connectionString);

/* connection.Open();


var tickets = new Query2<Tickets>(connection)
                .Select(ticket => new { ticket.Origin, ticket.State, ticket.Status })
                .ToList();

connection.Close(); */

// Avec where
/* connection.Open();

var ticketsWithWhere = new Query2<Tickets>(connection)
                .Where(u => u.Status < 3)
                .ToList();

connection.Close(); */


// Avec order by
/* connection.Open();
var users = new Query2<Tickets>(connection)
                .OrderBy(u => u.Id)
                .ToList();

connection.Close(); */

// SKIP/TAKE
/* connection.Open();

var ticketsSkip = new Query2<Tickets>(connection)
                .Skip(10)
                .Take(20)
                .ToList();

connection.Close();  */


// Les JOINs (concernant les clause where, pas possible au delà de 2 types)

// Fonctionnel mais géré les ambuigiité au niveaua des colonnes

// var connection = new SqlConnection(connectionString);

/* connection.Open();

var query = new Query2<Users>(connection)
   .Where(user => user.Id > 2)
   .Join<Tickets>((user, ticket) => user.Id == ticket.UserId)
   .Where((user, order) => order.State == 3)
   .OrderBy(user => user.Name)
   .ToList();

connection.Close();


Console.WriteLine(query); */

/* var query = new Query2<Users>(connection)
    .LeftJoin<Tickets>((user, ticket) => user.Id == ticket.UserId)
    .Where((user, order) => order.State == 3)
    .OrderBy(user => user.Name)
    .ToList(); */


/* var query = new Query2<Users>(connection)
    .RightJoin<Tickets>((user, ticket) => user.Id == ticket.UserId)
    .Where((user, order) => order.State == 3)
    .OrderBy(user => user.Name)
    .ToList(); */


// Multi Jointure !



/* var query = new Query2<Users>(connection)
    .Join<Tickets>((user, ticket) => user.Id == ticket.UserId)
    .LeftJoin<FollowUpSheets>((ticket, follow) => ticket.Id == follow.TicketId)
    .Where((user, follow) => follow.TicketId > 100)
    .OrderBy(user => user.Name)
    .ToList(); */


var query = new Query2<Users>(connection)
    .Join<Tickets>((users, tickets) => users.Id == tickets.UserId)
    .LeftJoin<FollowUpSheets>((tickets, followUpSheets) => tickets.Id == followUpSheets.TicketId)
    .Select((user, ticket) => new { AliasId = user.Id, AliasComment = ticket.Comment, CountMember = SqlFunctions.Count("Users.Id") })
    .Where((user, follow) => follow.TicketId > 100)
    .OrderBy(user => user.Name)
    .ToList();


