# Gudrais suns

Mierīga 2D un 3D suņu aprūpes, apmācības un menedžmenta spēle (Unity 6000.6.2f1, C#, top-down).
Grupa 9, rudens semestris 2026./2027. – 1. praktiskais darbs.

Spēlētājs saņem suņus no dzīvnieku labklājības centra, rūpējas par tiem, apmāca komandas un sagatavo adopcijai. Mērķis – 10 veiksmīgas adopcijas; pēc tam var turpināt brīvajā režīmā.

## Kā palaist

1. Atver projektu mapi Unity 6000.6.2f1.
2. Atver vienu no divām ainām un nospied **Play**:
   - `Assets/Scenes/GudraisSuns` – **2D** versija (vai izvēlne **Gudrais suns → Open game scene (2D)**)
   - `Assets/Scenes/GudraisSuns3D` – **3D** versija (vai izvēlne **Gudrais suns → Open game scene (3D)**)

Citas ainas (piem., SampleScene demo) netiek ietekmētas.

Abām versijām ir tā pati spēles loģika (suns, veikals, treniņš, adopcija, UI, skaņa); atšķiras tikai grafika, kustība, kamera un animācijas. 3D versijā kamera seko spēlētājam no augšas un aizmugures, un viss ir uzbūvēts no Unity gatavajām formām.

Spēle visu izveido pati no koda (karte, tēli, UI, skaņas) – nav jāpievieno objekti vai grafikas faili. Izmanto jauno Input System.

## Vadība

| Taustiņš | Darbība |
|---|---|
| WASD / bultiņas | Pārvietot tēlu (suns seko) |
| E | Mijiedarbība ar ēkām |
| Pele | Izvēlnes, pirkumi, komandas |
| Space | Aportēšanas minispēle: "Mest!" |
| Esc | Pauze / aizvērt izvēlni |

## Spēles norise

1. **Labklājības centrs** (ZR) – saņem suni.
2. **Māja** (DR) – pabaro suni, ļauj atpūsties, uzlabo māju.
3. **Treniņu laukums** (augšā vidū) – trenē komandas *Sēdi, Guli, Pie manis, Gaidi*; minispēle *Aportēšana*; laukuma uzlabojumi.
4. **Parks** (ZA) – pastaigas un rotaļas (garastāvoklis).
5. **Veikals** (D vidū) – barība, gardumi, rotaļlieta, pavada, konusi.
6. **Klīnika** (DA) – slima suņa ārstēšana, apskate.
7. Kad suns zina 4 komandas un ir labā stāvoklī, **Labklājības centrā** nokārto adopcijas pārbaudījumu (jāatkārto komandu secība) un saņem atlīdzību.

Suņa rādītāji: veselība, izsalkums, enerģija, garastāvoklis. Nogurums sodu nerada, bet nolaidība (bads, ļoti zema veselība/garastāvoklis) izraisa brīdinājumu, tad naudas sodu.

## Skripti (`Assets/Scripts`)

Kods ir rakstīts vienkāršā veidā: mainīgo, funkciju un palīgklašu nosaukumi ir latviski, komentāri – latviski. Tikai Unity pašu funkcijas (`Update`, `Start`, `Awake`...) paliek angliski. Galveno skriptu nosaukumi sakrīt ar prezentācijas 28. slaidu.

| Fails | Loma |
|---|---|
| `GameBootstrap.cs` | Izveido visu spēli un karti, startē ar Play |
| `GameManager.cs` | Centrālā piekļuve sistēmām (`GameManager.Galvenais`), uzdevumu teksts, komandu enum |
| `PlayerMovement.cs`, `CameraFollow.cs`, `DogFollow.cs` | Kustība, kamera, suņa sekošana |
| `SunaAnimacija.cs` | Suņa animācijas ar kodu: skriešana, elpošana, sēdēšana, guļēšana, ēšana, lēkāšana, slimības poza |
| `GameBootstrap3D.cs`, `Maksla3D.cs` | 3D versija: karte, tēli, suns no 3D formām |
| `PlayerMovement3D.cs`, `CameraFollow3D.cs`, `DogFollow3D.cs`, `SunaAnimacija3D.cs` | 3D kustība, kamera, suņa sekošana un animācijas |
| `InteractionController.cs` | E mijiedarbība |
| `DogStatus.cs`, `DogProgress.cs`, `PlayerProgress.cs` | Vajadzības, prasmes, līmeņi |
| `CareSystem.cs` | Māja, parks, klīnika |
| `TrainingSystem.cs` | Komandas un aportēšanas minispēle |
| `NujasSpele.cs` | Parka minispēle: nūjas mešana (5 mēģinājumi, griežas adata) |
| `ShopSystem.cs`, `EconomyManager.cs` | Pirkumi, monētas, pasīvie ienākumi |
| `WelfareAndAdoption.cs` | Suņu izsniegšana, pārbaudes, adopcija |
| `UIManager.cs`, `AudioManager.cs` | HUD, izvēlnes, minikarte, ģenerētas skaņas |
| `Maksla.cs`, `Pasaule.cs` | Procedurālā grafika, kartes dati un sadursmes |

## Zināmie ierobežojumi

- Grafika ir vienkāršas krāsainas formas (vieta īstajiem sprite un 3D modeļiem).
- Viens aktīvais suns; nav saglabāšanas.
- Atlīdzības un vajadzību izmaiņu ātrumi ir sākotnēji, jālīdzsvaro testēšanā.
