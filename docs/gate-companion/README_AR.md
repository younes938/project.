# Gate Companion — دليل المرحلة التأسيسية

## ما الذي أُضيف؟
- مشروع Windows Worker مستقل في `backend/src/GateCompanion`.
- يعمل كخدمة Windows باسم `ETicket Gate Companion` عند نشره/تثبيته كخدمة.
- يرسل نبضة اتصال دورية إلى `POST /api/gate-companion/heartbeat`.
- يوفّر Backend نقطة `GET /api/gate-companion/status` لعرض آخر حالة مسجلة لكل بوابة.
- محول تجريبي يعرض كاميرا ANPR وميزانًا وقارئ RFID وحاجزًا آليًا بحالة `simulated`.

## التشغيل أثناء التطوير
1. شغّل SQL Server وBackend بالطريقة المعتادة.
2. من مجلد المشروع:
   `dotnet run --project backend/src/ETicket.Api/ETicket.Api.csproj`
3. في نافذة أخرى:
   `dotnet run --project backend/src/GateCompanion/GateCompanion.csproj`
4. افحص:
   `http://localhost:5009/api/gate-companion/status`

يمكن تعديل `ApiBaseUrl` و`GateId` و`IntervalSeconds` في `backend/src/GateCompanion/appsettings.json`.

## مهم قبل ربط الأجهزة الحقيقية
هذه المرحلة محاكاة فقط، ولا تتصل بكاميرا أو ميزان فعلي ولا تتحكم بالحاجز. بعد معرفة الشركة والموديل وطريقة الاتصال لكل جهاز، يُستبدل `SimulatedDeviceAdapter` بمحوّل حقيقي مناسب مثل SDK أو TCP/IP أو Serial Port. لا تحول `SimulationMode` إلى false قبل تنفيذ واختبار المحولات الحقيقية. كما أن بيانات الحالة الحالية محفوظة في ذاكرة Backend وتُفقد عند إعادة تشغيله؛ ليست سجلًا دائمًا في SQL بعد.

## الأمان والتشغيل الإنتاجي
قبل وضع الخدمة على شبكة معبر فعلية، أضف مصادقة متبادلة أو API key محفوظًا بأمان، TLS، صلاحيات محدودة، وتسجيلًا دائمًا للأحداث. لا تعرض نقاط Gate Companion للعامة دون حماية.
