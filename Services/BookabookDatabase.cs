using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using BookabookWPF.Models;
using BookabookWPF.Services.Bookabook.Services;

namespace BookabookWPF.Services
{
    public class BookabookDatabase : SQLiteDatabase
    {
        protected override string FileName { get => "bookabook.db3"; }

        public BookabookDatabase()
        {
            Debug.WriteLine(FullPath);

            // Create the database and tables if they don't exist
            foreach (var modelType in Utility.FindClassesWithAttribute<Attributes.ModelAttribute>())
            {
                var method = typeof(BookabookDatabase).GetMethod("CreateTable")?.MakeGenericMethod(modelType);
                method?.Invoke(this, null);
            }

            /*
            if (GetList<BookClass>().Count == 0)
            {
                var dummyData = new List<BookClass>
                {
                    new BookClass { ID = 0, Title = "Sample Book 1", Author = "Author 1", ISBN = "111-1111111111", Published = new DateTime(2001, 1, 1), Grade = 5, Price = 10.0m, Subject = "PE" },
                    new BookClass { ID = 1, Title = "Sample Book 2", Author = "Author 2", ISBN = "222-2222222222", Published = new DateTime(2002, 2, 2), Grade = 6, Price = 20.0m, Subject = "Math" },
                    new BookClass { ID = 2, Title = "Sample Book 3", Author = "Author 3", ISBN = "333-3333333333", Published = new DateTime(2003, 3, 3), Grade = 7, Price = 30.0m, Subject = "Science" },
                    new BookClass { ID = 3, Title = "Sample Book 4", Author = "Author 4", ISBN = "444-4444444444", Published = new DateTime(2004, 4, 4), Grade = 8, Price = 40.0m, Subject = "History" },
                    new BookClass { ID = 4, Title = "Sample Book 5", Author = "Author 5", ISBN = "555-5555555555", Published = new DateTime(2005, 5, 5), Grade = 9, Price = 50.0m, Subject = "Geography" }
                };
                InsertAll(dummyData);
            }
            */
        }
    }
}
