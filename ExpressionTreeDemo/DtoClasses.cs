using System;
using System.Collections.Generic;

namespace ExpressionTreeDemo
{
    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public DateTime Dob { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}, Age: {Age}, Dob: {Dob}";
        }
    }

    public class Document
    {
        public string Title { get; set; }

        public DateTime IssuedBy { get; set; }

        public override string ToString()
        {
            return $"Title: {Title}, IssuedBy: {IssuedBy}";
        }
    }

    public class Recipe
    {
        public string Name { get; set; }

        public IList<string> Ingredients { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"{Name}: {{ {string.Join(", ", Ingredients)} }}";
        }
    }
}
