﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ControlzEx.Theming;
using FarplaneCore.Common;
using FarplaneCore.Common.Controls;
using FarplaneCore.FFX.Data;
using FarplaneCore.FFX.Values;
using FarplaneCore.Memory;
using MahApps.Metro;

namespace FarplaneCore.FFX.EditorPanels.ItemsPanel
{
    /// <summary>
    /// Interaction logic for ItemsPanel.xaml
    /// </summary>
    public partial class ItemsPanel : UserControl
    {
        private readonly int _offsetKeyItem = OffsetScanner.GetOffset(GameOffset.FFX_KeyItems);
        private readonly int _offsetAlBhed = OffsetScanner.GetOffset(GameOffset.FFX_AlBhed);

        private readonly ButtonGrid _itemButtons = new ButtonGrid(2, 112);
        private readonly ButtonGrid _keyItemButtons = new ButtonGrid(2, KeyItem.KeyItems.Length - 1);

        private static readonly ComboBox ComboItemList = new ComboBox() { ItemsSource = Item.Items.Select(item => item.Name) };
        private static readonly TextBox TextItemCount = new TextBox();
        private static readonly StackPanel PanelEditItem = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                ComboItemList,
                TextItemCount
            }
        };

        private bool _refreshing = false;

        private Item[] _currentItems;
        private int _editingItem = -1;

        private bool[] _keyItemState;
        private bool[] _alBhedState;

        private static readonly Theme currentStyle = ThemeManager.Current.DetectTheme(Application.Current);
        private readonly Brush _trueKeyItemBrush = new SolidColorBrush(Colors.Black);
        private readonly Brush _falseKeyItemBrush = new SolidColorBrush(Colors.Gray);

        public ItemsPanel()
        {
            InitializeComponent();
            TabItems.Content = _itemButtons;
            ContentKeyItems.Content = _keyItemButtons;

            _itemButtons.ButtonClicked += ItemButtonsOnButtonClicked;

            ComboItemList.KeyDown += ItemEditor_KeyDown;
            TextItemCount.KeyDown += ItemEditor_KeyDown;

            _keyItemButtons.ButtonClicked += KeyItemButtonsOnButtonClicked;
        }

        private void KeyItemButtonsOnButtonClicked(int buttonIndex)
        {
            var keyItemData = GameMemory.Read<byte>(_offsetKeyItem, 8, false);
            var bitIndex = KeyItem.KeyItems[buttonIndex].BitIndex;
            var keyByteIndex = bitIndex / 8;
            var keyBitIndex = bitIndex % 8;

            keyItemData[keyByteIndex] = BitHelper.ToggleBit(keyItemData[keyByteIndex], keyBitIndex);
            GameMemory.Write(_offsetKeyItem, keyItemData, false);
            Refresh();
        }

        private void ItemButtonsOnButtonClicked(int buttonIndex)
        {
            if (_editingItem == buttonIndex) return;

            Refresh();

            var clickedItem = _currentItems[buttonIndex];
            var baseItem = Item.Items.First(item => item.ID == clickedItem.ID);

            var itemIndex = Item.Items.ToList().IndexOf(baseItem);

            ComboItemList.SelectedIndex = itemIndex;
            ComboItemList.KeyDown += ItemEditor_KeyDown;

            TextItemCount.Text = clickedItem.Count.ToString();

            _itemButtons.SetContent(buttonIndex, PanelEditItem);
            _editingItem = buttonIndex;

            TextItemCount.SelectionStart = 0;
            TextItemCount.SelectionLength = TextItemCount.Text.Length;

            TextItemCount.Focus();
        }

        private void ItemEditor_KeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key != Key.Enter && keyEventArgs.Key != Key.Escape) return;

            switch (keyEventArgs.Key)
            {
                case Key.Enter:
                    var itemIndex = ComboItemList.SelectedIndex;
                    var itemCount = byte.Parse(TextItemCount.Text);
                    if (itemCount == 0) itemIndex = 0;
                    if (itemIndex == 0) itemCount = 0;
                    Item.WriteItem(_editingItem, Item.Items[itemIndex].ID, itemCount);
                    Refresh();
                    break;
                case Key.Escape:
                    Refresh();
                    break;
            }
        }

        public void Refresh()
        {
            _refreshing = true;
            _editingItem = -1;
            _currentItems = Item.ReadItems();

            // Refresh inventory items
            for (int i = 0; i < _currentItems.Length; i++)
            {
                if (_currentItems[i].ID == 0xFF)
                {
                    // Empty slot
                    _itemButtons.SetContent(i, "< EMPTY >");
                }
                else
                {
                    // Show item name and count
                    _itemButtons.SetContent(i, _currentItems[i].Name + " x" + _currentItems[i].Count);
                }
            }

            // Refresh key items and al bhed dictionaries
            var keyItemData = GameMemory.Read<byte>(_offsetKeyItem, 8, false);
            var alBhedData = GameMemory.Read<byte>(_offsetAlBhed, 4, false);
            _keyItemState = BitHelper.GetBitArray(keyItemData, 58);
            _alBhedState = BitHelper.GetBitArray(alBhedData, 26);

            // Key Items
            for (int i = 0; i < KeyItem.KeyItems.Length - 1; i++)
            {
                if (_keyItemState[KeyItem.KeyItems[i].BitIndex])
                {
                    // Key item owned
                    _keyItemButtons.Buttons[i].Foreground = _trueKeyItemBrush;
                    _keyItemButtons.SetContent(i, $"{KeyItem.KeyItems[i].Name}");
                }
                else
                {
                    // Key item not owned
                    _keyItemButtons.Buttons[i].Foreground = _falseKeyItemBrush;
                    _keyItemButtons.SetContent(i, $"{KeyItem.KeyItems[i].Name}");
                }
            }

            // Al Bhed Dictionaries
            for (int i = 0; i < 26; i++)
            {
                (PanelAlBhed.Children[i] as CheckBox).IsChecked = _alBhedState[i];
            }
            _refreshing = false;
        }

        private void AlBhedDictionary_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_refreshing) return;
            var checkBox = sender as CheckBox;
            var alBhedData = GameMemory.Read<byte>(_offsetAlBhed, 4,false);

            var boxIndex = PanelAlBhed.Children.IndexOf(checkBox);

            var byteIndex = boxIndex/8;
            var bitIndex = boxIndex%8;

            alBhedData[byteIndex] = BitHelper.ToggleBit(alBhedData[byteIndex], bitIndex);
            GameMemory.Write(_offsetAlBhed, alBhedData, false);
            Refresh();
        }
    }
}