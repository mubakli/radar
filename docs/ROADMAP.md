# RADAR — AI-Agent-Friendly Development Roadmap

## Roadmap'in amacı

Bu plan RADAR'ı bir özellik listesi olarak değil, küçük fakat anlamlı ürün artışlarıyla geliştirmek için hazırlanmıştır. Her milestone:

- tek bir anlaşılır kullanıcı capability'si üretir,
- bir AI coding agent'a sınırlandırılmış görev olarak verilebilir,
- satır satır manuel kod incelemesine dayanmadan doğrulanabilir,
- sonraki oturuma conversation history olmadan devredilebilir,
- gereksiz mimari karmaşıklığı sonraya bırakır.

Bir milestone agent içinde birkaç teknik alt göreve bölünebilir; ancak senin ürün kontrolün milestone'un tamamlanma ölçütlerine göre yapılır. Bir milestone tek bir odaklı geliştirme döngüsünde yönetilemeyecek kadar büyürse uygulama sırasında kapsam genişletilmez; kalan parça yeni milestone adayı yapılır.

---

# 1. Proje analizi

## Gerçek çekirdek problem

RADAR'ın asıl problemi içerik toplamak veya özet üretmek değildir. Asıl problem, sınırlı dikkatini şu üç kararda doğru kullanmaktır:

1. **Bugün hangi teknik gelişmeler gerçekten önemli?**
2. **Bu konularda hangi insanlar güvenilir ve takip edilmeye değer?**
3. **Hangi gelişmeyi yüzeysel okumak yerine derinlemesine araştırmalıyım?**

Bu nedenle ürünün başarısı toplanan içerik sayısıyla değil, az zamanda doğru gelişmeleri ve doğru insanları gösterebilmesiyle ölçülmelidir.

## Capability bağımlılıkları

| Capability | Ön koşul | Bağımsız geliştirilebilen kısmı |
| --- | --- | --- |
| Industry/Technology Radar | Kaynak, ingestion, normalize edilmiş item ve Story | İlk sürüm yalnızca RSS ve seçilmiş kaynaklarla çalışabilir |
| Story grouping | Yeterli tekrar/benzer içerik örneği | İlk sürüm URL ve başlık kurallarıyla deterministik olabilir |
| People/Expert Radar | İçeriklerde kişi atfı, kimlik ve evidence | Kişi sinyali erken kurulabilir; tavsiye sıralaması veri birikince yapılmalı |
| Open-web discovery | İlgi alanları, çalışan ingestion ve aday değerlendirme kuralları | Bilinen kaynaklardan çıkan referanslar ve sınırlı web aramasıyla başlayabilir |
| Personalization | Açık ilgi alanları ve gerçek kullanıcı feedback'i | İlk brief kişiselleştirmesiz çalışabilir |
| Deep Research | Araştırılacak Story ve doğrulanabilir kaynaklar | Basit, sınırlandırılmış sürüm gelişmiş öneri sisteminden bağımsızdır |
| Semantic intelligence | Golden/eval dataset ve temel deterministic pipeline | LLM veya embedding altyapısı olmadan önce veri toplanmalıdır |

## Erken test edilmesi gereken varsayımlar

1. Seçilmiş az sayıda kaliteli kaynak, algoritmik sosyal medya akışından daha faydalı bir günlük brief üretebilir mi?
2. Aynı gelişmeyi tek Story altında göstermek gürültüyü hissedilir biçimde azaltıyor mu?
3. Kullanıcı 10–15 dakikada “önemli ne değişti?” sorusuna cevap alabiliyor mu?
4. Story üzerinde görünen kişi ve evidence bilgisi gerçekten yeni uzmanlar keşfetmeyi sağlıyor mu?
5. Deep Research raporu, normal arama ve özetten anlamlı ölçüde daha iyi mi?
6. Açık feedback sinyalleri zamanla brief kalitesini artırıyor mu?
7. Bilinen kaynakların dışından bulunan adayların yeterli bölümü gerçekten yeni ve değerli mi?
8. Kontrollü keşif, brief'in gürültüsünü artırmadan kör noktaları azaltabiliyor mu?

## Başlıca teknik riskler

- Farklı içeriklerin yanlışlıkla aynı Story altında birleştirilmesi.
- Aynı kişinin farklı platform kimliklerinin yanlış eşleştirilmesi.
- Popülerliği uzmanlık veya kalite sanan People sıralaması.
- LLM çıktılarında kaynaksız iddia, hatalı çıkarım ve maliyet artışı.
- Çok fazla kaynak ekleyerek ürün değerinden önce ingestion altyapısına gömülmek.
- Kullanıcı feedback'i oluşmadan “kişiselleştirilmiş” sıralama tasarlamak.
- X, YouTube, Reddit gibi platformların erişim, kota ve kullanım koşullarına erken bağımlılık.
- Arama sonuçlarını veya dış bağlantıları doğrulamadan güvenilir kaynak kabul etmek.
- Source discovery'nin zamanla promotional içerik ve SEO spam tarafından ele geçirilmesi.

## Şimdilik yapılmaması gerekenler

- Microservice veya dağıtık event-driven mimari
- Vector database
- Graph database / tam knowledge graph
- Hermes veya karmaşık multi-agent araştırma runtime'ı
- Genel amaçlı crawler platformu
- Bütün kaynak türlerini kapsayan soyut connector framework'ü
- Öğrenilmiş recommendation modeli
- Mobil uygulama, sosyal özellikler ve çok kullanıcılı organizasyon sistemi

Bu bileşenlerin bazıları ileride değerli olabilir; fakat bugün gerçek bir problem çözmeden yalnızca gelecekteki olasılıklara yatırım yaparlar.

---

# Phase A — İlk kullanılabilir Radar

## Milestone 1 — Çalışan ürün omurgası

### Amaç

Repository'yi ve geliştirme kontratını kurarak tek bir örnek Story'nin persistence → API → minimal UI boyunca çalıştığı en ince dikey dilimi oluşturmak.

### Kullanıcı açısından çıktı

Uygulamayı tek komutla çalıştırabilir, örnek bir teknik Story'yi minimal ekranda görebilir ve detayına gidebilirsin.

### Scope

- Çalıştırılabilir uygulama omurgası
- Tek kullanıcı varsayımı
- Minimum `Source`, `Item` ve `Story` modeli
- Migration ile oluşturulan persistence
- Seed/fixture üzerinden bir Story
- Story listesi ve detayını gösteren minimal arayüz
- Build, lint/typecheck ve test komutları
- CI üzerinde aynı doğrulama komutlarının çalışması
- Sabit boyutlu project-memory ve handoff yapısı

### Out of Scope

- Gerçek kaynaklardan ingestion
- Authentication
- Tasarım sistemi
- Ranking, AI, scheduling ve deployment
- Gelecekte gerekebilir diye kapsamlı domain modeli

### Neden şimdi?

Sonraki bütün milestone'ların üzerine ekleneceği çalışır omurgayı ve doğrulama sözleşmesini kurar. İlk adımdan itibaren “çalışan sistem korunur” ilkesini zorunlu hale getirir.

### AI Agent Context

Agent yalnızca ürün özeti, bu milestone specification'ı, repository çalışma kuralları ve seçilen temel teknik kararları okumalıdır. People Radar'ın ileri sıralaması, Deep Research tasarımı veya gelecekteki connector planlarına ihtiyaç duymaz.

### Verification

- Temiz ortamda kurulum ve build başarılı.
- Migration boş veritabanına uygulanabiliyor.
- Seed işlemi tekrar çalıştırıldığında veri çoğalmıyor.
- API contract testi Story listesini ve detayını doğruluyor.
- UI smoke testi seed Story'nin görüntülendiğini doğruluyor.
- CI aynı kontrolleri sıfır hata ile tamamlıyor.

### Project Memory Update

