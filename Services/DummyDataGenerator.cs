using BookabookWPF;
using BookabookWPF.Models;
using BookabookWPF.Services;
using BookabookWPF.Services.Bookabook.Services;
using System;
using System.Collections.Generic;

public class DummyDataGenerator
{
    private static readonly string[] BookTitles = {
        "Mathematics Fundamentals Volume 1",
        "Advanced Biology Concepts",
        "World History: Modern Era",
        "English Literature Classics",
        "Chemistry in Practice",
        "Physics Made Simple",
        "Spanish for Beginners",
        "Geography: Our World",
        "Computer Science Basics",
        "Art History Through Ages",
        "Music Theory 101",
        "Physical Education Guide",
        "Environmental Science",
        "Economics Principles",
        "Ancient Civilizations"
    };

    private static readonly string[] Authors = {
        "Dr. Sarah Johnson",
        "Prof. Michael Chen",
        "Dr. Emily Williams",
        "James Anderson",
        "Dr. Robert Miller",
        "Lisa Thompson",
        "Prof. David Garcia",
        "Maria Rodriguez",
        "Dr. John Smith",
        "Prof. Amanda White"
    };

    private static readonly string[] Subjects = {
        "Mathematics",
        "Biology",
        "History",
        "English",
        "Chemistry",
        "Physics",
        "Spanish",
        "Geography",
        "Computer Science",
        "Art"
    };

    private static readonly string[] Conditions = {
        "New",
        "Excellent",
        "Good",
        "Fair",
        "Poor"
    };

    private static readonly string[] FirstNames = {
        "Emma", "Liam", "Olivia", "Noah", "Ava",
        "Oliver", "Isabella", "William", "Sophia", "James",
        "Charlotte", "Benjamin", "Mia", "Lucas", "Harper",
        "Henry", "Evelyn", "Theodore", "Alexander", "Sofia"
    };

    private static readonly string[] LastNames = {
        "Smith", "Johnson", "Williams", "Brown", "Jones",
        "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
        "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
        "Thomas", "Taylor", "Moore", "Jackson", "Martin"
    };

    private static readonly string[] ClassLetters = { "A", "B", "C", "D" };

    public static void GenerateDummyData()
    {
        var database = Globals.Database;
        var random = new Random();

        // Generate BookClass entries
        var bookClasses = new List<BookClass>();
        for (int i = 0; i < 50; i++)
        {
            var bookClass = new BookClass
            {
                Title = BookTitles[random.Next(BookTitles.Length)] + " " + (i + 1),
                Author = Authors[random.Next(Authors.Length)],
                ISBN = GenerateISBN(random),
                Published = DateTime.Now.AddYears(-random.Next(1, 10))
                    .AddMonths(-random.Next(12))
                    .AddDays(-random.Next(365)),
                Price = random.Next(1000, 10000) / 100.0m,
                Grade = random.Next(5, 14),
                Subject = Subjects[random.Next(Subjects.Length)]
            };
            bookClasses.Add(bookClass);
            database.Insert(bookClass);
        }

        // Generate Students
        var students = new List<Student>();
        for (int i = 0; i < 100; i++)
        {
            var student = new Student
            {
                FirstName = FirstNames[random.Next(FirstNames.Length)],
                LastName = LastNames[random.Next(LastNames.Length)],
                FifthClassYear = DateTime.Now.Year - random.Next(0, 8),
                ClassLetter = ClassLetters[random.Next(ClassLetters.Length)],
                Notes = random.Next(5) == 0 ? "Some notes about the student" : null
            };
            students.Add(student);
            database.Insert(student);
        }

        // Generate BookInstances
        var bookInstances = new List<BookInstance>();
        foreach (var bookClass in bookClasses)
        {
            // Create 1-5 instances for each book class
            int instanceCount = random.Next(1, 6);
            for (int i = 0; i < instanceCount; i++)
            {
                var bookInstance = new BookInstance
                {
                    BookClassID = bookClass.ID,
                    BookCondition = Conditions[random.Next(Conditions.Length)]
                };
                bookInstances.Add(bookInstance);
                database.Insert(bookInstance);
            }
        }

        // Generate Rents
        foreach (var student in students)
        {
            // 0-3 rentals per student
            int rentalCount = random.Next(4);
            for (int i = 0; i < rentalCount; i++)
            {
                var instance = bookInstances[random.Next(bookInstances.Count)];
                var rentStart = DateTime.Now.AddDays(-random.Next(1, 365));
                var rent = new Rent
                {
                    BookInstanceID = instance.ID,
                    StudentID = student.ID,
                    RentSince = rentStart,
                    RentUntil = rentStart.AddMonths(random.Next(1, 6)),
                    ReturnedDate = random.Next(2) == 0 ? rentStart.AddDays(random.Next(1, 180)) : null
                };
                database.Insert(rent);
            }
        }
    }

    private static string GenerateISBN(Random random)
    {
        // Generate ISBN-13 format: XXX-X-XXXXX-XXX-X
        var parts = new[]
        {
            random.Next(100, 999).ToString(),
            random.Next(0, 9).ToString(),
            random.Next(10000, 99999).ToString(),
            random.Next(100, 999).ToString(),
            random.Next(0, 9).ToString()
        };
        return string.Join("-", parts);
    }
}