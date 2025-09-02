namespace Fly8Cents.Services;

public static class BiliService
{
    public static UserInfo GetUserInfo(string uid)
    {
        return new UserInfo("测试", "测试");
    }

    public struct UserInfo(string name, string face)
    {
        public readonly string Name = name;
        public readonly string Face = face;
    }
}