Gerekli. Ürün amacı, minimum domain sözlüğü, çalışma/doğrulama komutları, repository haritası ve gerçekten alınmış mimari kararlar kendi durable owner belgelerine yazılır. Non-trivial delivery context'i gerekiyorsa `docs/features/` altında geçici spec olarak tutulur; tamamlanınca silinir.

### Exit Criteria

- Uygulama belgelenmiş tek akışla yerelde açılıyor.
- Seed Story API ve UI üzerinden görülebiliyor.
- Migration, test ve build otomatik doğrulanıyor.
- Yeni bir agent yalnızca repository belgeleriyle projeyi çalıştırabiliyor.

### Sonraki milestone'a handoff

Story'nin mevcut veri akışı, Source/Item/Story sınırları, çalıştırma komutları ve RSS ingestion'ın bağlanacağı extension point.

---

## Milestone 2 — Seçilmiş RSS kaynaklarından gerçek içerik

### Amaç

İlk gerçek ingestion akışını kurarak seçilmiş RSS/Atom kaynaklarından içerik toplayıp normalize edilmiş Item olarak saklamak.

### Kullanıcı açısından çıktı

Bir RSS/Atom kaynağı ekleyebilir, manuel olarak yenileyebilir ve gelen gerçek içerikleri RADAR içinde görebilirsin.

### Scope

- Kaynak ekleme, etkinleştirme ve devre dışı bırakma
- Manuel “şimdi getir” işlemi
- RSS ve Atom parse etme
- Başlık, URL, yayın tarihi, yazar ve özet alanlarının normalize edilmesi
- Canonical URL ve kaynak kimliğiyle idempotent kayıt
- Başarılı/başarısız fetch sonucunun görünmesi
- Fixture tabanlı parser ve ingestion testleri

### Out of Scope

- Otomatik scheduling
- Genel web crawling
- Birden fazla platform connector'ü
- Story clustering
- LLM ile özetleme veya sınıflandırma

### Neden şimdi?

Ürünün gerçek veriye temas eden ilk capability'sidir. Sınırlı bir kaynak türüyle ingestion varsayımlarını doğrular ve sonraki brief için veri üretir.

### AI Agent Context

Agent Source/Item domain belgesini, ingestion contract'ını, ilgili migration'ları, RSS fixture'larını ve bu milestone spec'ini okumalıdır. UI'ın geri kalanı, People ranking ve araştırma belgeleri gereksizdir.

### Verification

- RSS 2.0 ve Atom fixture'ları beklenen normalize edilmiş Item'ları üretir.
- Aynı feed iki kez işlendiğinde duplicate kayıt oluşmaz.
- Eksik tarih/yazar/özet içeren kayıtlar tanımlanmış kurala göre işlenir.
- Bozuk feed tüm işlemi çökertmez ve gözlemlenebilir hata üretir.
- Entegrasyon testi: kaynak ekle → fetch et → Item'ı API/UI'da gör.

### Project Memory Update

Yalnızca kalıcı normalization ve idempotency kuralları domain belgesine eklenir. Parser detayları testler ve kod içinde kalır.

### Exit Criteria

- En az üç seçilmiş gerçek kaynak başarıyla alınabiliyor.
- Tekrar fetch duplicate oluşturmuyor.
- Hatalı kaynak kullanıcı tarafından ayırt edilebiliyor.
- Gelen Item'lar arayüzde açılabiliyor.

### Sonraki milestone'a handoff

Item alanlarının anlamı, idempotency anahtarı, fetch tetikleme noktası ve doğrulanan fixture seti.

---

## Milestone 3 — Sonlu günlük brief ve triage

### Amaç

Toplanan içerikleri sonsuz akış yerine kısa, tamamlanabilir bir günlük brief haline getirmek ve ilk gerçek kullanım feedback'ini toplamak.

### Kullanıcı açısından çıktı

Bugünün sınırlı brief'ini açabilir; içerikleri okundu, önemli, kaydedildi veya ilgisiz olarak işaretleyebilirsin.

### Scope

- Tarihe göre sonlu günlük brief
- İlk sürüm için açık ve basit sıralama: kaynak önceliği + yayın zamanı
- Brief boyutu için yapılandırılabilir üst sınır
- `read`, `important`, `saved`, `not relevant` feedback eylemleri
- Brief tamamlanma durumu
- Her Item için kaynağın ve gösterilme nedeninin görünmesi
- Feedback persistence ve contract testleri

### Out of Scope

- Öğrenilmiş veya kişiselleştirilmiş ranking
- Story clustering
- Bildirimler ve e-posta özeti
- AI summary
- Gamification veya streak

### Neden şimdi?

RADAR'ın temel değer önerisini en erken noktada test eder: seçilmiş kaynaklardan gelen kısa bir akış gerçekten günlük bilgi ihtiyacını karşılıyor mu? Ayrıca ilerideki kişiselleştirme için gerçek feedback üretir.

### AI Agent Context

Agent Item API'sini, feedback durum modelini, ilgili UI bileşenlerini ve günlük brief acceptance kurallarını okumalıdır. Connector parser'larının iç detaylarına veya ilerideki semantik sıralamaya gerek yoktur.

### Verification

- Brief belirlenen maksimum sayıyı aşmaz.
- Tarih ve timezone sınırları sabit testlerle doğrulanır.
- Feedback eylemleri tekrarlandığında tutarlı sonuç verir.
- Aynı gün brief tekrar açıldığında durum korunur.
- E2E test: brief aç → Item'ı işaretle → yenile → durumun korunduğunu gör.

### Project Memory Update

Feedback durumlarının domain anlamı ve brief'in “sonlu” olma kuralı kalıcı belgelere eklenir. UI detayları belgelenmez.

### Exit Criteria

- Bir günlük gerçek veri brief'i 10–15 dakikada tamamlanabiliyor.
- Kullanıcı eylemleri kalıcı ve tutarlı.
- Brief'in neden bu sırada gösterildiği anlaşılabiliyor.
- En az yedi gerçek kullanım günü için veri toplamaya başlanabiliyor.

### Sonraki milestone'a handoff

Brief seçim kuralları, feedback event'lerinin anlamı ve ilk kullanım ölçümlerinin nereden okunacağı.

---

## Risk Checkpoint A — İlk ürün değeri

Milestone 3 sonrasında en az 7–14 gün gerçek kullanım yapılmalıdır.

Şu sorulara olumlu yanıt yoksa yeni connector veya AI özelliği eklenmemelidir:

- Brief'i gerçekten düzenli açıyor musun?
- İçeriğin kayda değer kısmı ilgisiz mi?
- Seçilmiş kaynaklar sosyal medya akışından daha iyi sinyal veriyor mu?
- 10–15 dakika sınırı gerçekçi mi?
- En çok eksik olan şey kaynak çeşitliliği mi, gruplama mı, açıklama mı?

---

# Phase B — Gürültüyü azaltma ve üç çekirdek capability'yi gösterme

## Milestone 4 — Deterministik Story oluşturma ve duplicate kontrolü

### Amaç

Aynı gelişmeye ait tekrar içerikleri tek Story altında toplamak ve yanlış gruplamayı kullanıcı tarafından düzeltilebilir yapmak.

### Kullanıcı açısından çıktı

Aynı haberin farklı kaynaklardaki kopyalarını ayrı kartlar yerine tek Story içinde görebilir; yanlış birleşimi ayırabilir veya iki Story'yi birleştirebilirsin.

### Scope

- Canonical URL eşleştirme
- Normalize edilmiş başlık karşılaştırması
- Açık deterministik duplicate/grouping kuralları
- Story içinde kaynakların listelenmesi
- Manuel merge ve split
- Karar nedeninin kaydı
- Gerçek örneklerden golden grouping dataset'i

### Out of Scope

- Embedding/vector search
- LLM ile clustering
- Otomatik importance veya novelty puanı
- Büyük ölçekli entity resolution

### Neden şimdi?

İlk kullanım verisi duplicate probleminin gerçek büyüklüğünü gösterir. Semantik yöntemlerden önce ucuz ve açıklanabilir kurallarla ne kadar yol alınabileceğini ölçer.

