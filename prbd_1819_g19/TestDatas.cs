﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static prbd_1819_g19.Program;

namespace prbd_1819_g19
{
    class TestDatas : EntityBase<Model>
    {
        private readonly DbType dbType;

        private Model model;

        private User admin, ben, bruno;
        private List<User> users = new List<User>();

        private Category catInformatique, catScienceFiction, catRoman, catLitterature, catEssai, catFantastique;
        private List<Category> categories = new List<Category>();

        private Book book1, book2, book3, book4, book5, book6, book7, book8, book9, book10;
        private List<Book> books = new List<Book>();

        public TestDatas(DbType dbType)
        {
            this.dbType = dbType;
        }

        public void Run()
        {
            using (model = Model.CreateModel(dbType))
            {
                model.Database.Log = Console.Write;
                model.ClearDatabase(); // vide la DB et Reseed les champs auto-incrémentés
                CreateEntities(model);
                testBooks();
                testCategory();
                testBasket();
                testModel();
            }
        }

        private void CreateEntities(Model model)
        {
            Console.WriteLine("Creating test data... ");
            CreateUsers();
            CreateCategories();
            CreateBooks();
        }

        private void CreateUsers()
        {
            admin = model.CreateUser("admin", "admin", "Administrator", "admin@test.com", null, role: Role.Admin);
            ben = model.CreateUser("ben", "ben", "Benoît Penelle", "ben@test.com", new DateTime(1968, 10, 1), role: Role.Member);
            bruno = model.CreateUser("bruno", "bruno", "Bruno Lacroix", "bruno@test.com");
            users.AddRange(new User[] { admin, ben, bruno });
        }

        private void CreateCategories()
        {
            catInformatique = model.CreateCategory("Informatique");
            catScienceFiction = model.CreateCategory("Science Fiction");
            catRoman = model.CreateCategory("Roman");
            catLitterature = model.CreateCategory("Littérature");
            catEssai = model.CreateCategory("Essai");
            catFantastique = model.CreateCategory("Fantastique");
            categories.AddRange(new Category[] { catInformatique, catScienceFiction, catRoman, catLitterature, catEssai, catFantastique });
        }

        private void CreateBooks()
        {
            book1 = model.CreateBook(
                isbn: "123",
                title: "Java for Dummies",
                author: "Duchmol",
                editor: "EPFC",
                numCopies: 1);
            book1.PicturePath = "123.jpg";
            book2 = model.CreateBook(
                isbn: "456",
                title: "Le Seigneur des Anneaux",
                author: "Tolkien",
                editor: "Bourgeois",
                numCopies: 1);
            book2.PicturePath = "456.jpg";
            book3 = model.CreateBook(
                isbn: "789",
                title: "Les misérables",
                author: "Victor Hugo",
                editor: "XO",
                numCopies: 1);
            book3.PicturePath = "789.jpg";

            book4 = model.CreateBook(
                isbn: "741",
                title: "Harry Potter 1",
                author: "J.K. Rowling",
                editor: "XO",
                numCopies: 10);
            book4.PicturePath = "741.jpg";

            book5 = model.CreateBook(
                isbn: "001",
                title: "Harry Potter 2",
               author: "J.K. Rowling",
                editor: "XO",
                numCopies: 10);
            book5.PicturePath = "001.jpg";

            book6 = model.CreateBook(
                isbn: "002",
                title: "Harry Potter 2",
               author: "J.K. Rowling",
                editor: "XO",
                numCopies: 10);
            book6.PicturePath = "002.jpg";

            book7 = model.CreateBook(
                isbn: "003",
                title: "Harry Potter 3",
               author: "J.K. Rowling",
                editor: "XO",
                numCopies: 5);
            book7.PicturePath = "003.jpg";

            book8 = model.CreateBook(
                isbn: "004",
                title: "Harry Potter 4",
               author: "J.K. Rowling",
                editor: "XO",
                numCopies: 10);
            book8.PicturePath = "004.jpg";

            book9 = model.CreateBook(
                isbn: "005",
                title: "Harry Potter 5",
               author: "J.K. Rowling",
                editor: "XO",
                numCopies: 7);
            book9.PicturePath = "005.jpg";

            book10 = model.CreateBook(
                isbn: "006",
                title: "Harry Potter 6",
                author: "J.K. Rowling",
                editor: "XO",
                numCopies: 9);
            book10.PicturePath = "006.jpg";

            books.AddRange(new Book[] { book1, book2, book3, book4, book5, book6, book7, book8, book9, book10 });
        }

