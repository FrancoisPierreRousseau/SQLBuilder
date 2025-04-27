# LinqToSQL

**LinqToSQL** est une bibliothèque permettant de générer des requêtes SQL à partir d'expressions LINQ en C#. Elle facilite la construction dynamique de requêtes SQL à l'aide de classes et d'extensions, notamment pour les opérations de **SELECT**, **JOIN**, **WHERE** et **INSERT**.


### Explication de l'utilisation de la réflexion

Dans ce projet, **la réflexion** est utilisée de manière ciblée pour accéder et manipuler dynamiquement les objets existants, tels que les entités et leurs propriétés, sans créer de nouveaux objets ou chaînes de caractères à chaque étape. Cela permet de générer des requêtes SQL de manière flexible tout en réutilisant les objets en mémoire, ce qui minimise les allocations inutiles. Bien que l'utilisation de la réflexion introduise un coût de performance, ce dernier est limité et n'occasionne pas une surcharge significative dans le cadre de ce projet.

---

### Comment la réflexion est utilisée

La réflexion dans **LinqToSQL** est utilisée pour inspecter les objets et leurs membres afin de construire dynamiquement les requêtes SQL sans avoir à créer de nouveaux objets ou chaînes à chaque manipulation. Cela permet de :
- Accéder aux propriétés d'objets dynamiquement.
- Créer des conditions, des jointures et des expressions SQL à partir des types d'objets existants.
- Réutiliser des objets existants pour générer le SQL, réduisant ainsi les allocations mémoire et améliorant l'efficacité.

Cela permet une grande flexibilité dans la génération de SQL, tout en évitant des surcharges de mémoire dues à la création d'objets supplémentaires.

## Fonctionnalités

- **Construction de requêtes SQL** : Créez des requêtes SQL avec des expressions LINQ.
- **Support des jointures** : Ajoutez des jointures internes (`INNER JOIN`) et gauches (`LEFT JOIN`).
- **Conditions WHERE dynamiques** : Ajoutez des conditions à vos requêtes avec des méthodes d'extension.
- **Support des alias** : Utilisez des alias pour les colonnes dans les requêtes.
- **Filtrage d'insertion** : Marquez les propriétés à ignorer lors des opérations d'insertion avec l'attribut `IgnoreInsert`.
- **Optimisation mémoire** : Grâce à l'utilisation de la réflexion, ce projet accède directement aux objets et à leurs membres de manière dynamique, réduisant ainsi l'allocation mémoire en réutilisant les objets existants et en évitant la création de nouveaux objets ou chaînes à chaque étape. Cela permet une gestion plus efficace des ressources mémoire tout en préservant une flexibilité maximale.
- **SQL pur** : Le SQL généré est "pur", ce qui permet une flexibilité accrue. Vous pouvez interagir directement avec les chaînes générées, les adapter et les utiliser selon vos besoins.
- **Optimisation future : Système de cache** : Une fonctionnalité future envisagée serait l'ajout d'un système de **cache** pour les chaînes SQL générées. Ce cache permettrait de stocker les résultats pré-générés des requêtes et d'éviter de recalculer les informations via la réflexion à chaque nouvelle exécution similaire. Cela permettrait d'améliorer les performances pour des requêtes récurrentes, similaire à ce que fait **Dapper** en mettant en cache les métadonnées des objets et des propriétés.


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
