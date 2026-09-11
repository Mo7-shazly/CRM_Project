# تسليم مشروع CRM Desktop - مرجع للمتابعة باستخدام AI

## 1. ملخص تنفيذي

هذا المشروع تطبيق CRM مكتبي يعمل على Windows. الهدف منه إدارة دورة المبيعات من العميل المحتمل (Lead) مروراً بالعميل والفرص وعروض الأسعار حتى المشروع، مع Dashboard وتقارير وإدارة مستخدمين.

التطبيق الحالي هو MVP متقدم: أغلب الشاشات الأساسية موجودة وتعمل بحفظ فعلي في قاعدة البيانات، لكن بعض الروابط التجارية المهمة بين المراحل لم تُنفذ بعد. أهم قاعدة يجب الحفاظ عليها أثناء التطوير:

```text
Lead مستقل
  -> تأهيل ومتابعة
  -> تحويل صريح إلى Customer (مع منع التكرار)
  -> Opportunity
  -> Quotation / Won Deal
  -> Project
```

**لا يجب جعل Customer شرطاً لإنشاء Lead.** تم تعديل التطبيق بالفعل ليحفظ Lead مستقلاً، ولا ينشئ Opportunity تلقائياً عند إضافة Lead.

## 2. التقنية والمعمارية

| العنصر | الوضع الحالي |
|---|---|
| نوع التطبيق | WPF Desktop Application |
| اللغة والإطار | C# / .NET 8 (`net8.0-windows`) |
| نمط الواجهة | MVVM باستخدام `CommunityToolkit.Mvvm` |
| ORM | Entity Framework Core 8 |
| قاعدة البيانات الافتراضية | SQLite: ملف `crm.db` بجوار التطبيق التنفيذي |
| قاعدة بيانات بديلة | SQL Server عند ضبط متغير البيئة `CRM_SQLSERVER_CONNECTION` |
| الحزم المهمة | `CommunityToolkit.Mvvm`, `MaterialDesignThemes`, EF Core SQLite وSQL Server |
| بناء المشروع | `dotnet build CRM.sln --no-restore` |

### بنية المجلدات

```text
CRM.sln
CRM.Desktop/
  Data/          DbContext, الترقية غير المدمرة للـ schema, وseed data
  Models/        الكيانات والـ enums
  Services/      قواعد العمل والوصول للبيانات
  ViewModels/    حالة كل شاشة والأوامر Commands
  Views/         واجهات XAML لكل شاشة
  MainWindow.xaml    التنقل والقوالب التي تربط ViewModel بالـ View
  App.xaml           التنسيقات العامة للواجهة
```

### قواعد مهمة قبل التعديل

1. لا تُدخل منطق أعمال داخل XAML؛ الواجهة للـ bindings والتنسيق فقط.
2. أي عملية على البيانات تكون في `Services`، وحالة الشاشة وأوامرها تكون في `ViewModels`.
3. عند تعديل نموذج بيانات، حدّث `Entities.cs` و`CrmDbContext.cs` و`DbSchemaUpgrader.cs` معاً، خصوصاً لدعم SQLite الموجود لدى المستخدمين.
4. لا تستخدم حذف قاعدة البيانات كحل للـ schema؛ يوجد `DbSchemaUpgrader` مخصص للترقيات غير المدمرة.
5. قبل التسليم شغّل build. المشروع كان يبني بنجاح بدون أخطاء أو تحذيرات بعد آخر تعديل.

## 3. قاعدة البيانات والكيانات الحالية

### AppUser

حقول: `Id`, `DisplayName`, `Email` (فريد), `PasswordHash`, `Role`, `IsActive`.

- الأدوار المتاحة: `Admin`, `Manager`, `Sales`.
- كلمة المرور تخزن كـ SHA-256 hash عبر `DbSeeder.Hash`.
- حسابات seed الافتراضية تستخدم كلمة المرور `123456`:
  - `admin@crm.local` - Admin
  - `manager@crm.local` - Manager
  - `sales@crm.local` - Sales

