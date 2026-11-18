# تشغيل Gate Companion وواجهة الأجهزة

## ما تمت إضافته
- صفحة Angular باسم Gate Companion Devices على المسار `/gate-devices`.
- رابط للصفحة في القائمة الجانبية والاختصارات في لوحة التحكم.
- خدمة Windows مبنية على .NET باسم GateCompanion لمحاكاة الأجهزة وإرسال heartbeat إلى API.
- نقاط API لحالة الأجهزة واستقبال heartbeat.

## تشغيل الواجهة محلياً
1. افتح PowerShell داخل مجلد `frontend`.
2. نفّذ `npm ci`.
3. نفّذ `npm start`.
4. افتح `http://localhost:4200` وسجّل الدخول.
5. من القائمة الجانبية اختر `Gate Companion Devices`.

## تشغيل Backend والمرافق
- شغّل Backend بالطريقة المعتادة في المشروع أولاً.
- شغّل خدمة المرافق من مجلد المشروع:
  `dotnet run --project backend/src/GateCompanion/GateCompanion.csproj`
- راجع `docs/gate-companion/README_AR.md` لإعدادات الاتصال.

## تنبيه
الأجهزة الحالية في وضع Simulation فقط. لا توجد كاميرا أو ميزان أو قارئ RFID حقيقي مرتبط قبل تحديد الشركة المصنّعة والطراز وبروتوكول الاتصال. كما أن بناء Angular لم يُتحقق منه في بيئة التجهيز بسبب انتهاء مهلة تنزيل حزم npm؛ لذلك يجب تنفيذ `npm ci` ثم `npm run build` محلياً قبل العرض النهائي.


## سجل الأوزان (Weight History)
قبل تشغيل حفظ القراءات لأول مرة، نفّذ الملف `backend/database/003_create_weight_history.sql` على قاعدة البيانات `ETicketPassengers`. خدمة GateCompanion الحالية تستخدم محاكاة للأجهزة؛ قراءات الميزان التجريبية تتغير بين 8,000 و28,000 كغ كل 15 ثانية، ولا تمثل قياسات حقيقية.
