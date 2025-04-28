// See https://aka.ms/new-console-template for more information
using LinqToSQL.Entities;
using LinqToSQL.Query;
using LinqToSQL.Query.Extensions;
using LinqToSQL.Traduction;


/**
 * Select 
 */

// "SELECT * FROM Tickets"
var sekectAll = new Query<Tickets>()
                .ToList();

// "SELECT Tickets.Id, Tickets.Status, Tickets.Origin FROM Tickets"
var selectWithField = new Query<Tickets>()
                            .Select((user) => new { user.Id, user.Status, user.Origin })
                            .ToList();

// SELECT Tickets.Id AS Alias1, Tickets.Status AS Alias2, Tickets.Origin AS Alias3 FROM Tickets
var selectWithAlias = new Query<Tickets>()
                            .Select((user) => new { Alias1 = user.Id, Alias2 = user.Status, Alias3 = user.Origin })
                            .ToList();

/**
 * Jointure
 * Restriction : si une jointure est déjà présente dans la requête, alors on génère une exception.
 *               les mêmes noms de tables doivent être réutilisés dans l'expression.
 */

// "SELECT * FROM Users INNER JOIN Tickets ON (Users.Id = Tickets.UserId) INNER JOIN FollowUpSheets ON (FollowUpSheets.TicketId = Tickets.Id)"
var join = new Query<Users>()
                .Join<Users, Tickets>((Users, Tickets) => Users.Id == Tickets.UserId)
                .Join<Users, Tickets, Users, FollowUpSheets>((Users, Tickets, Users2, FollowUpSheets) => FollowUpSheets.TicketId == Tickets.Id)
                .ToList();

// SELECT * FROM Users LEFT JOIN Tickets ON (Users.Id = Tickets.UserId) LEFT JOIN FollowUpSheets ON (FollowUpSheets.TicketId = Tickets.Id)
var leftJoin = new Query<Users>()
                .LeftJoin<Users, Tickets>((Users, Tickets) => Users.Id == Tickets.UserId)
                .LeftJoin<Users, Tickets, Users, FollowUpSheets>((Users, Tickets, Users2, FollowUpSheets) => FollowUpSheets.TicketId == Tickets.Id)
                .ToList();

// SELECT * FROM Users RIGHT JOIN Tickets ON (Users.Id = Tickets.UserId) RIGHT JOIN FollowUpSheets ON (FollowUpSheets.TicketId = Tickets.Id)
var rightJoin = new Query<Users>()
                .RightJoin<Users, Tickets>((Users, Tickets) => Users.Id == Tickets.UserId)
                .RightJoin<Users, Tickets, Users, FollowUpSheets>((Users, Tickets, Users2, FollowUpSheets) => FollowUpSheets.TicketId == Tickets.Id)
                .ToList();

// SELECT * FROM Users INNER JOIN (SELECT Tickets.UserId FROM Tickets WHERE (t.State = 1)) AS TicketsSub ON (user.Id = ticketSub.UserId)
var ticketsSubquery = new Query<Tickets>()
    .Where(t => t.State == 1)
    .Select(t => t.UserId);

var query = new Query<Users>()
    .JoinSubquery("TicketsSub", ticketsSubquery, (user, ticketSub) => user.Id == ticketSub.UserId)
    .ToList();


/*
 * Filtre 
 * SELECT * FROM Users WHERE (((Users.Id > 2) AND (Users.AgencyId > 4)) OR (Users.AgencyId > 5)) AND (FollowUpSheets.TicketId > 10) AND ((Tickets.Origin > 10) AND (Tickets.Status <> 100)) AND (FollowUpSheets2.TicketId > 100)
 */

var filtre = new Query<Users>()
               .Where(Users => (Users.Id > 2 && Users.AgencyId > 4) || Users.AgencyId > 5)
               .Where<Users, FollowUpSheets>((Users, FollowUpSheets) => FollowUpSheets.TicketId > 10)
               .Where<Users, FollowUpSheets, Tickets>((Users, FollowUpSheets, Tickets) => Tickets.Origin > 10 && Tickets.Status != 100)
               .Where<Users, FollowUpSheets, Tickets, FollowUpSheets>((Users, FollowUpSheets, Tickets, FollowUpSheets2) => FollowUpSheets2.TicketId > 100)
               .ToList();

