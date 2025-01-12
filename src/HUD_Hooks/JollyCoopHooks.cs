using JollyCoop.JollyHUD;
using JollyCoop.JollyMenu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TheOutsider.Player_Hooks;
using UnityEngine;

namespace TheOutsider.HUD_Hooks
{
    class JollyCoopHooks
    {
        public static ConditionalWeakTable<AbstractCreature, OutsiderPlayerIcon> OutsiderPlayerIconData = new();
        public static ConditionalWeakTable<AbstractCreature, OutsiderJollyOffRoom> OutsiderJollyOffRoomData = new();

        public static void Init()
        {
            //游戏中右下角头像
            On.JollyCoop.JollyHUD.JollyMeter.PlayerIcon.ctor += JollyMeter_PlayerIcon_ctor;
            On.JollyCoop.JollyHUD.JollyMeter.PlayerIcon.Draw += JollyMeter_PlayerIcon_Draw;
            On.JollyCoop.JollyHUD.JollyMeter.PlayerIcon.Update += JollyMeter_PlayerIcon_Update;
            On.JollyCoop.JollyHUD.JollyMeter.PlayerIcon.ClearSprites += JollyMeter_PlayerIcon_ClearSprites;
            //玩家在房间外时的头像
            //On.JollyCoop.JollyHUD.JollyPlayerSpecificHud.JollyOffRoom.Update += JollyPlayerSpecificHud_JollyOffRoom_Update;
            //On.JollyCoop.JollyHUD.JollyPlayerSpecificHud.JollyOffRoom.Draw += JollyPlayerSpecificHud_JollyOffRoom_Draw;
            //On.JollyCoop.JollyHUD.JollyPlayerSpecificHud.JollyOffRoom.ClearSprites += JollyPlayerSpecificHud_JollyOffRoom_ClearSprites;
            //联机界面选猫体型图标
            On.JollyCoop.JollyMenu.JollyPlayerSelector.GetPupButtonOffName += JollyPlayerSelector_GetPupButtonOffName;
            On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.LoadIcon += SymbolButtonTogglePupButton_LoadIcon;
            On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.HasUniqueSprite += SymbolButtonTogglePupButton_HasUniqueSprite;
            On.JollyCoop.JollyMenu.SymbolButtonToggle.LoadIcon += SymbolButtonToggleButton_LoadIcon;
        }

        #region 游戏中右下角头像
        private static void JollyMeter_PlayerIcon_ctor(On.JollyCoop.JollyHUD.JollyMeter.PlayerIcon.orig_ctor orig, JollyMeter.PlayerIcon self, JollyMeter meter, AbstractCreature associatedPlayer, Color color)
        {
            orig(self, meter, associatedPlayer, color);
            if ((//(associatedPlayer.state as PlayerState).slugcatCharacter == Plugin.SlugName ||
                (associatedPlayer.realizedCreature != null && (associatedPlayer.realizedCreature as Player).SlugCatClass == Plugin.SlugName)) &&
                !OutsiderPlayerIconData.TryGetValue(associatedPlayer, out _))
            {
                self.iconSprite.element = Futile.atlasManager.GetElementWithName("Kill_Slugcat" + "_" + Plugin.SlugName.value + "A");

                int i = -1;
                List<AbstractCreature> players = (meter.hud.owner as Player).abstractCreature.world.game.session.Players;
                for (int j = 0; j < players.Count; j++)
                    if (players[j] == associatedPlayer)
                    {
                        i = j;
                        break;
                    }
                if (i != -1)
                    OutsiderPlayerIconData.Add(associatedPlayer, new OutsiderPlayerIcon(meter, i, self, players[i]));
            }
        }

        private static void JollyMeter_PlayerIcon_Draw(On.JollyCoop.JollyHUD.JollyMeter.PlayerIcon.orig_Draw orig, JollyMeter.PlayerIcon self, float timeStacker)
        {
            orig(self, timeStacker);
            if (OutsiderPlayerIconData.TryGetValue(self.player, out var icon))
            {
                icon.Draw(timeStacker);
            }
        }

        private static void JollyMeter_PlayerIcon_Update(On.JollyCoop.JollyHUD.JollyMeter.PlayerIcon.orig_Update orig, JollyMeter.PlayerIcon self)
        {
            orig(self);
            if (OutsiderPlayerIconData.TryGetValue(self.player, out var icon))
            {
                icon.Update();
            }
        }

