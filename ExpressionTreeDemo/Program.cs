using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressionTreeDemo
{
    internal class Program
    {
        private static readonly IQueryable<Person> _people;
        private static readonly IQueryable<Document> _documents;
        private static readonly IQueryable<Recipe> _recipes;

        static Program() => (_people, _documents, _recipes) = SetupData();

        static void Main(string[] args)
        {
            DemoGetField();
            DemoGetInRange();
            DemoGetTextFieldStartsWith();
            DemoGetWithFieldContainValue();
        }

        private static void DemoGetField()
        {
            var names = GetField<Person, string>(_people, nameof(Person.Name));
            PrintCollection(names);
            var issuedDates = GetField<Document, DateTime>(_documents, nameof(Document.IssuedBy));
            PrintCollection(issuedDates);
        }

        private static void DemoGetInRange()
        {
            var peopleInRange = GetInRange(
                _people, nameof(Person.Dob), new DateTime(1980, 12, 31), new DateTime(1995, 1, 1));
            PrintCollection(peopleInRange);
            var documentsInRange = GetInRange(
                _documents, nameof(Document.IssuedBy), new DateTime(1980, 12, 31), new DateTime(2005, 1, 1));
            PrintCollection(documentsInRange);
        }

        private static void DemoGetTextFieldStartsWith()
        {
            var peopleStartWithJo = GetTextFieldStartsWith(_people, nameof(Person.Name), "Jo");
            PrintCollection(peopleStartWithJo);
            var documentsStartWithMa = GetTextFieldStartsWith(_documents, nameof(Document.Title), "Ma");
            PrintCollection(documentsStartWithMa);
        }

        private static void DemoGetWithFieldContainValue()
        {
            var recipeWithEggs = GetWithFieldContainValue(_recipes, nameof(Recipe.Ingredients), "eggs");
            PrintCollection(recipeWithEggs);
        }

        private static IQueryable<TColumn> GetField<TSource, TColumn>(IQueryable<TSource> collection, string columnName)
        {
            var collectionTypeExpr = Expression.Parameter(typeof(TSource));
            var columnPropertyExpr = Expression.Property(collectionTypeExpr, columnName);
            var predicate = Expression.Lambda<Func<TSource, TColumn>>(columnPropertyExpr, collectionTypeExpr);

            return collection.Select(predicate);
        }

        private static IQueryable<TSource> GetInRange<TSource>(
            IQueryable<TSource> collection, string timeColumnName, DateTime lower, DateTime upper)
        {
            var parameterExpr = Expression.Parameter(typeof(TSource));
            var timePropertyExpr = Expression.Property(parameterExpr, timeColumnName);

            var olderThanExpr = Expression.GreaterThan(timePropertyExpr, Expression.Constant(lower));
            var newerThanExpr = Expression.LessThan(timePropertyExpr, Expression.Constant(upper));
            var timeRangeExpr = Expression.And(newerThanExpr, olderThanExpr);

            var predicate = Expression.Lambda<Func<TSource, bool>>(timeRangeExpr, parameterExpr);

            return collection.Where(predicate);
        }

        private static IQueryable<TSource> GetTextFieldStartsWith<TSource>(
            IQueryable<TSource> collection, string columnName, string prefix)
        {
            var methodInfo = typeof(string).GetMethods()
                .Single(m => m.Name == nameof(string.StartsWith) &&
                    m.GetParameters().Length == 1 &&
                    m.GetParameters().Single().ParameterType == typeof(string));

            var parameterExpr = Expression.Parameter(typeof(TSource));
            var columnProperty = Expression.Property(parameterExpr, columnName);

            var startsWithExpr = Expression.Call(columnProperty, methodInfo, Expression.Constant(prefix));

            var predicate = Expression.Lambda<Func<TSource, bool>>(startsWithExpr, parameterExpr);

            return collection.Where(predicate);
        }

        private static IQueryable<TSource> GetWithFieldContainValue<TSource, TField>(
            IQueryable<TSource> collection, string columnName, TField value)
        {
            var methodInfo = typeof(Enumerable).GetMethods()
                .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);
            var containsMethod = methodInfo.MakeGenericMethod(typeof(TField));

            var parameterExpr = Expression.Parameter(typeof(TSource));
            var columnProperty = Expression.Property(parameterExpr, columnName);

            var containsExpr = Expression.Call(containsMethod, columnProperty, Expression.Constant(value));

            var predicate = Expression.Lambda<Func<TSource, bool>>(containsExpr, parameterExpr);

            return collection.Where(predicate);
        }

        private static void PrintCollection<T>(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                Console.WriteLine(item);
            }
        }

        private static (IQueryable<Person>, IQueryable<Document>, IQueryable<Recipe>) SetupData()
        {
            var people = new List<Person>
            {
                new Person
                {
                    Name = "John Doe",
                    Age = 42,
                    Dob = new DateTime(1980, 1, 1)
                },
                new Person
                {
                    Name = "Jane Doe",
                    Age = 41,
                    Dob = new DateTime(1981, 1, 1)
                },
                new Person
                {
                    Name = "Baby Doe",
                    Age = 12,
                    Dob = new DateTime(2010, 1, 1)
                },
            };

            var documents = new List<Document>
            {
                new Document
                {
                    Title = "Birth Certificate",
                    IssuedBy = new DateTime(1980, 1, 1)
                },
                new Document
                {
                    Title = "College Degree",
                    IssuedBy = new DateTime(2003, 8, 1)
                },
                new Document
                {
                    Title = "Marriage Certificate",
                    IssuedBy = new DateTime(2008, 1, 1)
                }
            };

            var recipes = new List<Recipe>
            {
                new Recipe
                {
                    Name = "Fried Rice",
                    Ingredients = new List<string>
                    { "eggs", "rice", "oil", "vegetables" }
                },
                new Recipe
                {
                    Name = "Omelette",
                    Ingredients = new List<string>
                    { "eggs", "butter", "oil" }
                },
                new Recipe
                {
                    Name = "Pho",
                    Ingredients = new List<string>
                    { "pho", "chicken", "spice" }
                },
                new Recipe
                {
                    Name = "sandwich",
                    Ingredients = new List<string>
                    { "bread", "ham", "vegetables" }
                }
            };

            return (people.AsQueryable(), documents.AsQueryable(), recipes.AsQueryable());
        }
    }
}
