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
        private Monster currentMonster;

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

            MoveTo(player.CurrentLocation);
            
        }

        private void MoveTo(Location newLocation)
        {
            //Does the location have any required items
            if (!player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                rtbMessages.Text += $"You must have a {newLocation.ItemRequiredToEnter.Name} to enter this location." +
                    Environment.NewLine;
                return;
            }

            //Update the player's current location
            player.CurrentLocation = newLocation;

            //Show/hide available movement buttons
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            //Display the current location name and description
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            //Completely heal the player
            player.CurrentHitPoints = player.MaximumHitPoints;

            //Update Hit Points in UI

            //Does the location have a quest?
            if (newLocation.QuestAvailableHere != null)
            {
                //See if the player already has the quest, and if they've completed it
                bool playerAlreadyHasQuest = player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest = player.CompletedThisQuest(newLocation.QuestAvailableHere);


                //See if the player already has the quest
                if (playerAlreadyHasQuest)
                {
                    //If the player has not completed quest
                    if (!playerAlreadyCompletedQuest)
                    {
                        //See if the player has all the items needed to complete the quest
                        bool playerHasAllItemsToCompleteQuest = player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

                        //The player has all required items to complete the quest   
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            //Display message
                            rtbMessages.Text += Environment.NewLine;
                            rtbMessages.Text += $"You completed the {newLocation.QuestAvailableHere.Name} quest." + Environment.NewLine;

                            player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);

                            //Give quest rewards
                            rtbMessages.Text += "You receive " + Environment.NewLine;
                            rtbMessages.Text += $"{newLocation.QuestAvailableHere.RewardExperiencePoints} experience points" + Environment.NewLine;
                            rtbMessages.Text += $"{newLocation.QuestAvailableHere.RewardGold} gold" + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
                            rtbMessages.Text += Environment.NewLine;

                            player.AddExperiencePoints(newLocation.QuestAvailableHere.RewardExperiencePoints);

                            player.Gold += newLocation.QuestAvailableHere.RewardGold;

                            //Add reward item to player inventory
                            player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                            //Mark the quest as completed
                            player.MarkQuestCompleted(newLocation.QuestAvailableHere);
                        }
                    }
                }
                else
                {
                    //The player does not have the quest

                    //Display the messages
                    rtbMessages.Text += $"You receive the {newLocation.QuestAvailableHere.Name} quest." + Environment.NewLine;
                    rtbMessages.Text += newLocation.QuestAvailableHere.Description + Environment.NewLine;
                    rtbMessages.Text += "To complete it, return with " + Environment.NewLine;

                    foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                        {
                            rtbMessages.Text += $"{qci.Quantity} {qci.Details.Name}" + Environment.NewLine;
                        }
                        else
                        {
                            rtbMessages.Text += $"{qci.Quantity} {qci.Details.NamePlural}" + Environment.NewLine;
                        }
                    }

                    rtbMessages.Text += Environment.NewLine;

                    //Add the quest to the player's quest list
                    player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }

            }
            //Does the location have a monster?
            if (newLocation.MonsterLivingHere != null)
            {
                rtbMessages.Text += $"You see a {newLocation.MonsterLivingHere.Name}" + Environment.NewLine;

                //Make a new monster, using the values from the standard monster in the World.Monster list
                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage, standardMonster.RewardExperiencePoints,
                    standardMonster.RewardGold, standardMonster.CurrentHitPoints, standardMonster.MaximumHitPoints);

                foreach(LootItem lootItem in standardMonster.LootTable)
                {
                    currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUseWeapon.Visible = true;
                btnUsePotion.Visible = true;
            }

            else
            {
                currentMonster = null;

                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
            }
            //Refresh weapon combobox
            UpdateWeaponListInUI();

            //Refresh potion combobox
            UpdatePotionListInUI();
        }
        
        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem inventoryItem in player.Inventory)
            {
                if (inventoryItem.Details is Weapon)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }

            if (weapons.Count == 0)
            {
                //The player doesn't have weapons, so hide the weapon combobox and 'Use' button
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else
            {
                cboWeapons.SelectedIndexChanged -= cboWeapons_SelectedIndexChange;
                cboWeapons.DataSource = weapons;
                cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChange;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                if(player.CurrentWeapon != null)
                {
                    cboWeapons.SelectedItem = player.CurrentWeapon;
                }
                else
                    cboWeapons.SelectedIndex = 0;
            }
        }

        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (InventoryItem inventoryItem in player.Inventory)
            {
                if (inventoryItem.Details is HealingPotion)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion)inventoryItem.Details);
                    }
                }
            }

            if (healingPotions.Count == 0)
            {
                //The player doesn't have weapons, so hide the weapon combobox and 'Use' button
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }
        }
        
        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToNorth);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToSouth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToEast);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToWest);
        }
        
        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            //Get the current selected weapon from the Weapons combobox
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            //Determine the amount of damage to do to the monster
            int damageToMonster = RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            //Apply damage to the monster's CurrentHitPoints
            currentMonster.CurrentHitPoints -= damageToMonster;

            //Display Message
            if(damageToMonster == 0)
            {
                rtbMessages.Text += $"Your attack missed its target. {currentMonster.Name} did not received any damage." + Environment.NewLine;
            }
            else
            {
                rtbMessages.Text += $"You hit the {currentMonster.Name} for {damageToMonster} points." + Environment.NewLine;
            }

            if(currentMonster.CurrentHitPoints <= 0)
            {
                //Monster is dead
                rtbMessages.Text += Environment.NewLine;
                rtbMessages.Text += $"You have defeated the {currentMonster.Name}." + Environment.NewLine;

                player.AddExperiencePoints(currentMonster.RewardExperiencePoints);

                rtbMessages.Text += $"You received {currentMonster.RewardExperiencePoints} experience points." + Environment.NewLine;

                player.Gold += currentMonster.RewardGold;
                rtbMessages.Text += $"You received {currentMonster.RewardGold} gold." + Environment.NewLine;

                //Get random loot items from the monster
                List<InventoryItem> lootedItems = new List<InventoryItem>();

                //Add items to the lootedItems list, comparing a number to the drop percentage
                foreach(LootItem lootItem in currentMonster.LootTable)
                {
                    if(RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }
                //If no items were randomly selected, then add the default loot items
                if(lootedItems.Count == 0)
                {
                    foreach (LootItem lootItem in currentMonster.LootTable)
                    {
                        if(lootItem.IsDefaultItem)
                        {
                            lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                        }
                    }
                }

                //Add the looted items to the inventory
                foreach(InventoryItem inventoryItem in lootedItems)
                {
                    player.AddItemToInventory(inventoryItem.Details);
                    if (inventoryItem.Quantity == 1)
                        rtbMessages.Text += $"You looted {inventoryItem.Quantity} {inventoryItem.Details.Name}." + Environment.NewLine;
                    else
                        rtbMessages.Text += $"You looted {inventoryItem.Quantity} {inventoryItem.Details.NamePlural}." + Environment.NewLine;
                }
                UpdateWeaponListInUI();
                UpdatePotionListInUI();

                //Adds a blank line for appearance
                rtbMessages.Text += Environment.NewLine;

                //Move player to current location (to heal player and create a new monster to fight)
                MoveTo(player.CurrentLocation);
            }
            else
            {
                //Monster is still alive
                EnemyAttack();
            }
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            //Get the currently selected potion from the combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            //Add healing amount to player CurrentHitPoints
            player.CurrentHitPoints = (player.CurrentHitPoints + potion.AmountToHeal);

            //CurrentHitPoints cannot exceed MaximumHitPoints
            if(player.CurrentHitPoints > player.MaximumHitPoints)
            {
                player.CurrentHitPoints = player.MaximumHitPoints;
            }

            //Remove the potion from player inventory
            foreach(InventoryItem inventoryItem in player.Inventory)
            {
                if(inventoryItem.Details.ID == potion.ID)
                {
                    inventoryItem.Quantity--;
                    break;
                }
            }
            //Display message
            rtbMessages.Text += $"You used a {potion.Name}." + Environment.NewLine;

            //Monster gets their turn to attack
            EnemyAttack();
            
            UpdatePotionListInUI();
            
        }

        private void rtbMessages_TextChanged(object sender, EventArgs e)
        {
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void EnemyAttack()
        {
            //Determine the amount of damage the monster does to the player
            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, currentMonster.MaximumDamage);

            //Display message
            rtbMessages.Text += $"The {currentMonster.Name} deals {damageToPlayer} points of damage." + Environment.NewLine;

            //Subtract damage from player
            player.CurrentHitPoints -= damageToPlayer;

            if (player.CurrentHitPoints <= 0)
            {
                rtbMessages.Text += $"The fucking {currentMonster.Name} killed your weak ass." + Environment.NewLine;

                //Move player back to 'Home'
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));

                MessageBox.Show("YOU DIED", "DEAD");
            }
        }

        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, player.ToXmlString());
        }

        private void cboWeapons_SelectedIndexChange(object sender, EventArgs e)
        {
            player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
        }
    }
}