### AI Agent Context

Agent yalnızca Item/Story domain kurallarını, golden dataset'i, mevcut brief sorgusunu ve bu spec'i okumalıdır. RSS parser veya People/Research kodunu bilmesi gerekmez.

### Verification

- Golden dataset'teki kesin duplicate'ler aynı Story'ye gider.
- Benzer başlıklı fakat farklı gelişmeler ayrı kalır.
- Aynı ingestion tekrarlandığında Story üyeliği değişmez.
- Merge/split sonrası brief sonucu ve kaynak sayısı doğrudur.
- Grouping kararları fixture üzerinde deterministic olarak tekrar üretilebilir.

### Project Memory Update

Story kimliği, kaynak üyeliği ve manuel düzeltmenin semantiği domain belgesine eklenir. Eşikler test fixture'larında yaşar; sık değişen sayılar ana dokümana taşınmaz.

### Exit Criteria

- Brief artık Item yerine Story gösteriyor.
- Bilinen duplicate fixture'ları doğru gruplanıyor.
- Yanlış gruplama veri kaybetmeden düzeltilebiliyor.
- Gruplama kararının nedeni gözlemlenebiliyor.

### Sonraki milestone'a handoff

Story membership contract'ı, golden dataset konumu ve manual correction akışı.

---

## Milestone 5 — People sinyali ve takip edilebilir kişi profili

### Amaç

People Radar'ı erken aşamada ürünün içine yerleştirerek içeriklerin arkasındaki insanları evidence ile görünür hale getirmek.

### Kullanıcı açısından çıktı

Bir Story'de adı geçen yazar, maintainer veya üreticiyi görebilir; kişi profilini açabilir ve takip listene ekleyebilirsin.

### Scope

- Minimum `Person`, `Identity` ve `PersonEvidence` modeli
- Feed metadata'sından yazar/üretici sinyali alma
- Kişi–Item–Story ilişkisi
- Evidence kaynağını gösteren kişi profili
- Kişiyi takip etme/bırakma
- Yanlış kimlik eşleşmesini manuel ayırma ve doğrulanmış kimlikleri birleştirme
- Kimlik eşleştirmede muhafazakâr yaklaşım

### Out of Scope

- Otomatik expert score
- “Kimi takip etmeliyim?” recommendation'ı
- Sosyal graph crawling
- Influence skoru
- Knowledge graph veya graph database

### Neden şimdi?

People Radar sonradan eklenen bağımsız bir modül olmamalıdır. Erken kişi/evidence modeli sonraki GitHub verisinin, kişiselleştirmenin ve expert discovery'nin doğru temel üzerinde birikmesini sağlar.

### AI Agent Context

Agent People domain sözlüğünü, mevcut Item/Story attribution alanlarını, identity merge/split kurallarını ve ilgili UI akışını okumalıdır. Ranking, recommendation ve Deep Research tasarımı gereksizdir.

### Verification

- Aynı kaynaktaki aynı author identity tekrar işlendiğinde tek kişi ilişkisi oluşur.
- Farklı isimler otomatik olarak agresif biçimde birleştirilmez.
- Merge/split işlemleri evidence kaybetmez.
- Story detayında kişiler ve kanıt kaynakları doğru görünür.
- Follow/unfollow işlemi kalıcı ve idempotent'tir.

### Project Memory Update

Person, Identity, Evidence ve doğrulanmış/manual eşleştirme kuralları kalıcı domain belgesine eklenir. Kaynağa özel parsing kod içinde kalır.

### Exit Criteria

- Story'lerde kaynak tarafından desteklenen kişi sinyalleri görülebiliyor.
- Kişi profili claims yerine evidence listeliyor.
- Takip listesi çalışıyor.
- Hatalı kimlik birleştirme geri alınabiliyor.

### Sonraki milestone'a handoff

Person identity contract'ı, evidence provenance modeli ve follow feedback sinyali.

---

## Milestone 6 — Sınırlandırılmış Deep Research v0

### Amaç

Bir Story'yi yüzeysel brief'ten doğrulanabilir kaynaklara dayanan araştırma raporuna taşımanın gerçekten değer üretip üretmediğini test etmek.

### Kullanıcı açısından çıktı

Bir Story üzerinde “Deep Research” başlatabilir; primary source, temel iddialar, evidence, belirsizlikler ve kaynaklarla sınırlı bir rapor okuyabilirsin.

### Scope

- Story üzerinden başlatılan tek research job
- Story'nin mevcut kaynakları ve sınırlı sayıdaki açık bağlantı üzerinde çalışma
- Kaynak snapshot'ı ve erişim zamanı
- Yapılandırılmış rapor: özet, gerçekten yeni olan, iddialar, evidence, inference, çelişki/belirsizlik, kaynaklar
- Her önemli iddiada kaynak bağlantısı zorunluluğu
- Zaman, sayfa ve model maliyeti bütçesi
- Job durumu ve başarısızlık görünürlüğü
- Küçük sabit research eval seti

### Out of Scope

- Hermes entegrasyonu
- Otonom multi-agent workflow
- Repo kodu, issue/PR ve paper'ların derin çapraz analizi
- Sınırsız web crawling
- Otomatik olarak her Story için araştırma

### Neden şimdi?

Deep Research diğer ranking özelliklerinden büyük ölçüde bağımsızdır ve ürünün üç temel capability'sinden biridir. Basit sürüm, gelişmiş runtime yatırımı yapmadan önce kullanım sıklığını ve rapor değerini ölçer.

### AI Agent Context

Agent Story contract'ını, research report şemasını, güven/evidence kurallarını, model adapter sınırını ve eval fixture'larını okumalıdır. Ingestion parser'ları, People recommendation veya gelecekteki Hermes planı gerekmez.

### Verification

- Rapor şeması contract testinden geçer.
- Her claim geçerli bir kaynak kimliğine referans verir.
- Kaynakta bulunmayan URL veya erişilmemiş sayfa citation olarak kullanılamaz.
- Aynı fixture üzerinde rapor yapısı ve evidence bağlantıları doğrulanır.
- Bütçe aşımında job kontrollü şekilde durur.
- Eval setinde citation coverage, unsupported-claim oranı ve kaynak erişilebilirliği eşikleri sağlanır.

### Project Memory Update

Research evidence modeli, report contract'ı, bütçe sınırları ve kabul edilen AI boundary kalıcı belgelere eklenir. Prompt metinleri ve model özel ayrıntılar kod/config/test içinde kalır.

### Exit Criteria

- En az beş farklı gerçek Story'de research job tamamlanır.
- Rapor evidence ile inference'ı ayırır.
- Unsupported claim kontrol eşiği karşılanır.
- Kullanıcı raporun normal Story özetinden daha değerli olup olmadığını değerlendirebilir.

### Sonraki milestone'a handoff

Research job state machine, report schema, eval sonuçları, maliyet/zaman ölçümü ve bilinen kalite sınırları.

---

## Risk Checkpoint B — Üç capability testi

Milestone 6 sonrasında ürünün üç çekirdek ekseni ilk kez görünürdür:

- Industry Radar: kaynaklar ve Story brief'i
- People Radar: evidence-backed kişi görünürlüğü
- Deep Research: isteğe bağlı araştırma raporu

Şunlar değerlendirilmelidir:

- Story grouping gerçekten gürültüyü azalttı mı, yoksa hata mı üretiyor?
- Story'deki insanlar yeni ve değerli keşifler sağlıyor mu?
- Deep Research hangi tür Story'lerde gerçekten kullanılıyor?
- Araştırma kalitesi ve maliyeti ilerletmeye değer mi?

People veya Deep Research kullanılmıyorsa daha karmaşık sürümlerine geçilmez; problem önce ürün akışında aranır.

---

# Phase C — Veri kalitesi ve güvenilir günlük işletim

## Milestone 7 — GitHub teknoloji ve insan sinyalleri

