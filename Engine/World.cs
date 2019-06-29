using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class World
    {
        public static readonly List<Item> Items = new List<Item>();
        public static readonly List<Monster> Monsters = new List<Monster>();
        public static readonly List<Quest> Quests = new List<Quest>();
        public static readonly List<Location> Locations = new List<Location>();

        public const int ITEM_ID_RUSTY_SWORD = 1;
        public const int ITEM_ID_RAT_TAIL = 2;
        public const int ITEM_ID_PIECE_OF_FUR = 3;
        public const int ITEM_ID_SNAKE_FANG = 4;
        public const int ITEM_ID_SNAKESKIN =  5;
        public const int ITEM_ID_CLUB= 6;
        public const int ITEM_ID_HEALING_POTION = 7;
        public const int ITEM_ID_SPIDER_FANG = 8;
        public const int ITEM_ID_SPIDER_SILK = 9;
        public const int ITEM_ID_ADVENTURER_PASS = 10;

        public const int MONSTER_ID_RAT = 1;
        public const int MONSTER_ID_SNAKE = 2;
        public const int MONSTER_ID_GIANT_SPIDER = 3;

        public const int QUEST_ID_CLEAR_ALCHEMIST_GARDEN = 1;
        public const int QUEST_ID_CLEAR_FARMERS_FIELD = 2;

        public const int LOCATION_ID_HOME = 1;
        public const int LOCATION_ID_TOWN_SQUARE = 2;
        public const int LOCATION_ID_GUARD_POST = 3;
        public const int LOCATION_ID_ALCHEMIST_HUT = 4;
        public const int LOCATION_ID_ALCHEMISTS_GARDEN = 5;
        public const int LOCATION_ID_FARMHOUSE = 6;
        public const int LOCATION_ID_FARM_FIELD = 7;
        public const int LOCATION_ID_BRIDGE = 8;
        public const int LOCATION_ID_SPIDER_FIELD = 9;

        public const int UNSELLABLE_ITEM_PRICE = -1;

        static World()
        {
            PopulateItems();
            PopulateMonsters();
            PopulateQuests();
            PopulateLocations();
        }

        private static void PopulateItems()
        {
            //inlining - created the value and added it to the list
            Items.Add(new Weapon(ITEM_ID_RUSTY_SWORD, "Rusty Sword of Powah", "Rusty ass swords", 0, 6, 5));
            Items.Add(new Item(ITEM_ID_RAT_TAIL, "Rat tail", "Rat tails", 1));
            Items.Add(new Item(ITEM_ID_PIECE_OF_FUR, "Piece of fur", "Pieces of fur", 1));
            Items.Add(new Item(ITEM_ID_SNAKE_FANG, "Snake fang", "Snake fangs", 1));
            Items.Add(new Item(ITEM_ID_SNAKESKIN, "Snakeskin", "Snakeskins", 2));
            Items.Add(new Weapon(ITEM_ID_CLUB, "Superclub", "Superclubs", 3, 10, 8));
            Items.Add(new HealingPotion(ITEM_ID_HEALING_POTION, "Healing Potion", "Healing Potions", 5, 3));
            Items.Add(new Item(ITEM_ID_SPIDER_FANG, "Spider fang", "Spider fangs", 2));
            Items.Add(new Item(ITEM_ID_SPIDER_SILK, "Spider silk", "Spider silks", 2));
            Items.Add(new Item(ITEM_ID_ADVENTURER_PASS, "Adventurer Pass", "Adventurer Passes", UNSELLABLE_ITEM_PRICE));
        }

        private static void PopulateMonsters()
        {
            Monster rat = new Monster(MONSTER_ID_RAT, "Rattata", 5, 35, 10, 3, 3);
            rat.LootTable.Add(new LootItem(ItemByID(ITEM_ID_RAT_TAIL), 75, false));
            rat.LootTable.Add(new LootItem(ItemByID(ITEM_ID_PIECE_OF_FUR), 75, true));

            Monster snake = new Monster(MONSTER_ID_SNAKE, "Ekans", 5, 3, 10, 3, 3);
            snake.LootTable.Add(new LootItem(ItemByID(ITEM_ID_SNAKE_FANG), 75, false));
            snake.LootTable.Add(new LootItem(ItemByID(ITEM_ID_SNAKESKIN), 75, true));

            Monster giantSpider = new Monster(MONSTER_ID_GIANT_SPIDER, "Tarantula~", 20, 5, 40, 10, 10);
            giantSpider.LootTable.Add(new LootItem(ItemByID(ITEM_ID_SPIDER_FANG), 75, true));
            giantSpider.LootTable.Add(new LootItem(ItemByID(ITEM_ID_SPIDER_SILK), 25, false));

            Monsters.Add(rat);
            Monsters.Add(snake);
            Monsters.Add(giantSpider);
        }

        private static void PopulateQuests()
        {
            Quest clearAlchemistGarden = new Quest(QUEST_ID_CLEAR_ALCHEMIST_GARDEN, "Clear the alchemist's garden",
               "Kill rats in the alchemist's garden and bring back 3 rat tails.You will receive a healing potion and 10 gold pieces.", 20, 10);
            clearAlchemistGarden.QuestCompletionItems.Add(new QuestCompletionItem(ItemByID(ITEM_ID_RAT_TAIL), 3));
            clearAlchemistGarden.RewardItem = ItemByID(ITEM_ID_HEALING_POTION);

            Quest clearFarmersField = new Quest(QUEST_ID_CLEAR_FARMERS_FIELD, "Clear the farmer's field",
               "Kill snakes in the farmer's field and bring back 3 snake fangs.You will receive an adventurer's pass and 20 gold pieces.", 20, 20);
            clearFarmersField.QuestCompletionItems.Add(new QuestCompletionItem(ItemByID(ITEM_ID_SNAKE_FANG), 3));
            clearFarmersField.RewardItem = ItemByID(ITEM_ID_ADVENTURER_PASS);

            Quests.Add(clearAlchemistGarden);
            Quests.Add(clearFarmersField);
        }

        private static void PopulateLocations()
        {
            // Create each location
            Location home = new Location(LOCATION_ID_HOME, "Home", "Your house. It's fucking filthy my dude.");

            Location townSquare = new Location(LOCATION_ID_TOWN_SQUARE, "Town Square", "You see a fountain. Smells like pee.");

            Vendor vendor = new Vendor("Bobby");
            vendor.AddItemToInventory(ItemByID(ITEM_ID_RAT_TAIL), 5);
            vendor.AddItemToInventory(ItemByID(ITEM_ID_PIECE_OF_FUR), 3);
            vendor.AddItemToInventory(ItemByID(ITEM_ID_HEALING_POTION), 5);

            townSquare.VendorPresent = vendor;

            Location alchemistHut = new Location(LOCATION_ID_ALCHEMIST_HUT, "Alchemist's Hut", "Some potions and weird ass plants surround you.")
            {
                QuestAvailableHere = QuestByID(QUEST_ID_CLEAR_ALCHEMIST_GARDEN)
            };

            Location alchemistsGarden = new Location(LOCATION_ID_ALCHEMISTS_GARDEN, "Alchemist's Garder", "Even more weird plants and some might even want to eat you.")
            {
                MonsterLivingHere = MonsterByID(MONSTER_ID_RAT)
            };

            Location farmHouse = new Location(LOCATION_ID_FARMHOUSE, "Farmhouse", "A small farmhouse. The farmer in front stares at you lifelessly.")
            {
                QuestAvailableHere = QuestByID(QUEST_ID_CLEAR_FARMERS_FIELD)
            };

            Location farmersField = new Location(LOCATION_ID_FARM_FIELD, "Farmer's Field", "This place is somehow even weirder than that Alchemist dude's place. Where the fuck am I?")
            {
                MonsterLivingHere = MonsterByID(MONSTER_ID_SNAKE)
            };

            Location guardPost = new Location(LOCATION_ID_GUARD_POST, "Guard's Post", "There's a tough looking bastard here. Dude's probably gay I bet.", ItemByID(ITEM_ID_ADVENTURER_PASS));

            Location bridge = new Location(LOCATION_ID_BRIDGE, "Strange Bridge", "A creepy bridge. You sure you wanna continue?");

            Location spiderField = new Location(LOCATION_ID_SPIDER_FIELD, "Spider Forest", "The name speaks for itself.")
            {
                MonsterLivingHere = MonsterByID(MONSTER_ID_GIANT_SPIDER)
            };

            //Link the locations together
            home.LocationToNorth = townSquare;

            townSquare.LocationToNorth = alchemistHut;
            townSquare.LocationToSouth = home;
            townSquare.LocationToEast = guardPost;
            townSquare.LocationToWest = farmHouse;

            farmHouse.LocationToEast = townSquare;
            farmHouse.LocationToWest = farmersField;

            farmersField.LocationToEast = farmHouse;

            alchemistHut.LocationToSouth = townSquare;
            alchemistHut.LocationToNorth = alchemistsGarden;

            alchemistsGarden.LocationToSouth = alchemistHut;

            guardPost.LocationToEast = bridge;
            guardPost.LocationToWest = townSquare;

            bridge.LocationToWest = guardPost;
            bridge.LocationToEast = spiderField;

            spiderField.LocationToWest = bridge;

            //Add the locations to the static list
            Locations.Add(home);
            Locations.Add(townSquare);
            Locations.Add(guardPost);
            Locations.Add(alchemistHut);
            Locations.Add(alchemistsGarden);
            Locations.Add(farmersField);
            Locations.Add(farmHouse);
            Locations.Add(bridge);
            Locations.Add(spiderField);
        }

        //the following codes that return null is generally not a good idea
        //since the function is expecting an object returned
        public static Monster MonsterByID(int id)
        {
            foreach (Monster monster in Monsters)
            {
                if (monster.ID == id)
                    return monster;
            }
            return null;
        }

        public static Quest QuestByID(int id)
        {
            foreach(Quest quest in Quests)
            {
                if (quest.ID == id)
                    return quest;
            }
            return null;
        }

        public static Item ItemByID(int id)
        {
            foreach(Item item in Items)
            {
                if (item.ID == id)
                    return item;
            }
            return null;
        }

        public static Location LocationByID(int id)
        {
            foreach(Location location in Locations)
            {
                if (location.ID == id)
                    return location;
            }
            return null;
        }
    }
}
