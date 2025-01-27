using SQLite;
using BookabookWPF.Attributes;
using BookabookWPF.Services;
using System.ComponentModel.DataAnnotations.Schema;
using BookabookWPF.Models;
using System.Reflection.Metadata;

namespace BookabookWPF.Models
{
    [Model]
    public class BookInstance : ModelBase
    {
        private int _id;
        private int? _bookClassId;
        private string? _bookCondition;

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

        [ForeignKey("BookabookWPF.Models.BookClass")]
        [MayNotBeNull]
        public int? BookClassID
        {
            get => _bookClassId;
            set
            {
                if (_bookClassId != value)
                {
                    _bookClassId = value;
                    OnPropertyChanged(nameof(BookClassID));
                }
            }
        }

        [MultipleInDatabase]
        public string? BookCondition
        {
            get => _bookCondition;
            set
            {
                if (_bookCondition != value)
                {
                    _bookCondition = value;
                    OnPropertyChanged(nameof(BookCondition));
                }
            }
        }

        public override object Clone()
        {
            return new BookInstance
            {
                BookClassID = BookClassID,
                BookCondition = BookCondition
            };
        }
    }
}
