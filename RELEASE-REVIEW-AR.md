# مراجعة النسخة قبل التسليم

## ما تمت مراجعته في هذه النسخة
- Angular route `/gate-devices` موجود ويعرض حالة الأجهزة وسجل آخر 100 قراءة ميزان.
- واجهة Gate Monitoring تعرض إحصائيات عدد القراءات المحمّلة وتصنيفات NORMAL وWARNING وOVERWEIGHT.
- `GateCompanion` يرسل heartbeat كل 15 ثانية، ويستخدم محاكاة آمنة للأجهزة.
- API يحفظ قراءات الميزان في SQL Server عبر `OperationsDbContext` ويعرض سجل القراءات للمستخدم المسجل.
- سكربت `003_create_weight_history.sql` موجود لإنشاء الجدول والفهارس.
- تم تصحيح النص العربي المتبقي في بطاقة إحصائيات لوحة التحكم إلى الإنجليزية.
- تم تحديث تعليمات إعداد SQL Server Express وتشغيل Gate Companion.

## حدود التحقق في بيئة إعداد الأرشيف
لم تتوفر أداة .NET SDK، كما أن مجلد `node_modules` المرفق لم يحتوِ Angular CLI التنفيذي؛ لذلك لم يكن ممكناً تشغيل `dotnet build` أو `npm run build` هنا. لا تعتبر هذه النسخة مختبرة بناءً حتى تمر أوامر التحقق أدناه على جهاز التطوير.

## تحقق محلي قبل الاعتماد
1. نفّذ ملفات SQL بالترتيب: `001_create_passengers.sql` ثم `002_create_operations.sql` ثم `003_create_weight_history.sql`.
2. من مجلد `backend`: `dotnet restore ETicket.slnx` ثم `dotnet build ETicket.slnx` ثم `dotnet test ETicket.slnx`.
3. من مجلد `frontend`: `npm ci` ثم `npm run check`.
4. شغّل API وAngular وGateCompanion في ثلاث نوافذ منفصلة.
5. افتح `/gate-devices` وتأكد من ظهور أربعة أجهزة، ثم تحقق من زيادة سجل الأوزان كل 15 ثانية.

## تنبيه تشغيلي وأمني
- Gate Companion في هذه النسخة محاكاة فقط؛ لا يوجد اتصال فعلي بالكاميرات أو الميزان أو RFID أو الحاجز.
- تسجيل الدخول التجريبي لا يصلح لحماية بيانات حقيقية. لا تعرض API على الإنترنت أو شبكة عامة بهذه الإعدادات.
- يجب قبل الإنتاج إضافة مصادقة موثوقة لوكيل Gate Companion، ومراجعة الصلاحيات، وHTTPS، وإدارة الأسرار والنسخ الاحتياطي واختبارات قبول فعلية.
- أرقام NORMAL وWARNING وOVERWEIGHT في لوحة Gate Monitoring محسوبة من آخر 100 قراءة محمّلة، وليست إحصائيات تاريخية شاملة لكل السجلات.
