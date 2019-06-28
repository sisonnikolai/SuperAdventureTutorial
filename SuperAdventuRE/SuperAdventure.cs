using Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SuperAdventuRE
{
    public partial class SuperAdventure : Form
    {
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";
        private Player player;

        public SuperAdventure()
        {
            InitializeComponent();

            if (File.Exists(PLAYER_DATA_FILE_NAME))
            {
                player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
            }
            else
            {
                player = Player.CreateDefaultPlayer();
            }

            //DataBindings - The databinding will connect to the Text property of the labels to the following properties of the player object
            lblHitPoints.DataBindings.Add("Text", player, "CurrentHitPoints");
            lblGold.DataBindings.Add("Text", player, "Gold");
            lblExperience.DataBindings.Add("Text", player, "ExperiencePoints");
            lblLevel.DataBindings.Add("Text", player, "Level");

            //Data Bindings for Inventory
            dgvInventory.RowHeadersVisible = false;
            dgvInventory.AutoGenerateColumns = false;

            dgvInventory.DataSource = player.Inventory;

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = "Description"
            });

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Quantity",
                DataPropertyName = "Quantity"
            });

            //Data Bindings for Quest List
            dgvQuests.RowHeadersVisible = false;
            dgvQuests.AutoGenerateColumns = false;

            dgvQuests.DataSource = player.Quests;

            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = "Name"
            });

            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Done?",
                DataPropertyName = "IsCompleted"
            });

            //Data Bindings for Weapon & Potion combo boxes
            cboWeapons.DataSource = player.Weapons;
            cboWeapons.DisplayMember = "Name";
            cboWeapons.ValueMember = "Id";

            if (player.CurrentWeapon != null)
                cboWeapons.SelectedItem = player.CurrentWeapon;

            cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChange;

            cboPotions.DataSource = player.Potions;
            cboPotions.DisplayMember = "Name";
            cboPotions.ValueMember = "Id";

            player.PropertyChanged += PlayerOnPropertyChanged;
            player.OnMessage += DisplayMessage;
            Monster.OnMessage += DisplayMessage;

            player.MoveTo(player.CurrentLocation);
            
        }

        private void DisplayMessage(object sender, MessageEventArgs messageEventArgs)
        {
            rtbMessages.Text += messageEventArgs.Message + Environment.NewLine;

            if(messageEventArgs.AddExtraNewLine)
            {
                rtbMessages.Text += Environment.NewLine;
            }
        }

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Weapons")
            {
                cboWeapons.DataSource = player.Weapons;
                if (!player.Weapons.Any())
                {
                    cboWeapons.Visible = false;
                    btnUseWeapon.Visible = false;
                }
            }

            if (propertyChangedEventArgs.PropertyName == "Potions")
            {
                cboPotions.DataSource = player.Potions;
                if (!player.Potions.Any())
                {
                    cboPotions.Visible = false;
                    btnUsePotion.Visible = false;
                }
            }

            if(propertyChangedEventArgs.PropertyName == "CurrentLocation")
            {
                //Show/hide available movements
                btnNorth.Visible = (player.CurrentLocation.LocationToNorth != null);
                btnSouth.Visible = (player.CurrentLocation.LocationToSouth != null);
                btnEast.Visible = (player.CurrentLocation.LocationToEast != null);
                btnWest.Visible = (player.CurrentLocation.LocationToWest != null);
                btnTrade.Visible = (player.CurrentLocation.VendorPresent != null);

                //Display the current location name and description
                rtbLocation.Text = player.CurrentLocation.Name + Environment.NewLine;
                rtbLocation.Text += player.CurrentLocation.Description + Environment.NewLine;

                if(player.CurrentLocation.MonsterLivingHere == null)
                {
                    cboWeapons.Visible = false;
                    btnUseWeapon.Visible = false;
                    cboPotions.Visible = false;
                    btnUsePotion.Visible = false;
                }
                else
                {
                    cboWeapons.Visible = player.Weapons.Any();
                    btnUseWeapon.Visible = player.Weapons.Any();
                    cboPotions.Visible = player.Potions.Any();
                    btnUsePotion.Visible = player.Potions.Any();
                }
            }
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            player.MoveNorth();
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            player.MoveSouth();
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            player.MoveEast();
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            player.MoveWest();
        }
        
        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            //Get the currently selected weapon from the combo box
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;
            player.UseWeapon(currentWeapon);
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            //Get the currently selected potion from the combo box
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;
            player.UsePotion(potion);
        }

        private void rtbMessages_TextChanged(object sender, EventArgs e)
        {
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, player.ToXmlString());
        }

        private void cboWeapons_SelectedIndexChange(object sender, EventArgs e)
        {
            player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
        }

        private void btnTrade_Click(object sender, EventArgs e)
        {
            TradingScreen tradingScreen = new TradingScreen(player);
            tradingScreen.StartPosition = FormStartPosition.CenterParent;
            tradingScreen.ShowDialog(this);
        }
    }
}