        private static void JollyMeter_PlayerIcon_ClearSprites(On.JollyCoop.JollyHUD.JollyMeter.PlayerIcon.orig_ClearSprites orig, JollyMeter.PlayerIcon self)
        {
            orig(self);
            if (OutsiderPlayerIconData.TryGetValue(self.player, out var icon))
            {
                icon.ClearSprites();
                OutsiderPlayerIconData.Remove(self.player);
            }
        }
        #endregion
        #region 玩家在房间外时的头像
        private static void JollyPlayerSpecificHud_JollyOffRoom_Update(On.JollyCoop.JollyHUD.JollyPlayerSpecificHud.JollyOffRoom.orig_Update orig, JollyPlayerSpecificHud.JollyOffRoom self)
        {
            orig(self);

            if ((//self.jollyHud.PlayerState.slugcatCharacter == Plugin.SlugName ||
                (self.PlayerState.creature != null && self.PlayerState.creature.realizedCreature != null && (self.PlayerState.creature.realizedCreature as Player).SlugCatClass == Plugin.SlugName)) &&
                !self.sprites[0].element.name.Contains(Plugin.SlugName.value) &&
                !OutsiderJollyOffRoomData.TryGetValue(self.PlayerState.creature, out _))
            {
                int i = -1;
                List<AbstractCreature> players = null;
                if (self.jollyHud.abstractPlayer.world != null)
                    players = self.jollyHud.abstractPlayer.world.game.session.Players;
                if (players != null)
                    for (int j = 0; j < players.Count; j++)
                        if (players[j] == self.jollyHud.abstractPlayer)
                            i = j;
                if (i != -1)
                    OutsiderJollyOffRoomData.Add(self.jollyHud.abstractPlayer, new OutsiderJollyOffRoom(i, self));
            }
        }

        private static void JollyPlayerSpecificHud_JollyOffRoom_Draw(On.JollyCoop.JollyHUD.JollyPlayerSpecificHud.JollyOffRoom.orig_Draw orig, JollyPlayerSpecificHud.JollyOffRoom self, float timeStacker)
        {
            if (self.hidden)
            {
                for (int i = 0; i < self.sprites.Count; i++)
                {
                    self.sprites[i].isVisible = false;
                }
            }

            orig(self, timeStacker);

            if (self.PlayerState.creature != null && OutsiderJollyOffRoomData.TryGetValue(self.PlayerState.creature, out var jollyOffRoom))
            {
                jollyOffRoom.Draw(timeStacker);
            }
        }

        private static void JollyPlayerSpecificHud_JollyOffRoom_ClearSprites(On.JollyCoop.JollyHUD.JollyPlayerSpecificHud.JollyOffRoom.orig_ClearSprites orig, JollyPlayerSpecificHud.JollyOffRoom self)
        {
            orig(self);
            if (self.PlayerState.creature != null && OutsiderJollyOffRoomData.TryGetValue(self.PlayerState.creature, out var jollyOffRoom))
            {
                OutsiderPlayerIconData.Remove(self.PlayerState.creature);
            }
        }
        #endregion
        #region 惬意合作选体型图标
        private static bool SymbolButtonTogglePupButton_HasUniqueSprite(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_HasUniqueSprite orig, SymbolButtonTogglePupButton self)
        {
            bool result = orig(self);
            result = result || self.symbolNameOff.Contains("outsider");
            return result;
        }

        private static string JollyPlayerSelector_GetPupButtonOffName(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_GetPupButtonOffName orig, JollyPlayerSelector self)
        {
            string result = orig(self);
            string text = "pup_off";
            SlugcatStats.Name playerClass = self.JollyOptions(self.index).playerClass;
            if (playerClass != null && playerClass.value.Equals(Plugin.SlugName.value))
            {
                result = "outsider_" + text;
            }
            return result;
        }

