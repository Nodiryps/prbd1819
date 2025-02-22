﻿using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace prbd_1819_g19
{
    public enum AppMessages
    {
        MSG_NEW_BOOK,
        MSG_TIMER,
        MSG_CLOSE_TAB,
        MSG_BOOK_DETAIL,
        MSG_ADD_BOOK_TO_BASKET,
        MSG_DISPLAY_MEMBER,
        MSG_DISPLAY_CAT,
        MSG_DISPLAY_BOOK,
        MSG_BOOK_CHANGED,
        MSG_ISBN_CHANGED,
        MSG_CAT_CHANGED,
        MSG_BASKET_CHANGED,
        MSG_CAT_DEL,
        MSG_CONFIRM_BASKET,
        MSG_NBCOPIES_CHANGED,
        MSG_LINK_CAT,
        MSG_FILL_CAT_INPUT,
        ADD_BASKET

    }

    public partial class App : ApplicationBase
    {

        private static DbType type;
        private static DbType Type
        {
            get => type;
            set
            {
#if MSSQL
             type = DbType.MsSQL;
#else
                type = DbType.MySQL;
#endif
            }
        }

        public static Model Model = Model.CreateModel(Type);

        public static User CurrentUser { get; set; }
        public static User SelectedUser { get; set; }

        public static readonly string IMAGE_PATH = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../images");

        public App()
        {
            Model.ClearDatabase();
            Model.CreateTestData();
            InitializeComponent();
        }

        public static bool IsAdmin()
        {
            return CurrentUser.Role == Role.Admin;
        }

        public static bool IsUserLogged()
        {
            return CurrentUser != null;
        }
    }
}
