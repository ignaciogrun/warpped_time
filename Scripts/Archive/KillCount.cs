public static class KillCount
{
    #region  Variables
    private static float killCount = 0.0f;
    #endregion

    #region Functions
    //Get Current Kill Count
    public static float GetKillCount()
    {
        return killCount;
    }

    //Set Kill Count to #
    public static void SetKillCount(float setValue)
    {
        killCount = setValue;
    }

    //Add # to Kill Count
    public static void AddToKillCount(float addValue)
    {
        killCount = killCount + addValue;
    }

    //Subtract # from Kill Count
    public static void SubtractFromKillCount(float subtractValue)
    {
        killCount = killCount - subtractValue;
    }

    //Reset Kill Count back to zero
    public static void ResetKillCount()
    {
        killCount = 0;
    }
    #endregion
}
