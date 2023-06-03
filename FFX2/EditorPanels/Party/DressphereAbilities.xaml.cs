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
using FarplaneCore.FFX2.Values;
using MahApps.Metro.Controls;

namespace FarplaneCore.FFX2.EditorPanels
{
	/// <summary>
	/// Interaction logic for DressphereAbilities.xaml
	/// </summary>
	public partial class DressphereAbilities : UserControl
	{
		private int _baseOffset = 0;
		private Button[] buttons = new Button[16];
		private int[] values = new int[16];

		private int _selectedDs = 0;

		public int SelectedIndex { get; set; } = 0;

		public DressphereAbilities()
		{
			InitializeComponent();
			//Dressphere.Items.Add("None");
			//foreach (var dressphere in Dresspheres.GetDresspheres())
			//	Dressphere.Items.Add(dressphere.Name);
			//Dressphere.SelectedIndex = 0;
			ReloadDresspheres();
		}

		public void ReloadDresspheres()
		{
			var dresses = Dresspheres.GetDresspheres().ToList();
			dresses.RemoveAll(r => r.Special != -1 && r.Special != SelectedIndex);

			var oldSelection = _selectedDs;
			var newSelection = 0;
			Dressphere.Items.Clear();
			Dressphere.Items.Add("None");
			foreach (var dress in dresses)
			{
				Dressphere.Items.Add(new ComboBoxItem {Content = dress.Name, Tag = dress.ID});
				if (dress.ID == oldSelection)
				{
					newSelection = Dressphere.Items.Count - 1;
					_selectedDs = dress.ID;
				}
			}

			Dressphere.SelectedIndex = newSelection;
		}

		public void RefreshAbilities()
		{
			var selectedId = (int)((Dressphere.SelectedItem as ComboBoxItem)?.Tag ?? 0);

			if (selectedId == 0)
				return;

			var dressInfo = Dresspheres.GetDresspheres().FirstOrDefault(ds => ds.ID == selectedId);
			if (dressInfo == null)
				return;
			
			// Special dresspheres always fall under Yuna's offset
			_baseOffset = (int) OffsetType.AbilityBase + (dressInfo.Special == -1 ? SelectedIndex * 0x6A0 : 0);

			var abilities = dressInfo.Abilities;

			for (int a = 0; a < 16; a++)
			{
				var button = (Button) this.FindName($"Ability{a}");
				if (button == null) continue;

				button.Content = string.Empty;
				button.IsEnabled = false;

				if (a >= abilities.Length) continue;

				var abil = abilities[a];

				int currentAP = 0;

				if (abil.Offset != -1)
					currentAP = LegacyMemoryReader.ReadInt16(_baseOffset + abil.Offset);

				values[a] = currentAP;

				string apText;

				if (currentAP >= abil.MasteredAP || abil.MasteredAP == 0)
					apText = " [***]";
				else
					apText = $" {currentAP} / {abil.MasteredAP}";

				button.Content = $"{abil.Name} {apText}";

				if (abil.ReadOnly == false) button.IsEnabled = true;
			}
		}

		private void Dressphere_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_selectedDs = (int)((Dressphere.SelectedItem as ComboBoxItem)?.Tag ?? 0);
			RefreshAbilities();
		}

		private void AbilityButton_Click(object sender, RoutedEventArgs e)
		{
			if (Dressphere.SelectedIndex == 0 || _baseOffset == 0) return;
			var abilityNum = int.Parse((sender as Button).Name.Substring(7));
			var dressInfo = Dresspheres.GetDresspheres().FirstOrDefault(ds => ds.ID == Dressphere.SelectedIndex);

			var ability = dressInfo.Abilities[abilityNum];

			var apDialog = new AbilitySlider(ability.MasteredAP, values[abilityNum]);
			var setAp = apDialog.ShowDialog();

			if (!setAp.HasValue || setAp.Value != true) return;
			var newAp = apDialog.AP;

			LegacyMemoryReader.WriteBytes(_baseOffset + ability.Offset, BitConverter.GetBytes((ushort) newAp));

			RefreshAbilities();
		}

		private void MasterAll_Click(object sender, RoutedEventArgs e)
		{
			if (Dressphere.SelectedIndex == 0 || _baseOffset == 0) return;
			var dressInfo = Dresspheres.GetDresspheres().FirstOrDefault(ds => ds.ID == Dressphere.SelectedIndex);
			if (dressInfo == null) return;

			var abilities = dressInfo.Abilities;

			for (int a = 0; a < 16; a++)
			{
				if (a >= abilities.Length) continue;

				var abil = abilities[a];

				int currentAP = 0;

				if (abil.Offset == -1 || abil.ReadOnly) continue;

				LegacyMemoryReader.WriteBytes(_baseOffset + abil.Offset, BitConverter.GetBytes((ushort) abil.MasteredAP));
			}

			RefreshAbilities();
		}
	}
}