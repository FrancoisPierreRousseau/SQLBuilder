// See https://aka.ms/new-console-template for more information
using LinqToSQL.Entities;
using LinqToSQL.Query;
using LinqToSQL.Query.Extensions;


/**
 * Jointure
 * Restriction : si une jointure est déjà présente dans la requête, alors on génère une exception.
 * 
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

Console.WriteLine(paginaded);

