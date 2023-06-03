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

namespace FarplaneCore.FFX.EditorPanels
{
    /// <summary>
    /// Interaction logic for NotImplementedPanel.xaml
    /// </summary>
    public partial class NotAvailablePanel : UserControl
    {
        public NotAvailablePanel()
        {
            InitializeComponent();
        }

        public void SetText(string header, string text)
        {
            TextNYI.Text = text;
            GroupNYI.Header = header;
        }
    }
}