        private void testBooks()
        {
            runTest("Test livres", () => {
                book1.AddCategory(catInformatique);
                book2.AddCategories(new Category[] { catRoman, catScienceFiction });
                book3.AddCategories(new Category[] { catRoman, catLitterature });
                book4.AddCategories(new Category[] { catRoman, catFantastique });
                book5.AddCategories(new Category[] { catRoman, catFantastique });
                book6.AddCategories(new Category[] { catEssai, catFantastique });
                book7.AddCategories(new Category[] { catInformatique, catFantastique });
                book8.AddCategories(new Category[] { catRoman, catFantastique });
                book9.AddCategories(new Category[] { catRoman, catLitterature });
                book10.AddCategories(new Category[] { catScienceFiction, catFantastique, catInformatique });

                printList("Books", books);
                Console.WriteLine($"book1.RemoveCategory(catInformatique) : suppression de {catInformatique}");
                //book1.RemoveCategory(catInformatique);
                //Debug.Assert(book1.Categories.Count == 0);
                Console.WriteLine($"book2.RemoveCategory(catEssai) : suppression de {catEssai} (inexistante dans ce livre)");
                book2.RemoveCategory(catEssai);
                printList("Books", books);
                testBookCopies();
            });
        }

        private void testBookCopies()
        {
            Console.WriteLine($"Ajout de 3 copies à book3");
            book3.AddCopies(3, new DateTime(2018, 12, 31, 17, 30, 0));
            printList<BookCopy>("book3.Copies", book3.Copies);
            Debug.Assert(book3.NumAvailableCopies == 4);
            Console.WriteLine("obtention d'une copie du book3 - BookCopy bookCopy = book3.GetAvailableCopy()");
            explicationGetAvailableCopy();
            BookCopy bookCopy = book3.GetAvailableCopy();
            Console.WriteLine($"bookCopy : {bookCopy}");
            Console.WriteLine($"suppression de bookCopy - book3.DeleteCopy(bookCopy)");
            book3.DeleteCopy(bookCopy);
            Debug.Assert(book3.NumAvailableCopies == 3);
            printList<BookCopy>("book3.Copies", book3.Copies);
        }

        private void explicationGetAvailableCopy()
        {
            Console.WriteLine("\nLa méthode book.GetAvailableCopy() retourne une copie de book qui n'est pas référencé par un RentalItem avec une date de retour à null\n");
        }

        private void testCategory()
        {
            runTest("Test Category", () => {
                printList("catEssai.Books", catEssai.Books);
                Console.WriteLine("catEssai.HasBook(book1) : " + catEssai.HasBook(book1));
                Console.WriteLine("catEssai.AddBook(book1)");
                catEssai.AddBook(book1);
                printList("catEssai.Books", catEssai.Books);
                Console.WriteLine("catEssai.RemoveBook(book1)");
                catEssai.RemoveBook(book1);
                printList("catEssai.Books", catEssai.Books);
            });
        }

