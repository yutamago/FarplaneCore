﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ControlzEx.Theming;
using FarplaneCore.Properties;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace FarplaneCore.Common.Controls
{
    /// <summary>
    /// Interaction logic for ConfigFlyout.xaml
    /// </summary>
    public partial class ConfigFlyout : Flyout
    {
        private bool canSetTheme = false;

        public ConfigFlyout()
        {
            InitializeComponent();

            ComboTheme.ItemsSource = ThemeManager.Current.Themes;
            ComboAccent.ItemsSource = ThemeManager.Current.BaseColors;

            var currentTheme = ThemeManager.Current.GetTheme(Settings.Default.AppTheme);
            // var currentAccent = ThemeManager.Current.GetAccent(Settings.Default.AppAccent);

            // ComboTheme.SelectedIndex =
                // ThemeManager.AppThemes.ToList().IndexOf(currentTheme);

            // ComboAccent.SelectedIndex =
                // ThemeManager.Accents.ToList().IndexOf(currentAccent);

            CheckNeverShowUnXWarning.IsChecked = Settings.Default.NeverShowUnXWarning;
            CheckCloseWithGame.IsChecked = Settings.Default.CloseWithGame;
            CheckShowAllProcesses.IsChecked = Settings.Default.ShowAllProcesses;
			

            canSetTheme = true;
        }

        private void ComboAccent_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!canSetTheme) return;
            // ThemeManager.ChangeAppStyle(Application.Current, (Accent)ComboAccent.SelectedItem, (AppTheme)ComboTheme.SelectedItem);
            SettingUpdated(sender, e);
        }

        private void SettingUpdated(object sender, RoutedEventArgs e)
        {
            if (!canSetTheme) return;
            Settings.Default.NeverShowUnXWarning = CheckNeverShowUnXWarning.IsChecked.Value;
            Settings.Default.CloseWithGame = CheckCloseWithGame.IsChecked.Value;
            Settings.Default.ShowAllProcesses = CheckShowAllProcesses.IsChecked.Value;
            // Settings.Default.AppAccent = (ComboAccent.SelectedItem as Accent).Name;
            // Settings.Default.AppTheme = (ComboTheme.SelectedItem as AppTheme).Name;
            Settings.Default.Save();
        }
    }
}
