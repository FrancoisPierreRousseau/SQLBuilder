# LinqToSQL

**LinqToSQL** est une biblioth�que permettant de g�n�rer des requ�tes SQL � partir d'expressions LINQ en C#. Elle facilite la construction dynamique de requ�tes SQL � l'aide de classes et d'extensions, notamment pour les op�rations de **SELECT**, **JOIN**, **WHERE** et **INSERT**.

## Fonctionnalit�s

- **Construction de requ�tes SQL** : Cr�ez des requ�tes SQL avec des expressions LINQ.
- **Support des jointures** : Ajoutez des jointures internes (`INNER JOIN`) et gauches (`LEFT JOIN`).
- **Conditions WHERE dynamiques** : Ajoutez des conditions � vos requ�tes avec des m�thodes d'extension.
- **Support des alias** : Utilisez des alias pour les colonnes dans les requ�tes.
- **Filtrage d'insertion** : Marquez les propri�t�s � ignorer lors des op�rations d'insertion avec l'attribut `IgnoreInsert`.
- **Optimisation m�moire** : Gr�ce � l'utilisation de la r�flexion, ce projet acc�de directement aux objets et � leurs membres de mani�re dynamique, r�duisant ainsi l'allocation m�moire en r�utilisant les objets existants et en �vitant la cr�ation de nouveaux objets ou cha�nes � chaque �tape. Cela permet une gestion plus efficace des ressources m�moire tout en pr�servant une flexibilit� maximale.
- **SQL pur** : Le SQL g�n�r� est "pur", ce qui permet une flexibilit� accrue. Vous pouvez interagir directement avec les cha�nes g�n�r�es, les adapter et les utiliser selon vos besoins.

## Installation

Clonez ce d�p�t pour commencer :

```bash
git clone https://github.com/FrancoisPierreRousseau/SQLBuilder.git
```

## Utilisation

Exemple de requ�te `SELECT` simple :

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