> هذا مناسب للعرض والتطوير فقط. للإنتاج يلزم استبدال SHA-256 المباشر بـ PBKDF2 أو BCrypt/Argon2، وإزالة كلمات المرور الافتراضية.

### Lead

حقول موجودة: `LeadCode`, `CustomerId?`, `OpportunityId?`, `Company`, `ContactName`, `Phone`, `WhatsApp`, `Email`, `Industry`, `Source`, `AssignedTo`, `Status`, `Score`, `EstimatedValue`, `Notes`, `CreatedAt`, `LastContactAt`, `NextFollowUpAt`.

- حالات الـ Lead الحالية: `New`, `Contacted`, `Qualified`, `ProposalSent`, `Negotiation`, `Won`, `Lost`.
- `CustomerId` و`OpportunityId` اختياريان على مستوى قاعدة البيانات.
- كود Lead يتم توليده بشكل `L-001`.
- مصادر الـ Lead seeded: Website, LinkedIn, Facebook, Referral, Instagram, Walk-in.

### Customer وContactPerson وCustomerActivity

- Customer: كود، اسم الشركة، الصناعة، حجم الشركة، الموقع، الهاتف، البريد، العنوان، الحالة، الأولوية، تاريخ الإنشاء.
- ContactPerson: الاسم، المسمى الوظيفي، الهاتف، البريد، هل هو جهة الاتصال الرئيسية.
- CustomerActivity: العميل، النوع، العنوان، التفاصيل، المسؤول، وقت الحدث، موعد التذكير، حالة الإكمال.
- حذف Customer يحذف جهات الاتصال والأنشطة المرتبطة به Cascade، لكنه سيفشل/يُمنع إذا كان هناك Quotation أو Project مرتبط به بسبب `Restrict`.

### Opportunity

حقول: `CustomerId?`, الاسم، المالك، المرحلة، القيمة، تاريخ الإنشاء، موعد الإغلاق المتوقع.

مراحل الـ pipeline: `NewLead`, `Qualified`, `Proposal`, `Negotiation`, `Won`, `Lost`.

### Quotation وQuotationItem

- Quotation مرتبط بـ Customer إلزامياً.
- حقول العرض: `QuoteNumber`, `OwnerName`, `Status`, `IssueDate`, `ValidUntil`, `Notes`.
- البنود: الوصف، الكمية، سعر الوحدة. الإجمالي محسوب من البنود ولا يُخزن كعمود مستقل.
- الحالات: `Draft`, `Sent`, `Accepted`, `Rejected`.
- كود العرض: `Q-001`.

### CrmProject

حقول: `ProjectCode`, `CustomerId` إلزامي، الاسم، المسؤول، الحالة، نسبة الإنجاز، تاريخ البداية والنهاية، الملاحظات.

الحالات: `Planned`, `InProgress`, `OnHold`, `Completed`, `Cancelled`. كود المشروع: `P-001`.

## 4. ما تم إنجازه فعلياً

### الأساس والبيانات

- تطبيق WPF يعمل وتتم تهيئة قاعدة البيانات عند بدء التطبيق.
- `DbSchemaUpgrader` ينشئ الجداول ويضيف أعمدة مفقودة لقاعدة SQLite القديمة دون مسح البيانات.
- `DbSeeder` يضيف بيانات عرض أولية عند كون الجداول فارغة.
- تسجيل دخول، جلسة مستخدم، تسجيل خروج، وتنقل بين الشاشات.
- بحث عام بعد إدخال حرفين أو أكثر في Customers وLeads وOpportunities.

### الصلاحيات المطبقة حالياً في الكود

- **Sales** يرى Leads وActivities وOpportunities وQuotations وProjects المسندة/المملوكة له فقط في معظم الخدمات.
- **Sales** عند الحفظ يُفرض عليه أن يكون `AssignedTo` أو `OwnerName` باسمه في Leads وActivities وOpportunities وProjects.
- **Sales** لا يمكنه تعديل أو حذف Lead أو Opportunity أو Activity أو Quotation أو Project ليست ملكه/مسندة له.
- **Admin وManager** يمكنهما اختيار المسؤول في شاشات Leads وActivities وOpportunities وProjects.
- **Admin فقط** يدير المستخدمين من Settings. يوجد منع لقيام الـ Admin بإزالة صلاحية Admin عن نفسه أو تعطيل نفسه.

