using System.Linq;
using TerrorTown;
using SM1Minigames;
using Sandbox;
using Sandbox.Weapons;

namespace HPGMinigame {

    public partial class HotPotatoGunMinigame : Minigame
    {
        public override string Name { get; set; } = "Hot Potato Gun";
        public override string Description { get; set; } = "Wouldn't recommend holding it for longer than 30 seconds";
        public static TerrorTown.Player GetAlivePlayer()
        {
            var aliveclients = Game.Clients.Where(x => x.Pawn is TerrorTown.Player ply && ply.LifeState == LifeState.Alive && !x.IsBot);
            var randomindex = Game.Random.Int(aliveclients.Count() - 1);
            return aliveclients.ElementAt(randomindex).Pawn as TerrorTown.Player;
        }

        [Event("Player.PreTakeDamage")]
        public static void PreTakeDamage(DamageInfo info, TerrorTown.Player ply)
        {
            if (info.Attacker is TerrorTown.Player)
            {
                ply.PendingDamage.Damage = 0;
            }
        }

        public override void RoundStart()
        {
            base.RoundStart();

            foreach (IClient client in Game.Clients)
            {
                TerrorTown.Player ply = client.Pawn as TerrorTown.Player;
                // Can't just add to inventory, because radar has custom functions when picked up.
                Radar radar = new();
                radar.Position = ply.Position;

            }
            TerrorTown.Player victim = GetAlivePlayer();
            HotPotatoGun gun = new();
            victim.Inventory.AddItem(gun);
            HotPotatoGun.SetHotPotatoGunActive(victim, gun);
        }
    }
}
