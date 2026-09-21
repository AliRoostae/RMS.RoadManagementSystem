namespace RMS.Domain.Interfaces;

/// <summary>هش و بررسی امن رمز عبور را بدون وابسته‌کردن لایهٔ کاربرد به الگوریتم مشخص تعریف می‌کند.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