### تعديل Lead الذي تم مؤخراً

كان التصميم القديم يجبر المستخدم على اختيار Customer عند إنشاء Lead، ويقوم كذلك بإنشاء Opportunity تلقائياً. تم تغيير ذلك كما يلي:

- شاشة Lead أصبحت تطلب `Company` و`ContactName` فقط كحد أدنى.
- تم حذف اختيار Customer الإلزامي من نافذة Lead.
- `LeadService.Save` لم يعد يتحقق من Customer إلزامي.
- لم يعد إنشاء Lead جديد ينشئ Opportunity تلقائياً.
- بيانات Lead القديمة المرتبطة بـ Customer لن تتلف؛ العلاقة الاختيارية ما زالت موجودة في الموديل، لكن لا توجد واجهة حالية لإدارتها.

### تحديث التصميم

- تم إضافة styles عامة في `App.xaml` للأزرار، TextBox، ComboBox، DatePicker، DataGrid وProgressBar.
- تصميم الجداول أصبح برؤوس وصفوف متبادلة وحدود هادئة.
- شاشة Projects تعرض Progress Bar حقيقياً في العمود بدلاً من نسبة نصية فقط.
- التعديل كان XAML فقط ولم يُغير أي Service أو Command أو منطق أعمال.

## 5. شرح الشاشات الحالية بالتفصيل

### A. تسجيل الدخول والتنقل العام

**الملفات:** `MainWindow.xaml`, `MainWindow.xaml.cs`, `MainViewModel.cs`, `AuthService.cs`.

- نافذة الدخول تعرض Email وPassword وزر Sign in.
- القيم الافتراضية في ViewModel: `admin@crm.local` و`123456` لتسهيل العرض.
- عند نجاح تسجيل الدخول يتم تحميل Dashboard وLeads وSettings، ثم يفتح Dashboard.
- القائمة الجانبية تفتح: Dashboard, Leads, Customers, Activities, Opportunities, Quotations, Projects, Reports, Settings.
- شريط أعلى الواجهة يحتوي البحث العام واسم المستخدم الحالي وزر Sign out.
- البحث العام يعرض حتى 12 نتيجة إجمالاً: Customers وLeads وOpportunities. لا ينتقل المستخدم حالياً تلقائياً عند الضغط على نتيجة؛ هو عرض نتائج فقط.

### B. Dashboard

**الملفات:** `DashboardView.xaml`, `DashboardViewModel.cs`, `DashboardService.cs`.

يعرض snapshot حسب الفترة: Today، This Week، This Month، This Year. لا يوجد Custom Date في Dashboard حالياً؛ Custom range موجود في Reports.

المعروض حالياً:

- KPI: عدد Leads الجديدة، العملاء الجدد، الفرص المفتوحة، قيمة Pipeline، الإيراد الرابح، ونسبة الفوز.
- Pipeline حسب المرحلة وعدد وقيمة الفرص.
- Top sales performance حسب قيمة الفرص الرابحة/المبيعات.
- مخطط/أعمدة قيمة المبيعات الرابحة لآخر 6 شهور.
- Follow-ups المستحقة: Activities غير المكتملة ذات Reminder وLeads ذات NextFollowUpAt المستحق.
- Recent activities.

Sales يرى بياناته فقط في Leads وOpportunities وActivities. نقطة تحتاج مراجعة لاحقاً: عداد Customers في Dashboard غير مفلتر لـ Sales حالياً.

### C. Leads

**الملفات:** `LeadsView.xaml`, `LeadsViewModel.cs`, `LeadEditViewModel.cs`, `LeadService.cs`.

