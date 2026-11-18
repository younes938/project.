# إعداد قاعدة بيانات SQL Server

نفّذ الملفات من SQL Server Management Studio بالترتيب، مع اختيار قاعدة البيانات `ETicketPassengers` عند الحاجة:

1. `001_create_passengers.sql` — ينشئ قاعدة `ETicketPassengers` وجداول المسافرين وزيارات المركبات.
2. `002_create_operations.sql` — ينشئ جداول الشحنات والتفتيش وسجل التدقيق.
3. `003_create_weight_history.sql` — ينشئ جدول `WeightHistory` لحفظ قراءات الميزان وحالاتها.

إعداد الاتصال الافتراضي في `backend/src/ETicket.Api/appsettings.json` يستخدم SQL Server Express:
`Server=.\SQLEXPRESS;Database=ETicketPassengers;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True`

إذا كنت تستخدم خادماً مختلفاً، غيّر `ConnectionStrings:Passengers` إلى اسم الخادم الصحيح. لا تضع كلمات مرور أو أسراراً في ملفات يتم رفعها إلى Git.
