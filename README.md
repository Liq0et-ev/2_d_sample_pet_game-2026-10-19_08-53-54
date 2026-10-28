# Gudrais suns

Mierīga 2D un 3D suņu aprūpes, apmācības un menedžmenta spēle (Unity 6000.6.2f1, C#, top-down).
Grupa 9, rudens semestris 2026./2027. – 1. praktiskais darbs.

Spēlētājs saņem suņus no dzīvnieku labklājības centra, rūpējas par tiem, apmāca komandas un sagatavo adopcijai. Mērķis – 10 veiksmīgas adopcijas; pēc tam var turpināt brīvajā režīmā.

## Kā palaist (uz cita datora)

**Vajag:** Windows dators, [Unity Hub](https://unity.com/download) ar **Unity 6000.6.2f1** (precīza versija, citādi projekts var pieprasīt atjaunināšanu), [Git](https://git-scm.com/) un [Git LFS](https://git-lfs.com/) (repozitorijā lielie faili, piemēram, modeļi un tekstūras, ir glabāti ar LFS).

1. Instalē Git LFS vienu reizi: `git lfs install`
2. Lejupielādē projektu:
   ```
   git clone https://github.com/Liq0et-ev/2_d_sample_pet_game-2026-10-19_08-53-54.git
   ```
   (Ja projektu ielādē kā ZIP no GitHub, LFS faili nebūs pilni – labāk izmanto `git clone`.)
3. Unity Hub → **Add → Add project from disk** → izvēlies klonēto mapi. Ja Hub prasa Unity versiju, izvēlies 6000.6.2f1 (to var instalēt tajā pašā Hub).
4. Pirmā atvēršana ir lēna (vairākas minūtes): Unity lejupielādē pakotnes un ģenerē mapi `Library`. Vajadzīgs internets. Ja parādās drošā režīma (Safe Mode) logs, izvēlies *Ignore*, ja kļūdu nav.
5. Atver vienu no divām ainām un nospied **Play**:
   - `Assets/Scenes/GudraisSuns` – **2D** versija (vai izvēlne **Gudrais suns → Open game scene (2D)**)
   - `Assets/Scenes/GudraisSuns3D` – **3D** versija (vai izvēlne **Gudrais suns → Open game scene (3D)**)

   Ainas tiek izveidotas automātiski pirmajā atvēršanas reizē. Ja Project logā tās nav, izmanto izvēlni **Gudrais suns** augšā.

**Problēmas:**
- *Modeļi nav redzami / rozā*: LFS faili nav lejupielādēti. Palaid `git lfs pull` projekta mapē.
- *Kompilācijas kļūdas Console logā*: pārbaudi, vai izmanto Unity 6000.6.2f1, un izvēlnē **Assets → Reimport All**.
- *Pele neklikšķina uz pogām*: spēle izmanto jauno Input System; pārbaudi, ka Project Settings → Player → Active Input Handling ir *Input System Package (New)*.

**Gatava .exe versija (netestēta):** Unity → **File → Build Profiles** → Windows → ainu sarakstā atstāj tikai vajadzīgo (2D: `GudraisSuns`, 3D: `GudraisSuns3D`) → **Build**. Uzmanību: spēle šeiderus atrod pēc nosaukuma, un tie var netikt iekļauti būvējumā (objekti var kļūt rozā). Drošākais veids spēlēt ir palaist no Unity ar **Play**.

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
| `Apgaismojums2D.cs` | 2D gaisma: diena/nakts, laterna, lampas pie ēkām, normālkarte apaļajiem objektiem |
| `GameBootstrap3D.cs`, `Maksla3D.cs` | 3D versija: karte, tēli, suns no 3D formām |
| `PlayerMovement3D.cs`, `CameraFollow3D.cs`, `DogFollow3D.cs`, `SunaAnimacija3D.cs` | 3D kustība, kamera, suņa sekošana un animācijas |
| `Modeli3D.cs`, `ModelaAnimacijas.cs` | Gatavo 3D modeļu ielāde (izmērs, pagrieziens, materiāli) un to animācijas (suns, spēlētājs) |
| `InteractionController.cs` | E mijiedarbība |
| `DogStatus.cs`, `DogProgress.cs`, `PlayerProgress.cs` | Vajadzības, prasmes, līmeņi |
| `CareSystem.cs` | Māja, parks, klīnika |
| `TrainingSystem.cs` | Komandas un aportēšanas minispēle |
| `NujasSpele.cs` | Parka minispēle: nūjas mešana (5 mēģinājumi, griežas adata) |
| `ShopSystem.cs`, `EconomyManager.cs` | Pirkumi, monētas, pasīvie ienākumi |
| `WelfareAndAdoption.cs` | Suņu izsniegšana, pārbaudes, adopcija |
| `UIManager.cs`, `AudioManager.cs` | HUD, izvēlnes, minikarte, ģenerētas skaņas |
| `Maksla.cs`, `Pasaule.cs` | Procedurālā grafika, kartes dati un sadursmes |

## Spēles sajūta (2D) un gaisma

Pēc tehniskās dokumentācijas ieviesti tie uzlabojumi, kas der šai spēlei (skats no augšas):

| Uzlabojums | Kur |
|---|---|
| Plūdens paātrinājums un bremzēšana (nav pēkšņu apstāšanos) | `PlayerMovement.cs`, `DogFollow.cs` |
| Kameras trīcēšana (screen shake): līmeņa kāpums, komandas apguve, adopcija, slimība, sods, perfekts metiens | `CameraFollow.cs` (2D), `CameraFollow3D.cs` (3D) |
| Trieciena pauze (hit stop): īsa pauze svarīgos brīžos | `UIManager.ApstadinatLaiku` |
| Suņa nomirgošana krāsā (zaļa = labi, sarkana = slikti, dzeltena = ļoti labi) | `SunaIzskats.Uzplaiksnit` |
| Sadursmju robežas atdalītas no zīmētā izskata (mazāks taisnstūris pie kājām) | `Pasaule.cs`, `PlayerMovement.cs` |
| 2D gaisma: diena un nakts (viena diena ~6 min), laterna pie spēlētāja, lampas pie ēkām naktī | `Apgaismojums2D.cs` |
| Normālkartes apaļajiem objektiem (koku lapotnēm, sunim, strūklakai), lai tie izskatītos apjomīgi pret lampu gaismu | `Apgaismojums2D.cs`, `Maksla.cs` |

Nav ieviests (neder spēlei ar skatu no augšas bez lēkšanas): Coyote Time, Jump Buffering un paralakses fona slāņi.

## 3D modeļi

3D versijā izmantoti gatavi bezmaksas modeļi (mape `Assets/Resources/Modeli`). Ja kāds modelis netiek atrasts, spēle automātiski izmanto vienkāršās formas, tāpēc tā vienmēr darbojas.

| Ko izmanto | Modeļi | Autors un licence |
|---|---|---|
| Suns | `Dog.fbx` (animācijas: stāv, iet) | [Quaternius – Animal Pack Vol.2](https://opengameart.org/content/animated-animales-low-poly), CC0 |
| Spēlētājs | `character-a.fbx` (animācijas: stāv, iet, skrien u.c.) | [Kenney – Blocky Characters](https://opengameart.org/content/blocky-characters), CC0 |
| Ēkas | `building-type-e/f/g/h.fbx` | [Kenney – City Kit (Suburban)](https://opengameart.org/content/city-kit-suburban), CC0 |
| Koki, ziedi, krūmi, akmeņi, žogs, zīme | `tree_*`, `flower_*`, `plant_*`, `rock_*`, `fence_simple`, `sign` | [Kenney – Nature Kit](https://opengameart.org/content/nature-kit), CC0 |

Visi modeļi ir CC0 (publiskais īpašums), norādīšana nav obligāta, bet paldies Kenney un Quaternius!

Piezīmes:
- Sunim ir tikai divas gatavas animācijas (stāv un iet). Sēdēšanu, guļēšanu, ēšanu un lēkāšanu veido kods (suni pagriežot un saspiežot).
- Modeļi tiek automātiski pielāgoti izmēram. Suņa skatiena virzienu kods nosaka pats. Ja spēlētāja modelis ir pagriezts uz nepareizo pusi, maini `SpeletajaPagrieziens` failā `Modeli3D.cs`.
- Projekts izmanto URP 2D renderētāju; 3D ainas apgaismojums tāpēc ir vienkāršāks nekā 3D renderētājā.

## Zināmie ierobežojumi

- Grafika ir vienkāršas krāsainas formas (vieta īstajiem sprite un 3D modeļiem).
- Viens aktīvais suns; nav saglabāšanas.
- Atlīdzības un vajadzību izmaiņu ātrumi ir sākotnēji, jālīdzsvaro testēšanā.
