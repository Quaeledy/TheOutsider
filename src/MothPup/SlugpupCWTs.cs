using MoreSlugcats;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TheOutsider.MothPup
{
    public static class SlugpupCWTs
    {
        public static readonly ConditionalWeakTable<PlayerGraphics, PupGraphics> pupGraphicsCWT = new();

        public static readonly ConditionalWeakTable<Player, ParentVariables> parentVariablesCWT = new();

        public static readonly ConditionalWeakTable<SlugNPCAI, PupVariables> pupCWT = new();

        public static readonly ConditionalWeakTable<PlayerNPCState, PupNPCState> pupStateCWT = new();

        public static readonly ConditionalWeakTable<AbstractCreature, PupAbstract> pupAbstractCWT = new();


        public static bool isMothPup(this Player self)
        {
            bool state = false;
            if (self.playerState.TryGetPupState(out var pupNPCState))
            {
                state = pupNPCState.Variant == SlugpupStuff.VariantName.MothPup;
            }
            return state || self.slugcatStats.name == SlugpupStuff.VariantName.MothPup;
        }
        public static bool isMothPup(this SlugNPCAI self)
        {
            bool state = false;
            if (self.cat.playerState.TryGetPupState(out var pupNPCState))
            {
                state = pupNPCState.Variant == SlugpupStuff.VariantName.MothPup;
            }
            return state || self.cat.slugcatStats.name == SlugpupStuff.VariantName.MothPup;
        }

        public static PupNPCState GetPupState(this PlayerNPCState self)
        {
            if (self != null)
            {
                return pupStateCWT.GetValue(self, _ => new PupNPCState());
            }
            return null;
        }
        public static bool TryGetPupState(this PlayerState self, out PupNPCState pupNPCState)
        {
            if (self != null && self is PlayerNPCState playerNPCState)
            {
                pupNPCState = playerNPCState.GetPupState();
            }
            else pupNPCState = null;

            return pupNPCState != null;
        }
        public static bool TryGetPupState(this PlayerNPCState self, out PupNPCState pupNPCState)
        {
            if (self != null)
            {
                pupNPCState = self.GetPupState();
            }
            else pupNPCState = null;

            return pupNPCState != null;
        }

        public static ParentVariables GetParentVariables(this Player self)
        {
            if (self != null)
            {
                return parentVariablesCWT.GetValue(self, _ => new ParentVariables());
            }
            return null;
        }
        public static bool TryGetParentVariables(this Player self, out ParentVariables parentVariables)
        {
            if (self != null)
            {
                parentVariables = self.GetParentVariables();
            }
            else parentVariables = null;

            return parentVariables != null;
        }
        public static PupVariables GetPupVariables(this SlugNPCAI self)
        {
            if (self != null)
            {
                return pupCWT.GetValue(self, _ => new PupVariables());
            }
            return null;
        }
        public static bool TryGetPupVariables(this Player self, out PupVariables pupVariables)
        {
            if (self.AI != null)
            {
                pupVariables = self.AI.GetPupVariables();
            }
            else pupVariables = null;

            return pupVariables != null;
        }
        public static bool TryGetPupVariables(this SlugNPCAI self, out PupVariables pupVariables)
        {
            if (self != null)
            {
                pupVariables = self.GetPupVariables();
            }
            else pupVariables = null;

            return pupVariables != null;
        }

        public static PupGraphics GetPupGraphics(this PlayerGraphics self)
        {
            if (self != null)
            {
                return pupGraphicsCWT.GetValue(self, _ => new PupGraphics());
            }
            return null;
        }
        public static bool TryGetPupGraphics(this PlayerGraphics self, out PupGraphics pupGraphics)
        {
            if (self != null)
            {
                pupGraphics = self.GetPupGraphics();
            }
            else pupGraphics = null;

            return pupGraphics != null;
        }

        public static PupAbstract GetPupAbstract(this AbstractCreature self)
        {
            if (self != null)
            {
                return pupAbstractCWT.GetValue(self, _ => new PupAbstract());
            }
            return null;
        }
        public static bool TryGetPupAbstract(this AbstractCreature self, out PupAbstract pupAbstract)
        {
            if (self != null)
            {
                pupAbstract = self.GetPupAbstract();
            }
            else pupAbstract = null;

            return pupAbstract != null;
        }


        public class ParentVariables
        {
            public bool rotundPupExhaustion;
        }
        public class PupVariables
        {
            public bool regurgitating;
            public bool swallowing;
            public bool wantsToRegurgitate;
            public bool wantsToSwallowObject;

            public AbstractPhysicalObject giftedItem;

            public SlugpupDebugger.DebugLabelManager labelManager;
            public SlugpupDebugger.PathingVisualizer pathingVisualizer;
        }
        public class PupGraphics
        {
            public int TongueSpriteIndex;
        }

        public class PupNPCState // DONT CHANGE THIS FFS, BEASTMASTERPUPEXTRAS RELIES ON IT!!!!!
        {
            public SlugcatStats.Name Variant;
            public AbstractPhysicalObject PupsPlusStomachObject;
        }

        public class PupAbstract
        {
            public bool moth;
        }
    }

}