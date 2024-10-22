using BepInEx.Logging;

namespace TheOutsider
{
    class HookBase
    {
        protected HookBase(ManualLogSource log)
        {
            _log = log;
        }
        protected ManualLogSource _log;

        virtual public void OnModsInit(RainWorld rainWorld)
        {

        }

    }
}
