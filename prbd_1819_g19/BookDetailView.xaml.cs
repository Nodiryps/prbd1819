﻿using Microsoft.Win32;
using PRBD_Framework;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Input;

namespace prbd_1819_g19
{
    /// <summary>
    /// Logique d'interaction pour BookDetailView.xaml
    /// </summary>
    public partial class BookDetailView : UserControlBase
    {
        public User Member { get; set; }
        private ImageHelper imageHelper;

        private ObservableCollection<Category> cats;
        public ObservableCollection<Category> Cats
        {
            get => cats;
            set => SetProperty<ObservableCollection<Category>>(ref cats, value);
        }

        private int quantity;
        public int Quantity
        {
            get { return quantity; }
            set { quantity = value;
               RaisePropertyChanged(nameof(Quantity));
            }
        }


        private bool isNew;
        public bool IsNew
        {
            get { return isNew; }
            set
            {
                isNew = value;
                RaisePropertyChanged(nameof(IsNew));
                RaisePropertyChanged(nameof(IsExisting));
            }
        }

        public bool IsExisting { get => !isNew; }


        public string PicturePath
        {
            get { return Book.AbsolutePicturePath; }
            set
            {
                Book.PicturePath = value;
                RaisePropertyChanged(nameof(PicturePath));
            }
        }



        private Book book;
        public Book Book
        {
            get => book;
            set => SetProperty<Book>(ref book, value);
        }

        public string ISBN
        {
            get { return book.Isbn; }
            set
            {
                book.Isbn = value;
                RaisePropertyChanged(nameof(ISBN));
                App.NotifyColleagues(AppMessages.MSG_ISBN_CHANGED, string.IsNullOrEmpty(value) ? "<new book>" : value);
            }
        }

        private void ValidateQuantite()
        {
            ClearErrors();
            if (Quantity < 0)
                AddError("Quantity", Properties.Resources.Error_NbCopiesNotValid);
            RaiseErrors();
        }

        public string Title
        {
            get { return Book.Title; }
            set
            {
                Book.Title = value;
                RaisePropertyChanged(nameof(Title));
            }
        }

        public string Author
        {
            get { return book.Author; }
            set
            {
                book.Author = value;
                RaisePropertyChanged(nameof(Author));
            }
        }
        private DateTime selectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                selectedDate = value;
                RaisePropertyChanged(nameof(SelectedDate));
            }
        }


        public string Editor
        {
            get { return book.Editor; }
            set
            {
                book.Editor = value;
                RaisePropertyChanged(nameof(Editor));
            }
        }



        public string AbsolutePicturePath
        {
            get { return book.AbsolutePicturePath; }
            set { }
        }        

        public ICommand Save { get; set; }
        public ICommand Cancel { get; set; }
        public ICommand Delete { get; set; }
        public ICommand LoadImage { get; set; }
        public ICommand ClearImage { get; set; }
        public ICommand AddCopy { get; set; }

#if DEBUG_USERCONTROLS_WITH_TIMER
        private Timer timer = new Timer(1000);
#endif

        public BookDetailView(Book book, bool isNew)
        {
            InitializeComponent();

            DataContext = this;
            Book = book;
            IsNew = isNew;
            Cats = new ObservableCollection<Category>(App.Model.Categories);

            imageHelper = new ImageHelper(App.IMAGE_PATH, Book.PicturePath);

            Save = new RelayCommand(SaveAction, CanSaveOrCancelAction);
            Cancel = new RelayCommand(CancelAction, CanSaveOrCancelAction);
            Delete = new RelayCommand(DeleteAction, () => !IsNew);
            LoadImage = new RelayCommand(LoadImageAction);
            ClearImage = new RelayCommand(ClearImageAction, () => PicturePath != null);
            AddCopy = new RelayCommand(AddCopyBook);


#if DEBUG_USERCONTROLS_WITH_TIMER
            App.Register<string>(this, AppMessages.MSG_TIMER, s => {
                Console.WriteLine($"{Member.Pseudo} received: {s}");
            });

            timer.Elapsed += (o, e) => { App.NotifyColleagues(AppMessages.MSG_TIMER, $"Timer from {Member.Pseudo}"); };
            timer.Start();
#endif
        }

        private void AddCopyBook()
        {
            if (selectedDate == null)
            {
                selectedDate = DateTime.Now;
            }
            book.AddCopies(Quantity,selectedDate);
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_BOOK_CHANGED, Book);
        }

        private void DeleteAction()
        {
            this.CancelAction();
            if (File.Exists(PicturePath))
            {
                File.Delete(PicturePath);
            }
            Book.Delete();
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_BOOK_CHANGED, Book);
            App.NotifyColleagues(AppMessages.MSG_CLOSE_TAB, this);
        }

        private void LoadImageAction()
        {
            var fd = new OpenFileDialog();
            if (fd.ShowDialog() == true)
            {
                imageHelper.Load(fd.FileName);
                PicturePath = imageHelper.CurrentFile;
            }
        }

        private void ClearImageAction()
        {
            imageHelper.Clear();
            PicturePath = imageHelper.CurrentFile;
        }


        public override void Dispose()
        {
#if DEBUG_USERCONTROLS_WITH_TIMER
            timer.Stop();
#endif
            base.Dispose();
            if (imageHelper.IsTransitoryState)
            {
                imageHelper.Cancel();
                PicturePath = imageHelper.CurrentFile;
            }
        }

        private void SaveAction()
        {
            if (IsNew)
            {
                App.Model.Books.Add(Book);

                IsNew = false;
            }
            imageHelper.Confirm(Book.Title);
            PicturePath = imageHelper.CurrentFile;
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_BOOK_CHANGED, Book);
        }


        private void CancelAction()
        {
            if (imageHelper.IsTransitoryState)
            {
                imageHelper.Cancel();
            }
            if (IsNew)
            {
                ISBN = null;
                Title = null;
                Author = null;
                Editor = null;
                PicturePath = imageHelper.CurrentFile;
                RaisePropertyChanged(nameof(Book));
            }
            else
            {
                var change = (from c in App.Model.ChangeTracker.Entries<Book>()
                              where c.Entity == Book
                              select c).FirstOrDefault();
                if (change != null)
                {
                    change.Reload();
                    RaisePropertyChanged(nameof(ISBN));
                    RaisePropertyChanged(nameof(Title));
                    RaisePropertyChanged(nameof(Author));
                    RaisePropertyChanged(nameof(Editor));
                    RaisePropertyChanged(nameof(PicturePath));
                }
            }
        }

        private bool CanSaveOrCancelAction()
        {
            if (IsNew)
            {
                return IsOk(ISBN) && IsOk(Title) && IsOk(Author) && IsOk(Editor);
            }
            var change = (from c in App.Model.ChangeTracker.Entries<Book>()
                          where c.Entity == Book
                          select c).FirstOrDefault();
            return change != null && change.State != EntityState.Unchanged;
        }

        private bool IsOk(string s)
        {
            return !string.IsNullOrEmpty(s) && !HasErrors;
        }
    }
}
