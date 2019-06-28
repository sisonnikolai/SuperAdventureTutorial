using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Engine
{
    public class Monster : LivingCreature
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int MaximumDamage { get; set; }
        public int RewardExperiencePoints { get; set; }
        public int RewardGold { get; set; }
        public List<LootItem> LootTable { get; set; }

        public Monster(int iD, string name, int maximumDamage, int rewardExperiencePoints, int rewardGold, int currentHitPoints, 
            int maximumHitPoints) : base (currentHitPoints, maximumHitPoints)
        {
            ID = iD;
            Name = name;
            MaximumDamage = maximumDamage;
            RewardExperiencePoints = rewardExperiencePoints;
            RewardGold = rewardGold;
            LootTable = new List<LootItem>();
        }
        
        public void EnemyAttack(Player player)
        {
            //Determine the amount of damage the monster does to the player
            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, MaximumDamage);

            //Display message
            rtbMessages.Text += $"The {Name} deals {damageToPlayer} points of damage." + Environment.NewLine;

            //Subtract damage from player
            player.CurrentHitPoints -= damageToPlayer;

            if (player.CurrentHitPoints <= 0)
            {
                rtbMessages.Text += $"The fucking {currentMonster.Name} killed your weak ass." + Environment.NewLine;

                //Move player back to 'Home'
                player.MoveTo(World.LocationByID(World.LOCATION_ID_HOME));

                MessageBox.Show("YOU DIED", "DEAD");
            }
        }
    }
}
