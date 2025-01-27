
using BookabookWPF.Attributes;
using BookabookWPF.Services;
using SQLite;

namespace BookabookWPF.Models
{
    [Model]
    public class Student : ModelBase
    {
        private int _id;
        private string? _firstName;
        private string? _lastName;
        private int? _fifthClassYear;
        private string? _classLetter;
        private string? _notes;

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

        [MayNotBeNull]
        [MultipleInDatabase]
        public string? FirstName
        {
            get => _firstName;
            set
            {
                if (_firstName != value)
                {
                    _firstName = value;
                    OnPropertyChanged(nameof(FirstName));
                }
            }
        }

        [MayNotBeNull]
        public string? LastName
        {
            get => _lastName;
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    OnPropertyChanged(nameof(LastName));
                }
            }
        }

        public int? FifthClassYear
        {
            get => _fifthClassYear;
            set
            {
                if (_fifthClassYear != value)
                {
                    _fifthClassYear = value;
                    OnPropertyChanged(nameof(FifthClassYear));
                }
            }
        }

        public string? ClassLetter
        {
            get => _classLetter;
            set
            {
                if (_classLetter != value)
                {
                    _classLetter = value;
                    OnPropertyChanged(nameof(ClassLetter));
                }
            }
        }

        public string? Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged(nameof(Notes));
                }
            }
        }

        public override object Clone()
        {
            return new Student
            {
                FirstName = FirstName,
                LastName = LastName,
                FifthClassYear = FifthClassYear,
                ClassLetter = ClassLetter,
                Notes = Notes
            };
        }
    }
}