- جدول يعرض: Lead ID، Company، Contact، Phone، Source، Status، Assigned to، Actions.
- فلاتر حالية: search، status، assignee. يوجد كود يدعم source filter في service/viewmodel لكن واجهة الفلتر لا تعرض source حالياً.
- أوامر: New Lead، Edit Lead، Delete Lead، Refresh/filters.
- نموذج الإضافة/التعديل الحالي يعرض Company, Contact Person, Phone, Email, Source, Status, Assigned Sales, Score, Notes.
- حقول Lead الموجودة في الموديل لكن غير معروضة بالكامل في الواجهة: WhatsApp, Industry, EstimatedValue, LastContactAt, NextFollowUpAt.
- إنشاء Lead يولد كود Lead وتاريخ إنشاء. لا يربطه بعميل ولا ينشئ Opportunity تلقائياً.
- Sales لا يغير AssignedTo ولا يعدل/يحذف Lead ليس مسنداً له.

**مهم جداً - المطلوب التالي:** تنفيذ زر/Workflow `Convert to Customer`. يجب أن:

1. يظهر فقط للـ Lead المؤهل أو يكون تأكيداً صريحاً.
2. يبحث عن Customer مطابق بالهاتف أو البريد أو اسم الشركة لتجنب التكرار.
3. ينشئ Customer عند عدم وجود تطابق، وينسخ بيانات الشركة الأساسية.
4. ينشئ ContactPerson من ContactName/Phone/Email كـ primary contact.
5. يضبط `Lead.CustomerId` إلى العميل الناتج.
6. لا ينشئ Opportunity إلا باختيار المستخدم أو ضمن خطوة منفصلة واضحة.
7. يضيف Activity/Timeline تصف عملية التحويل إن تم تنفيذ timeline موحد لاحقاً.

### D. Customers

**الملفات:** `CustomersView.xaml`, `CustomersViewModel.cs`, `CustomerFormViewModel.cs`, `CustomerService.cs`.

- قائمة تعرض Customer code, company, industry, phone, priority, status.
- بحث بالاسم أو الهاتف أو البريد.
- أوامر: Add Customer، Edit، Delete، Open Details.
- نموذج Customer: CompanyName إلزامي؛ Industry, Phone, Email, Website, CompanySize, Priority, Status, Address.
- Open Details يفتح modal يحتوي تبويبات:
  - **Overview:** Website, Phone, Email, Address.
  - **Contacts:** عرض contacts وإضافة contact جديد.
  - **Activities:** عرض سجل Activities للعميل.
  - **Opportunities / Quotations / Projects / Files:** حالياً placeholders فقط وليست مربوطة ببيانات حقيقية داخل تفاصيل العميل.

**نواقص مهمة:** لا توجد شاشة Customer Timeline موحدة تجمع calls/emails/meetings/quotations/opportunities/deals/projects/files، ولا وسائل لحذف أو تعديل ContactPerson من الواجهة.

### E. Activities

**الملفات:** `ActivitiesView.xaml`, `ActivitiesViewModel.cs`, `ActivityFormViewModel.cs`, `ActivityService.cs`.

- Activities مرتبطة بـ Customer إلزامياً في النموذج الحالي.
- جدول يعرض Customer, Type, Activity title, Assigned to, Reminder, Status, Actions.
- الفلاتر: type وstatus (Open/Completed).
- نموذج Log Activity يدعم: customer, type, title, details, assigned sales, reminder date/time.
- أوامر: إضافة، تعديل، Mark Complete.
- عند الإضافة يتم ضبط `OccurredAt` على الوقت الحالي. Sales يُسند النشاط لنفسه.

**نواقص:** لا توجد حقول رسمية منفصلة لكل نوع (مدة المكالمة، حضور الاجتماع، مكان الاجتماع، subject/content email)، ولا reminders على مستوى Windows أو scheduler، ولا Activities مرتبطة بـ Lead قبل التحويل إلى Customer.

### F. Opportunities Pipeline

**الملفات:** `OpportunitiesView.xaml`, `OpportunitiesViewModel.cs`, `OpportunityFormViewModel.cs`, `OpportunityService.cs`.

