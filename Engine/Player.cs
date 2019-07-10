using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;

namespace Engine
{
    public class Player : LivingCreature
    {
        #region Fields
        private int gold;
        private int experiencePoints;
        private Location currentLocation;
        private Monster currentMonster;
        #endregion

        #region Properties
        public int Gold
        {
            get { return gold; }
            set
            {
                gold = value;
                OnPropertyChanged("Gold");
            }
        }
        
        public int ExperiencePoints
        {
            get { return experiencePoints; }
            private set
            {
                experiencePoints = value;
                OnPropertyChanged("ExperiencePoints");
                OnPropertyChanged("Level");
            }
        }
        public int Level
        {
            get
            {
                return (ExperiencePoints / 100) + 1;
            }

        }
        
        public BindingList<InventoryItem> Inventory { get; set; }
        public BindingList<PlayerQuest> Quests { get; set; }
        public Location CurrentLocation
        {
            get { return currentLocation; }
            set
            {
                currentLocation = value;
                OnPropertyChanged("CurrentLocation");
            }
        }
        public Weapon CurrentWeapon { get; set; }

        public List<Weapon> Weapons
        {
            get { return Inventory.Where(x => x.Details is Weapon).Select(x => x.Details as Weapon).ToList(); }
        }

        public List<HealingPotion> Potions
        {
            get { return Inventory.Where(x => x.Details is HealingPotion).Select(x => x.Details as HealingPotion).ToList(); }
        }
        #endregion

        public event EventHandler<MessageEventArgs> OnMessage;

        //CONSTRUCTOR
        private Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints) : base (currentHitPoints
            ,maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;
            Inventory = new BindingList<InventoryItem>();
            Quests = new BindingList<PlayerQuest>();
        }

        #region Functions
        public static Player CreateDefaultPlayer()
        {
            Player player = new Player(10, 10, 20, 0);
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
            player.CurrentLocation = World.LocationByID(World.LOCATION_ID_HOME);

            return player;
        }

        public void AddExperiencePoints(int experiencePointsToAdd)
        {
            ExperiencePoints += experiencePointsToAdd;
            MaximumHitPoints = (Level * 10);

        }

        public static Player CreatePlayerFromXmlString(string xmlPlayerData)
        {
            try
            {
                XmlDocument playerData = new XmlDocument();

                playerData.LoadXml(xmlPlayerData);

                int currentHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
                int maximumHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
                int gold = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
                int experiencePoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);

                Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints);

