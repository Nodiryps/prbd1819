﻿using System;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using PRBD_Framework;
using MySql.Data.EntityFramework;

namespace prbd_1819_g19 {
    public enum DbType { MsSQL, MySQL }
    public enum EFDatabaseInitMode { CreateIfNotExists, DropCreateIfChanges, DropCreateAlways }

    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class MySqlModel : Model {
        public MySqlModel(EFDatabaseInitMode initMode) : base("name=library-mysql") {
            switch (initMode) {
                case EFDatabaseInitMode.CreateIfNotExists:
                    Database.SetInitializer<MySqlModel>(new CreateDatabaseIfNotExists<MySqlModel>());
                    break;
                case EFDatabaseInitMode.DropCreateIfChanges:
                    Database.SetInitializer<MySqlModel>(new DropCreateDatabaseIfModelChanges<MySqlModel>());
                    break;
                case EFDatabaseInitMode.DropCreateAlways:
                    Database.SetInitializer<MySqlModel>(new DropCreateDatabaseAlways<MySqlModel>());
                    break;
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            // see: https://blog.craigtp.co.uk/Post/2017/04/05/Entity_Framework_with_MySQL_-_Booleans,_Bits_and_%22String_was_not_recognized_as_a_valid_boolean%22_errors.
            modelBuilder.Properties<bool>().Configure(c => c.HasColumnType("bit"));
        }

        public override void Reseed(string tableName) {
            Database.ExecuteSqlCommand($"ALTER TABLE {tableName} AUTO_INCREMENT=1");
        }
    }

    public class MsSqlModel : Model {
        public MsSqlModel(EFDatabaseInitMode initMode) : base("name=library-mssql") {
            switch (initMode) {
                case EFDatabaseInitMode.CreateIfNotExists:
                    Database.SetInitializer<MsSqlModel>(new CreateDatabaseIfNotExists<MsSqlModel>());
                    break;
                case EFDatabaseInitMode.DropCreateIfChanges:
                    Database.SetInitializer<MsSqlModel>(new DropCreateDatabaseIfModelChanges<MsSqlModel>());
                    break;
                case EFDatabaseInitMode.DropCreateAlways:
                    Database.SetInitializer<MsSqlModel>(new DropCreateDatabaseAlways<MsSqlModel>());
                    break;
            }
        }

        public override void Reseed(string tableName) {
            Database.ExecuteSqlCommand($"DBCC CHECKIDENT('{tableName}', RESEED, 0)");
        }
    }

    public abstract class Model : DbContext {
        protected Model(string name) : base(name) { }

        public static Model CreateModel(DbType type, EFDatabaseInitMode initMode = EFDatabaseInitMode.DropCreateIfChanges) {
            Console.WriteLine($"Creating model for {type}\n");
            switch (type) {
                case DbType.MsSQL:
                    return new MsSqlModel(initMode);
                case DbType.MySQL:
                    return new MySqlModel(initMode);
                default:
                    throw new ApplicationException("Undefined database type");
            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<RentalItem> RentalItems { get; set; }

        public abstract void Reseed(string tableName);

        public void ClearDatabase()
        {
            Users.RemoveRange(Users);
            Rentals.RemoveRange(Rentals);
            RentalItems.RemoveRange(RentalItems);
            BookCopies.RemoveRange(BookCopies);
            Books.RemoveRange(Books);
            Categories.RemoveRange(Categories);

            Reseed(nameof(Users));
            Reseed(nameof(Books));
            Reseed(nameof(Rentals));
            Reseed(nameof(BookCopies));
            Reseed(nameof(Categories));
            Reseed(nameof(RentalItems));
            SaveChanges();
        }

        public void CreateTestData()
        {
            new TestDatas(DbType.MsSQL).Run();
        }

        //birthdate = null et role = Member
        public User CreateUser(string userName, string password, string fullName, string email, DateTime? birthDate = null, Role role = Role.Member)
        {
            User newUser = null;
            if (userName != "" || password != "" || fullName != "" || email != "")
            {
                newUser = Users.Create();

                newUser.UserName = userName; newUser.Password = password;
                newUser.FullName = fullName; newUser.Email = email;
                newUser.BirthDate = birthDate; newUser.Role = role;

                Users.Add(newUser);
                SaveChanges();
            }
            return newUser;
        }

        public Book CreateBook(string isbn, string title, string author, string editor, int numCopies )
        {
       

                Book newBook = Books.Create();
                newBook.Isbn = isbn; newBook.Title = title; newBook.Author = author; newBook.Editor = editor;
                Books.Add(newBook);
                newBook.AddCopies(numCopies, DateTime.Now);
                SaveChanges();
                return newBook;

        }

        public Category CreateCategory(string name)
        {
            Category newCat = null;
            if (Categories.Find(GetCatId(name)) == null)
            {
                newCat = Categories.Create();
                newCat.Name = name;
                newCat.Books = new List<Book>();
                Categories.Add(newCat);

                SaveChanges();
            }
            return newCat;
        }

        private int GetCatId(string catName)
        {
            return (from c in Categories
                    where c.Name == catName
                    select c.CategoryId).FirstOrDefault();
        }

        public List<Book> FindBooksByText(string key)
        {
            List<Book> list = new List<Book>();
            foreach (var b in Books)
            {
                if (b.Author.Contains(key))
                    list.Add(b);
                if (b.Title.Contains(key))
                    list.Add(b);
                if (b.Editor.Contains(key))
                    list.Add(b);
                if (b.Isbn.Contains(key))
                    list.Add(b);
            }
            return list;
        }

        public List<RentalItem> GetActiveRentalItems()
        {
            List<RentalItem> list = new List<RentalItem>();
            var query = from rentItem in RentalItems
                        where rentItem.ReturnDate == null
                        select rentItem;
            foreach(var r in query) 
            {
                list.Add(r);
            }
            return list;
        }
    }
}
