using System.ComponentModel.DataAnnotations;
using BookabookWPF.Attributes;
using SQLite;
using BookabookWPF.Services;
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;

namespace BookabookWPF.Models
{
    [Model]
    public class BookClass : ModelBase
    {

        public static bool ValidateISBN(object? value)
        {
            // Check if the value is a string
            if (value is string isbn)
            {
                // Remove all helping "-" characters 
                isbn = isbn.Replace("-", string.Empty);

                // Check if string only contains digits and has a length of 10 or 13
                return long.TryParse(isbn, out _) && (isbn.Length == 10 || isbn.Length == 13);
            }
            // Otherwise return false
            return true;
        }

        private int _id;
        private string? _isbn;
        private string? _title;
        private string? _author;
        private DateTime? _published;
        private decimal? _price;
        private int? _grade;
        private string? _subject;

        [PrimaryKey, AutoIncrement]
        public int ID
        {
            get => _id;
            set 
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(ID));
                }
            }
        }

        [RequiresValidation("BookabookWPF.Models.BookClass.ValidateISBN", "An ISBN has either the format X-XXXXX-XXX-X or XXX-X-XXXX-XXXX-X")]
        public string? ISBN
        {
            get => _isbn;
            set
            {
                if (_isbn != value)
                {
                    _isbn = value;
                    OnPropertyChanged(nameof(ISBN));
                }
            }
        }

        public string? Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        [MultipleInDatabase]
        public string? Author
        {
            get => _author;
            set
            {
                if (_author != value)
                {
                    _author = value;
                    OnPropertyChanged(nameof(Author));
                }
            }
        }

        public DateTime? Published
        {
            get => _published;
            set
            {
                if (_published != value)
                {
                    _published = value;
                    OnPropertyChanged(nameof(Published));
                }
            }
        }

        public decimal? Price
        {
            get => _price;
            set
            {
                if (_price != value)
                {
                    _price = value;
                    OnPropertyChanged(nameof(Price));
                }
            }
        }

        [Range(5, 13)]
        public int? Grade
        {
            get => _grade;
            set
            {
                if (_grade != value)
                {
                    if (value.HasValue && (value.Value < 5 || value.Value > 13))
                    {
                        throw new ArgumentOutOfRangeException(nameof(Grade), "Grade must be between 5 and 13");
                    }
                    _grade = value;
                    OnPropertyChanged(nameof(Grade));
                }
            }
        }

        [MultipleInDatabase]
        public string? Subject
        {
            get => _subject;
            set
            {
                if (_subject != value)
                {
                    _subject = value;
                    OnPropertyChanged(nameof(Subject));
                }
            }
        }

        public override object Clone()
        {
            return new BookClass
            {
                ISBN = ISBN,
                Title = Title,
                Author = Author,
                Published = Published,
                Price = Price,
                Grade = Grade,
                Subject = Subject
            };
        }
    }
}