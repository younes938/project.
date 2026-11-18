
import { DOCUMENT } from '@angular/common';
import { computed, inject, Injectable, signal } from '@angular/core';

export type AppLanguage = 'en' | 'ar';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly document = inject(DOCUMENT);
  private readonly storageKey = 'eticket-language';

  readonly language = signal<AppLanguage>(this.readLanguage());
  readonly isArabic = computed(() => this.language() === 'ar');

  private readonly translations: Record<string, string> = {
    'Main Menu': 'القائمة الرئيسية',
    'Dashboard': 'لوحة التحكم',
    'Vehicle Entry': 'تسجيل المركبات',
    'Vehicle Visits': 'سجل المركبات',
    'Crossing Settings': 'إعدادات المعبر',
    'Passenger Management': 'إدارة المسافرين',
    'Shipment Management': 'إدارة الشحنات',
    'Security Inspections': 'التفتيش الأمني',
    'Reports': 'التقارير',
    'Audit Log': 'سجل التدقيق',
    'Gate Companion Devices': 'أجهزة البوابة',
    'API Services Enabled': 'خدمات API مفعّلة',
    'Border Control Management System': 'نظام إدارة مراقبة الحدود',
    'Border Control': 'مراقبة الحدود',
    'Electronic System': 'النظام الإلكتروني',
    'Sign Out': 'تسجيل الخروج',
    'Welcome Back': 'مرحبًا بعودتك',
    'Active Work Location': 'موقع العمل النشط',
    'Change Location': 'تغيير الموقع',
    'Crossing Overview': 'نظرة عامة على المعبر',
    'Refresh Statistics': 'تحديث الإحصائيات',
    'Passengers': 'المسافرون',
    'Vehicles': 'المركبات',
    'Shipments': 'الشحنات',
    'Security Alerts': 'التنبيهات الأمنية',
    'Quick Access': 'الوصول السريع',
    'Currently Available Services': 'الخدمات المتاحة حاليًا',
    'Register Passenger': 'تسجيل مسافر',
    'Register Vehicle': 'تسجيل مركبة',
    'System Status': 'حالة النظام',
    'Angular Frontend': 'واجهة Angular',
    'ASP.NET Core Services': 'خدمات ASP.NET Core',
    'SQL Server Database': 'قاعدة بيانات SQL Server',
    'Active': 'نشط',
    'Connected': 'متصل',
    'Allowed': 'مسموح',
    'Denied': 'مرفوض',
    'Pending': 'قيد الانتظار',
    'Loading...': 'جارٍ التحميل...',
    'Loading available border crossings…': 'جارٍ تحميل المعابر الحدودية المتاحة…',
    'Save': 'حفظ',
    'Cancel': 'إلغاء',
    'Search': 'بحث',
    'Refresh': 'تحديث',
    'Submit': 'إرسال',
    'Back': 'رجوع',
    'No records found': 'لم يتم العثور على سجلات',
    'No records found.': 'لم يتم العثور على سجلات.',
    'Try again': 'حاول مرة أخرى',
    'Save Details': 'حفظ التفاصيل',
    'Register New Passenger': 'تسجيل مسافر جديد',
    'Edit Passenger Details': 'تعديل بيانات المسافر',
    'Travel Document Details': 'بيانات وثيقة السفر',
    'Passenger Details': 'بيانات المسافر',
    'First Name': 'الاسم الأول',
    'Last Name': 'اسم العائلة',
    'Passport Number': 'رقم جواز السفر',
    'Nationality': 'الجنسية',
    'Date of Birth': 'تاريخ الميلاد',
    'Status': 'الحالة',
    'Refresh Records': 'تحديث السجلات',
    'Search vehicle records': 'البحث في سجلات المركبات',
    'Gate Monitoring': 'مراقبة البوابة',
    'Gate Companion': 'مرافق البوابة',
    'Weight History': 'سجل الأوزان',
    'Gate ID': 'معرّف البوابة',
    'Device ID': 'معرّف الجهاز',
    'Weight (KG)': 'الوزن (كغ)',
    'Weight Status': 'حالة الوزن',
    'Date & Time': 'التاريخ والوقت',
    'NORMAL': 'طبيعي',
    'WARNING': 'تحذير',
    'OVERWEIGHT': 'وزن زائد',
    'ONLINE': 'متصل',
    'OFFLINE': 'غير متصل',
    'Target Type': 'نوع الهدف',
    'Inspection Status': 'حالة التفتيش',
    'Customs Status': 'الحالة الجمركية',
    'Shipment Number': 'رقم الشحنة',
    'Goods Type': 'نوع البضائع',
    'Weight KG': 'الوزن بالكيلوغرام',
    'Origin Country': 'بلد المنشأ',
    'Destination Country': 'بلد الوجهة',
    'Notes': 'ملاحظات',
    'Approved': 'موافق عليه',
    'Under Inspection': 'قيد التفتيش',
    'Clear': 'سليم',
    'Suspicious': 'مشتبه به',
    'Requires Review': 'يتطلب المراجعة',
    'Passenger': 'مسافر',
    'Vehicle': 'مركبة',
    'Shipment': 'شحنة',
    'Sign in': 'تسجيل الدخول',
    'Sign in with Demo Account': 'الدخول بحساب تجريبي',
    'Creating session…': 'جارٍ إنشاء الجلسة…',
    'Welcome': 'مرحبًا',
    'Access Denied': 'تم رفض الوصول',
    'Confirm Operating Mode': 'تأكيد وضع التشغيل',
    'Saving…': 'جارٍ الحفظ…',
    'Current Mode:': 'الوضع الحالي:',
    'Modes:': 'الأوضاع:',
    'Try Again': 'حاول مجددًا',
    'Search crossing': 'البحث عن معبر'
  };

  private readonly textState = new WeakMap<Text, {
    original: string;
    rendered: string;
  }>();

  private observer?: MutationObserver;
  private translating = false;

  constructor() {
    this.apply(this.language());
    this.translateDocument();

    this.observer = new MutationObserver(() => {
      if (!this.translating) {
        this.translateDocument();
      }
    });

    this.observer.observe(this.document.body, {
      childList: true,
      subtree: true,
      characterData: true
    });
  }

  toggle(): void {
    this.setLanguage(this.isArabic() ? 'en' : 'ar');
  }

  setLanguage(language: AppLanguage): void {
    this.language.set(language);

    try {
      this.document.defaultView?.localStorage.setItem(
        this.storageKey,
        language
      );
    } catch {
      // Storage may be unavailable.
    }

    this.apply(language);
    this.translateDocument();
  }

  private translateDocument(): void {
    if (!this.document.body) return;

    this.translating = true;

    const walker = this.document.createTreeWalker(
      this.document.body,
      4
    );

    let node: Node | null;

    while ((node = walker.nextNode())) {
      const textNode = node as Text;
      const current = textNode.nodeValue ?? '';
      const state = this.textState.get(textNode);

      const original =
        state && current === state.rendered
          ? state.original
          : current;

      const trimmed = original.trim();

      if (!trimmed) continue;

      const translated = this.isArabic()
        ? (this.translations[trimmed] ?? trimmed)
        : trimmed;

      const nextValue = original.replace(trimmed, translated);

      this.textState.set(textNode, {
        original,
        rendered: nextValue
      });

      if (current !== nextValue) {
        textNode.nodeValue = nextValue;
      }
    }

    this.translating = false;
  }

  private readLanguage(): AppLanguage {
    try {
      return this.document.defaultView?.localStorage.getItem(
        this.storageKey
      ) === 'ar'
        ? 'ar'
        : 'en';
    } catch {
      return 'en';
    }
  }

  private apply(language: AppLanguage): void {
    this.document.documentElement.lang = language;
    this.document.documentElement.dir =
      language === 'ar' ? 'rtl' : 'ltr';
  }
}