- Kanban board بأعمدة مراحل الـ pipeline الست.
- كل بطاقة تعرض الاسم، customer إن وجد، القيمة، owner.
- أوامر: New Opportunity، Edit، نقل للمرحلة السابقة/التالية بالسهمين.
- النموذج: Name، Customer (اختياري حالياً)، Value، Stage، Owner، Expected close date.
- Sales يرى ويعدل فرصه فقط.

**نواقص:** لا يوجد ربط صريح بـ Lead، ولا validation قوي لاسم الفرصة/القيمة، ولا سجل لتغييرات المرحلة، ولا سبب Lost، ولا automation عند Won (مثل اقتراح Project أو تحويل Quote Accepted).

### G. Quotations

**الملفات:** `QuotationsView.xaml`, `QuotationsViewModel.cs`, `QuotationFormViewModel.cs`, `QuotationService.cs`.

- قائمة تعرض Quote #، Customer، Issue date، Status، Total، Edit وPDF.
- إنشاء/تعديل عرض باختيار Customer إلزامي والحالة وتاريخ الصلاحية.
- بنود العرض قابلة للإضافة والحذف، وبها Description, Quantity, Unit Price.
- Total هو مجموع البنود.
- التصدير الحالي PDF نصي بسيط بـ Helvetica باللغة الإنجليزية؛ ليس template تجاري منسق ولا يدعم العربية بشكل جيد.
- Sales يرى ويعدل عروضه فقط؛ عند الإنشاء OwnerName يكون المستخدم الحالي.

**نواقص:** لا يوجد ربط OpportunityId في quotation، ولا workflow approval/accepted -> opportunity won/project، ولا tax/discount/currency per quote، ولا إرسال بريد، ولا template PDF احترافي.

### H. Projects

**الملفات:** `ProjectsView.xaml`, `ProjectsViewModel.cs`, `ProjectFormViewModel.cs`, `ProjectService.cs`.

- جدول يعرض Project #، name، customer، owner، status، وProgress Bar.
- إنشاء/تعديل مشروع يحتاج Customer وName ونسبة Progress من 0 إلى 100.
- النموذج: customer، project name، assigned person، status، progress، end date، notes.
- Sales يرى/يعدل المشاريع المسندة له فقط.
- Admin وManager يمكنهما اختيار المسؤول؛ Sales يُسند لنفسه.

**نواقص:** لا توجد Tasks داخل المشروع، ولا project budget/actual cost، ولا ملفات، ولا علاقة بـ Opportunity أو Quotation أو Won Deal. المطلوب مستقبلاً إضافة `OpportunityId?` و/أو `QuotationId?` كمرجع مصدر اختياري، وليس `LeadId` كعلاقة أساسية.

### I. Reports

**الملفات:** `ReportsView.xaml`, `ReportsViewModel.cs`, `ReportService.cs`.

- اختيار From وTo ثم Apply.
- KPI: Leads، Customers، Pipeline value، Won value، Quotation value.
- جدول Sales performance: الاسم، عدد الفرص، عدد Won، القيمة.
- جدول Pipeline by stage: المرحلة، العدد، القيمة.
- Sales يرى Leads وOpportunities الخاصة به فقط؛ Customers وQuotations غير مفلترة لـ Sales حالياً، ويجب حسم سياسة الصلاحية المطلوبة.

**نواقص:** لا توجد تقارير Qualified Leads, Lost Deals, Average Deal Size, Meetings per Sales، Conversion Rate تفصيلي، تصدير Excel/PDF، ولا رسوم بيانية متقدمة.

### J. Settings

**الملفات:** `SettingsView.xaml`, `SettingsViewModel.cs`, `UserService.cs`.

- Company name وCurrency موجودان كـ properties في الواجهة، لكن Save حالياً يعرض رسالة `Settings saved for this session.` فقط ولا يحفظ إعدادات في قاعدة البيانات.
- Admin يرى User management: users grid، Refresh، New user، Edit، Save user.
- User management يدعم display name, email, role, new password, active/inactive.
- Manager وSales لا تظهر لهم إدارة المستخدمين.

