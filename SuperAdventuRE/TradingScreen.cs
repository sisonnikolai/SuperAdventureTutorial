﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;

namespace SuperAdventuRE
{
    public partial class TradingScreen : Form
    {
        private Player currentPlayer;

        public TradingScreen(Player player)
        {
            currentPlayer = player;

            InitializeComponent();

            //Style, to display numeric column values
            DataGridViewCellStyle rightAlignedCellStyle = new DataGridViewCellStyle();
            rightAlignedCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            #region Player Inventory

            //Populate the datagrid for the player's inventory
            dgvMyItems.RowHeadersVisible = false;
            dgvMyItems.AutoGenerateColumns = false;

            //This hidden column holds the item ID, so we know which item to sell
            dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ItemID",
                Visible = false
            });

            dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 100,
                DataPropertyName = "Description"
            });

            dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Qty",
                Width = 30,
                DefaultCellStyle = rightAlignedCellStyle,
                DataPropertyName = "Quantity"
            });

            dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Price",
                Width = 35,
                DefaultCellStyle = rightAlignedCellStyle,
                DataPropertyName = "Price"
            });

            dgvMyItems.Columns.Add(new DataGridViewButtonColumn
            {
                Text = "Sell 1",
                UseColumnTextForButtonValue = true,
                Width = 50,
                DataPropertyName = "ItemID"
            });

            //Bind the player's inventory to the datagridview
            dgvMyItems.DataSource = currentPlayer.Inventory;

            #endregion

            //When the user clicks a row, call this function
            dgvMyItems.CellClick += dgvMyItems_CellClick;

            #region Vendor Inventory
            //Populate the datagrid for the player's inventory
            dgvVendorItems.RowHeadersVisible = false;
            dgvVendorItems.AutoGenerateColumns = false;

            //This hidden column holds the item ID, so we know which item to sell
            dgvVendorItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ItemID",
                Visible = false
            });

            dgvVendorItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 100,
                DataPropertyName = "Description"
            });

            dgvVendorItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Price",
                Width = 35,
                DefaultCellStyle = rightAlignedCellStyle,
                DataPropertyName = "Price"
            });

            dgvVendorItems.Columns.Add(new DataGridViewButtonColumn
            {
                Text = "Buy 1",
                UseColumnTextForButtonValue = true,
                Width = 50,
                DataPropertyName = "ItemID"
            });

            //Bind the player's inventory to the datagridview
            dgvVendorItems.DataSource = currentPlayer.CurrentLocation.VendorPresent.Inventory;

            #endregion

            //When the user clicks on a row, call this function
            dgvVendorItems.CellClick += dgvVendorItems_CellClick;
        }

        private void dgvVendorItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                var itemId = dgvVendorItems.Rows[e.RowIndex].Cells[0].Value;

                Item itemBeingBought = World.ItemByID(Convert.ToInt32(itemId));

                if (currentPlayer.Gold >= itemBeingBought.Price)
                {
                    currentPlayer.AddItemToInventory(itemBeingBought);
                    currentPlayer.Gold -= itemBeingBought.Price;
                }
                else
                {
                    MessageBox.Show($"You don't have enough gold to buy the {itemBeingBought.Name}");
                }
            }
        }

        private void dgvMyItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //The first column of a dataagrid view has a ColumnIndex = 0
            //This is known as "zero-based" array/collection/list
            //If the player clicked the button column, we will sell an item from that row
            if (e.ColumnIndex == 4)
            {
                //This gets the ID value of the item, from the hidden 1st column
                var itemID = dgvMyItems.Rows[e.RowIndex].Cells[0].Value;

                //Get the item object for the selected row
                Item itemBeingSold = World.ItemByID(Convert.ToInt32(itemID));

                if (itemBeingSold.Price == World.UNSELLABLE_ITEM_PRICE)
                {
                    MessageBox.Show($"Unable to sell {itemBeingSold.Name}.");
                }
                else
                {
                    //Remove one of these items from the inventory
                    currentPlayer.RemoveItemFromInventory(itemBeingSold);

                    //Give the player gold
                    currentPlayer.Gold += itemBeingSold.Price;
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
