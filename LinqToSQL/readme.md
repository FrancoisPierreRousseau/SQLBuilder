# LinqToSQL

**LinqToSQL** est une bibliothèque permettant de générer des requêtes SQL à partir d'expressions LINQ en C#. Elle facilite la construction dynamique de requêtes SQL à l'aide de classes et d'extensions, notamment pour les opérations de **SELECT**, **JOIN**, **WHERE** et **INSERT**.

## Fonctionnalités

- **Construction de requêtes SQL** : Créez des requêtes SQL avec des expressions LINQ.
- **Support des jointures** : Ajoutez des jointures internes (`INNER JOIN`) et gauches (`LEFT JOIN`).
- **Conditions WHERE dynamiques** : Ajoutez des conditions à vos requêtes avec des méthodes d'extension.
- **Support des alias** : Utilisez des alias pour les colonnes dans les requêtes.
- **Filtrage d'insertion** : Marquez les propriétés à ignorer lors des opérations d'insertion avec l'attribut `IgnoreInsert`.
- **Optimisation mémoire** : Grâce à l'utilisation de la réflexion, ce projet accède directement aux objets et à leurs membres de manière dynamique, réduisant ainsi l'allocation mémoire en réutilisant les objets existants et en évitant la création de nouveaux objets ou chaînes à chaque étape. Cela permet une gestion plus efficace des ressources mémoire tout en préservant une flexibilité maximale.
- **SQL pur** : Le SQL généré est "pur", ce qui permet une flexibilité accrue. Vous pouvez interagir directement avec les chaînes générées, les adapter et les utiliser selon vos besoins.

## Installation

Clonez ce dépôt pour commencer :

```bash
git clone https://github.com/FrancoisPierreRousseau/SQLBuilder.git
```

## Utilisation

Exemple de requête `SELECT` simple :

```csharp
var result = new Query<Tickets>()
    .Where(t => t.Status == "Open")
    .ToList();
```

Exemple de jointure :

```csharp
var result = new Query<Tickets>()
    .Join<Tickets, User>((ticket, user) => ticket.UserId == user.Id)
    .ToList();
```

---