        private static void SymbolButtonTogglePupButton_LoadIcon(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_LoadIcon orig, SymbolButtonTogglePupButton self)
        {
            orig(self);
            if (self.uniqueSymbol != null && self.HasUniqueSprite() && self.symbol.fileName.Contains("on") && self.uniqueSymbol.fileName.Contains("outsider"))
            {
                self.uniqueSymbol.fileName = "unique_" + "outsider_pup_on";
                self.uniqueSymbol.LoadFile();
                self.uniqueSymbol.sprite.SetElementByName(self.uniqueSymbol.fileName);
                self.uniqueSymbol.pos.y = self.size.y / 2f;
            }
        }

        private static void SymbolButtonToggleButton_LoadIcon(On.JollyCoop.JollyMenu.SymbolButtonToggle.orig_LoadIcon orig, SymbolButtonToggle self)
        {
            orig(self);
            string text = self.symbolNameOff;
            if (self.isToggled)
            {
                text = self.symbolNameOn;
            }
            if (text.Contains("on") && self.symbolNameOff.Contains("outsider"))
            {
                self.symbol.fileName = "outsider_pup_on";
                self.symbol.LoadFile();
                self.symbol.sprite.SetElementByName(self.symbol.fileName);
                self.symbol.fileName = text;
            }
        }
        #endregion
    }

    class OutsiderPlayerIcon
    {
        public static WeakReference<AbstractCreature> associatedPlayerRef;
        public FSprite newIconSprite;
        public AbstractCreature player;
        public Color iconColor;
        public Color color;
        private bool dead;
        private bool setColor;
        JollyMeter meter;
        JollyMeter.PlayerIcon icon;
        int i;

        private PlayerState playerState
        {
            get
            {
                return this.player.state as PlayerState;
            }
        }

        public OutsiderPlayerIcon(JollyMeter meter, int i, JollyMeter.PlayerIcon icon, AbstractCreature associatedPlayer)
        {
            associatedPlayerRef = new WeakReference<AbstractCreature>(associatedPlayer);
            this.newIconSprite = new FSprite("Kill_Slugcat" + "_" + Plugin.SlugName.value + "B", true);
            this.player = associatedPlayer;
            this.setColor = false;
            this.meter = meter;
            this.i = i;
            this.color = icon.color;
            this.iconColor = icon.color;
            icon.meter.fContainer.AddChild(this.newIconSprite);
        }

        public void Draw(float timeStacker)
        {
            if (!associatedPlayerRef.TryGetTarget(out var associatedPlayer) || icon == null)
                return;
            if (!icon.iconSprite.element.name.Contains(Plugin.SlugName.value))
            {
                if (this.playerState.permaDead || this.playerState.dead)
                    icon.iconSprite.element = Futile.atlasManager.GetElementWithName("Multiplayer_Death" + "_" + Plugin.SlugName.value);
                else
                    icon.iconSprite.element = Futile.atlasManager.GetElementWithName("Kill_Slugcat" + "_" + Plugin.SlugName.value + "A");
            }
            this.newIconSprite.alpha = icon.iconSprite.alpha;
            this.newIconSprite.x = icon.iconSprite.x;
            this.newIconSprite.y = icon.iconSprite.y + (float)(this.dead ? 5 : 0);

            if (icon.meter.counter % 6 < 2 && icon.lastBlink > 0f)
            {
                this.color = Color.Lerp(this.iconColor, Custom.HSL2RGB(Custom.RGB2HSL(this.iconColor).x, Custom.RGB2HSL(this.iconColor).y, Custom.RGB2HSL(this.iconColor).z + 0.2f), Mathf.InverseLerp(0f, 0.5f, Mathf.Lerp(icon.lastBlink, icon.blink, timeStacker)));
            }
            this.newIconSprite.color = this.color;
        }

        public void Update()
        {
            if (!associatedPlayerRef.TryGetTarget(out var associatedPlayer))
                return;
            if (this.icon == null)
                this.icon = meter.playerIcons[i];
            if (!setColor && !(this.playerState.permaDead || this.playerState.dead) &&
                player.realizedCreature != null && PlayerHooks.PlayerData.TryGetValue(player.realizedCreature as Player, out var playerEX) &&
                Plugin.AntennaeColor.GetColor(player.realizedCreature.graphicsModule as PlayerGraphics) != null)
            {
                setColor = true;
                this.color = playerEX.GetAntennaeColor();
                this.iconColor = playerEX.GetAntennaeColor();
            }
            if (this.playerState.permaDead || this.playerState.dead)
            {
                setColor = false;
                this.color = Color.gray;
                if (!this.dead)
                {
                    this.newIconSprite.scale *= 0.8f;
                    this.dead = true;
                }
            }
        }

