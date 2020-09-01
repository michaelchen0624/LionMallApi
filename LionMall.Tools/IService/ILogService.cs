namespace LionMall.Tools
{
  public  interface ILogService
    {
        bool E(string catLog, string msg);

        bool D(string catLog, string msg);

        bool I(string catLog, string msg);
    }
}
