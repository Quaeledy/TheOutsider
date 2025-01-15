using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MoreSlugcats;

namespace TheOutsider.CustomLore.CustomCreature
{
    public class MothPupState : PlayerNPCState
    {
        // Token: 0x060023C1 RID: 9153 RVA: 0x002CAA40 File Offset: 0x002C8C40
        public MothPupState(AbstractCreature abstractCreature, int playerNumber) : base(abstractCreature, playerNumber)
        {
            this.player = abstractCreature;
            this.slugcatCharacter = Plugin.MothPup;
        }
    }
}