### Amaç

RADAR'a yalnızca haber değil, doğrudan teknik üretim ve maintainer evidence'ı getiren ikinci yüksek değerli kaynak türünü eklemek.

### Kullanıcı açısından çıktı

Seçtiğin GitHub repository'lerinin release ve önemli proje güncellemelerini Story olarak görebilir; ilgili maintainer/author profillerine ulaşabilirsin.

### Scope

- Takip edilecek repository ekleme/çıkarma
- Release ve seçilmiş repository metadata'sını alma
- GitHub verisini mevcut Item/Story modeline normalize etme
- Maintainer/author identity'sini PersonEvidence olarak bağlama
- API kota ve cursor/checkpoint yönetimi
- Fixture ve fake API ile deterministik connector testleri

### Out of Scope

- GitHub'ın tamamını tarama
- Trending sistemi
- Bütün commit, issue ve PR'ları feed'e aktarma
- Contributor sayısını uzmanlık skoru sayma
- Genel connector platformu

### Neden şimdi?

RSS akışı ürün değerini kanıtladıktan sonra GitHub, Industry ve People Radar'ı aynı veri üzerinden besler. Böylece ikinci connector gerçekten iki çekirdek capability'ye hizmet eder.

### AI Agent Context

Agent connector contract'ını, Item normalization'ı, PersonEvidence modelini, Story grouping extension point'ini ve GitHub fixture'larını okumalıdır. Research orchestration veya ranking detayı gerekmez.

### Verification

- Aynı release tekrar çekildiğinde duplicate oluşmaz.
- Pagination/checkpoint tekrar başlatmada veri kaybetmez.
- Kota hataları kontrollü ve tekrar denenebilir durum üretir.
- Release doğru repository ve kişi evidence'ına bağlanır.
- Entegrasyon testi fake API → Item → Story → UI akışını doğrular.

### Project Memory Update

Yalnızca connector contract'ı, checkpoint/idempotency ve GitHub sinyalinin domain anlamı eklenir. API istemci ayrıntıları belgelenmez.

### Exit Criteria

- Seçilmiş repository'ler düzenli alınabiliyor.
- Release'ler mevcut brief'e doğru şekilde giriyor.
- People profilleri GitHub evidence'ı gösterebiliyor.
- Kota ve tekrar başlatma davranışı testlerle güvence altında.

### Sonraki milestone'a handoff

İki connector'ün ortak job contract'ı, checkpoint yaklaşımı ve gözlemlenmesi gereken hata sınıfları.

---

## Milestone 8 — Güvenilir zamanlanmış çalışma ve gözlemlenebilirlik

### Amaç

Manuel fetch edilen prototipi, günlük kullanılabilecek şekilde kendi kendine çalışan ve hata verdiğinde nedenini gösterebilen sisteme dönüştürmek.

### Kullanıcı açısından çıktı

Kaynaklar belirlenen aralıklarla otomatik güncellenir; son çalışma, hata, gecikme ve alınan kayıt sayısını görebilirsin.

### Scope

- Tek süreç içinde basit scheduler/background job yaklaşımı
- Per-source schedule ve son başarılı checkpoint
- Retry/backoff ve eşzamanlı çalışma koruması
- Job run kayıtları
- Minimum health ve ingestion metrics ekranı/logları
- Research model kullanım maliyetinin ölçülmesi
- Backup/restore için doğrulanmış temel prosedür

### Out of Scope

- Mesaj kuyruğu
- Ayrı worker cluster'ı
- Distributed scheduler
- Kubernetes
- Tam gözlemlenebilirlik platformu

### Neden şimdi?

Birden fazla gerçek connector oluştuğu için otomasyon artık somut bir ihtiyaca hizmet eder. Daha erken yapılsaydı gereksiz altyapı olurdu.

### AI Agent Context

Agent yalnızca connector job contract'larını, persistence checkpoint'lerini, mevcut deployment/runtime sınırlarını ve operational acceptance testlerini okumalıdır. UI domain detayları ve gelecekteki semantic intelligence gerekmez.

### Verification

- Fake clock ile schedule testleri.
- Aynı job'ın çakışan iki çalışması duplicate üretmez.
- Geçici hata retry edilir; kalıcı hata görünür duruma gelir.
- Uygulama yeniden başladığında checkpoint'ten devam eder.
- Backup al → boş ortamda restore et → kayıt sayıları ve kritik ilişkileri doğrula testi.

### Project Memory Update

Operational runbook, job state semantiği, backup/restore komutları ve alınan önemli runtime kararı eklenir.

### Exit Criteria

- Sistem en az yedi gün manuel müdahale olmadan veri toplayabilir.
- Başarısız kaynak ve nedeni görünürdür.
- Restart duplicate veya veri kaybı üretmez.
- Restore prosedürü en az bir kez başarıyla test edilmiştir.

### Sonraki milestone'a handoff

Job state modeli, scheduler giriş noktaları, operasyonel komutlar ve yedi günlük çalışma sonucu.

---

# Phase D — Open-web discovery ve kontrollü kaynak genişlemesi

## Milestone 9 — Open-web discovery ve aday havuzu

### Amaç

Kullanıcının verdiği kaynak ve kişileri ürünün sınırı olmaktan çıkarıp başlangıç tohumu haline getirmek; ilgi alanlarından ve mevcut Story'lerden açık web üzerinde yeni içerik, kaynak, repository, paper ve kişi adayları keşfetmek.

### Kullanıcı açısından çıktı

Takip listende bulunmayan teknik gelişmeleri ve insanları bir discovery inbox içinde görebilir; her adayın nasıl bulunduğunu ve neden ilginle ilişkili olduğunu inceleyebilirsin.

### Scope

- Kullanıcının verdiği basit topic/query seed'leri ve mevcut Story'lerden sınırlı discovery query üretimi
- Bir arama sağlayıcısı üzerinden bounded open-web search adapter'ı
- Bilinen Source Item'lardaki dış bağlantı, repository, paper ve kişi referanslarını aday olarak çıkarma
- `Discovery Lead` / candidate havuzu ve provenance
- Aday türleri: içerik, kaynak, repository, paper ve kişi
- Her aday için discovery path, topic relevance ve temel quality/evidence özellikleri
- Duplicate adayların deterministik birleştirilmesi
- Günlük query, sonuç, süre ve maliyet bütçeleri
- Kullanıcı eylemleri: inspect, accept for probation, dismiss, mute origin
- Fixture ve golden discovery dataset'i

### Out of Scope

- Bütün interneti crawl etmek
- Arama sonucunu doğrudan güvenilir kaynak veya brief Story'si kabul etmek
- Otomatik source promotion
- X, Reddit, YouTube veya Hacker News'e özel connector'lar
- Öğrenilmiş source-quality modeli
- Bu geçici discovery seed'lerini tam kişiselleştirilmiş interest profile dönüştürmek
- Sınırsız recursive link traversal

### Neden şimdi?

Çalışan ingestion, Story, People ve scheduler altyapısı artık vardır. Daha erken eklenirse ürün değeri kanıtlanmadan crawler altyapısına dönüşür; daha geç eklenirse RADAR uzun süre yalnızca manuel takip listesi okuyucusu olarak kalır.

### AI Agent Context

Agent Source/Item/Story/Person kavramlarını, provenance invariants'ı, basit discovery-seed contract'ını, job/budget mekanizmasını, discovery policy'yi ve bu milestone spec'ini okumalıdır. Personalized interest/ranking, advanced expert scoring veya Deep Research orchestration detaylarına ihtiyaç duymaz.

### Verification

- Sabit discovery seed ve search fixture'ı aynı query/candidate sonuçlarını üretir.
- Her candidate discovery origin ve path ile izlenebilir.
- Duplicate URL/repository/paper/identity adayları tek kayıtta birleşir.
- Bütçe aşıldığında discovery kontrollü terminal duruma geçer.
- Search adapter kapalıyken mevcut takip/brief akışı çalışmaya devam eder.
- Golden dataset'te beklenen yeni aday recall eşiği sağlanırken ilgisiz aday oranı kabul sınırını aşmaz.