        public void ClearSprites()
        {
            this.newIconSprite.RemoveFromContainer();
        }
    }

    class OutsiderJollyOffRoom
    {
        public static WeakReference<JollyPlayerSpecificHud.JollyOffRoom> jollyOffRoomRef;
        private int newSprite;
        private int i;

        public OutsiderJollyOffRoom(int i, JollyPlayerSpecificHud.JollyOffRoom jollyOffRoom)
        {
            jollyOffRoomRef = new WeakReference<JollyPlayerSpecificHud.JollyOffRoom>(jollyOffRoom);
            this.i = i;

            foreach (var sprite in jollyOffRoom.sprites)
            {
                if (sprite.element.name.StartsWith("GuidanceSlugcat"))
                {
                    sprite.element = Futile.atlasManager.GetElementWithName(Plugin.SlugName.value + "GuidanceSlugcat" + "A");
                }
            }

            newSprite = jollyOffRoom.sprites.Count;
            jollyOffRoom.sprites.Add(new FSprite(Plugin.SlugName.value + "GuidanceSlugcat" + "B", true)
            {
                shader = jollyOffRoom.jollyHud.hud.rainWorld.Shaders["Hologram"],
                scale = jollyOffRoom.scale
            });

            if (jollyOffRoom.jollyHud.RealizedPlayer.graphicsModule != null)
            {
                jollyOffRoom.sprites[newSprite].color = jollyOffRoom.jollyHud.playerColor;
                /*Plugin.AntennaeColor.GetColor(jollyOffRoom.jollyHud.RealizedPlayer.graphicsModule as PlayerGraphics) != null ?
                (Color)Plugin.AntennaeColor.GetColor(jollyOffRoom.jollyHud.RealizedPlayer.graphicsModule as PlayerGraphics) : new Color(106f / 255f, 229f / 255f, 191f / 255f);*/
                jollyOffRoom.sprites[newSprite].alpha = 0.1f;
                jollyOffRoom.jollyHud.fContainer.AddChild(jollyOffRoom.sprites[newSprite]);
                jollyOffRoom.sprites[newSprite].MoveInFrontOfOtherNode(jollyOffRoom.sprites[0]);
            }
        }

        public void Draw(float timeStacker)
        {
            if (!jollyOffRoomRef.TryGetTarget(out var jollyOffRoom))
                return;

            if (jollyOffRoom.jollyHud.PlayerState.slugcatCharacter == Plugin.SlugName && jollyOffRoom.sprites[0].element.name.Contains(Plugin.SlugName.value))
            {
                jollyOffRoom.uAlpha = Mathf.SmoothStep(jollyOffRoom.lastAlpha, jollyOffRoom.alpha, timeStacker);
                Vector2 vector = Vector2.Lerp(jollyOffRoom.drawPos, jollyOffRoom.lastDrawPos, timeStacker);
                jollyOffRoom.sprites[newSprite].isVisible = true;
                jollyOffRoom.sprites[newSprite].x = vector.x;
                jollyOffRoom.sprites[newSprite].y = vector.y;
                jollyOffRoom.sprites[newSprite].scale = jollyOffRoom.scale;
                jollyOffRoom.sprites[newSprite].alpha = Mathf.Lerp(jollyOffRoom.sprites[newSprite].alpha, jollyOffRoom.uAlpha, timeStacker * 0.5f);

                /*
                List<AbstractCreature> players = null;
                if (jollyOffRoom.jollyHud.abstractPlayer.world != null)
                    players = jollyOffRoom.jollyHud.abstractPlayer.world.game.session.Players;
                AbstractCreature player = players != null ? players[i] : jollyOffRoom.jollyHud.abstractPlayer;*/
                Player player = jollyOffRoom.jollyHud.abstractPlayer.realizedCreature as Player;
                if (player != null && PlayerHooks.PlayerData.TryGetValue(player, out var playerEX))//!setColor &&
                {
                    jollyOffRoom.sprites[newSprite].color = playerEX.GetAntennaeColor();
                }
            }
        }
    }
}