## 6. الواجهات والتصميم

- التصميم الحالي مبني على خلفية فاتحة، cards وجداول بيضاء، primary blue `#1677C8`، sidebar كحلي `#102B4E`.
- تم وضع تنسيقات عامة في `App.xaml` للأزرار والحقول والجداول والـ ProgressBar.
- لا تغيّر bindings أو أسماء Commands عند إعادة تصميم الشاشة. التعديل البصري يكون في XAML فقط.
- يوجد مرجع تصميم من المستخدم يشبه CRM dashboard حديث. المطلوب مستقبلاً تطوير sidebar النشط، badges للحالة، cards، وkanban بشكل أجمل، مع الحفاظ على نفس Commands والـ bindings.

## 7. المطلوب المتبقي - مرتب حسب الأولوية

### P0 - ضروري لإكمال سير العمل الأساسي

1. **Convert Lead to Customer** حسب الخطوات المذكورة في قسم Leads.
2. **ربط Customer details فعلياً** بقوائم Opportunities وQuotations وProjects بدل placeholders.
3. **نظام Timeline موحد للعميل** يجمع Activities, quotation events, opportunity stage changes, project events في ترتيب زمني.
4. **مراجعة صلاحيات Sales** في Customers وReports وDashboard؛ حالياً بعض aggregations لا تُفلتر بالكامل.
5. **تحسين validation وقواعد الحذف**: تأكيد قبل الحذف، رسائل أفضل، ومنع حذف عميل عند وجود علاقات واضحة مع شرح السبب.

### P1 - وظائف CRM المطلوبة في المواصفات

1. توسيع Lead form: WhatsApp, Website, Industry, Lead source الكاملة، Estimated value، Last Contact، Next Follow-up.
2. CRUD كامل لمصادر Leads من Settings للـ Admin.
3. Tasks / Follow-ups مستقلة: Task name, Lead/Customer, due date/time, priority, status, assigned to, notes، مع reminders.
4. أنواع Activities تفصيلية: Call, Meeting, Email, Follow-up ونماذج حقول مخصصة.
5. Opportunity history وLost reason وautomation عند Won/Lost.
6. Project Tasks ونسبة التقدم المحسوبة من المهام أو تحديثها بشكل منضبط.
7. Quotation template احترافي، discounts/taxes/currency، ربط Opportunity، وحالة قبول مؤثرة على الصفقة.
8. بحث متقدم بفلاتر لكل entity مع الانتقال عند اختيار نتيجة بحث.

### P2 - تجهيز للإنتاج وتحسينات متقدمة

1. EF Core migrations حقيقية بدلاً من upgrader اليدوي عند الاستقرار على schema.
2. Authentication آمن: password hashing قوي، reset password، audit logs.
3. Attachments/files مع تخزين منظم وصلاحيات تنزيل.
4. Dashboard charts احترافية: leads over time, sales over time, leads by source, won vs lost, sales by employee.
5. تقارير قابلة للتصدير Excel/PDF وإعدادات report filters.
6. اختبارات unit/integration لخدمات الصلاحيات والتحويل والحذف.
7. Logging، handling للأخطاء، backup/restore، وإعدادات اتصال SQL Server آمنة.
8. توطين عربي/إنجليزي كامل للواجهة بدل النصوص الإنجليزية الحالية.

## 8. نقاط يجب ألا يكسرها الـ AI الذي سيكمل العمل

