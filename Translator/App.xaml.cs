using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TTSAutomate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            if (!String.IsNullOrEmpty(TTSAutomate.Properties.Settings.Default.SelectedCulture))
            {
                CultureInfo ci = CultureInfo.GetCultures(CultureTypes.AllCultures).First(n => n.DisplayName == TTSAutomate.Properties.Settings.Default.SelectedCulture);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;

            }
        }

    }
}
