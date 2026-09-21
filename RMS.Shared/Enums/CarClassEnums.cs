namespace RMS.Shared.Enums;


/// <summary>
/// رده‌بندی وسیلهٔ نقلیه را بر پایهٔ شکل بدنه، کاربری یا فناوری پیشرانه مشخص می‌کند.
/// </summary>
public enum CarClassEnums : byte
{
    /// <summary>ردهٔ وسیلهٔ نقلیه مشخص یا ثبت نشده است.</summary>
    Unknown = 0,

    /// <summary>هاچ‌بک؛ خودرویی با صندوق یکپارچه و درِ عقب متصل به شیشه.</summary>
    Hatchback = 1,

    /// <summary>سدان؛ خودروی سواری با صندوق بار جدا از اتاق.</summary>
    Sedan = 2,

    /// <summary>لیفت‌بک؛ خودرویی با ظاهر سدان و درِ عقب متصل به شیشه.</summary>
    Liftback = 3,

    /// <summary>فست‌بک؛ خودرویی با خط سقف پیوسته و شیب‌دار تا انتهای بدنه.</summary>
    Fastback = 4,

    /// <summary>کوپه؛ خودروی سواری معمولاً دودر با فرم بدنهٔ اسپرت.</summary>
    Coupe = 5,

    /// <summary>خودروی روباز با سقف ثابت‌نبودن یا جمع‌شونده.</summary>
    Convertible = 6,

    /// <summary>استیشن واگن؛ خودروی سواری با فضای بار کشیده و متصل به اتاق.</summary>
    StationWagon = 7,

    /// <summary>لیموزین؛ خودروی کشیده برای جابه‌جایی تشریفاتی مسافر.</summary>
    Limousine = 8,

    /// <summary>مینی‌ون؛ خودروی چندمنظوره با ظرفیت بیشتر برای حمل سرنشین.</summary>
    Minivan = 9,

    /// <summary>شاسی‌بلند (SUV) با ارتفاع و فاصلهٔ بیشتر از سطح زمین.</summary>
    SsuV = 10,

    /// <summary>کراس‌اوور؛ ترکیبی از ویژگی‌های خودروی سواری و شاسی‌بلند.</summary>
    Crossover = 11,

    /// <summary>آفرود — خودروی مخصوص مسیرهای صعب‌العبور خارج از جاده</summary>
    OffRoad = 12,

    /// <summary>وانت با محفظهٔ بار باز برای حمل بار.</summary>
    Pickup = 13,

    /// <summary>ون برای حمل بار یا مسافر.</summary>
    Van = 14,

    /// <summary>کامیون — خودروی سنگین برای حمل بارهای حجیم</summary>
    Truck = 15,

    /// <summary>کشنده یا کامیون نیمه‌تریلر برای حمل بار سنگین.</summary>
    SemiTrailer = 16,

    /// <summary>اتوبوس برای حمل‌ونقل عمومی مسافر.</summary>
    Bus = 17,

    /// <summary>مینی‌بوس — خودروی کوچک برای حمل‌ونقل عمومی مسافر</summary>
    Minibus = 18,

    /// <summary>اتوبوس دوطبقه با ظرفیت حمل مسافر بیشتر.</summary>
    DoubleDeckerBus = 19,

    /// <summary>ترولی یا وسیلهٔ نقلیهٔ عمومی متصل به شبکهٔ برق مسیر.</summary>
    Trolley = 20,

    /// <summary>تاکسی شهری — برای جابه‌جایی عمومی درون‌شهری</summary>
    Taxi = 21,

    /// <summary>آمبولانس — خودروی تجهیزات پزشکی برای اورژانس</summary>
    Ambulance = 22,

    /// <summary>خودروی پلیس یا نیروهای انتظامی.</summary>
    Police = 23,

    /// <summary>خودروی آتش‌نشانی و امداد اطفای حریق.</summary>
    FireTruck = 24,

    /// <summary>خودروی اسپرت — برای شتاب و عملکرد بالا</summary>
    SportsCar = 25,

    /// <summary>سوپراسپرت با عملکرد و توان حرکتی بسیار بالا.</summary>
    Supercar = 26,

    /// <summary>هایپرکار؛ رده‌ای فراتر از سوپراسپرت از نظر عملکرد و فناوری.</summary>
    Hypercar = 27,

    /// <summary>خودروی لوکس — با تجربه‌ای پر از امکانات رفاهی</summary>
    Luxury = 28,

    /// <summary>خودروی الکتریکی — با موتور برقی (EV)</summary>
    Electric = 29,

    /// <summary>خودروی هیبریدی با ترکیب موتور درون‌سوز و موتور برقی.</summary>
    Hybrid = 30,

    /// <summary>خودروی هیبریدی شارژی با امکان شارژ باتری از شبکهٔ برق.</summary>
    PluginHybrid = 31,

    /// <summary>خودروی پیل سوختی — با سلول‌های هیدروژنی</summary>
    FuelCell = 32,

    /// <summary>خودروی نظامی — تخصصی برای کاربردهای نظامی</summary>
    Military = 33,

    /// <summary>خودروی آبی-خاکی — قابلیت شناوری و حرکت روی آب</summary>
    Amphibious = 34,

    /// <summary>خودروی سفارشی یا تغییریافته با مشخصات غیرکارخانه‌ای.</summary>
    Custom = 35,

    /// <summary>خودروی جمع‌وجور شهری — ساختار بسیار کوچک و پرشتاب</summary>
    MicroCar = 36,

    /// <summary>موتورسیکلت یا وسیلهٔ نقلیهٔ موتوری دوچرخ.</summary>
    Motorcycle = 37,

    /// <summary>تراکتور — خودروی کشاورزی / صنعتی سنگین</summary>
    Tractor = 38,

    /// <summary>گاری یا وسیلهٔ نقلیهٔ بدون موتور برای حمل بار.</summary>
    Cart = 39
}