                int currentLocationID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);
                player.CurrentLocation = World.LocationByID(currentLocationID);

                if(playerData.SelectSingleNode("/Player/Stats/CurrentWeapon") != null)
                {
                    int currentWeaponID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentWeapon").InnerText);
                    player.CurrentWeapon = (Weapon)World.ItemByID(currentWeaponID);
                }

                foreach(XmlNode node in playerData.SelectNodes("/Player/InventoryItems/InventoryItem"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);

                    for(int i = 0; i < quantity; i++)
                    {
                        player.AddItemToInventory(World.ItemByID(id));
                    }
                }

                foreach(XmlNode node in playerData.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);

                    PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(id));
                    playerQuest.IsCompleted = isCompleted;
                    player.Quests.Add(playerQuest);
                }

                return player;
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                //If there was an error with the XML data, return a default player object
                return Player.CreateDefaultPlayer();
            }
        }

        public static Player CreatePlayerFromDatabase(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints,
            int currentLocationID)
        {
            Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints);

            //player.MoveTo(World.LocationByID(currentLocationID));

            return player;
        }

        public bool PlayerDoesNotHaveTheItemRequiredToEnter (Location location)
        {
            if(location.ItemRequiredToEnter != null)
            {
                //See if the player has the required item in their inventory
                Inventory.Any(ii => ii.Details.ID == location.ItemRequiredToEnter.ID);
                return true;
            }
            else
            {
                //There is no required item for this location, so return 'true'
                return false;
            }
        }

        public bool HasThisQuest (Quest quest)
        {
            return Quests.Any(pq => pq.Details.ID == quest.ID);
        }

        public bool CompletedThisQuest (Quest quest)
        {
            foreach(PlayerQuest playerQuest in Quests)
            {
                if(playerQuest.Details.ID == quest.ID)
                {
                    return playerQuest.IsCompleted;
                }
            }
            return false;
        }

        public bool HasAllQuestCompletionItems (Quest quest)
        {
            //See if the player has all the items needed to complete the quest there
            foreach(QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                //Check each item in the player's inventory, to see if they have it and enough of it
                if(!Inventory.Any(ii => ii.Details.ID == qci.Details.ID && ii.Quantity >= qci.Quantity))
                {
                    return false;
                }
            }
            //If we got here, then the player must have all the required items, and enough of them, to complete the quest
            return true;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            foreach(QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == qci.Details.ID);

                if(item != null)
                {
                    //Subtract the quantity
                    RemoveItemFromInventory(item.Details, qci.Quantity);
                }
            }
        }

        public void AddItemToInventory(Item itemToAdd, int quantity = 1)
        {
            InventoryItem inventoryItem = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);

            if (inventoryItem == null)
            {
                //They don't have the item, so add 1
                Inventory.Add(new InventoryItem(itemToAdd, quantity));
            }
            else
            {
                //They have the item, increase by 1
                inventoryItem.Quantity += quantity;
            }
            RaiseInventoryChangeEvent(itemToAdd);
        }

        public void MarkQuestCompleted(Quest quest)
        {
            //Find the quest in the quest list
            PlayerQuest playerQuest = Quests.SingleOrDefault(pq => pq.Details.ID == quest.ID);
            if (playerQuest != null)
                playerQuest.IsCompleted = true;
        }

        public string ToXmlString()
        {
            XmlDocument playerData = new XmlDocument();

            //Create the top-level node
            XmlNode player = playerData.CreateElement("Player");
            playerData.AppendChild(player);

            //Create the "Stats" child node to hold the other player stats nodes
            XmlNode stats = playerData.CreateElement("Stats");
            player.AppendChild(stats);

            //Create the child nodes for the "Stats" nodes
            XmlNode currentHitPoints = playerData.CreateElement("CurrentHitPoints");
            currentHitPoints.AppendChild(playerData.CreateTextNode(CurrentHitPoints.ToString()));
            stats.AppendChild(currentHitPoints);

            XmlNode maximumHitPoints = playerData.CreateElement("MaximumHitPoints");
            maximumHitPoints.AppendChild(playerData.CreateTextNode(MaximumHitPoints.ToString()));
            stats.AppendChild(maximumHitPoints);

            XmlNode gold = playerData.CreateElement("Gold");
            gold.AppendChild(playerData.CreateTextNode(Gold.ToString()));
            stats.AppendChild(gold);

            XmlNode experiencePoints = playerData.CreateElement("ExperiencePoints");
            experiencePoints.AppendChild(playerData.CreateTextNode(ExperiencePoints.ToString()));
            stats.AppendChild(experiencePoints);
            
            XmlNode currentLocation = playerData.CreateElement("CurrentLocation");
            currentLocation.AppendChild(playerData.CreateTextNode(CurrentLocation.ID.ToString()));
            stats.AppendChild(currentLocation);

            if(CurrentWeapon != null)
            {
                XmlNode currentWeapon = playerData.CreateElement("CurrentWeapon");
                currentWeapon.AppendChild(playerData.CreateTextNode(CurrentWeapon.ID.ToString()));
                stats.AppendChild(currentWeapon);
            }

            //Create the "InventoryItems" child node to hold each InventoryItem node
            XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
            player.AppendChild(inventoryItems);

            //Create an "InventoryItem" node for each item in the player inventory
            foreach(InventoryItem item in this.Inventory)
            {
                XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = item.Details.ID.ToString();
                inventoryItem.Attributes.Append(idAttribute);

                XmlAttribute quantityAttribute = playerData.CreateAttribute("Quantity");
                quantityAttribute.Value = item.Quantity.ToString();
                inventoryItem.Attributes.Append(quantityAttribute);

                inventoryItems.AppendChild(inventoryItem);
            }

            //Create the "PlayerQuests" child node to hold each PlayerQuest node
            XmlNode playerQuests = playerData.CreateElement("PlayerQuests");
            player.AppendChild(playerQuests);

            //Create a "PlayerQuest" node for each quest the player has acquired
            foreach(PlayerQuest quest in this.Quests)
            {
                XmlNode playerQuest = playerData.CreateElement("PlayerQuest");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = quest.Details.ID.ToString();
                playerQuest.Attributes.Append(idAttribute);

                XmlAttribute isCompletedAttribute = playerData.CreateAttribute("IsCompleted");
                isCompletedAttribute.Value = quest.IsCompleted.ToString();
                playerQuest.Attributes.Append(isCompletedAttribute);

                playerQuests.AppendChild(playerQuest);
            }
            return playerData.InnerXml; //The XML document, as a string, so we can save the data to disk
        }

        public void RemoveItemFromInventory(Item itemToRemove, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToRemove.ID);

            if (item == null) { } //The item is not in the inventory, so ignore it. We might want to raise an error in this situation
            else
            {
                //The item is present, so decrease the quantity
                item.Quantity -= quantity;

                //Don't allow negative quantities. We might want to raise an error in this situation
                if (item.Quantity < 0)
                    item.Quantity = 0;

                //If the quantity is zero, remove the item from the list
                if (item.Quantity == 0)
                    Inventory.Remove(item);

                //Notify the UI that the inventory has changed
                RaiseInventoryChangeEvent(itemToRemove);
            }
        }

        private void RaiseInventoryChangeEvent (Item item)
        {
            if (item is Weapon)
                OnPropertyChanged("Weapons");

            if (item is HealingPotion)
                OnPropertyChanged("Potions");
        }


        public void MoveTo(Location location)
        {
            //Does the location have any required items
            if (PlayerDoesNotHaveTheItemRequiredToEnter(location))
            {
                RaiseMessage($"You must have a {location.ItemRequiredToEnter.Name} to enter this location." +
                    Environment.NewLine);
                return;
            }

            //Update the player's current location
            CurrentLocation = location;
            
            //Completely heal the player
            CurrentHitPoints = MaximumHitPoints;
            
            if (location.HasAQuest)
            {
                //See if the player already has the quest
                if (HasThisQuest(location.QuestAvailableHere))
                {
                    //If the player has not completed quest
                    if (!CompletedThisQuest(location.QuestAvailableHere))
                    {
                        //The player has all required items to complete the quest   
                        if (HasAllQuestCompletionItems(location.QuestAvailableHere))
                        {
                            //Display message
                            RaiseMessage(Environment.NewLine);
                            RaiseMessage($"You completed the {location.QuestAvailableHere.Name} quest." + Environment.NewLine);

                            RemoveQuestCompletionItems(location.QuestAvailableHere);

                            //Give quest rewards
                            RaiseMessage("You receive " + Environment.NewLine);
                            RaiseMessage($"{location.QuestAvailableHere.RewardExperiencePoints} experience points" + Environment.NewLine);
                            RaiseMessage($"{location.QuestAvailableHere.RewardGold} gold" + Environment.NewLine);
                            RaiseMessage(location.QuestAvailableHere.RewardItem.Name + Environment.NewLine);
                            RaiseMessage(Environment.NewLine);

                            AddExperiencePoints(location.QuestAvailableHere.RewardExperiencePoints);

                            Gold += location.QuestAvailableHere.RewardGold;

                            //Add reward item to player inventory
                            AddItemToInventory(location.QuestAvailableHere.RewardItem);

                            //Mark the quest as completed
                            MarkQuestCompleted(location.QuestAvailableHere);
                        }
                    }
                }
                else
                {
                    //The player does not have the quest

                    //Display the messages
                    RaiseMessage($"You received the {location.QuestAvailableHere.Name} quest." + Environment.NewLine);
                    RaiseMessage(location.QuestAvailableHere.Description + Environment.NewLine);
                    RaiseMessage("To complete it, return with " + Environment.NewLine);

                    foreach (QuestCompletionItem qci in location.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                        {
                            RaiseMessage($"{qci.Quantity} {qci.Details.Name}" + Environment.NewLine);
                        }
                        else
                        {
                            RaiseMessage($"{qci.Quantity} {qci.Details.NamePlural}" + Environment.NewLine);
                        }
                    }

                    RaiseMessage(Environment.NewLine);

                    //Add the quest to the player's quest list
                    Quests.Add(new PlayerQuest(location.QuestAvailableHere));
                }

            }
            //Does the location have a monster?
            if (location.MonsterLivingHere != null)
            {
                RaiseMessage($"You see a {location.MonsterLivingHere.Name}" + Environment.NewLine);

                //Make a new monster, using the values from the standard monster in the World.Monster list
                Monster standardMonster = World.MonsterByID(location.MonsterLivingHere.ID);

                currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage, standardMonster.RewardExperiencePoints,
                    standardMonster.RewardGold, standardMonster.CurrentHitPoints, standardMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standardMonster.LootTable)
                {
                    currentMonster.LootTable.Add(lootItem);
                }
            }

            else
            {
                currentMonster = null;
            }
        }


        public void MoveNorth()
        {
            if(CurrentLocation.LocationToNorth != null)
                MoveTo(CurrentLocation.LocationToNorth);
        }

        public void MoveSouth()
        {
            if(CurrentLocation.LocationToSouth != null)
                MoveTo(CurrentLocation.LocationToSouth);
        }

        public void MoveEast()
        {
            if (CurrentLocation.LocationToEast != null)
                MoveTo(CurrentLocation.LocationToEast);
        }

        public void MoveWest()
        {
            if (CurrentLocation.LocationToWest != null)
                MoveTo(CurrentLocation.LocationToWest);
        }

        public void UseWeapon(Weapon weapon)
        {

            //Determinte the amount of damage done to the enemy
            int damageToMonster = RandomNumberGenerator.NumberBetween(weapon.MinimumDamage, weapon.MaximumDamage);

            //Apply the damage
            currentMonster.CurrentHitPoints -= damageToMonster;

            //Display message
            RaiseMessage($"You hit the {currentMonster.Name} for {damageToMonster} points.");

            //Check if monster is dead
            if(currentMonster.CurrentHitPoints <= 0)
            {
                //Monster is dead
                RaiseMessage("");
                RaiseMessage($"You defeated the {currentMonster.Name}.");

                //Give player experience points
                AddExperiencePoints(currentMonster.RewardExperiencePoints);
                RaiseMessage($"You receive {currentMonster.RewardExperiencePoints} experience points.");

                //Give player gold
                Gold += currentMonster.RewardGold;
                RaiseMessage($"You receive {currentMonster.RewardGold} gold.");

                //Get random loot from the monster
                List<InventoryItem> lootedItems = new List<InventoryItem>();

                //Add items to the lootedItems list, comparing a random number to the drop percentage
                foreach(LootItem lootItem in currentMonster.LootTable)
                {
                    if(RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }

                //If no items were selected, add default item(s)
                if(lootedItems.Count == 0)
                {
                    foreach(LootItem lootItem in currentMonster.LootTable)
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
                    AddItemToInventory(inventoryItem.Details);

                    if(inventoryItem.Quantity == 1)
                    {
                        RaiseMessage($"You looted {inventoryItem.Quantity} {inventoryItem.Details.Name}.");
                    }
                    else
                    {
                        RaiseMessage($"You looted {inventoryItem.Quantity} {inventoryItem.Details.NamePlural}");
                    }
                }

                //Add a blank line for appearance
                RaiseMessage("");
                //Move player to current location
                MoveTo(CurrentLocation);
            }
            else
            {
                currentMonster.EnemyAttack(this);
                //Monster is still alive
            }
        }

        public void UsePotion(HealingPotion potion)
        {
            //Add healing amount to player's hit points
            CurrentHitPoints += (CurrentHitPoints + potion.AmountToHeal);

            //Current Hit Points cannot exceed MaximumHitPoints
            if(CurrentHitPoints > MaximumHitPoints)
            {
                CurrentHitPoints = MaximumHitPoints;
            }

            //Remove potion from inventory
            RemoveItemFromInventory(potion, 1);

            RaiseMessage($"You drink a {potion.Name}");

            //Monster gets their turn to attack
            currentMonster.EnemyAttack(this);
        }

        private void MoveHome()
        {
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
        }

        private void RaiseMessage(string message, bool addExtraNewLine = false)
        {
            if(OnMessage != null)
            {
                OnMessage(this, new MessageEventArgs(message, addExtraNewLine));
            }
        }
        #endregion
    }
}