/*
 * Trie 
 * SELECT * FROM Users WHERE (((Users.Id > 2) AND (Users.AgencyId > 4)) OR (Users.AgencyId > 5)) ORDER BY Users.Name, FollowUpSheets.Comment, Users.Name, Tickets.CreateAt
 * Options : True => ASC, False => DESC
 */

var sorted = new Query<Users>()
                    .Where(Users => (Users.Id > 2 && Users.AgencyId > 4) || Users.AgencyId > 5)
                    .OrderBy(Users => Users.Name, false)
                    .OrderBy<Users, FollowUpSheets, Tickets, Users>((Users, FollowUpSheets, Tickets, Users2) => new { FollowUpSheets.Comment, Users.Name, Tickets.CreateAt })
                    .ToList();



/*
 * Pagination
 * SELECT * FROM Tickets ORDER BY (SELECT NULL) OFFSET 10 ROWS FETCH NEXT 20 ROWS ONLY
 */

var paginaded = new Query<Tickets>()
                .Skip(10)
                .Take(20)
                .ToList();


/*
 * Fonction Scallaire
 * SELECT Tickets.Id, COUNT(Orders.Id) AS Count FROM Tickets
 *   
 */

var selectFonctionScallaire = new Query<Tickets>()
    .Select(tickets => new { tickets.Id, Count = SqlFunctions.Count("Orders.Count") })
    .Select(tickets => new { Sum = SqlFunctions.Sum("Orders.Sum"), Avg = SqlFunctions.Avg("Order.Sum") })
    .ToList();

/*
 * Having / Group By
 * Restriction: Le HAVING est ignoré si la clause GROUP BY n'est pas renseignée.
 * SELECT * FROM Tickets GROUP BY Tickets.Id, Tickets.ClotureAt HAVING (Tickets.State > AVG(Order.Id))
 */

var having = new Query<Tickets>()
    .GroupBy(Tickets => new { Tickets.Id, Tickets.ClotureAt })
    .Having(Tickets => Tickets.State > SqlFunctions.Avg("Order.Id"))
    .ToList();




// Il manque le BulkInsert et les fonction sql à présenter et potentiellement pofiner




// J'aimerais implémenter les sous requête, la clause (IN), Merge...


/*
 * Mis à jour
 * UPDATE Users SET Id = @Id, AgencyId = @AgencyId, Name = @Name, Password = @Password WHERE (Users.Id = 1)
 */

var updateUser = new Users { Name = "name maj", Password = "password maj", AgencyId = 2 };

var sqlUpdate = new Query<Users>().Update(updateUser, (Users) => Users.Id == 1);


/*
 * Insertion 
 * INSERT INTO Users (Id, AgencyId, Name, Password, CreateAt, UpdateAt, FirstName) VALUES (@Id, @AgencyId, @Name, @Password, @CreateAt, @UpdateAt, @FirstName)
 * Plut tard il serra important de précisé qu'est ce qui se passe si on ne renseigne pas certaines valeurs (je pense quels sont défini auotmatiquement à null)
 */

var newUser = new Users
{
    Name = "François",
    Password = "password",
    FirstName = "firstName"
};

var sqlInsert = new Query<Users>().Insert(newUser);

/*
 * Insertion en masse (BulkInsert)
 * INSERT INTO Users (Id, AgencyId, Name, Password, CreateAt, UpdateAt, FirstName) VALUES (@Id_0, @AgencyId_0, @Name_0, @Password_0, @CreateAt_0, @UpdateAt_0, @FirstName_0), (@Id_1, @AgencyId_1, @Name_1, @Password_1, @CreateAt_1, @UpdateAt_1, @FirstName_1), (@Id_2, @AgencyId_2, @Name_2, @Password_2, @CreateAt_2, @UpdateAt_2, @FirstName_2)
 */

