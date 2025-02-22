﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PRBD_Framework;

namespace prbd_1819_g19
{
    public class Category : EntityBase<Model>
    {
        [Key]
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Book> Books { get; set; } = new HashSet<Book>();


        protected Category(){}/////////////////////////////CONSTRUCT

        public bool HasBook(Book book)
        {
            return Books.Contains(book);
        }

        public void AddBook(Book book)
        {
            if(!Books.Contains(book))
                Books.Add(book);
            
        }

        public void RemoveBook(Book book)
        {
            if (Books.Contains(book))
                Books.Remove(book);
        }

        public void Delete()
        {
            if (Model.Categories.Find(this) != null)
            {
                Model.Categories.Remove(this);
                Model.SaveChanges();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
    
}