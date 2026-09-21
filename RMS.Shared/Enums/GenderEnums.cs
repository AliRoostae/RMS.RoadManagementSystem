namespace RMS.Shared.Enums;

/// <summary>
/// جنسیت افراد برای ثبت اطلاعات (مانند راننده و سرنشین) در سوانح رانندگی.
/// نوع پایه <see cref="byte"/> برای کاهش حجم داده در حافظه و دیتابیس.
/// </summary>
public enum GenderEnums : byte
{
    /// <summary>نامشخص/درج نشده</summary>
    Unknown = 0,

    /// <summary>مرد</summary>
    Male = 1,

    /// <summary>زن</summary>
    Female = 2
}