List<Users> users = [
    new Users {
        Name = "François",
        Password = "password",
        FirstName = "firstName"
    },
    new Users {
        Name = "Marie",
        Password = "password1",
        FirstName = "oeoeoe"
    },
    new Users {
        Name = "Jacque",
        Password = "Jacquie",
        FirstName = "dddd"
    }
];

var manyInsert = new Query<Users>().InsertMany(users);


/*
 * Delete
 * DELETE FROM Users WHERE (Users.Id = @p0)
 */

var delete = new Query<Users>().Delete(Users => Users.Id == 1);

// DELETE FROM Users
var deleteAll = new Query<Users>().DeleteAll();


/*
 * Distinct
 * SELECT DISTINCT * FROM Users
 * Bemole : Je peux rajouter plusieurs Distinct et cela ne fonctiionne pas avec '*'
 */

var distinct = new Query<Users>()
    .Distinct().ToList();


/*
 * SELECT * FROM Users
 * UNION
 * SELECT * FROM Tickets
 */

var union = new Query<Users>().Union(new Query<Tickets>()).ToList();


/****** IN / NOT IN ******/

/*
 * Sous requêtes
 */

// SELECT * FROM Users WHERE Users.Id IN (SELECT Tickets.UserId FROM Tickets WHERE (Tickets.Status = 1))
var subQuery = new Query<Tickets>()
                    .Where(Tickets => Tickets.Status == 1)
                    .Select(Tickets => Tickets.UserId);

var inWithSubQuerues = new Query<Users>()
                .WhereIn(Users => Users.Id, subQuery)
                .ToList();

// SELECT * FROM Users WHERE Users.Id NOT IN (SELECT Tickets.UserId FROM Tickets WHERE (Tickets.Status = 1))
var notIn = new Query<Tickets>()
                    .Where(Tickets => Tickets.Status == 1)
                    .Select(Tickets => Tickets.UserId);

var notInWithSubQuerues = new Query<Users>()
                .WhereNotIn(Users => Users.Id, subQuery)
                .ToList();



// EXISTS / NOT EXISTS


/*
 * Sous requêtes
 * Restiction: au niveau des selection on ne peut pas faire (SELECT 1 )
 */

var subQueryExists = new Query<Tickets>()
                    .Where(Tickets => Tickets.Status == 1)
                    .Select(Tickets => Tickets.UserId);

// SELECT * FROM Users WHERE EXISTS (SELECT Tickets.UserId FROM Tickets WHERE (Tickets.Status = 1))

var exitsWithSubQuerues = new Query<Users>()
                .WhereExists(subQuery)
                .ToList();

// SELECT * FROM Users WHERE NOT EXISTS (SELECT Tickets.UserId FROM Tickets WHERE (Tickets.Status = 1))
var notExists = new Query<Users>()
                .WhereNotExists(subQuery)
                .ToList();




// Extensions à faire gafs


/*
 * Like
 * Ne prend en compte pour l'instant que les %%
 * Choisir le format de la date 
 */


// SELECT * FROM Users WHERE Users.Name LIKE %François%
var LikeString = new Query<Users>()
    .Like(Users => Users.Name, "François")
    .ToList();

// SELECT * FROM Users WHERE Users.CreateAt LIKE %2025-04-28% 
var LikeDate = new Query<Users>()
    .Like(Users => Users.CreateAt, DateTime.Now)
    .ToList();

/*
 * SELECT CASE WHEN ((Users.AgencyId = 1) OR (Users.Id <> 2)) THEN Fait cela ELSE Sinon fait ceci END FROM Users
 * Un peu trop limité. Pas d'imbrication, pas d'alias rien.. :/
 */

var CaseWhen = new Query<Users>()
    .CaseWhen(Users => Users.AgencyId == 1 || Users.Id != 2, "Fait cela", "Sinon fait ceci")
    .ToList();

Console.WriteLine(sqlInsert);