### Project Memory Update

Gerekli. Kullanıcının yapılandırdığı Source/Person'ın seed olduğu; Discovery Lead'in kanıt veya trusted Source olmadığı; discovery provenance, budget ve candidate lifecycle kuralları PRODUCT, DOMAIN, DISCOVERY ve ARCHITECTURE belgelerine eklenir. Sağlayıcı seçimi geri dönüşü maliyetliyse ADR değerlendirilir.

### Exit Criteria

- Manuel girilmemiş gerçek içerik, kaynak, repository, paper ve kişi adayları bulunabiliyor.
- Her adayın neden bulunduğu açıklanabiliyor.
- Adaylar doğrudan günlük brief'i kirletmiyor.
- Discovery bütçesi ve başarısızlıkları gözlemlenebiliyor.
- Kullanıcı bir adayı probation'a alabiliyor veya reddedebiliyor.

### Sonraki milestone'a handoff

Discovery Lead contract'ı, aday türleri, provenance path, golden dataset, bütçe sınırları ve probation giriş noktası.

---

## Milestone 10 — Source probation ve kontrollü otomatik genişleme

### Amaç

Keşfedilen aday kaynakları tek sonuç üzerinden güvenilir saymak yerine zaman içinde değerlendirmek; yeterli kanıt oluştuğunda kontrollü biçimde RADAR'ın gözlem alanına eklemek.

### Kullanıcı açısından çıktı

RADAR yeni kaynakları kendisi dener, ürettikleri sinyali değerlendirir ve neden önerildiğini gösterir; onaylanan veya düşük riskli kriterleri geçen kaynaklardan yeni Story'ler toplayabilir.

### Scope

- Source lifecycle: candidate → probation → active/trusted-for-purpose → muted/rejected
- Probation kaynaklarının sınırlı sıklık ve hacimle toplanması
- Claim-specific olmayan genel seçim özellikleri: topical relevance, original/primary work, citation quality, promotional ratio, duplicate contribution, freshness ve devamlılık
- Source assessment'ın evidence ve zaman penceresiyle saklanması
- Exploration ve exploitation için ayrı günlük bütçeler
- Brief'te discovery kaynaklarını görünür etiketleme
- Manual promote/demote/mute ve düşük riskli auto-promotion policy'si
- Drift ve kalite gerilemesinde probation'a geri alma
- Source expansion replay/eval dataset'i

### Out of Scope

- Kalıcı ve evrensel domain/person trust score'u
- Tek içerikle otomatik güven verme
- Kullanıcı kontrolü olmadan yüksek riskli platformları ekleme
- Popularity veya SEO sırasını kalite kabul etme
- Tüm connector türlerini genelleştirme
- Personalized Story ranking'i değiştirme

### Neden şimdi?

Milestone 9 yalnızca aday bulur. Bu milestone discovery'yi gerçekten sürdürülebilir bir kaynak genişletme döngüsüne çevirir; yeni kaynakları kontrolsüz biçimde brief'e sokmadan otonomluğu artırır.

### AI Agent Context

Agent discovery candidate contract'ını, Source lifecycle ve provenance kurallarını, scheduler/budget mekanizmasını, source assessment eval setini ve brief source etiketlemesini okumalıdır. Expert recommendation, semantic Story clustering ve Research Agent context'i gerekli değildir.

### Verification

- Lifecycle transition'ları izin verilen durumlarla sınırlıdır ve geri alınabilir.
- Probation kaynakları configured exploration bütçesini aşamaz.
- Aynı replay dataset'i aynı assessment bileşenlerini ve transition önerisini üretir.
- Promotional/duplicate-heavy fixture'lar promotion eşiğini geçemez.
- Source demote/mute edildiğinde yeni ingestion durur; önceki evidence silinmez.
- Auto-promotion kapalıyken sistem tam manual modda çalışır.

### Project Memory Update

Source lifecycle, exploration/exploitation ayrımı, assessment evidence'ı ve promotion/demotion invariants'ı kalıcı belgelere eklenir. Değişebilir eşik ve ağırlıklar config/eval içinde kalır.

### Exit Criteria

- RADAR başlangıçta verilmemiş en az birkaç kaliteli kaynağı probation üzerinden keşfedip izleyebiliyor.
- Kaynakların neden aktif veya reddedildiği açıklanabiliyor.
- Discovery kaynaklarının brief'e katkısı ayrı ölçülebiliyor.
- Gürültü artışı, duplicate katkı ve promotional oranı için kabul eşikleri sağlanıyor.
- Kullanıcı her otomatik kararı geri alabiliyor.

### Sonraki milestone'a handoff

Source lifecycle, assessment component'leri, exploration budget, replay sonuçları ve kullanıcı override kuralları.

---

## Risk Checkpoint C — Discovery gerçekten yeni değer buluyor mu?

Milestone 10 sonrasında en az iki hafta discovery açık kullanılmalıdır:

- Manuel olarak verilmemiş kaç değerli Story, kaynak, repository, paper ve kişi bulundu?
- Keşif inbox'ındaki adayların ne kadarı gerçekten incelenmeye değer?
- Probation kaynakları brief kalitesini yükseltiyor mu, gürültü mü ekliyor?
- Sistem aynı popüler kaynak çevresinde dönüyor mu, niş ve özgün üreticileri bulabiliyor mu?
- Search/API maliyeti bulunan yararlı aday başına kabul edilebilir mi?
- Source promotion nedenleri savunulabilir ve geri alınabilir mi?

Discovery değer üretmiyorsa daha geniş crawl veya daha fazla connector eklenmez. Query üretimi, candidate kaynakları, değerlendirme sinyalleri ve interest kapsamı düzeltilir.

---

# Phase E — Personal intelligence

## Milestone 11 — İlgi alanları ve öğrenilebilir feedback modeli

### Amaç

Personalization başlamadan önce açık ilgi alanlarını ve feedback sinyallerinin anlamını güvenilir biçimde tanımlamak.

### Kullanıcı açısından çıktı

İlgilendiğin teknik konuları tanımlayabilir; Story'leri konuya bağlayabilir ve sistemin hangi feedback'leri senden öğrendiğini görebilirsin.

### Scope

- Kullanıcı tarafından yönetilen topic/interests
- Story'ye manuel topic atama ve basit açıklanabilir keyword rule'ları
- Feedback sinyallerinin pozitif/negatif/nötr anlamı
- Takip edilen kişi ve kaynakların tercih sinyali olarak görünmesi
- Feedback export/debug görünümü
- Geçmiş brief'ler üzerinde replay edilebilir veri seti

### Out of Scope

- LLM topic classification
- Embedding
- Otomatik profil çıkarımı
- Personalized ranking
- Gizli davranış takibi

### Neden şimdi?

Milestone 3'ten beri gerçek feedback birikmiştir. Ranking formülü yazmadan önce hangi sinyalin ne anlama geldiği açıklaştırılır; aksi halde sistem rastgele davranışı “öğrenir.”

### AI Agent Context

Agent feedback domain belgesini, mevcut kullanıcı eylemlerini, Person follow ve Source öncelik modellerini, örnek replay dataset'ini okumalıdır. Connector implementasyonları ve Deep Research içeriği gerekmez.

### Verification

- Feedback event'leri sabit anlamlarla sınıflandırılır.
- Aynı replay dataset'i aynı tercih özetini üretir.
- Topic kuralları golden örneklerde beklenen sonuç verir.
- Kullanıcı topic ve sinyal kaynağını görüntüleyip düzeltebilir.

### Project Memory Update

Topic ve feedback semantiği domain belgesine eklenir. Değişebilir ağırlıklar config/test içinde tutulur.

### Exit Criteria

- İlgi alanları açıkça yönetilebiliyor.
- Geçmiş feedback replay edilebiliyor.
- Her tercih sinyalinin kaynağı açıklanabiliyor.
- Ranking için yeterli, temiz bir giriş contract'ı var.

