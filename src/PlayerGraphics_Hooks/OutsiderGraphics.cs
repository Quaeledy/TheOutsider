using System;

namespace TheOutsider.PlayerGraphics_Hooks
{
    public class OutsiderGraphics
    {
        public WeakReference<PlayerGraphics> playerRef;
        public WeakReference<TheOutsider> outsiderRef;
        public bool isPup;

        public OutsiderGraphics(PlayerGraphics self, TheOutsider outsider)
        {
            playerRef = new WeakReference<PlayerGraphics>(self);
            outsiderRef = new WeakReference<TheOutsider>(outsider);
            isPup = self.player.playerState.isPup || (self.player.isSlugpup && !self.player.playerState.forceFullGrown);
        }
    }
}
