# ابدأ من هنا — E-Ticket Border Control

هذه نسخة تطوير Full Stack باستخدام Angular + ASP.NET Core / C# + SQL Server. ليست نسخة إنتاجية معتمدة؛ تسجيل الدخول ما زال تجريبيًا ويجب إكمال المصادقة وإدارة المستخدمين قبل الاستخدام الحقيقي.

## المتطلبات
- Windows 10/11.
- .NET SDK 10.x.
- Node.js 22.12 أو أحدث وnpm.
- SQL Server أو SQL Server LocalDB، مع SSMS لتنفيذ ملفات SQL.

## 1) إعداد قاعدة البيانات
1. افتح SQL Server Management Studio واتصل بالخادم الذي ستستخدمه.
2. نفّذ بالترتيب:
   - `backend/database/001_create_passengers.sql`
   - `backend/database/002_create_operations.sql`
   - `backend/database/003_create_weight_history.sql`
3. إعداد الاتصال الافتراضي يستخدم SQL Server Express `.\SQLEXPRESS`؛ عدّل `ConnectionStrings:Passengers` في `backend/src/ETicket.Api/appsettings.json` ليتوافق مع خادمك.

الملف الأول ينشئ جداول المسافرين وزيارات المركبات، والثاني ينشئ الشحنات والتفتيش وسجل التدقيق، والثالث ينشئ سجل قراءات الميزان.

## 2) تشغيل الخادم الخلفي
افتح PowerShell داخل مجلد المشروع:

```powershell
cd backend
dotnet --info
dotnet restore ETicket.slnx
dotnet run --project src/ETicket.Api/ETicket.Api.csproj --launch-profile http
```

اختبر `http://localhost:5009/health` ويجب أن يرجع `{"status":"healthy"}`.

## 3) تشغيل Gate Companion
افتح نافذة PowerShell ثالثة داخل مجلد المشروع وشغّل:

```powershell
dotnet run --project backend/src/GateCompanion/GateCompanion.csproj
```

الخدمة الحالية تحاكي كاميرا ANPR والميزان وقارئ RFID والحاجز، وترسل heartbeat كل 15 ثانية. قراءات الميزان بين 8,000 و28,000 كغ تجريبية وليست قياسات فعلية. اترك النافذة مفتوحة أثناء الاختبار.

## 4) تشغيل الواجهة
افتح نافذة PowerShell ثانية:

```powershell
cd frontend
npm ci
npm start
```

افتح `http://localhost:4200`. اترك نافذتي الطرفية تعملان أثناء الاستخدام.

## الحساب التجريبي
في بيئة Development يظهر زر «الدخول بالحساب التجريبي». لا يوجد حاليًا تحقق فعلي من اسم المستخدم وكلمة المرور، ولذلك لا تستخدم هذه النسخة لحماية بيانات حقيقية أو على شبكة عامة.

## الوظائف الموجودة
- لوحة تحكم وإحصائيات من API.
- إدارة المسافرين (CRUD) وتخزين SQL Server.
- تسجيل المركبات وسجل زيارات المركبات.
- إدارة الشحنات (إضافة/عرض/بحث/تعديل/حذف) وتخزين SQL Server.
- التفتيش الأمني والإنذارات (إضافة/عرض/بحث/تعديل/حذف) وتخزين SQL Server.
- تقرير ملخص مع تصدير CSV.
- سجل عمليات تدقيق في SQL Server.
- مراقبة أجهزة البوابة وحفظ آخر 100 قراءة ميزان في `WeightHistory` مع تصنيف NORMAL / WARNING / OVERWEIGHT.
- إعداد موقع العمل، وجلسة Cookie وحماية CSRF في بيئة التطوير.

## ملاحظات قبل التسليم النهائي/الإنتاج
- يجب تنفيذ `npm ci` و`npm run build` واختبارات Angular، وكذلك `dotnet restore`, `dotnet build`, و`dotnet test` على جهاز يحتوي المتطلبات؛ لم يمكن تأكيد البناء في بيئة إنشاء الأرشيف بسبب تعذر الوصول إلى npm registry وعدم توفر .NET SDK هنا.
- المصادقة الحالية تجريبية. المطلوب قبل الإنتاج: حسابات حقيقية بكلمات مرور مجزأة، إدارة مستخدمين وأدوار وصلاحيات لكل وحدة، وتدقيق أمني مستقل.
- إعدادات المعبر وقائمة المعابر/البوابات ما زالت تعتمد على مخزن تجريبي في الذاكرة. يجب نقلها إلى SQL Server إذا كان المطلوب الاحتفاظ بها بعد إعادة تشغيل الخادم.
- الربط مع الأنظمة الحكومية، ANPR والكاميرات، والجمارك الفعلية غير مفعّل؛ توجد موائمات Mock فقط.
- النسخ الاحتياطي، HTTPS بشهادة صالحة، الأسرار خارج ملفات الإعداد، ومراجعة أمنية واختبارات قبول مطلوبة قبل التشغيل الفعلي.