### Sonraki milestone'a handoff

Feedback → preference dönüşüm kuralları, topic contract'ı ve replay dataset'i.

---

## Milestone 12 — Açıklanabilir kişiselleştirilmiş brief v1

### Amaç

Kara kutu model kullanmadan, açık ilgi ve feedback sinyalleriyle günlük brief'i kullanıcıya göre sıralamak.

### Kullanıcı açısından çıktı

Brief'inde sana daha uygun Story'leri üstte görür ve her Story'nin neden gösterildiğini anlayabilirsin.

### Scope

- Deterministik ranking bileşenleri: recency, source priority, topic interest, followed person, previous feedback
- Her Story için reason codes
- Skor bileşenlerini debug edebilme
- Eski brief'ler üzerinde offline ranking replay
- Basit çeşitlilik ve üst sınır kuralları
- Eski sıralama ile karşılaştırma görünümü/raporu

### Out of Scope

- Machine learning recommendation
- Collaborative filtering
- Semantic similarity
- Otomatik ağırlık optimizasyonu
- Kullanıcıyı sonsuz engagement'a yönlendiren metrikler

### Neden şimdi?

Gerçek feedback ve topic contract'ı oluşmadan kişiselleştirme spekülasyon olurdu. Şimdi ucuz, açıklanabilir ve geri alınabilir bir baseline kurulabilir.

### AI Agent Context

Agent brief selection contract'ını, feedback/topic domain kurallarını, Person follow sinyalini ve ranking replay dataset'ini okumalıdır. Ingestion parser veya research pipeline gerekmez.

### Verification

- Aynı snapshot aynı sıralamayı üretir.
- Reason codes skor bileşenleriyle tutarlıdır.
- Negatif feedback tanımlanan şekilde gelecekteki sıralamayı etkiler.
- Maksimum brief boyutu ve çeşitlilik kuralı property testleriyle korunur.
- Offline replay'de yeni ranking önceden tanımlanan kalite ölçütünde baseline'dan kötü değildir.

### Project Memory Update

Ranking'in kavramsal bileşenleri ve reason-code contract'ı belgelenir. Ağırlık değerleri ana dokümana yazılmaz.

### Exit Criteria

- Brief deterministik ve açıklanabilir biçimde kişiselleşiyor.
- Her sonuç için “neden görüyorum?” yanıtı var.
- Ranking geçmiş snapshot üzerinde tekrar üretilebiliyor.
- Baseline karşılaştırması kaydedilmiş acceptance eşiğini karşılıyor.

### Sonraki milestone'a handoff

Ranking input/output contract'ı, reason codes, replay komutu ve baseline sonuçları.

---

## Risk Checkpoint D — Personalization değeri

Milestone 12 sonrasında en az iki hafta eski ve yeni brief sonuçları karşılaştırılmalıdır:

- Önemli olarak işaretlenen Story oranı arttı mı?
- `not relevant` oranı azaldı mı?
- Kaynak veya topic yankı odası oluşuyor mu?
- Followed person sinyali kalite mi getiriyor, sadece aynı isimleri mi tekrar ediyor?
- Açıklamalar verilen sıralamayı gerçekten anlaşılır kılıyor mu?

Baseline'dan ölçülebilir biçimde daha iyi değilse semantic model eklenmez; sinyal anlamları ve brief tasarımı düzeltilir.

---

# Phase F — Semantik zekâ ve Expert Discovery

## Milestone 13 — Eval kontrollü semantik Story intelligence

### Amaç

Deterministik kuralların çözemediği Story eşleme, topic ve novelty kararlarında sınırlı semantik reasoning kullanmak.

### Kullanıcı açısından çıktı

Farklı başlıklarla anlatılan aynı gelişmeler daha iyi birleşir; Story topic'leri ve “neden yeni?” açıklaması daha doğru görünür.

### Scope

- Önce deterministik candidate generation
- Yalnızca belirsiz adaylarda semantic karar
- Story similarity/grouping, topic suggestion ve bounded novelty explanation
- Model-independent adapter
- Versiyonlanmış input/output şeması
- Gerçek manuel düzeltmelerden oluşan eval dataset
- Precision ağırlıklı acceptance metriği
- Cost, latency ve fallback ölçümü

### Out of Scope

- Bütün corpus'u modele gönderme
- Vector database
- Otomatik sınırsız yeniden clustering
- Semantic skorun tek başına ranking'i belirlemesi
- Model çıktısını doğrulamadan persistence'a yazma

### Neden şimdi?

Deterministik baseline, gerçek hata örnekleri ve manuel correction verisi artık vardır. Böylece AI “iyi görünüyor” diye değil, baseline'a karşı ölçülerek sisteme girer.

### AI Agent Context

Agent Story grouping contract'ını, manual correction kayıtlarını, eval dataset'i, semantic adapter sınırını ve cost budget'ı okumalıdır. Scheduler veya People UI detayları gereksizdir.

### Verification

- Eval dataset'inde precision/recall eşikleri sağlanır; yanlış birleşim için daha sıkı precision şartı uygulanır.
- Structured output şema doğrulamasından geçmeyen sonuç reddedilir.
- Model kapalıyken deterministic fallback çalışır.
- Cost ve latency üst sınırları otomatik test/benchmark ile kontrol edilir.
- Yeni model/prompt ancak eval regression kontrolünden sonra kullanılabilir.

### Project Memory Update

AI boundary, eval metriği, fallback kuralı ve pahalı/geri dönüşü zor model kararı varsa ADR eklenir. Prompt iterasyonları ana project memory'ye taşınmaz.

### Exit Criteria

- Semantic yaklaşım baseline'dan ölçülebilir şekilde iyi.
- Yanlış Story merge oranı kabul sınırının altında.
- Model olmadan sistem çalışmaya devam ediyor.
- Maliyet her iş ve günlük toplam olarak görünür.

### Sonraki milestone'a handoff

Eval dataset sürümü, baseline/semantic sonuçları, model adapter contract'ı ve bütçe limitleri.

---

## Milestone 14 — Evidence-backed expert profilleri

### Amaç

Dağınık kişi sinyallerini “bu kişi hangi konuda ve hangi kanıtlarla derin çalışıyor?” sorusuna cevap veren profile dönüştürmek.

### Kullanıcı açısından çıktı

Bir kişinin hangi konularda sürekli üretim yaptığını, hangi Story/repository/kaynakların bunu desteklediğini ve promotional sinyallerden nasıl ayrıldığını görebilirsin.

### Scope

- Kişi evidence'larını topic ve zaman ekseninde toplama
- Evidence türleri: yazarlık, maintainer/release, primary technical source, tekrarlanan konu üretimi
- Evidence-backed expertise summary
- Claim → supporting evidence bağlantısı
- Signal freshness ve tekrar eden üretim görünümü
- Kullanıcının yanlış topic/claim'i düzeltmesi
- Expert profile eval fixture'ları

### Out of Scope

- Tek ve evrensel “expert score”
- Follower sayısını kalite saymak
- Otomatik kişi recommendation sıralaması
- Social graph crawling
- Graph database

### Neden şimdi?

People sinyalleri ve topic modeli yeterince birikmiştir. Recommendation'dan önce profil kalitesini çözmek gerekir; kötü profillerden iyi tavsiye çıkmaz.

### AI Agent Context

Agent Person/Identity/Evidence domain'ini, topic contract'ını, GitHub ve RSS evidence türlerini ve profile eval fixture'larını okumalıdır. Brief ranking veya research orchestration detayı gerekmez.

### Verification

- Her expertise claim en az bir erişilebilir evidence'a bağlıdır.
- Aynı evidence iki kez işlendiğinde ağırlık kazanmaz.
- Kimlik split/merge sonrası profile aggregation tutarlı kalır.
- Eski evidence freshness kuralına göre işaretlenir.
- Golden kişi profilleri beklenen topic/evidence ilişkilerini üretir.

