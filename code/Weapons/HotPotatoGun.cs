using System;
using System.Collections.Generic;
using System.Linq;
using TerrorTown;

namespace Sandbox.Weapons
{

    [Title("Hot Potato Gun"), Category("Weapons")]
    public partial class HotPotatoGun : Gun
    {
        public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
        public override string WorldModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
        public override float PrimaryAttackDelay => 0.5f;
        public override int MaxPrimaryAmmo => 1;
        public override AmmoType PrimaryAmmoType => AmmoType.None;
        public override bool Automatic => false;
        private RealTimeSince HotTimer { get; set; }
        private Sound Ticker { get; set; }
        private List<TerrorTown.Player> KnownVictim { get; set; }
  
        public HotPotatoGun()
        {
            Droppable = false;
            HotTimer = 0;
            KnownVictim = new List<TerrorTown.Player>();
        }
        public override void Spawn()
        {
            base.Spawn();
            Ticker = Sound.FromEntity("ticktick", this);
        }

        protected override void OnDestroy()
        {
            Ticker.Stop();
        }

        public static TerrorTown.Player GetOtherAlivePlayer(Entity me)
        {
            var aliveclients = Game.Clients.Where(x => x.Pawn is TerrorTown.Player ply && ply.LifeState == LifeState.Alive && ply != me);
            if (aliveclients.Any())
            {
                var randomindex = Game.Random.Int(aliveclients.Count() - 1);
                return aliveclients.ElementAt(randomindex).Pawn as TerrorTown.Player;
            }
            return null;
        }
        public override void PrimaryAttack()
        {
            ShootBullet(0f, 0f, 0f);
            PlaySound("rust_pistol.shoot");
            (Owner as AnimatedEntity)?.SetAnimParameter("b_attack", true);
            ShootEffects();
        }
        public override void OnBulletTraceHit(TraceResult tr)
        {
            if (tr.Entity is TerrorTown.Player victim)
            {
                if (Game.IsServer)
                {
                    TerrorTown.Player ply = Owner as TerrorTown.Player;
                    ply.Inventory.Items.Remove(this);
                    if (!KnownVictim.Contains(victim))
                    {
                        KnownVictim.Add(victim);
                        HotTimer = Math.Min((int)HotTimer, 25);
                    }
                    victim.Inventory.AddItem(this);
                    SetHotPotatoGunActive(victim, this);
                }
            }
        }

        [ClientRpc]
        public static void SetHotPotatoGunActiveClient(TerrorTown.Player player, HotPotatoGun gun)
        {
            player.Inventory.SetActiveSlot(player.Inventory.Items.IndexOf(gun));
        }

        public static void SetHotPotatoGunActive(TerrorTown.Player player, HotPotatoGun gun)
        {
            SetHotPotatoGunActiveClient(player, gun);
            player.Inventory.SetActiveSlot(player.Inventory.Items.IndexOf(gun));
        }

        [GameEvent.Tick.Server]
        public void Gametick()
        {
            if (HotTimer > 30)
            {
                var OwnerPreExplode = Owner;
                var exploder = new ExplosionEntity();
                exploder.Damage = 200f;
                exploder.Position = Owner.Position;
                exploder.Radius *= 2;
                exploder.RemoveOnExplode = true;
                exploder.Explode(null);
                HotTimer = 0;
                if (OwnerPreExplode == Owner)
                {
                    Owner.TakeDamage(new DamageInfo { Damage = 25f });
                }
                
            }
        }

        [Event("Player.PreOnKilled")]
        public void PreOnOwnerKilled(DamageInfo _, TerrorTown.Player ply)
        {
            if (ply == Owner)
            {
                if (Game.IsServer)
                {
                    TerrorTown.Player victim = GetOtherAlivePlayer(Owner);
                    ply.Inventory.Items.Remove(this);
                    victim?.Inventory.AddItem(this);
                    HotTimer = 0;
                    SetHotPotatoGunActive(victim, this);
                    KnownVictim = new List<TerrorTown.Player>();
                }
            }
        }

        public override void ReloadPrimary()
        {

        }
    }
}
