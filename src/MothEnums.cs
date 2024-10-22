namespace TheOutsider
{
    public static class MothEnums
    {
        public static void RegisterValues()
        {
            MothBuzz = new SoundID("mothbuzz", true);
            //MothFlower = new AbstractPhysicalObject.AbstractObjectType("MothFlower", true);
        }

        public static void UnregisterValues()
        {
            Unregister(MothBuzz);
        }

        private static void Unregister<T>(ExtEnum<T> extEnum) where T : ExtEnum<T>
        {
            if (extEnum != null)
            {
                extEnum.Unregister();
            }
        }

        public static SoundID MothBuzz;

        //public static AbstractPhysicalObject.AbstractObjectType MothFlower;
    }
}