### Project Memory Update

Expertise claim, evidence türü, freshness ve correction semantiği domain belgesine eklenir. Değişebilir scoring ağırlıkları config/test içinde kalır.

### Exit Criteria

- Profildeki her uzmanlık iddiası kanıtlanabilir.
- En az birkaç gerçek kişi için anlamlı topic zaman çizgisi oluşur.
- Kimlik düzeltmeleri veri kaybetmez.
- Profil kullanıcıya “neden takip etmeye değer?” sorusu için somut malzeme verir.

### Sonraki milestone'a handoff

Expertise aggregation contract'ı, desteklenen evidence türleri, profile eval seti ve bilinen boşluklar.

---

## Milestone 15 — Expert discovery ve People ↔ Story çapraz sinyali

### Amaç

Popüler kişi önermek yerine kullanıcının ilgi alanlarına göre takip edilmeye değer insanları evidence ile keşfetmek ve kişi sinyallerini Story öneminde kullanmak.

### Kullanıcı açısından çıktı

“Neden bu kişiyi takip etmeliyim?” kanıtlarıyla aday uzmanlar görebilir; takip ettiğin kaliteli kişilerin aynı gelişme etrafında toplanmasını Story sinyali olarak görebilirsin.

### Scope

- Aday kişi üretimi: Story attribution, repository maintainer'ı ve güvenilir kaynak referansı
- Explainable candidate ranking: topic relevance, evidence depth, süreklilik, bağımsız yüksek kaliteli referanslar
- Tavsiye nedenleri ve supporting evidence
- Follow/dismiss feedback'i
- Birden fazla takip edilen/evidence-backed kişinin aynı Story ile ilişkisini sınırlı önem sinyali yapma
- Diversity ve tekrar kontrolü
- Curated expert-discovery eval seti

### Out of Scope

- “Bu kişiyi takip edenler şunu da takip ediyor” yaklaşımı
- Follower/engagement merkezli sıralama
- Otomatik takip
- Tam relationship graph ürünü
- Source erişimi olmayan platformlarda agresif scraping

### Neden şimdi?

Kişi profilleri, topic'ler, feedback ve yeterli evidence oluşmadan expert recommendation güvenilir olmazdı. Bu milestone Industry ve People Radar'ın birbirini gerçekten beslediği ilk tam döngüdür.

### AI Agent Context

Agent evidence-backed profile contract'ını, personalized topic modelini, Story ranking extension point'ini, recommendation feedback'ini ve expert eval setini okumalıdır. Connector parser iç detayları veya Deep Research prompt'ları gerekmez.

### Verification

- Her recommendation reason gerçek evidence kimliklerine bağlıdır.
- Aynı kişi tekrarlı aday üretiminden sonra tek sonuç olur.
- Dismiss/follow feedback'i sonraki aday listesine deterministik etki eder.
- Diversity sınırları tek topic veya tek kaynak hâkimiyetini önler.
- Eval setinde alakasız/popülerlik-temelli aday oranı kabul sınırının altındadır.

### Project Memory Update

Candidate generation, recommendation reason ve çapraz Story sinyalinin kavramsal anlamı belgelenir. Değişebilir ranking ağırlıkları belgelenmez.

### Exit Criteria

- Tavsiyelerin tamamı evidence ile açıklanabiliyor.
- Yeni ve gerçekten alakalı kişiler keşfedilebiliyor.
- People sinyali Story ranking'ini tek başına domine etmiyor.
- Recommendation feedback'i kalıcı ve replay edilebilir.

### Sonraki milestone'a handoff

Candidate/reason contract'ı, eval sonuçları, feedback döngüsü ve People → Story katkı sınırları.

---

## Risk Checkpoint E — Expert Discovery değeri

Milestone 15 sonrasında şunlar ölçülmelidir:

- Önerilen kişilerden kaçı gerçekten takip listesine giriyor?
- Tavsiyeler yeni insan keşfi mi sağlıyor, bilinen popüler isimleri mi tekrar ediyor?
- Evidence kişi hakkında doğru bir karar vermeye yetiyor mu?
- People sinyali önemli Story'leri daha erken gösteriyor mu?
- Yanlış identity eşleşmeleri kabul edilebilir seviyede mi?

Sonuç zayıfsa graph database veya daha büyük model eklemek çözüm sayılmaz; evidence kalitesi, aday üretimi ve kaynak çeşitliliği düzeltilmelidir.

---

# Phase G — Derin araştırmanın olgunlaşması

## Milestone 16 — Deep Research v1: kaynaklar arası teknik inceleme

### Amaç

Deep Research v0'ın değeri kanıtlandıysa araştırmayı primary source, repository, paper ve teknik tartışmalar arasında kontrollü biçimde derinleştirmek.

### Kullanıcı açısından çıktı

Bir Story, repo, paper veya kavram için teori, implementation, issue/PR tartışmaları, alternatifler ve uzman görüşlerini evidence ile karşılaştıran teknik rapor alabilirsin.

### Scope

- Konuya göre seçilen sınırlı araştırma adımları
- Primary source önceliği
- Uygun olduğunda repository, issue/PR ve paper inceleme adapter'ları
- İddia, evidence, expert opinion ve model inference ayrımı
- Çelişkileri çözmek yerine görünür koruma
- Alternative/prior-art bölümü
- Kaynak kalitesi ve kapsama raporu
- Checkpoint/resume, bütçe ve iptal
- Genişletilmiş research eval dataset'i
- Hermes değerlendirmesi yalnızca v0 sınırları ölçülmüşse ve somut fayda sağlıyorsa

### Out of Scope

- Sınırsız otonom browsing
- Her Story için otomatik research
- Kanıtsız tek “doğru cevap” üretimi
- Karmaşık multi-agent yapı sırf mimari olarak ilginç olduğu için
- Research raporunu doğrulanmış gerçek gibi feed ranking'e doğrudan yazmak

### Neden şimdi?

Ürünün araştırma ihtiyacı, maliyet baseline'ı ve eval seti artık gerçektir. Gelişmiş runtime seçimi spekülasyon yerine ölçülmüş darboğaza dayanabilir.

### AI Agent Context

Agent v0 research contract'ını, hata/maliyet ölçümlerini, genişletilmiş eval setini, kaynak adapter sınırlarını ve varsa runtime ADR'sini okumalıdır. Feed UI, RSS parser veya expert ranking koduna gerek yoktur.

### Verification

- Her önemli claim evidence veya açık inference etiketi taşır.
- Citation hedefi alınmış source snapshot'ında bulunur.
- Repo/paper/issue adapter'ları fixture'larla ayrı test edilir.
- Çelişkili kaynak fixture'ında rapor çelişkiyi korur.
- Eval setinde citation coverage, unsupported claim, primary-source oranı, maliyet ve süre eşikleri sağlanır.
- Job iptal/resume veri bütünlüğünü bozmaz.

### Project Memory Update

Kalıcı research workflow sınırları, yeni evidence türleri ve Hermes/runtime gibi geri dönüşü maliyetli kararlar ADR ile eklenir. Tek tek araştırma planları project memory'ye eklenmez.

### Exit Criteria

- Farklı araştırma türlerinde eval eşikleri karşılanır.
- Rapor theory/implementation/opinion/inference ayrımını korur.
- Bütçe ve süre kontrol edilebilir.
- v0'a göre ölçülebilir kalite kazanımı vardır.
- Runtime karmaşıklığı yalnızca kanıtlanan kazanım kadar artmıştır.

### Sonraki milestone'a handoff

Araştırma capability matrisi, eval sürümü ve sonuçları, bütçe sınırları, kaynak adapter'ları ve kabul edilmiş runtime kararı.

---

# Phase boundaries özeti

