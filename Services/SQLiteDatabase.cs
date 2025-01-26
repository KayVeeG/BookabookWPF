using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BookabookWPF.Services
{
    using SQLite;

    namespace Bookabook.Services
    {
        public class SQLiteDatabase : IDisposable
        {
            protected virtual string FileName { get => "sqlite.db3"; }

            public string FullPath { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FileName); }

            protected SQLiteConnection connection;

            public SQLiteDatabase()
            {
                connection = new SQLiteConnection(FullPath);
            }

            public void Insert<T>(T obj) where T : new()
            {
                connection.Insert(obj);
            }

            public void Update<T>(T obj) where T : new()
            {
                connection.Update(obj);
            }

            public void Delete<T>(T obj) where T : new()
            {
                connection.Delete(obj);
            }

            public List<T> GetList<T>() where T : new()
            {
                return connection.Table<T>().ToList();
            }

            public T GetById<T>(int id) where T : new()
            {
                return connection.Get<T>(id);
            }

            public T? GetMinValue<T>(string tableName, string columnName)
            {
                return connection.ExecuteScalar<T>($"SELECT MIN({columnName}) FROM {tableName}");
            }

            public T? GetMaxValue<T>(string tableName, string columnName)
            {
                return connection.ExecuteScalar<T>($"SELECT MAX({columnName}) FROM {tableName}");
            }

            public IEnumerable<string> GetDistinctValues(string tableName, string columnName)
            {
                return connection.QueryScalars<string>(
                    $"SELECT DISTINCT {columnName} FROM {tableName} WHERE {columnName} IS NOT NULL");
            }

            public void CreateTable<T>() where T : new()
            {
                connection.CreateTable<T>();
            }

            public void InsertAll<T>(IEnumerable<T> objects) where T : new()
            {
                connection.RunInTransaction(() =>
                {
                    connection.InsertAll(objects);
                });
            }

            public void Dispose()
            {
                connection?.Dispose();
            }
        }
    }
}
