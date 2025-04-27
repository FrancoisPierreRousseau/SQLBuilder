# LinqToSQL

**LinqToSQL** est une biblioth�que permettant de g�n�rer des requ�tes SQL � partir d'expressions LINQ en C#. Elle facilite la construction dynamique de requ�tes SQL � l'aide de classes et d'extensions, notamment pour les op�rations de **SELECT**, **JOIN**, **WHERE** et **INSERT**.


### Explication de l'utilisation de la r�flexion

Dans ce projet, **la r�flexion** est utilis�e de mani�re cibl�e pour acc�der et manipuler dynamiquement les objets existants, tels que les entit�s et leurs propri�t�s, sans cr�er de nouveaux objets ou cha�nes de caract�res � chaque �tape. Cela permet de g�n�rer des requ�tes SQL de mani�re flexible tout en r�utilisant les objets en m�moire, ce qui minimise les allocations inutiles. Bien que l'utilisation de la r�flexion introduise un co�t de performance, ce dernier est limit� et n'occasionne pas une surcharge significative dans le cadre de ce projet.

---

### Comment la r�flexion est utilis�e

La r�flexion dans **LinqToSQL** est utilis�e pour inspecter les objets et leurs membres afin de construire dynamiquement les requ�tes SQL sans avoir � cr�er de nouveaux objets ou cha�nes � chaque manipulation. Cela permet de :
- Acc�der aux propri�t�s d'objets dynamiquement.
- Cr�er des conditions, des jointures et des expressions SQL � partir des types d'objets existants.
- R�utiliser des objets existants pour g�n�rer le SQL, r�duisant ainsi les allocations m�moire et am�liorant l'efficacit�.

Cela permet une grande flexibilit� dans la g�n�ration de SQL, tout en �vitant des surcharges de m�moire dues � la cr�ation d'objets suppl�mentaires.

## Fonctionnalit�s

- **Construction de requ�tes SQL** : Cr�ez des requ�tes SQL avec des expressions LINQ.
- **Support des jointures** : Ajoutez des jointures internes (`INNER JOIN`) et gauches (`LEFT JOIN`).
- **Conditions WHERE dynamiques** : Ajoutez des conditions � vos requ�tes avec des m�thodes d'extension.
- **Support des alias** : Utilisez des alias pour les colonnes dans les requ�tes.
- **Filtrage d'insertion** : Marquez les propri�t�s � ignorer lors des op�rations d'insertion avec l'attribut `IgnoreInsert`.
- **Optimisation m�moire** : Gr�ce � l'utilisation de la r�flexion, ce projet acc�de directement aux objets et � leurs membres de mani�re dynamique, r�duisant ainsi l'allocation m�moire en r�utilisant les objets existants et en �vitant la cr�ation de nouveaux objets ou cha�nes � chaque �tape. Cela permet une gestion plus efficace des ressources m�moire tout en pr�servant une flexibilit� maximale.
- **SQL pur** : Le SQL g�n�r� est "pur", ce qui permet une flexibilit� accrue. Vous pouvez interagir directement avec les cha�nes g�n�r�es, les adapter et les utiliser selon vos besoins.
- **Optimisation future : Syst�me de cache** : Une fonctionnalit� future envisag�e serait l'ajout d'un syst�me de **cache** pour les cha�nes SQL g�n�r�es. Ce cache permettrait de stocker les r�sultats pr�-g�n�r�s des requ�tes et d'�viter de recalculer les informations via la r�flexion � chaque nouvelle ex�cution similaire. Cela permettrait d'am�liorer les performances pour des requ�tes r�currentes, similaire � ce que fait **Dapper** en mettant en cache les m�tadonn�es des objets et des propri�t�s.


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
