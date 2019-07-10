using Engine;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace SuperAdventureConsole
{
    public class Program
    {
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        private static Player player;

        private static void Main(string[] args)
        {
            //Load the player
            LoadGameData();

            Console.WriteLine("Type 'Help' to see a list of commands.");
            Console.WriteLine();

            DisplayCurrentLocation();

            //Connect player events to functions that will display in the UI
            player.PropertyChanged += PlayerOnPropertyChanged;
            player.OnMessage += PlayerOnMessage;

            //Infinite loop until the user types 'Exit'
            while(true)
            {
                //Display a prompt
                Console.WriteLine(">");

                string userInput = Console.ReadLine();

                if(string.IsNullOrEmpty(userInput))
                {
                    continue;
                }

                //Convert to lower case to make comparisons easier
                string cleanedInput = userInput.ToLower();

                if(cleanedInput == "exit")
                {
                    SaveGameData();

                    break;
                }

                //If the user typed something, try to determine what to do
                ParseInput(cleanedInput);
            }
        }

        private static void ParseInput(string input)
        {
            if (input.Contains("help") || input == "?")
            {
                Console.WriteLine("Available Commands:");
                Console.WriteLine("=====================================");
                Console.WriteLine("Stats - Display player information");
                Console.WriteLine("Look - Get the description of your location");
                Console.WriteLine("Inventory - Display your inventory");
                Console.WriteLine("Quests - Display your quests");
                Console.WriteLine("Attack - Fight the enemy");
                Console.WriteLine("Equip <weapon name> - Set your current weapon");
                Console.WriteLine("Drink <potion name> - Drink a potion");
                Console.WriteLine("Trade - Display your inventory & vendor's inventory");
                Console.WriteLine("Buy <item name> - Buy an item from the vendor");
                Console.WriteLine("Sell <item name> - Sell an item to the vendor");
                Console.WriteLine("North - Move North");
                Console.WriteLine("South - Move South");
                Console.WriteLine("East - Move East");
                Console.WriteLine("West - Move West");
                Console.WriteLine("Exit - Save the game and Exit");
            }
            else if (input == "stats")
            {
                Console.WriteLine($"Current Hit Points: {player.CurrentHitPoints}");
                Console.WriteLine($"Maximum Hit Points: {player.MaximumHitPoints}");
                Console.WriteLine($"Experience Points: {player.ExperiencePoints}");
                Console.WriteLine($"Level: {player.Level}");
                Console.WriteLine($"Gold: {player.Gold}");
            }
            else if (input == "look")
            {
                DisplayCurrentLocation();
            }
            else if (input.Contains("north"))
            {
                if (player.CurrentLocation.LocationToNorth == null)
                {
                    Console.WriteLine("You cannot move North");
                }
                else
                {
                    player.MoveNorth();
                }
            }
            else if (input.Contains("south"))
            {
                if (player.CurrentLocation.LocationToSouth == null)
                {
                    Console.WriteLine("You cannot move South");
                }
                else
                {
                    player.MoveSouth();
                }
            }
            else if (input.Contains("east"))
            {
                if (player.CurrentLocation.LocationToEast == null)
                {
                    Console.WriteLine("You cannot move East");
                }
                else
                {
                    player.MoveEast();
                }
            }
            else if (input.Contains("west"))
            {
                if (player.CurrentLocation.LocationToWest == null)
                {
                    Console.WriteLine("You cannot move West");
                }
                else
                {
                    player.MoveWest();
                }
            }
            else if (input == "inventory")
            {
                foreach(InventoryItem inventoryItem in player.Inventory)
                {
                    Console.WriteLine($"{inventoryItem.Description} : {inventoryItem.Quantity}");
                }
            }
            else if (input == "quests")
            {
                if (player.Quests.Count == 0)
                {
                    Console.WriteLine("You don't have any quests.");
                }
                else
                {
                    foreach (PlayerQuest playerQuest in player.Quests)
                    {
                        Console.WriteLine($"{playerQuest.Name} {(playerQuest.IsCompleted ? "Completed" : "Incomplete")}");
                    }
                }
            }
            else if (input.Contains("attack"))
            {
                if(player.CurrentLocation.MonsterLivingHere == null)
                {
                    Console.WriteLine("There's nothing here to attack. Are you getting crazy or what?");
                }
                else
                {
                    if(player.CurrentWeapon == null)
                    {
                        //Select the first weapon in the inventory
                        //(or 'null' if they don't have any weapons
                        player.CurrentWeapon = player.Weapons.FirstOrDefault();
                    }
                    if(player.CurrentWeapon == null)
                    {
                        Console.WriteLine("You don't have any weapons. Goodbye.");
                    }
                    else
                    {
                        player.UseWeapon(player.CurrentWeapon);
                    }
                }
            }
            else if (input.StartsWith("equip "))
            {
                string inputWeaponName = input.Substring(6).Trim();

                if(string.IsNullOrEmpty(inputWeaponName))
                {
                    Console.WriteLine("You must enter a weapon to equip, jackass.");
                }
                else
                {
                    Weapon weaponToEquip = player.Weapons.SingleOrDefault(x => x.Name.ToLower() == inputWeaponName || x.NamePlural.ToLower() == inputWeaponName);
                    if (weaponToEquip == null)
                    {
                        Console.WriteLine($"You don't have the weapon named {inputWeaponName}");
                    }
                    else
                    {
                        player.CurrentWeapon = weaponToEquip;
                        Console.WriteLine($"You equipped {player.CurrentWeapon.Name}");
                    }
                }
            }
            else if (input.StartsWith("drink "))
            {
                string inputPotionName = input.Substring(6).Trim();

                if (string.IsNullOrEmpty(inputPotionName))
                {
                    Console.WriteLine("You must enter a potion to drink, jackass.");
                }
                else
                {
                    HealingPotion potionToDrink = player.Potions.SingleOrDefault(x => x.Name.ToLower() == inputPotionName || x.NamePlural.ToLower() == inputPotionName);
                    if (potionToDrink == null)
                    {
                        Console.WriteLine($"You don't have the potion named {inputPotionName}");
                    }
                    else
                    {
                        player.UsePotion(potionToDrink);
                    }
                }
            }
            else if (input == "trade")
            {
                if(player.CurrentLocation.VendorPresent == null)
                {
                    Console.WriteLine("There is no vendor here.");
                }
                else
                {
                    Console.WriteLine("YOUR INVENTORY");
                    Console.WriteLine("==============");

                    if(player.Inventory.Count(x => x.Price != World.UNSELLABLE_ITEM_PRICE) == 0)
                    {
                        Console.WriteLine("Your inventory is empty...");
                    }
                    else
                    {
                        foreach(InventoryItem inventoryItem in player.Inventory.Where(x => x.Price != World.UNSELLABLE_ITEM_PRICE))
                        {
                            Console.WriteLine($"{inventoryItem.Quantity} {inventoryItem.Description} Price: {inventoryItem.Price}");
                        }
                    }
                    Console.WriteLine();
                    Console.WriteLine("VENDOR's INVENTORY");
                    Console.WriteLine("==================");
                    if(player.CurrentLocation.VendorPresent.Inventory.Count == 0)
                    {
                        Console.WriteLine("Vendor's inventory is empty...probably got robbed");
                    }
                    else
                    {
                        foreach(InventoryItem inventoryItem in player.CurrentLocation.VendorPresent.Inventory)
                        {
                            Console.WriteLine($"{inventoryItem.Quantity} {inventoryItem.Description} Price: {inventoryItem.Price}");
                        }
                    }
                }
            }
            else if (input.StartsWith("buy "))
            {
                if(player.CurrentLocation.VendorPresent == null)
                {
                    Console.WriteLine("There's no vendor present. Are you getting crazy or something?");
                }
                else
                {
                    string itemName = input.Substring(4).Trim();

                    if (string.IsNullOrEmpty(itemName))
                    {
                        Console.WriteLine("You must enter the item name to buy, the vendor ain't no mind reader");
                    }
                    else
                    {
                        //Get the InventoryItem from the Vendor
                        InventoryItem itemToBuy = player.CurrentLocation.VendorPresent.Inventory.SingleOrDefault(x => x.Details.Name.ToLower()
                        == itemName);

                        //Check if the vendor has the item
                        if(itemToBuy == null)
                        {
                            Console.WriteLine($"The vendor doesn't have the item named {itemName}.");
                        }
                        else
                        {
                            //Check if the player has enough gold to buy
                            if (player.Gold < itemToBuy.Price)
                            {
                                Console.WriteLine($"Not enough gold to buy {itemToBuy.Description}");
                            }
                            else
                            {
                                player.AddItemToInventory(itemToBuy.Details);
                                player.Gold -= itemToBuy.Price;

                                Console.WriteLine($"You bought one {itemToBuy.Details.Name} for {itemToBuy.Price} gold.");
                            }
                        }
                    }
                }
            }
            else if (input.StartsWith("sell "))
            {
                if(player.CurrentLocation.VendorPresent == null)
                {
                    Console.WriteLine("There's no vendor present. Are you getting crazy or something?");
                }
                else
                {
                    string itemName = input.Substring(4).Trim();

                    if (string.IsNullOrEmpty(itemName))
                    {
                        Console.WriteLine("You must enter the item name to sell, the vendor ain't no mind reader");
                    }
                    else
                    {
                        //Get the InventoryItem from the Player
                        InventoryItem itemToSell = player.Inventory.SingleOrDefault(x => x.Details.Name.ToLower() == itemName
                        && x.Quantity > 0 && x.Quantity != World.UNSELLABLE_ITEM_PRICE);

                        //Check if the vendor has the item
                        if (itemToSell == null)
                        {
                            Console.WriteLine($"You can't sell any {itemName}.");
                        }
                        else
                        {
                            player.RemoveItemFromInventory(itemToSell.Details);
                            player.Gold += itemToSell.Price;

                            Console.WriteLine($"You received {itemToSell.Price} gold for your {itemToSell.Details.Name}.");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid command entered");
                Console.WriteLine("Type 'Help' to see a list of available commands");
            }

            Console.WriteLine();
        }

        private static void SaveGameData()
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, player.ToXmlString());

            PlayerDataMapper.SaveToDatabase(player);
        }

        private static void PlayerOnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e.Message);
            if(e.AddExtraNewLine)
            {
                Console.WriteLine();
            }
        }

        private static void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "CurrentLocation")
            {
                DisplayCurrentLocation();

                if(player.CurrentLocation.VendorPresent != null)
                {
                    Console.WriteLine($"You see a vendor named {player.CurrentLocation.VendorPresent.Name}");
                }
            }
        }

        private static void DisplayCurrentLocation()
        {
            Console.WriteLine($"You are at {player.CurrentLocation.Name}.");

            if(player.CurrentLocation.Description != "")
            {
                Console.WriteLine(player.CurrentLocation.Description);
            }
        }

        private static void LoadGameData()
        {
            player = PlayerDataMapper.CreateFromDatabase();

            if(player == null)
            {
                //if(File.Exists(PLAYER_DATA_FILE_NAME))
                //{
                //    player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
                //}
                //else
                //{
                    player = Player.CreateDefaultPlayer();
                //}
            }
        }
    }
}
