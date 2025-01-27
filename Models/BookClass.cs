using System.ComponentModel.DataAnnotations;
using BookabookWPF.Attributes;
using SQLite;
using BookabookWPF.Services;
using System.ComponentModel;

namespace BookabookWPF.Models
{
    [Model]
    public class BookClass : ModelBase
    {
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