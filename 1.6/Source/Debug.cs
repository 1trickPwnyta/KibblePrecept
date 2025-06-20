namespace KibblePrecept
{
    public static class Debug
    {
        public static void Log(string message)
        {
#if DEBUG
            Verse.Log.Message($"[{KibblePreceptMod.PACKAGE_NAME}] {message}");
#endif
        }
    }
}
