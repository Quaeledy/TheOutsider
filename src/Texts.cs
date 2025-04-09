using RWCustom;

namespace TheOutsider
{
    public class Texts : UpdatableAndDeletable
    {

        public Texts(Room room, Message[] list)
        {
            this.room = room;
            if (room.game.session.Players[0].pos.room != room.abstractRoom.index)
                Destroy();
            messageList = list;
        }


        public override void Update(bool eu)
        {
            base.Update(eu);

            if (room.game.session.characterStats.name != Plugin.SlugName)
            {
                return;
            }

            if (room.game.session.Players[0].realizedCreature != null && room.game.cameras[0].hud != null &&
                room.game.cameras[0].hud.textPrompt != null && room.game.cameras[0].hud.textPrompt.messages.Count < 1)
            {
                if (index < messageList.Length)
                {
                    var texts = messageList[index].text.Split('/');
                    string transTest = "";
                    foreach (var text in texts)
                        transTest += Custom.rainWorld.inGameTranslator.Translate(text);

                    room.game.cameras[0].hud.textPrompt.AddMessage(transTest, messageList[index].wait, messageList[index].time, false, ModManager.MMF);
                    index++;
                }
                else
                    slatedForDeletetion = true;
            }
        }

        public class Message
        {
            public string text;
            public int wait;
            public int time;
            Message(string s, int w, int t)
            {
                text = s;
                wait = w;
                time = t;
            }
            static public Message NewMessage(string s, int w, int t)
            {
                return new Message(s, w, t);
            }
        }
        int index = 0;
        readonly Message[] messageList;
    }

    class IntroText1 : Texts
    {
        public IntroText1(Room room) : base(room, new Message[]
            {
                Message.NewMessage("While in the air, tap jump to flutter.", 120, 180),
                //Message.NewMessage("[Please do not see echoes in the first rain cycle. This will prevent your game from being saved.]", 120, 180)
            })

        {
            if (room.game.session.Players[0].pos.room != room.abstractRoom.index)
            {
                Destroy();
            }
        }
    }

    class IntroText2 : Texts
    {
        public IntroText2(Room room) : base(room, new Message[]
            {
                Message.NewMessage("While stand on the ground, Tap jump and pick-up together to flare at the cost of consuming food.", 120, 180)
            })

        {
            if (room.game.session.Players[0].pos.room != room.abstractRoom.index)
            {
                Destroy();
            }
        }
    }

    class IntroText3 : Texts
    {
        public IntroText3(Room room) : base(room, new Message[]
            {
                Message.NewMessage("You have a wider range of vegetarian diet, including those Firebushes, Bubble Weeds, Batnips and so on.", 120, 180)
            })

        {
            if (room.game.session.Players[0].pos.room != room.abstractRoom.index)
            {
                Destroy();
            }
        }
    }
}