1. لا يرجع شرط Customer الإلزامي في Add Lead.
2. لا يعيد إنشاء Opportunity تلقائياً بمجرد حفظ Lead.
3. لا يربط Project مباشرة بـ Lead؛ المصدر التجاري للمشروع يكون Customer ثم Opportunity/Quotation اختيارياً.
4. لا يزيل فلترة Sales على `AssignedTo` أو `OwnerName` عند تعديل services.
5. لا يغير طرق توليد الأكواد `L-`, `C-`, `Q-`, `P-` بدون migration/قرار واضح.
6. لا يعتمد على حذف `crm.db` أثناء التطوير؛ راعِ بيانات المستخدم والترقيات غير المدمرة.
7. لا تضع credentials حقيقية في الكود أو داخل `CRM_SQLSERVER_CONNECTION`.
8. لا تخلط بين design-only changes في XAML وبين تغييرات logic في Services/ViewModels.

## 9. اقتراح خطة تنفيذ قصيرة للـ AI التالي

### المرحلة 1: تثبيت سير العمل التجاري

- نفّذ Convert Lead to Customer مع اختبار منع التكرار.
- أضف CustomerId اختياري/visible في Lead details بعد التحويل.
- أضف OpportunityId/QuotationId اختياريين لـ Project مع migration سليمة.
- اربط Customer detail بتجميع البيانات الحقيقية.

### المرحلة 2: Timeline وTasks

- صمم كيان TimelineEvent أو استعلام موحد يجمع الجداول الموجودة بدون duplicating data.
- أضف كيان Task مستقل مع priority/status/reminder وربط اختياري بـ Lead أو Customer أو Project.
- أضف reminder manager مناسب لتطبيق Desktop.

### المرحلة 3: تقارير وتجهيز إنتاج

- وحّد قواعد صلاحيات كل تقرير وكل قائمة.
- أضف dashboard/report metrics الناقصة والرسوم البيانية.
- نفذ migrations/backup/tests/security قبل الاستخدام الفعلي.

## 10. أوامر مفيدة للبدء

```powershell
# من root المشروع
dotnet build CRM.sln --no-restore
dotnet run --project CRM.Desktop\CRM.Desktop.csproj
```

قاعدة SQLite الافتراضية تظهر بجانب الـ exe أثناء التشغيل في:

```text
CRM.Desktop\bin\Debug\net8.0-windows\crm.db
```

للتبديل إلى SQL Server، اضبط قبل تشغيل التطبيق:

```powershell
$env:CRM_SQLSERVER_CONNECTION = "Server=...;Database=...;Trusted_Connection=True;TrustServerCertificate=True"
```

## 11. ملفات تم تعديلها في آخر جلسة

- `CRM.Desktop/Views/SettingsView.xaml`: تمت إضافته لأنه كان مفقوداً وكان يسبب خطأ `InitializeComponent`.
- `CRM.Desktop/Views/LeadsView.xaml`: تعديل Lead ليكون مستقلاً ولا يطلب Customer.
- `CRM.Desktop/ViewModels/LeadsViewModel.cs`: validation أصبح Company + Contact Person بدلاً من Customer.
- `CRM.Desktop/Services/LeadService.cs`: إزالة Customer mandatory وإزالة إنشاء Opportunity التلقائي.
- `CRM.Desktop/App.xaml`: styles عامة للتصميم.
- `CRM.Desktop/Views/ProjectsView.xaml`: Progress Bar للـ projects.
- `output/pdf/CRM_Screens_Guide_AR.pdf`: دليل PDF عربي للشاشات.

---

### Prompt جاهز لإرساله إلى الـ AI التالي

```text
أنت تكمل مشروع CRM Desktop مكتوب بـ C#/.NET 8/WPF/MVVM/EF Core. اقرأ أولاً ملف PROJECT_HANDOFF_AR.md بالكامل والتزم به. لا تكسر أي وظيفة موجودة، ولا تجعل Customer إلزامياً عند إنشاء Lead، ولا تنشئ Opportunity تلقائياً مع Lead. ابدأ بقراءة الكود والـ build الحالي، ثم نفذ المطلوب على مراحل صغيرة مع build بعد كل مرحلة. افصل XAML/design عن business logic، وراعِ الصلاحيات الموجودة وDbSchemaUpgrader للترقيات غير المدمرة. أول أولوية هي تنفيذ Convert Lead to Customer مع منع التكرار وربط Timeline/Customer details بشكل صحيح.
```