        private void testBasket()
        {
            runTest("Test Basket", () =>
            {
                Console.WriteLine("Création d'un panier pour ben contenant des copies de book1, book2, book3");
                Console.WriteLine("Appels : ben.AddToBasket(book1); ben.AddToBasket(book2)");
                explicationAddToBasket();
                ben.AddToBasket(book1);
                ben.AddToBasket(book2);
                Console.WriteLine("Appel RentalItem rentalItemBook3 = ben.AddToBasket(book3); On récupère le rentalItem créé");
                RentalItem rentalItemBook3 = ben.AddToBasket(book3);
                Console.WriteLine(ben.Basket);
                printList("Rental Items du panier de ben", ben.Basket.Items);
                Console.WriteLine("Suppression d'un élément du panier de ben - ben.RemoveFromBasket(rentalItemBook3)");
                explicationRemoveFromBasket();
                ben.RemoveFromBasket(rentalItemBook3);
                printList("Rental Items du panier de ben", ben.Basket.Items);
                Console.WriteLine("Confirmation du panier de ben - basket.Confirm()");
                explicationConfirm();
                ben.ConfirmBasket();
                Console.WriteLine(ben.Basket);
                Console.WriteLine("Re-Création du panier de ben essayant d'ajouter des copies de book1, book2, book3");
                Console.WriteLine("");
                Console.WriteLine("On constate que ce ne sont pas les mêmes copies (puisque les précédentes sont déjà louées)");
                ben.AddToBasket(book1);
                ben.AddToBasket(book2);
                ben.AddToBasket(book3);
                Console.WriteLine(ben.Basket);
                printList("Rental Items du panier de ben", ben.Basket.Items);
                Console.WriteLine("Vidage du panier de ben - ben.ClearBasket()");
                ben.ClearBasket();
                Console.WriteLine(ben.Basket);
            });
        }

        private void explicationAddToBasket()
        {
            Console.WriteLine("\nLa méthode user.AddToBasket(Book book) doit :");
            Console.WriteLine("\t- obtenir le basket courant de user (ou le créer si il n'existe pas)");
            Console.WriteLine("\t- obtenir une copie disponible de book : bookCopy");
            Console.WriteLine("\t- si une copie est disponible, appeler la méthode rental.RentCopy(bookCopy)");
            explicationRentCopy();
            Console.WriteLine("\t- retourne le RentalItem créé\n");
        }

        private void explicationRentCopy()
        {
            Console.WriteLine("\n\t(La méthode rental.RentCopy(BookCopy bookCopy) crée un nouvel RentalItem et lui associe bookCopy)\n");
        }

        private void explicationRemoveFromBasket()
        {
            Console.WriteLine("\nLa méthode user.RemoveFromBasket(RentalItem rentalItem) retire rentalItem du panier (qui est un Rental) courant");
            Console.WriteLine("\tAttention, il faut que le panier existe");
            Console.WriteLine("\tFait appel à Rental.RemoveItem(rentalItem) qui retire l'item de la liste des items du Rental\n");
        }

        private void explicationConfirm()
        {
            Console.WriteLine("\nLa méthode Rental.Confirm() :");
            Console.WriteLine("\t- donne la date courante comme RentalDate au rental");
            Console.WriteLine("\t- sauvegarde le panier");
            Console.WriteLine("\t- Attention : après l'appel à cette méthode sur un panier courant, celui-ci n'existe plus (puisqu'il a été sauvegardé)");
        }

        private void explicationClearBasket()
        {
            Console.WriteLine("\nLa méthode user.ClearBasket() vide le panier courant (s'il existe)");
            Console.WriteLine("\tAttention, il faut que le panier existe");
            Console.WriteLine("\tFait appel à Rental.Clear() qui vide la liste des items du Rental\n");
        }

        private void testModel()
        {
            runTest("Test Model", () =>
            {
                List<Book> search = model.FindBooksByText("Tolkien");
                printList("model.FindBooksByText(\"Tolkien\")", search);
                explicationFindBooksByText();
                printList("model.FindRentalItemsActive()", model.GetActiveRentalItems());
                explicationFindRentalItemsActive();
            });

        }

        private void explicationFindBooksByText()
        {
            Console.WriteLine("\nLa méthode model.FindBooksByText(str) retourne une liste de livres contenant le String 'str' dans :");
            Console.WriteLine("\tISBN, Author, Editor, Title\n");
        }

        private void explicationFindRentalItemsActive()
        {
            Console.WriteLine("\nLa méthode model.FindRentalItemsActive() retourne la liste d'items actifs, c'est à dire ceux dont ReturnDate est à null\n");
        }

        private void runTest(String title, Action action)
        {
            Console.WriteLine($"\n{ title }");
            action.Invoke();
            Console.WriteLine("--------------------------------------------------------------------------------------------------------");
        }

        private void printList<T>(String title, ICollection<T> list)
        {
            Console.WriteLine($"\n{title} :");
            String s = String.Join("\n", list);
            Console.WriteLine(s + "\n");
        }
    }
}
