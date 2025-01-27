using BookabookWPF.Services;
using BookabookWPF.Attributes;
using System.ComponentModel.DataAnnotations.Schema;
using SQLite;

namespace BookabookWPF.Models
{
    [Model]
    public class Rent : ModelBase
    {
        private int _id;
        private int _bookInstanceId;
        private int _studentId;
        private DateTime _rentSince;
        private DateTime? _rentUntil;
        private DateTime? _returnedDate;

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

        [ForeignKey("BookabookWPF.Models.BookInstance")]
        public int BookInstanceID
        {
            get => _bookInstanceId;
            set
            {
                if (_bookInstanceId != value)
                {
                    _bookInstanceId = value;
                    OnPropertyChanged(nameof(BookInstanceID));
                }
            }
        }

        [ForeignKey("BookabookWPF.Models.Student")]
        public int StudentID
        {
            get => _studentId;
            set
            {
                if (_studentId != value)
                {
                    _studentId = value;
                    OnPropertyChanged(nameof(StudentID));
                }
            }
        }

        public DateTime RentSince
        {
            get => _rentSince;
            set
            {
                if (_rentSince != value)
                {
                    _rentSince = value;
                    OnPropertyChanged(nameof(RentSince));
                }
            }
        }

        public DateTime? RentUntil
        {
            get => _rentUntil;
            set
            {
                if (_rentUntil != value)
                {
                    _rentUntil = value;
                    OnPropertyChanged(nameof(RentUntil));
                }
            }
        }

        public DateTime? ReturnedDate
        {
            get => _returnedDate;
            set
            {
                if (_returnedDate != value)
                {
                    _returnedDate = value;
                    OnPropertyChanged(nameof(ReturnedDate));
                }
            }
        }

        public override object Clone()
        {
            return new Rent
            {
                ID = ID,
                BookInstanceID = BookInstanceID,
                StudentID = StudentID,
                RentSince = RentSince,
                RentUntil = RentUntil,
                ReturnedDate = ReturnedDate
            };
        }
    }
}