| Phase | Milestone'lar | Phase sonunda kanıtlanan şey |
| --- | --- | --- |
| A — İlk kullanılabilir Radar | M1–M3 | Seçilmiş kaynaklardan sonlu günlük brief gerçek kullanım değeri üretiyor mu? |
| B — Üç çekirdek capability | M4–M6 | Story, People ve Deep Research birlikte anlamlı mı? |
| C — Güvenilir işletim | M7–M8 | RSS + GitHub sinyalleri otomatik ve gözlemlenebilir çalışabiliyor mu? |
| D — Open-web discovery | M9–M10 | RADAR manuel kaynakların dışından kontrollü biçimde yeni değer bulabiliyor mu? |
| E — Personal intelligence | M11–M12 | Açık feedback açıklanabilir biçimde brief kalitesini artırıyor mu? |
| F — Semantik ve Expert Discovery | M13–M15 | AI baseline'ı geçiyor mu ve evidence-backed insan keşfi oluşuyor mu? |
| G — Olgun Deep Research | M16 | Derin teknik araştırma ölçülebilir kaliteyle ve sınırlandırılmış maliyetle çalışıyor mu? |

---

# Deferred ideas

Aşağıdakiler değerli olabilir; fakat ilgili risk checkpoint'i geçilmeden roadmap'e alınmamalıdır:

- X, Reddit, YouTube ve Hacker News connector'ları
- İnternetin tamamını indeksleme ve sınırsız recursive crawling
- Sürekli paper ingestion ve citation graph
- E-posta, mobil push veya Slack digest
- Vector database
- Graph database ve relationship explorer
- Learned recommendation model
- Collaborative filtering
- Öğrenilmiş source-quality/trust modeli
- Multi-user/team workspace
- Mobil uygulama
- Browser extension
- Otomatik alert/anomaly detection
- Tam Hermes veya çok-agent'lı research orchestration
- Microservices, queue ve distributed worker altyapısı

Yeni bir connector ancak mevcut brief'te açıkça tanımlanmış bir bilgi açığını kapatıyorsa eklenmelidir. “Daha çok kaynak daha iyi Radar” varsayımı kabul edilmemelidir.

---

# Context strategy summary

## 1. Repository project memory yapısı

Repository başladığında minimum yapı şu sorumlulukları taşımalıdır:

- `AGENTS.md`: agent'ın çalışma kuralları, doğrulama zorunlulukları ve belge okuma yönlendirmesi
- `docs/PRODUCT.md`: ürün amacı, kullanıcı değeri ve kapsam sınırları
- `docs/DOMAIN.md`: yalnızca kalıcı kavramlar ve invariants
- `docs/DISCOVERY.md`: open-web discovery, aday/source lifecycle ve exploration sınırları
- `docs/ARCHITECTURE.md`: yüksek seviyeli sistem sınırları ve repository haritası
- `docs/architecture/decisions/`: pahalı veya geri dönüşü zor kararlar
- `docs/features/`: yalnızca aktif ve non-trivial milestone'un geçici feature specification'ı

Tamamlanan milestone spec'i kalıcı bilgi için arşiv yapılmamalıdır. Gerekli gerçekler PRODUCT/DOMAIN/ARCHITECTURE/ADR'ye aktarılır; geri kalanı testler, kod ve Git geçmişinde kalır. Aktarım sonrası geçici spec silinir ve yeni milestone spec'i oluşturulur.

## 2. Her milestone için agent input paketi

Agent'a bütün proje belgeleri verilmez. Varsayılan paket:

1. `AGENTS.md`
2. Aktif `docs/features/<feature-slug>.md`
3. Spec'in açıkça işaret ettiği PRODUCT/DOMAIN/ARCHITECTURE bölümleri
4. Discovery görevi ise yalnızca ilgili `DISCOVERY.md` bölümleri
5. Yalnızca ilgili ADR'ler
6. Değişecek modüller ve yakın testler
7. Verification komutları

Spec içinde ayrıca **“okunacak context”** ve **“okunmayacak context”** listeleri bulunmalıdır. Agent repository'nin tamamını analiz ederek işe başlamamalıdır.

## 3. Task decomposition

Milestone içinde agent gerektiğinde şu sırayla çalışır:

1. Mevcut contract ve testleri oku.
2. Acceptance testlerini veya fixture'ları ekle.
3. En küçük dikey implementation'ı yap.
4. Otomatik verification'ı çalıştır.
5. Kapsam dışı değişiklikleri geri çıkar.
6. Project memory gereksinimini değerlendir.
7. Durable owner belgelerini reconcile et ve tamamlanan geçici feature spec'ini sil.

Tek milestone yeni bir bağımsız subsystem, birden fazla belirsiz domain kararı ve geniş UI yeniden tasarımı gerektiriyorsa büyüktür; yeniden bölünmelidir.

## 4. ADR kullanımı

ADR yalnızca şu durumlarda oluşturulmalıdır:

- geri dönüşü maliyetli teknoloji veya data-model kararı,
- iki makul alternatif arasında kalıcı seçim,
- güvenlik, maliyet veya operasyon sınırını değiştiren karar,
- sonraki agent'ın “neden böyle?” diye haklı olarak soracağı karar.

Kütüphane seçimi, küçük refactor veya her implementation detayı için ADR yazılmaz.

## 5. Tests ve evals

- Deterministik kod: unit, integration, contract, property ve E2E testleri
- Ingestion: sabit fixture + idempotency + checkpoint testleri
- Discovery: golden candidate set + source-lifecycle replay + gürültü/maliyet eşikleri
- Story/identity eşleme: golden datasets
- Ranking: snapshot/replay dataset ve baseline karşılaştırması
- AI özellikleri: versioned eval dataset + kalite/maliyet/latency eşikleri
- Deep Research: citation coverage, unsupported-claim ve primary-source oranları

Üreten agent kendi testlerini çalıştırır; sen milestone kontrolünde aynı tek doğrulama komutunu temiz ortamda tekrar çalıştırırsın. Kritik veya AI ağırlıklı milestone'larda implementation context'i verilmeyen ayrı bir review oturumuna yalnızca spec, diff ve test/eval sonuçları verilebilir.

## 6. Clean handoff

Her milestone sonunda handoff şu beş başlığı içermelidir; bu bilgi için ayrı bir
durum dosyası oluşturulmaz:

1. Çalışan capability
2. Ana entry point'ler
3. Verification komutları ve son sonuç
4. Bilinen sınırlar/borçlar
5. Sıradaki milestone ve gerekli context linkleri

Conversation özeti handoff sayılmaz. Handoff repository'ye commit edilmeden milestone tamamlanmış kabul edilmez.

## 7. Stale context prevention

- Aynı bilgi birden çok belgede kopyalanmaz; tek owner dosyası olur.
- Aktif milestone bitince geçici spec silinir.
- Değişmiş contract'ı temsil eden eski golden fixture güncellenmeden test geçemez.
- Kodda doğrulanabilen ayrıntı documentation'a kopyalanmaz.
- Belge linkleri ve referans verilen dosya/komutlar CI ile doğrulanır.
- Her milestone başında ilgili dokümanların kod/test ile hâlâ uyumlu olup olmadığı kontrol edilir.

---

# Milestone kabul rutini

Her milestone sonunda senin kontrolün şu sırada olmalıdır:

1. **Temiz doğrulama:** Agent'ın verdiği tek verification komutunu temiz ortamda çalıştır.
2. **Exit Criteria:** Maddeleri tek tek PASS/FAIL olarak işaretle.
3. **Ürün kontrolü:** “Kullanıcı açısından çıktı” bölümündeki akışı kendin uygula.
4. **Scope kontrolü:** Out of Scope maddelerinin yanlışlıkla eklenmediğini diff üzerinden kontrol et.
5. **Project memory kontrolü:** Sadece kalıcı kararların taşındığını ve aktif feature spec'inin tamamlanınca silindiğini kontrol et.
6. **Risk checkpoint:** İlgili phase gate geldiyse gerçek kullanım yapmadan sonraki phase'e geçme.

Bu altı adım geçmeden sonraki milestone agent'a verilmemelidir.
