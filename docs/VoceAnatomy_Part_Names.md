# VoceAnatomy: What You Can Show and Isolate

Every name in this file is a 3D object (or a group of objects) in the VoceAnatomy body model. Any of them can be used in a macro with `isolate`, `show`, `hide` and `focus`.

The list was generated from the model itself (`Assets/Prefabs/@Human Body.prefab` and its English names in `Assets/Resources/Translations0.txt`). If a structure isn't listed here, the model doesn't have it.

---

## How to use these names in a macro

| You write | What happens |
|---|---|
| `isolate heart, coronary arteries; focus` | Shows **only** these and hides everything else, then frames them |
| `show thoracic aorta` | Adds it to what's already visible and highlights it |
| `hide pleura` | Hides it |
| `focus ascending aorta` | Frames the camera on it (and selects it) |
| `isolate lungs, pleura, skeleton` | Body systems can be mixed in: `skeleton`, `muscles`, `nerves`, `organs`, `blood vessels`, `lymph`, `joints` |

**Rules worth knowing**

1. **A name includes everything indented under it.** In the lists below, `Superior vena cava` has the azygos and brachiocephalic veins under it, so `show superior vena cava` shows all of them. To show only a branch, name the branch (`show azygos vein`).
2. **Left and right.** Entries marked *(left & right)* exist on both sides. Add `left` or `right` to get one side (`show left internal thoracic artery`). With no side, you get both.
3. **Capitals don't matter**, and any of the *also:* names works too (for example `LAD`, `tricuspid valve`, `esophagus`, `arch of aorta`).
4. **Use the full name when two things are alike.** `show ventricle` names several parts, so nothing happens and the console lists them. Write `left ventricle`.
5. **Test with `check <macro name>`.** It reports any name that the model doesn't recognise before you present.

**Named teaching groups** (defined in `Assets/StreamingAssets/anatomy/groups.json`, so more can be added):

| Group name | Contains |
|---|---|
| `great vessels` | ascending aorta, aortic arch, thoracic aorta, pulmonary trunk, left and right pulmonary arteries, pulmonary veins, superior vena cava, inferior vena cava (thoracic part) |
| `coronary arteries` | arteries of heart (left and right coronary arteries and their branches) |
| `mediastinum` | heart, great vessels, trachea, oesophagus, thymus |
| `heart chambers` | left and right atria, left and right ventricles |

---

## Chest quick reference (heart, aorta, pulmonary)

The structures most useful for the chest pain tour. Each name below is also in the full list, where you can see what's under it.

### Heart

| Name to use | Notes |
|---|---|
| `Heart` | The four chambers plus the valves. Does **not** include the coronary arteries or cardiac veins. |
| `Left atrium`, `Right atrium` | |
| `Left ventricle`, `Right ventricle` | Each includes its papillary muscles |
| `Inferior papillary muscle of left ventricle` | The only LV papillary muscle in the model |
| `Anterior / Inferior / Septal papillary muscle of right ventricle` | Write the full name of each, e.g. `Septal papillary muscle of right ventricle` |
| `Valvular complex of heart` | All the valves together |
| `Right atrioventricular valve` (also `Tricuspid valve`) | ⚠ Also contains the model's single mitral leaflet (see the gaps below) |
| `Root of aorta` (also `Aortic bulb`) | Aortic root, including the aortic valve |
| `Aortic valve` | `Left coronary leaflet`, `Right coronary leaflet`, `Non-coronary leaflet` |
| `Root of pulmonary trunk` / `Pulmonary valve` | Anterior, left and right semilunar leaflets |

### Coronary arteries and cardiac veins

| Name to use | Notes |
|---|---|
| `Arteries of heart` (also `coronary arteries`) | The whole coronary tree |
| `Left coronary artery` | Includes the LAD and circumflex |
| `Anterior interventricular artery` (also `LAD`, `Left anterior descending artery`) | Includes its septal branches |
| `Septal branches of anterior interventricular artery` | |
| `Circumflex artery of heart` | |
| `Right coronary artery` | Includes the branch below |
| `Right inferolateral branch of right coronary artery` (also `Right posterolateral branch…`) | |
| `Cardiac veins` | `Coronary sinus`, `Great cardiac vein`, `Middle cardiac vein`, `Inferior vein of left ventricle` |

### Aorta and arch branches

| Name to use | Notes |
|---|---|
| `Aorta` | The **whole** aorta, down to the iliac arteries |
| `Ascending aorta` | |
| `Aortic arch` (also `Arch of aorta`) | Includes the brachiocephalic trunk |
| `Brachiocephalic trunk` | |
| `Left common carotid artery`, `Right common carotid artery` | Each includes its whole head and neck tree |
| `Left subclavian artery`, `Right subclavian artery` | Each includes the arm arteries and the internal thoracic artery |
| `Thoracic aorta` (also `Descending thoracic aorta`) | Includes posterior intercostal, subcostal and superior phrenic arteries |
| `Posterior intercostal arteries` | |
| `Internal thoracic artery` | Left and right |
| `Abdominal aorta` | For contrast below the diaphragm |

### Pulmonary circulation

| Name to use | Notes |
|---|---|
| `Pulmonary arteries` | Trunk plus both pulmonary arteries and all their lobar and segmental branches |
| `Pulmonary trunk` | Includes `Bifurcation of pulmonary trunk` |
| `Left pulmonary artery`, `Right pulmonary artery` | Each includes its lobar and segmental arteries |
| `Superior lobar artery of left lung`, `Inferior lobar artery of left lung` | |
| `Superior / Middle / Inferior lobar artery of right lung` | |
| `Pulmonary veins` | All four veins with their tributaries |
| `Left superior pulmonary vein`, `Left inferior pulmonary vein`, `Right superior pulmonary vein`, `Right inferior pulmonary vein` | |

### Systemic veins of the chest

| Name to use | Notes |
|---|---|
| `Superior vena cava` | ⚠ Includes the azygos system and both brachiocephalic veins |
| `Left brachiocephalic vein`, `Right brachiocephalic vein` | |
| `Azygos vein`, `Hemi-azygos vein`, `Accessory hemi-azygos vein` | |
| `Inferior vena cava (thoracic part)` | ⚠ Includes the hepatic veins and the abdominal IVC |
| `Inferior vena cava (abdominal part)` | |

### Lungs, airways and pleura

| Name to use | Notes |
|---|---|
| `Lungs`, `Left lung`, `Right lung` | |
| `Superior lobe of left lung`, `Inferior lobe of left lung` | |
| `Superior / Middle / Inferior lobe of right lung` | |
| `Pleura` | One object; the model doesn't separate parietal and visceral pleura |
| `Trachea` | |
| `Tracheobronchial tree`, `Bronchi` | |
| `Left main bronchus`, `Right main bronchus` | Each includes its lobar and segmental bronchi |
| Segmental bronchi | e.g. `B6`, `Superior segmental bronchus of right lung (BVI)`; see the full list |

### Other mediastinal and chest wall structures

| Name to use | Notes |
|---|---|
| `Oesophagus` (also `Esophagus`) | |
| `Thymus`, `Left lobe of thymus`, `Right lobe of thymus` | |
| `Mediastinal lymph nodes` | Includes the tracheobronchial, prepericardial and lateral pericardial nodes |
| `Vagus nerve (X)` | Left and right |
| `Sympathetic trunk` | Left and right |
| `Intercostal nerves` | |
| `Diaphragm` | |
| `Sternum`, `Manubrium of sternum`, `Body of sternum`, `Ribs`, `Costal cartilages` | |
| `Thoracic skeleton`, `Thoracic vertebrae` | |
| `External / Internal / Innermost intercostal muscles` | |

### ⚠ Not in the model (describe these in the captions instead)

- **Pericardium**: the model has no sac, and there's no single object called "mediastinum" (use the `mediastinum` teaching group).
- **Mitral valve**: the model has only one mitral leaflet, `Posterior leaflet of left atrioventricular valve`, and it is filed under the tricuspid valve. `show tricuspid valve` therefore shows it as well. To show it alone, name it in full.
- **Posterior descending artery**, **right marginal branch**, **obtuse marginal** and **diagonal branches**, and the **SA/AV nodal arteries** are not separate objects.
- **`Marginal artery`** is the colon's marginal artery, not a cardiac one.
- **Phrenic nerves**, **recurrent laryngeal nerves**, and the **ligamentum arteriosum** / ductus arteriosus.
- **Grooves and landmarks** (interventricular sulci, coronary sulcus, apex, borders and surfaces of the heart) are labels or lines, not objects, so they can't be shown on their own.

---

# Full list of everything in the model

Organised by body system, in the same tree as the model. Indented names are part of the name above them.

## Skeleton

Whole system: `Skeletal system` — also: Skeleton, Osteology


### Auditory ossicles

- `Auditory ossicles`
  - `Incus` *(left & right)*
  - `Malleus` *(left & right)*
  - `Stapes` *(left & right)*

### Bones of lower limb

- `Bones of lower limb`
  - `Bones of free part of lower limb`
    - `Bones of foot`
      - `Metatarsal bones` — also: Metatarsals 1-5
        - `Fifth metatarsal bone` *(left & right)*
        - `First metatarsal bone` *(left & right)*
        - `Fourth metatarsal bone` *(left & right)*
        - `Second metatarsal bone` *(left & right)*
        - `Third metatarsal bone` *(left & right)*
      - `Phalanges of foot` — also: Phalangeal bones of foot
        - `Distal phalanx of fifth finger of foot` *(left & right)*
        - `Distal phalanx of first finger of foot` *(left & right)*
        - `Distal phalanx of fourth finger of foot` *(left & right)*
        - `Distal phalanx of second finger of foot` *(left & right)*
        - `Distal phalanx of third finger of foot` *(left & right)*
        - `Middle phalanx of fifth finger of foot` *(left & right)*
        - `Middle phalanx of fourth finger of foot` *(left & right)*
        - `Middle phalanx of second finger of foot` *(left & right)*
        - `Middle phalanx of third finger of foot` *(left & right)*
        - `Proximal phalanx of fifth finger of foot` *(left & right)*
        - `Proximal phalanx of first finger of foot` *(left & right)*
        - `Proximal phalanx of fourth finger of foot` *(left & right)*
        - `Proximal phalanx of second finger of foot` *(left & right)*
        - `Proximal phalanx of third finger of foot` *(left & right)*
      - `Sesamoid bones of foot` *(left & right)*
      - `Tarsal bones`
        - `Calcaneus` *(left & right)* — also: Calcaneal bone
        - `Cuboid bone` *(left & right)*
        - `Intermediate cuneiform bone` *(left & right)* — also: Middle cuneiform, Second cuneiform
        - `Lateral cuneiform bone` *(left & right)* — also: Third cuneiform
        - `Medial cuneiform bone` *(left & right)* — also: First cuneiform
        - `Navicular bone` *(left & right)*
        - `Talus` *(left & right)* — also: Astragalus, Talus bone, Talar bone, Ankle bone
    - `Femur` *(left & right)* — also: Thigh bone, Femoral bone
    - `Fibula` *(left & right)*
    - `Patella` *(left & right)* — also: Knee bone, Patellar bone
    - `Tibia` *(left & right)* — also: Shin bone, Tibial bone
  - `Bones of pelvic girdle` — also: Bones of lower limb girdle
    - `Hip bone` *(left & right)* — also: Coxal bone, Pelvic bone, Innominate bone

### Bones of upper limb

- `Bones of upper limb`
  - `Bones of free part of upper limb`
    - `Capitate bone` *(left & right)*
    - `Distal phalanx of fifth finger of hand` *(left & right)*
    - `Distal phalanx of first finger of hand` *(left & right)*
    - `Distal phalanx of fourth finger of hand` *(left & right)*
    - `Distal phalanx of second finger of hand` *(left & right)*
    - `Distal phalanx of third finger of hand` *(left & right)*
    - `Fifth metacarpal bone` *(left & right)* — also: Metacarpal 5
    - `First metacarpal bone` *(left & right)* — also: Metacarpal 1
    - `Fourth metacarpal bone` *(left & right)* — also: Metacarpal 4
    - `Hamate bone` *(left & right)*
    - `Humerus` *(left & right)* — also: Humeral bone
    - `Lunate bone` *(left & right)* — also: Semilunar bone
    - `Middle phalanx of fifth finger of hand` *(left & right)*
    - `Middle phalanx of fourth finger of hand` *(left & right)*
    - `Middle phalanx of second finger of hand` *(left & right)*
    - `Middle phalanx of third finger of hand` *(left & right)*
    - `Pisiform bone` *(left & right)*
    - `Proximal phalanx of fifth finger of hand` *(left & right)*
    - `Proximal phalanx of first finger of hand` *(left & right)*
    - `Proximal phalanx of fourth finger of hand` *(left & right)*
    - `Proximal phalanx of second finger of hand` *(left & right)*
    - `Proximal phalanx of third finger of hand` *(left & right)*
    - `Radius` *(left & right)* — also: Radial bone
    - `Scaphoid bone` *(left & right)* — also: Navicular bone of hand
    - `Second metacarpal bone` *(left & right)* — also: Metacarpal 2
    - `Third metacarpal bone` *(left & right)* — also: Metacarpal 3
    - `Trapezium bone` *(left & right)* — also: Greater multangular bone
    - `Trapezoid bone` *(left & right)*
    - `Triquetrum bone` *(left & right)*
    - `Ulna` *(left & right)* — also: Ulnar bone
  - `Bones of pectoral girdle` — also: Bones of shoulder girdle, Bones of upper limb girdle
    - `Clavicle` *(left & right)* — also: Clavicular bone
    - `Scapula` *(left & right)*

### Cranium

- `Cranium` — also: Skull
  - `Bones of cranium` — also: Bones of skull
    - `Ethmoid bone` — also: Ethmoid, Ethmoidal bone
      - `Anterior cells of ethmoid bone` *(left & right)* — also: Bony anterior ethmoidal cells, Anterior ethmoidal sinus
      - `Middle cells of ethmoid bone` *(left & right)* — also: Bony middle ethmoidal cells
      - `Posterior cells of ethmoid bone` *(left & right)* — also: Bony posterior ethmoidal cells
    - `Frontal bone`
      - `Sinus of frontal bone` — also: Bony frontal sinus
    - `Inferior nasal concha bone` *(left & right)* — also: Inferior nasal turbinate, inferior nasal concha, Inferior nasal concha
    - `Lacrimal bone` *(left & right)*
    - `Maxilla` *(left & right)* — also: Maxillary bone
    - `Nasal bone` *(left & right)*
    - `Occipital bone`
    - `Palatine bone` *(left & right)*
    - `Parietal bone` *(left & right)*
    - `Sphenoid bone` — also: Sphenoid, Sphenoidal bone
      - `Sinus of sphenoid bone` — also: Bony sphenoidal sinus, Sphenoid sinus
    - `Temporal bone` *(left & right)*
    - `Vomer` — also: Vomeral bone, Vomer bone
    - `Zygomatic bone` *(left & right)* — also: Zygoma, Malar bone

### Extracranial bones of head

- `Extracranial bones of head` — also: Other bones of head
  - `Hyoid bone` — also: Lingual bone
  - `Mandible` — also: Mandibular bone, Mandibular bone

### Laryngeal cartilages

- `Laryngeal cartilages`
  - `Arytenoid cartilage` *(left & right)*
  - `Corniculate cartilage` *(left & right)* — also: Santorini's cartilage
  - `Cricoid cartilage`
  - `Thyroid cartilage`

### Nasal cartilages

- `Nasal cartilages`
  - `Lateral process of nasal septal cartilage` *(left & right)* — also: Lateral process of septal nasal cartilage
  - `Major alar cartilage` *(left & right)*
  - `Nasal septal cartilage` — also: Septal nasal cartilage

### Teeth

- `Teeth`
  - `Anterior teeth`
    - `Lower canine` *(left & right)*
    - `Lower lateral incisor` *(left & right)*
    - `Lower medial incisor` *(left & right)*
    - `Upper canine` *(left & right)*
    - `Upper lateral incisor` *(left & right)*
    - `Upper medial incisor` *(left & right)*
  - `Posterior teeth`
    - `Lower first molar tooth` *(left & right)*
    - `Lower first premolar` *(left & right)*
    - `Lower second molar tooth` *(left & right)*
    - `Lower second premolar` *(left & right)*
    - `Upper first molar tooth` *(left & right)*
    - `Upper first premolar` *(left & right)*
    - `Upper second molar tooth` *(left & right)*
    - `Upper second premolar` *(left & right)*

### Thoracic skeleton

- `Thoracic skeleton` — also: Thoracic cage, Rib cage, Bony thorax
  - `Bones of thorax`
    - `Ribs` — also: Ribs 1-12
      - `False ribs` — also: Rib 8-12
        - `Eighth rib` *(left & right)* — also: Rib 8
        - `Floating ribs` — also: Ribs 11-12
          - `Eleventh rib` *(left & right)* — also: Rib 11
          - `Twelfth rib` *(left & right)* — also: Rib 12
        - `Ninth rib` *(left & right)* — also: Rib 9
        - `Tenth rib` *(left & right)* — also: Rib 10
      - `True ribs` — also: Ribs 1-7
        - `Fifth rib` *(left & right)* — also: Rib 5
        - `First rib` *(left & right)* — also: Rib 1
        - `Fourth rib` *(left & right)* — also: Rib 4
        - `Second rib` *(left & right)* — also: Rib 2, Second rib bone
        - `Seventh rib` *(left & right)* — also: Rib 7
        - `Sixth rib` *(left & right)* — also: Rib 6
        - `Third rib` *(left & right)* — also: Rib 3
    - `Sternum`
      - `Body of sternum`
      - `Manubrium of sternum`
      - `Xiphoid process`
  - `Costal cartilages`
    - `Costal cartilage of eighth rib` *(left & right)*
    - `Costal cartilage of fifth rib` *(left & right)*
    - `Costal cartilage of first rib` *(left & right)*
    - `Costal cartilage of fourth rib` *(left & right)*
    - `Costal cartilage of ninth rib` *(left & right)*
    - `Costal cartilage of second rib` *(left & right)*
    - `Costal cartilage of seventh rib` *(left & right)*
    - `Costal cartilage of sixth rib` *(left & right)*
    - `Costal cartilage of tenth rib` *(left & right)*
    - `Costal cartilage of third rib` *(left & right)*

### Vertebral column

- `Vertebral column`
  - `Cervical vertebrae` — also: Vertebrae C1-C7
    - `Atlas (C1)` — also: Vertebra C1, Atlas bone
    - `Axis (C2)` — also: Vertebra C2, Axis bone
    - `Vertebra C3`
    - `Vertebra C4`
    - `Vertebra C5`
    - `Vertebra C6`
    - `Vertebra C7` — also: Vertebra prominens
  - `Coccyx` — also: Coccygeal bone
  - `Lumbar vertebrae` — also: Vertebrae L1-L5
    - `Vertebra L1`
    - `Vertebra L2`
    - `Vertebra L3`
    - `Vertebra L4`
    - `Vertebra L5`
  - `Sacrum` — also: Sacral bone
  - `Thoracic vertebrae` — also: Vertebrae T1-T12
    - `Vertebra T1` — also: First thoracic vertebra
    - `Vertebra T2`
    - `Vertebra T3`
    - `Vertebra T4`
    - `Vertebra T5`
    - `Vertebra T6`
    - `Vertebra T7`
    - `Vertebra T8`
    - `Vertebra T9`
    - `Vertebra T10`
    - `Vertebra T11`
    - `Vertebra T12`

## Joints

Whole system: `Joints` — also: Bony joints, Articular system


### Joints of lower limb

- `Joints of lower limb`
  - `Joints of free lower limb`
    - `Fibrous joints of free lower limb`
      - `Interosseous membrane of leg` *(left & right)* — also: Middle tibiofibular joint
      - `Tibiofibular syndesmosis` — also: Inferior tibiofibular joint
        - `Anterior tibiofibular ligament` *(left & right)*
        - `Posterior tibiofibular ligament` *(left & right)*
        - `Transverse tibiofibular ligament` *(left & right)*
    - `Synovial joints of free lower limb`
      - `Ankle joint`
        - `Lateral collateral ligament of ankle`
          - `Anterior talofibular ligament` *(left & right)*
          - `Calcaneofibular ligament` *(left & right)*
          - `Posterior talofibular ligament` *(left & right)*
        - `Medial collateral ligament of ankle` — also: Deltoid ligament
          - `Posterior tibiotalar ligament` *(left & right)* — also: Posterior tibiotalar part
          - `Tibiocalcaneal ligament` *(left & right)* — also: Tibiocalcaneal part
          - `Tibionavicular ligament` *(left & right)* — also: Tibionavicular part
      - `Hip joint`
        - `Acetabular labrum` *(left & right)* — also: Glenoid labrum of hip
        - `Articular capsule of hip joint` *(left & right)*
        - `Descending part of iliofemoral ligament` *(left & right)*
        - `Frenula capsulae` *(left & right)*
        - `Ischiofemoral ligament` *(left & right)*
        - `Ligament of head of femur` *(left & right)* — also: Ligamentum teres of femur
        - `Pubofemoral ligament` *(left & right)*
        - `Transverse acetabular ligament` *(left & right)*
        - `Transverse part of iliofemoral ligament` *(left & right)*
        - `Zona orbicularis` *(left & right)*
      - `Joints of foot`
        - `Intermetatarsal joints`
          - `Dorsal metatarsal ligaments` *(left & right)*
          - `Intersesamoid ligament` *(left & right)*
          - `Metatarsal interosseous ligaments` *(left & right)*
          - `Plantar metatarsal ligaments` *(left & right)*
        - `Interphalangeal joints of foot`
          - `Articular capsule of interphalangeal joint of great toe` *(left & right)*
          - `Articular capsules of distal interphalangeal joints of foot` *(left & right)*
          - `Articular capsules of proximal interphalangeal joints of foot` *(left & right)*
          - `Collateral interphalangeal ligaments of foot` *(left & right)*
          - `Plantar interphalangeal ligaments` *(left & right)*
        - `Intertarsal joints` — also: Tarsal joints
          - `Anterior talocalcaneal ligament` *(left & right)* — also: Cervical ligament
          - `Calcaneocuboid ligament` *(left & right)*
          - `Calcaneonavicular ligament` *(left & right)*
          - `Cuneocuboid interosseous ligament` *(left & right)*
          - `Dorsal calcaneocuboid ligament` *(left & right)*
          - `Dorsal cuboideonavicular ligament` *(left & right)*
          - `Dorsal cuneocuboid ligament` *(left & right)*
          - `Dorsal cuneonavicular ligaments` *(left & right)*
          - `Dorsal intercuneiform ligaments` *(left & right)*
          - `Intercuneiform interosseous ligaments` *(left & right)*
          - `Lateral talocalcaneal ligament` *(left & right)*
          - `Long plantar ligament` *(left & right)*
          - `Medial talocalcaneal ligament` *(left & right)*
          - `Plantar calcaneocuboid ligament` *(left & right)* — also: Short plantar ligament
          - `Plantar calcaneonavicular ligament` *(left & right)* — also: Spring ligament
          - `Plantar cuboideonavicular ligament` *(left & right)*
          - `Plantar cuneocuboid ligament` *(left & right)*
          - `Plantar cuneonavicular ligaments` *(left & right)*
          - `Plantar intercuneiform ligaments` *(left & right)*
          - `Posterior talocalcaneal ligament` *(left & right)*
          - `Talocalcaneal interosseous ligament` *(left & right)*
          - `Talonavicular ligament` *(left & right)*
        - `Metatarsophalangeal joints`
          - `Articular capsules of metatarsophalangeal joints` *(left & right)*
          - `Collateral metatarsophalangeal ligaments` *(left & right)*
          - `Deep transverse metatarsal ligament` *(left & right)*
          - `Plantar metatarsophalangeal ligaments` *(left & right)*
        - `Tarsometatarsal joints` — also: Lisfranc's joint
          - `Cuneometatarsal interosseous ligaments` *(left & right)*
          - `Dorsal tarsometatarsal ligaments` *(left & right)*
          - `Plantar tarsometatarsal ligaments` *(left & right)*
      - `Knee joint`
        - `Anterior cruciate ligament` *(left & right)*
        - `Anterior meniscotibial ligament (Lateral meniscus)` *(left & right)*
        - `Anterior meniscotibial ligament (Medial meniscus)` *(left & right)*
        - `Arcuate popliteal ligament` *(left & right)*
        - `Articular capsule of knee joint` *(left & right)*
        - `Deep part of tibial collateral ligament` *(left & right)* — also: Deep part of medial collateral ligament of knee
        - `Fibular collateral ligament` *(left & right)* — also: Lateral collateral ligament of knee
        - `Infrapatellar fat pad` *(left & right)*
        - `Lateral meniscus` *(left & right)*
        - `Medial meniscus` *(left & right)*
        - `Meniscopatellar ligament` *(left & right)*
        - `Oblique popliteal ligament` *(left & right)* — also: Ligament of Bourgery, Ligament of Winslow
        - `Popliteofibular ligament` *(left & right)*
        - `Posterior cruciate ligament` *(left & right)*
        - `Posterior meniscotibial ligament (Lateral meniscus)` *(left & right)*
        - `Posterior meniscotibial ligament (Medial meniscus)` *(left & right)*
        - `Superficial part of tibial collateral ligament` *(left & right)* — also: Superficial part of medial collateral ligament of knee
        - `Transverse ligament of knee` *(left & right)* — also: Transverse genicular ligament
      - `Superior tibiofibular joint`
        - `Anterior ligament of fibular head` *(left & right)*
        - `Articular capsule of superior tibiofibular joint` *(left & right)*
        - `Posterior ligament of fibular head` *(left & right)*
  - `Joints of pelvic girdle`
    - `Cartilaginous joints of pelvic girdle`
      - `Pubic symphysis`
        - `Inferior pubic ligament` — also: Arcuate pubic ligament
        - `Interpubic disc` — also: Interpubic fibrocartilage
        - `Superior pubic ligament`
      - `Triradiate cartilage` *(left & right)*
    - `Fibrous joints of pelvic girdle`
      - `Iliolumbar ligament` *(left & right)*
      - `Obturator membrane` *(left & right)*
      - `Sacrospinous ligament` *(left & right)* — also: Sacrospinal ligament
      - `Sacrotuberous ligament` *(left & right)* — also: Sacrotuberal ligament
    - `Synovial joints of pelvic girdle`
      - `Sacro-iliac joint` — also: Sacroiliac joint
        - `Anterior sacro-iliac ligament` *(left & right)*
        - `Interosseous sacro-iliac ligament` *(left & right)*
        - `Posterior sacro-iliac ligament` *(left & right)*

### Joints of skull

- `Joints of skull`
  - `Cranial fibrous joints`
    - `Cranial syndesmoses`
      - `Pterygospinous ligament` *(left & right)*
      - `Sphenomandibular ligament` *(left & right)*
      - `Stylohyoid ligament` *(left & right)*
      - `Stylomandibular ligament` *(left & right)*
  - `Temporomandibular joint`
    - `Articular capsule of temporomandibular joint` *(left & right)*
    - `Articular disc of temporomandibular joint` *(left & right)*
    - `Lateral temporomandibular ligament` *(left & right)*

### Joints of upper limb

- `Joints of upper limb`
  - `Joints of free upper limb`
    - `Fibrous joints of free upper limb`
      - `Radio-ulnar syndesmoses`
        - `Interosseous membrane of forearm` *(left & right)*
        - `Oblique cord` *(left & right)*
    - `Synovial joints of free upper limb`
      - `Distal radio-ulnar joint`
        - `Articular disc of distal radio-ulnar joint` *(left & right)*
        - `Dorsal radio-ulnar ligament` *(left & right)*
        - `Palmar radio-ulnar ligament` *(left & right)*
      - `Elbow joint`
        - `Annular ligament of radius` *(left & right)* — also: Anular ligament of radius
        - `Articular capsule of elbow joint` *(left & right)*
        - `Quadrate ligament` *(left & right)*
        - `Radial collateral ligament` *(left & right)*
        - `Ulnar collateral ligament` *(left & right)*
      - `Glenohumeral joint` — also: Shoulder joint
        - `Articular capsule of glenohumeral joint` *(left & right)*
        - `Coracohumeral ligament` *(left & right)*
        - `Glenoid labrum` *(left & right)*
        - `Inferior glenohumeral ligament` *(left & right)* — also: Inferior glenohumeral band
        - `Middle glenohumeral ligament` *(left & right)* — also: Middle glenohumeral band
        - `Superior glenohumeral ligament` *(left & right)* — also: Superior glenohumeral band
        - `Transverse humeral ligament` *(left & right)*
      - `Joints of hand`
        - `Carpometacarpal joints`
          - `Dorsal carpometacarpal ligaments` *(left & right)*
          - `Palmar carpometacarpal ligaments` *(left & right)*
        - `Intercarpal joints` — also: Carpal joints
          - `Capitohamate interosseous ligament` *(left & right)*
          - `Dorsal intercarpal ligaments` *(left & right)*
            - `Dorsal scaphotriquetral ligament` *(left & right)*
          - `Dorsal ulnocarpal ligament` *(left & right)*
          - `Lunotriquetral interosseous ligament` *(left & right)*
          - `Palmar capitohamate ligament` *(left & right)*
          - `Palmar lunotriquetral ligament` *(left & right)*
          - `Palmar scaphotriquetral ligament` *(left & right)*
          - `Palmar trapezoideocapitate ligament` *(left & right)*
          - `Pisohamate ligament` *(left & right)*
          - `Pisometacarpal ligament` *(left & right)*
          - `Pisotriquetral ligament` *(left & right)*
          - `Radial collateral ligament of wrist joint` *(right)*
          - `Radiate carpal ligament` *(left & right)*
          - `Scaphocapitate ligament` *(left & right)*
          - `Scapholunate interosseous ligament` *(left & right)*
          - `Scaphotrapeziotrapezoidal ligament` *(left & right)*
          - `Trapeziotrapezoidal interosseous ligament` *(left & right)*
          - `Trapezoideocapitate interosseous ligament` *(left & right)* — also: Trapezocapitate interosseous ligament
          - `Triquetrocapitate ligament` *(left & right)*
          - `Triquetrohamate ligament` *(left & right)*
          - `Ulnar collateral ligament of wrist joint` *(right)* — also: Ulnotriquetral ligament
        - `Intermetacarpal joints`
          - `Dorsal metacarpal ligaments` *(left & right)*
          - `Interosseous metacarpal ligaments` *(left & right)*
          - `Palmar metacarpal ligaments` *(left & right)*
        - `Interphalangeal joints of hand`
          - `Articular capsules of distal interphalangeal joints` *(left & right)*
          - `Articular capsules of proximal interphalangeal joints` *(left & right)*
          - `Collateral interphalangeal ligaments of hand` *(left & right)*
          - `Palmar interphalangeal ligaments` *(left & right)*
        - `Metacarpophalangeal joints`
          - `Articular capsules of metacarpophalangeal joints` *(left & right)*
          - `Collateral metacarpophalangeal ligaments` *(left & right)*
          - `Deep transverse metacarpal ligament` *(left & right)*
      - `Radiocarpal joint` — also: Wrist joint
        - `Articular capsule of radiocarpal joint` *(left & right)*
        - `Dorsal radiocarpal ligament` *(left & right)* — also: Dorsal radiotriquetral ligament
        - `Radial collateral ligament of wrist joint` *(left)*
        - `Radiocapitate ligament` *(left & right)*
        - `Radioscaphocapitate ligament` *(left & right)*
        - `Ulnar collateral ligament of wrist joint` *(left)* — also: Ulnotriquetral ligament
        - `Ulnocapitate ligament` *(left & right)*
        - `Ulnolunate ligament` *(left & right)*
        - `Ulnopisiform ligament` *(left)*
        - `Ulnotriquetral ligament` *(left & right)*
  - `Joints of pectoral girdle` — also: Joints of shoulder girdle
    - `Fibrous joints of pectoral girdle` — also: Fibrous joints of shoulder girdle, Syndesmoses of upper limb girdle, Syndesmoses of pectoral girdle, Syndesmoses of shoulder girdle
      - `Coraco-acromial ligament` *(left & right)* — also: Coracoacromial ligament
      - `Inferior transverse scapular ligament` *(left & right)*
      - `Superior transverse scapular ligament` *(left & right)*
    - `Synovial joints of pectoral girdle` — also: Synovial joints of shoulder girdle
      - `Acromioclavicular joint`
        - `Acromioclavicular ligament` *(left & right)*
        - `Articular capsule of acromioclavicular joint` *(left & right)*
        - `Articular disc of acromioclavicular joint` *(left & right)*
        - `Conoid ligament` *(left & right)*
        - `Trapezoid ligament` *(left & right)*
      - `Sternoclavicular joint`
        - `Anterior sternoclavicular ligament` *(left & right)*
        - `Articular capsule of sternoclavicular joint` *(left & right)*
        - `Articular disc of sternoclavicular joint` *(left & right)*
        - `Costoclavicular ligament` *(left & right)*
        - `Interclavicular ligament` *(left & right)*
        - `Posterior sternoclavicular ligament` *(left & right)*

### Joints of vertebral column

- `Joints of vertebral column` — also: Vertebral joints
  - `Cartilaginous joints of vertebral column`
    - `Intervertebral symphysis`
      - `Intervertebral disc C2-C3`
      - `Intervertebral disc C3-C4`
      - `Intervertebral disc C4-C5`
      - `Intervertebral disc C5-C6`
      - `Intervertebral disc C6-C7`
      - `Intervertebral disc C7-T1`
      - `Intervertebral disc L1-L2`
      - `Intervertebral disc L2-L3`
      - `Intervertebral disc L3-L4`
      - `Intervertebral disc L4-L5`
      - `Intervertebral disc L5-S1`
      - `Intervertebral disc T1-T2`
      - `Intervertebral disc T2-T3`
      - `Intervertebral disc T3-T4`
      - `Intervertebral disc T4-T5`
      - `Intervertebral disc T5-T6`
      - `Intervertebral disc T6-T7`
      - `Intervertebral disc T7-T8`
      - `Intervertebral disc T8-T9`
      - `Intervertebral disc T9-T10`
      - `Intervertebral disc T10-T11`
      - `Intervertebral disc T11-T12`
      - `Intervertebral disc T12-L1`
      - `Nucleus pulposus C2-C3`
      - `Nucleus pulposus C3-C4`
      - `Nucleus pulposus C4-C5`
      - `Nucleus pulposus C5-C6`
      - `Nucleus pulposus C6-C7`
      - `Nucleus pulposus C7-T1`
      - `Nucleus pulposus L1-L2`
      - `Nucleus pulposus L2-L3`
      - `Nucleus pulposus L3-L4`
      - `Nucleus pulposus L4-L5`
      - `Nucleus pulposus L5-S1`
      - `Nucleus pulposus T1-T2`
      - `Nucleus pulposus T2-T3`
      - `Nucleus pulposus T3-T4`
      - `Nucleus pulposus T4-T5`
      - `Nucleus pulposus T5-T6`
      - `Nucleus pulposus T6-T7`
      - `Nucleus pulposus T7-T8`
      - `Nucleus pulposus T8-T9`
      - `Nucleus pulposus T9-T10`
      - `Nucleus pulposus T10-T11`
      - `Nucleus pulposus T11-T12`
      - `Nucleus pulposus T12-L1`
      - `Sacrococcygeal symphysis`
  - `Fibrous joints of vertebral column`
    - `Anterior longitudinal ligament`
    - `Intercornual ligament` *(left & right)*
    - `Interspinous ligaments`
    - `Intertransverse ligaments` *(left & right)*
    - `Ligamenta flava`
    - `Nuchal ligament`
    - `Posterior longitudinal ligament`
    - `Supraspinous ligament`

### Laryngeal joints

- `Laryngeal joints`
  - `Fibrous joints of larynx`
    - `Fibro-elastic membrane of larynx`
      - `Quadrangular membrane` *(left & right)*
    - `Median cricothyroid ligament`
    - `Thyrohyoid membrane`
      - `Lateral thyrohyoid ligament` *(left & right)*
      - `Median thyrohyoid ligament`
  - `Synovial joints of larynx`
    - `Crico-arytenoid joint`
      - `Cricopharyngeal ligament` *(left & right)*

### Thoracic joints

- `Thoracic joints`
  - `Fibrous joints of thorax`
    - `External intercostal membrane` *(left & right)*
    - `Internal intercostal membrane` *(left & right)*
  - `Synovial joints of thorax`
    - `Costovertebral joints`
      - `Costotransverse joint`
        - `Costotransverse ligament` *(left & right)*
      - `Joint of head of rib`
        - `Intra-articular ligament of head of rib` *(right)*
        - `Radiate ligament of head of rib` *(left & right)*

## Muscles

Whole system: `Muscular system`


### Abdominal part of muscular system

- `Abdominal part of muscular system`
  - `Muscles of abdomen`
    - `External abdominal oblique muscle` *(left & right)* — also: External oblique muscle
      - `Inguinal ligament` *(left & right)* — also: Poupart's ligament, Lateral ligament of Vesalius, Vesalius' ligament, Fallopian ligament
    - `Internal abdominal oblique muscle` *(left & right)* — also: Internal oblique muscle
    - `Linea alba`
    - `Pyramidalis muscle` *(left & right)*
    - `Quadratus lumborum muscle` *(left & right)*
    - `Rectus abdominis muscle` *(left & right)*
    - `Transversus abdominis muscle` *(left & right)* — also: Transverse abdominal muscle

### Cervical part of muscular system

- `Cervical part of muscular system`
  - `Muscles of neck`
    - `Infrahyoid muscles` — also: Strap muscles
      - `Omohyoid muscle` *(left & right)*
      - `Sternohyoid muscle` *(left & right)*
      - `Sternothyroid muscle` *(left & right)*
      - `Thyrohyoid muscle` *(left & right)*
    - `Laryngeal muscles`
      - `Ary-epiglottic part of oblique arytenoid muscle` *(left & right)*
      - `External part of thyro-arytenoid muscle` *(left & right)*
      - `Lateral crico-arytenoid muscle` *(left & right)*
      - `Oblique part of cricothyroid muscle` *(left & right)*
      - `Posterior crico-arytenoid muscle` *(left & right)*
      - `Straight part of cricothyroid muscle` *(left & right)*
      - `Thyro-epiglottic part of thyro-arytenoid muscle` *(left & right)*
      - `Transverse arytenoid muscle`
    - `Longus capitis muscle` *(left & right)*
    - `Longus colli muscle` *(left & right)* — also: Longus cervicis muscle
    - `Pharyngeal muscles` — also: Muscles of pharynx, Muscular layer of pharynx
      - `Inferior pharyngeal constrictor` *(left & right)* — also: Inferior constrictor muscle of pharynx
      - `Middle pharyngeal constrictor` *(left & right)* — also: Middle constrictor muscle of pharynx
      - `Stylopharyngeus muscle` *(left & right)*
      - `Superior pharyngeal constrictor` *(left & right)* — also: Superior constrictor muscle of pharynx
    - `Platysma` *(left & right)* — also: Platysma muscle
    - `Rectus anterior capitis muscle` *(left & right)* — also: Rectus capitis anterior muscle
    - `Rectus lateralis capitis muscle` *(left & right)* — also: Rectus capitis lateralis muscle
    - `Scalenus anterior muscle` *(left & right)* — also: Anterior scalene muscle
    - `Scalenus medius muscle` *(left & right)* — also: Middle scalene muscle
    - `Scalenus posterior muscle` *(left & right)* — also: Posterior scalene muscle
    - `Sternocleidomastoid muscle` *(left & right)* — also: Sternomastoid muscle
    - `Suprahyoid muscles`
      - `Digastric muscle`
        - `Anterior belly of digastric muscle` *(left & right)*
        - `Intermediate tendon of digastric muscle` *(left & right)*
        - `Posterior belly of digastric muscle` *(left & right)*
      - `Geniohyoid muscle` *(left & right)*
      - `Mylohyoid muscle` *(left & right)*
      - `Stylohyoid muscle` *(left & right)*

### Cranial part of muscular system

- `Cranial part of muscular system`
  - `Muscles of head`
    - `Extra-ocular muscles` — also: Extrinsic muscles of eyeball, Extraocular muscles
      - `Common tendinous ring` *(left & right)* — also: Common anular tendon
      - `Inferior oblique muscle` *(left & right)*
      - `Inferior rectus muscle` *(left & right)*
      - `Inferior tarsus` *(left & right)*
      - `Lateral rectus muscle` *(left & right)*
      - `Levator palpebrae superioris` *(left & right)*
      - `Medial rectus muscle` *(left & right)*
      - `Superior oblique muscle` *(left & right)*
      - `Superior rectus muscle` *(left & right)*
      - `Superior tarsus` *(left & right)*
      - `Trochlea of superior oblique muscle` *(left & right)*
    - `Masticatory muscles` — also: Muscles of mastication
      - `Deep part of masseter` *(left & right)*
      - `Inferior head of lateral pterygoid muscle` *(left & right)* — also: Lower head of lateral pterygoid muscle
      - `Medial pterygoid muscle` *(left & right)*
      - `Superficial part of masseter` *(left & right)*
      - `Superior head of lateral pterygoid muscle` *(left & right)* — also: Upper head of lateral pterygoid muscle, Sphenomeniscus muscle
      - `Temporalis muscle` *(left & right)* — also: Temporal muscle
    - `Muscles of soft palate`
      - `Palatopharyngeus muscle` *(left & right)*
    - `Muscles of tongue`
      - `Genioglossus muscle` *(left & right)*
      - `Hyoglossus muscle` *(left & right)*
    - `Superficial muscles of head`
      - `Epicranius muscle`
        - `Epicranial aponeurosis` *(left & right)*
        - `Frontalis muscle` *(left & right)* — also: Frontal belly of occipitofrontalis muscle, Frontal belly of epicranius muscle
        - `Occipitalis muscle` *(left & right)* — also: Occipital belly of occipitofrontalis muscle
        - `Temporoparietalis muscle` *(left & right)*
      - `Facial muscles`
        - `Bucinator` *(left & right)* — also: Buccinator muscle
        - `Corrugator supercilii` *(left & right)* — also: Muscle of Koyter, Koyter's muscle
        - `Depressor anguli oris` *(left & right)*
        - `Depressor labii inferioris` *(left & right)* — also: Muscle of Aeby
        - `Depressor septi nasi` *(left & right)* — also: Depressor septi nasi muscle
        - `Levator anguli oris` *(left & right)*
        - `Levator labii superioris` *(left & right)*
        - `Levator nasolabialis` *(left & right)* — also: Levator labii superioris alaeque nasi
        - `Mentalis muscle` *(left & right)*
        - `Nasalis muscle` *(left & right)*
        - `Orbicularis oris muscle` *(left & right)*
        - `Orbital part of orbicularis oculi` *(left & right)*
        - `Palpebral part of orbicularis oculi` *(left & right)*
        - `Procerus muscle` *(left & right)* — also: Santorini's muscle
        - `Risorius muscle` *(left & right)* — also: Muscle of Albinus, Santorini's muscle
        - `Zygomaticus major muscle` *(left & right)*
        - `Zygomaticus minor muscle` *(left & right)*

### Dorsal part of muscular system

- `Dorsal part of muscular system`
  - `Epaxial muscles` — also: Muscles of back proper, Deep back muscles, True back muscles
    - `Erector spinae` — also: Sacrospinalis muscle
      - `Iliocostalis muscles` — also: Iliocostalis muscle
        - `Iliocostalis colli muscle` *(right)* — also: Iliocostalis cervicis muscle
        - `Iliocostalis lumborum muscle` *(left & right)* — also: Lateral division of lumbar erector spinae, Lumbar part of iliocostalis lumborum muscle
        - `Iliocostalis thoracis muscle` *(left & right)* — also: Thoracic part of iliocostalis lumborum muscle
      - `Longissimus muscles` — also: Longissimus muscle
        - `Longissimus capitis muscle` *(left & right)*
        - `Longissimus colli muscle` *(left & right)* — also: Longissimus cervicis muscle
        - `Longissimus thoracis muscle` *(left & right)*
      - `Spinalis muscles` — also: Spinalis muscle
        - `Spinalis capitis muscle` *(left & right)*
        - `Spinalis colli muscle` *(left & right)* — also: Spinalis cervicis muscle
        - `Spinalis thoracis muscle` *(left & right)*
    - `Interspinales muscles`
      - `Interspinales colli muscles` *(left & right)* — also: Interspinales cervicis muscles
      - `Interspinales lumborum muscles` *(left & right)*
      - `Interspinales thoracis muscles` *(left & right)*
    - `Spinotransversales muscles`
      - `Splenius muscles` — also: Splenius muscle
        - `Splenius capitis muscle` *(left & right)*
        - `Splenius colli muscle` *(left & right)* — also: Splenius cervicis muscle
    - `Suboccipital muscles`
      - `Obliquus inferior capitis muscle` *(left & right)* — also: Obliquus capitis inferior muscle
      - `Obliquus superior capitis muscle` *(left & right)* — also: Obliquus capitis superior muscle
      - `Rectus posterior major capitis muscle` *(left & right)* — also: Rectus capitis posterior major muscle
      - `Rectus posterior minor capitis muscle` *(left & right)* — also: Rectus capitis posterior minor muscle
    - `Transversospinal muscles` — also: Transversospinales muscles
      - `Multifidus muscles` — also: Multifidus muscle
        - `Multifidus colli muscle` *(left & right)* — also: Multifidus cervicis muscle
        - `Multifidus lumborum muscle` *(left & right)*
        - `Multifidus thoracis muscle` *(left & right)*
      - `Rotatores` — also: Rotator muscles
        - `Rotatores` *(left & right)* — also: Rotator muscles
      - `Semispinalis muscles` — also: Semispinalis muscle
        - `Semispinalis colli muscle` *(left & right)*
        - `Semispinalis thoracis muscle` *(left & right)*
  - `Hypaxial muscles of back`
    - `Dorsal parts of lateral intertransversarii lumborum muscles` *(left & right)* — also: Posterior parts of lateral intertransversarii lumborum
    - `Latissimus dorsi muscle` *(left & right)*
    - `Levator scapulae` *(left & right)*
    - `Rhomboid major muscle` *(left & right)*
    - `Rhomboid minor muscle` *(left & right)*
    - `Serratus posterior inferior muscle` *(left & right)*
    - `Serratus posterior superior muscle` *(left & right)*
    - `Trapezius muscle`
      - `Ascending part of trapezius muscle` *(left & right)* — also: Superior part of trapezius muscle
      - `Descending part of trapezius muscle` *(left & right)* — also: Inferior part of trapezius muscle
      - `Transverse part of trapezius muscle` *(left & right)* — also: Middle part of trapezius muscle
    - `Ventral parts of lateral intertransversarii lumborum muscles` *(left & right)* — also: Anterior parts of lateral intertransversarii lumborum

### Muscular system of lower limb

- `Muscular system of lower limb`
  - `Bursae of lower limb`
    - `(Iliopectineal bursa)` *(left & right)*
    - `Anserine bursa` *(left & right)*
    - `Bursa of piriformis muscle` *(left & right)*
    - `Deep infrapatellar bursa` *(left & right)*
    - `Inferior subtendinous bursa of biceps femoris muscle` *(left & right)*
    - `Intermuscular gluteal bursae` *(left & right)*
    - `Lateral subtendinous bursa of gastrocnemius muscle` *(left & right)*
    - `Medial subtendinous bursa of gastrocnemius muscle` *(left & right)*
    - `Sciatic bursa of gluteus maximus muscle` *(left & right)* — also: Isciatic bursa of gluteus maximus muscle
    - `Sciatic bursa of obturator internus` *(left & right)* — also: Isciatic bursa of obturator internus
    - `Semimembranosus bursa` *(left & right)*
    - `Subcutaneous bursa of lateral malleolus` *(left & right)*
    - `Subcutaneous bursa of medial malleolus` *(left & right)*
    - `Subcutaneous bursa of tuberosity of tibia` *(left & right)*
    - `Subcutaneous calcaneal bursa` *(left & right)*
    - `Subcutaneous infrapatellar bursa` *(left & right)*
    - `Subcutaneous prepatellar bursa` *(left & right)*
    - `Subcutaneous trochanteric bursa` *(left & right)*
    - `Subfacial prepatellar bursa` *(left & right)*
    - `Subtendinous bursa of iliacus muscle` *(left & right)* — also: Subtendinous bursa of iliopsoas
    - `Subtendinous bursa of obturator internus` *(left & right)*
    - `Subtendinous bursa of sartorius muscle` *(left & right)*
    - `Subtendinous bursa of tibialis anterior` *(left & right)*
    - `Subtendinous calcaneal bursa` *(left & right)* — also: Bursa of calcaneal tendon, Retrocalcaneal bursa
    - `Subtendinous prepatellar bursa` *(left & right)*
    - `Superior bursa of biceps femoris muscle` *(left & right)*
    - `Suprapatellar bursa` *(left & right)*
    - `Trochanteric bursa of gluteus maximus muscle` *(left & right)*
    - `Trochanteric bursa of gluteus medius muscle` *(left & right)*
    - `Trochanteric bursa of gluteus minimus muscle` *(left & right)*
  - `Muscles of lower limb`
    - `Anterior compartment of leg` — also: Extensor compartment of leg
      - `Extensor digitorum longus` *(left & right)*
        - `Tendon of extensor digitorum longus` *(left & right)*
      - `Extensor hallucis longus` *(left & right)*
      - `Fibularis tertius muscle` *(left & right)*
      - `Tibialis anterior muscle` *(left & right)*
    - `Anterior compartment of thigh` — also: Extensor compartment of thigh
      - `Lateral patellar retinaculum` *(left & right)*
      - `Medial patellar retinaculum` *(left & right)*
      - `Rectus femoris muscle` *(left & right)*
      - `Sartorius muscle` *(left & right)*
      - `Vastus intermedius muscle` *(left & right)*
      - `Vastus lateralis muscle` *(left & right)*
      - `Vastus medialis muscle` *(left & right)*
    - `Deep gluteal muscles` — also: Lateral rotators of thigh
      - `Inferior gemellus muscle` *(left & right)* — also: Gemellus inferior muscle
      - `Obturator internus` *(left & right)*
      - `Piriformis muscle` *(left & right)*
      - `Quadratus femoris muscle` *(left & right)*
      - `Superior gemellus muscle` *(left & right)* — also: Gemellus superior muscle
    - `Iliopsoas muscle` — also: Hyrtl's muscle
      - `Iliacus muscle` *(left & right)*
      - `Psoas major` *(left & right)*
    - `Lateral compartment of leg` — also: Fibular compartment of leg
      - `Fibularis brevis muscle` *(left & right)*
      - `Fibularis longus muscle` *(left & right)*
    - `Medial compartment of thigh` — also: Adductor compartment of thigh
      - `Adductor brevis` *(left & right)*
      - `Adductor longus` *(left & right)*
      - `Adductor magnus` *(left & right)*
        - `(Adductor minimus)` *(left & right)*
      - `Gracilis muscle` *(left & right)*
      - `Obturator externus` *(left & right)*
      - `Pectineus muscle` *(left & right)*
    - `Muscles of foot`
      - `(Opponens digiti minimi muscle of foot)` *(left & right)*
      - `Abductor digiti minimi of foot` *(left & right)*
      - `Abductor hallucis` *(left & right)*
      - `Adductor hallucis`
        - `Oblique head of adductor hallucis` *(left & right)*
        - `Transverse head of adductor hallucis` *(left & right)*
      - `Dorsal interossei muscles of foot` *(left & right)*
      - `Extensor digitorum brevis` *(left & right)*
      - `Extensor hallucis brevis` *(left & right)*
      - `Flexor digiti minimi of foot` *(left & right)*
      - `Flexor digitorum brevis` *(left & right)*
      - `Flexor hallucis brevis`
        - `Lateral head of flexor hallucis brevis` *(left & right)*
        - `Medial head of flexor hallucis brevis` *(left & right)*
      - `Lumbrical muscles of foot` *(left & right)* — also: Lumbricals of foot
      - `Plantar interossei muscles` *(left & right)*
      - `Quadratus plantae muscle` *(left & right)* — also: Flexor accessorius muscle
    - `Posterior compartment of leg` — also: Flexor compartment of leg
      - `Deep part of posterior compartment of leg`
        - `Flexor digitorum longus` *(left & right)*
        - `Flexor hallucis longus` *(left & right)*
        - `Popliteus muscle` *(left & right)*
        - `Tibialis posterior muscle` *(left & right)*
      - `Superficial part of posterior compartment of leg`
        - `Calcaneal tendon` *(left & right)* — also: Achilles' tendon
        - `Lateral head of gastrocnemius` *(left & right)*
        - `Medial head of gastrocnemius` *(left & right)*
        - `Plantaris muscle` *(left & right)*
        - `Soleus muscle` *(left & right)*
    - `Posterior compartment of thigh` — also: Flexor compartment of thigh
      - `Long head of biceps femoris` *(left & right)*
      - `Semimembranosus muscle` *(left & right)*
      - `Semitendinosus muscle` *(left & right)*
      - `Short head of biceps femoris` *(left & right)*
    - `Superficial gluteal muscles`
      - `Gluteus maximus muscle` *(left & right)*
      - `Gluteus medius muscle` *(left & right)*
      - `Gluteus minimus muscle` *(left & right)*
      - `Tensor fasciae latae` *(left & right)* — also: Tensor of fascia lata
        - `Iliotibial tract` *(left & right)* — also: Iliotibial band, Bandelette of Maissiat
  - `Tendon sheaths of lower limb`
    - `Anterior tarsal tendon sheaths`
      - `Tendon sheath of extensor digitorum longus` *(left & right)*
      - `Tendon sheath of extensor hallucis longus` *(left & right)*
      - `Tendon sheath of tibialis anterior` *(left & right)*
    - `Fibular tarsal tendon sheaths`
      - `Common tendon sheath of fibularis muscles` *(left & right)* — also: Common tendinous sheath of peroneus muscles
      - `Plantar tendon sheath of fibularis longus muscle` *(left & right)* — also: Plantar tendinous sheath of peroneus longus muscle
    - `Tibial tarsal tendon sheaths`
      - `Tendon sheath of flexor digitorum longus` *(left & right)*
      - `Tendon sheath of flexor hallucis longus` *(left & right)*
      - `Tendon sheath of tibialis posterior muscle` *(left & right)*

### Muscular system of upper limb

- `Muscular system of upper limb`
  - `Bursae of upper limb`
    - `Bicipitoradial bursa` *(left & right)*
    - `Coracobrachial bursa` *(left & right)*
    - `Subacromial bursa` *(left & right)*
    - `Subcutaneous acromial bursa` *(left & right)*
    - `Subdeltoid bursa` *(left & right)*
    - `Subtendinous bursa of infraspinatus muscle` *(left & right)*
    - `Subtendinous bursa of teres major muscle` *(left & right)*
    - `Subtendinous bursa of trapezius muscle` *(left & right)*
    - `Subtendinous bursa of triceps brachii muscle` *(left & right)*
  - `Muscles of upper limb`
    - `Anterior compartment of arm` — also: Flexor compartment of arm
      - `Biceps brachii muscle`
        - `Long head of biceps brachii` *(left & right)*
        - `Short head of biceps brachii` *(left & right)*
      - `Brachialis muscle` *(left & right)*
      - `Coracobrachialis muscle` *(left & right)* — also: Casserio's muscle
    - `Anterior compartment of forearm` — also: Flexor compartment of forearm
      - `Deep part of anterior compartment of forearm` — also: Parona's space
        - `Flexor digitorum profundus` *(left & right)*
        - `Flexor pollicis longus` *(left & right)*
        - `Pronator quadratus` *(left & right)*
      - `Superficial part of anterior compartment of forearm`
        - `Deep head of pronator teres` *(left & right)* — also: Ulnar head of pronator teres
        - `Flexor carpi radialis` *(left & right)*
        - `Humeral head of flexor carpi ulnaris` *(left & right)*
        - `Humero-ulnar head of flexor digitorum superficialis` *(left & right)* — also: Humeroulnar head of flexor digitorum superficialis
        - `Palmaris longus muscle` *(left & right)*
        - `Radial head of flexor digitorum superficialis` *(left & right)*
        - `Superficial head of pronator teres` *(left & right)* — also: Humeral head of pronator teres
        - `Ulnar head of flexor carpi ulnaris` *(left & right)*
    - `Muscles of hand`
      - `Abductor digiti minimi of hand` *(left & right)*
      - `Abductor pollicis brevis` *(left & right)*
      - `Deep head of flexor pollicis brevis` *(left & right)*
      - `Dorsal interossei muscles of hand` *(left & right)*
      - `Flexor digiti minimi of hand` *(left & right)*
      - `Lumbrical muscles of hand` *(left & right)* — also: Lumbricals of hand
      - `Oblique head of adductor pollicis` *(left & right)*
      - `Opponens digiti minimi muscle of hand` *(left & right)*
      - `Opponens pollicis muscle` *(left & right)*
      - `Palmar interossei muscles` *(left & right)*
      - `Superficial head of flexor pollicis brevis` *(left & right)*
      - `Transverse head of adductor pollicis` *(left & right)*
    - `Posterior compartment of arm` — also: Extensor compartment of arm
      - `Triceps brachii muscle`
        - `Lateral head of triceps brachii` *(left & right)*
        - `Long head of triceps brachii` *(left & right)*
        - `Medial head of triceps brachii` *(left & right)* — also: Deep head of triceps brachii
    - `Posterior compartment of forearm` — also: Extensor compartment of forearm
      - `Deep part of posterior compartment of forearm`
        - `Abductor pollicis longus` *(left & right)*
        - `Extensor indicis` *(left & right)*
        - `Extensor pollicis brevis` *(left & right)*
        - `Extensor pollicis longus` *(left & right)*
        - `Supinator` *(left & right)*
      - `Superficial part of posterior compartment of forearm`
        - `Anconeus muscle` *(left & right)*
        - `Brachioradialis muscle` *(left & right)*
        - `Extensor carpi radialis brevis` *(left & right)*
        - `Extensor carpi radialis longus` *(left & right)*
        - `Extensor digiti minimi` *(left & right)*
        - `Extensor digitorum` *(left & right)*
        - `Humeral head of extensor carpi ulnaris` *(left & right)*
        - `Ulnar head of extensor carpi ulnaris` *(left & right)*
    - `Scapulohumeral muscles`
      - `Deltoid muscle`
        - `Acromial part of deltoid muscle` *(left & right)*
        - `Clavicular part of deltoid muscle` *(left & right)*
        - `Scapular spinal part of deltoid muscle` *(left & right)* — also: Spinal part of deltoid muscle
      - `Rotator cuff muscles`
        - `Infraspinatus muscle` *(left & right)*
        - `Subscapularis muscle` *(left & right)*
        - `Supraspinatus muscle` *(left & right)*
        - `Teres minor muscle` *(left & right)*
      - `Teres major muscle` *(left & right)*
  - `Tendon sheaths of upper limb`
    - `Carpal tendon sheaths`
      - `Dorsal carpal tendon sheaths`
        - `Tendon sheath - abd. pollicis longus - ext. pollicis brevis` *(left & right)* — also: Tendinous sheath of abductor pollicis longus and extensor pollicis brevis muscles
        - `Tendon sheath of extensor carpi ulnaris` *(left & right)*
        - `Tendon sheath of extensor digiti minimi manus` *(left & right)*
        - `Tendon sheath of extensor digitorum and extensor indicis` *(left & right)*
        - `Tendon sheath of extensor pollicis longus` *(left & right)*
        - `Tendon sheath of extensors carpi radialis` *(left & right)*
      - `Palmar carpal tendon sheaths`
        - `Common flexor tendon sheath` *(left & right)* — also: Ulnar bursa
        - `Tendon sheath of flexor carpi radialis` *(left & right)*
        - `Tendon sheath of flexor pollicis longus` *(left & right)* — also: Radial bursa
    - `Intertubercular tendon sheath` *(left & right)*
    - `Tendon sheaths of digits of hand`
      - `Cruciform part of fibrous sheath of digit of hand` *(left & right)*
      - `Synovial sheaths of digits of hand` *(left & right)*

### Pelvic part of muscular system

- `Pelvic part of muscular system`
  - `Muscles of pelvis`
    - `Pelvic diaphragm`
      - `Coccygeus muscle` *(left & right)* — also: Ischiococcygeus muscle
      - `Levator ani`
        - `Iliococcygeus muscle` *(left & right)*
        - `Pubo-analis muscle` *(left & right)* — also: Puborectalis muscle
        - `Pubococcygeus muscle` *(left & right)*
        - `Tendinous arch of levator ani` *(left & right)*
    - `Perineal muscles`
      - `External anal sphincter` *(left & right)*

### Thoracic part of muscular system

- `Thoracic part of muscular system`
  - `Diaphragm` — also: Thoracic diaphragm, Respiratory diaphragm, Thoracoabdominal diaphragm
  - `Muscles of thorax`
    - `External intercostal muscles` *(left & right)*
    - `Innermost intercostal muscles` *(left & right)*
    - `Internal intercostal muscles` *(left & right)*
    - `Levatores costarum` — also: Levator costae muscles
      - `Levatores breves costarum` *(left & right)* — also: Levatores costarum breves muscles
      - `Levatores longi costarum` *(left & right)* — also: Levatores costarum longi muscles
    - `Pectoralis major muscle`
      - `(Abdominal part of pectoralis major muscle)` *(left & right)*
      - `Clavicular head of pectoralis major muscle` *(left & right)*
      - `Sternocostal head of pectoralis major muscle` *(left & right)*
    - `Pectoralis minor muscle` *(left & right)*
    - `Serratus anterior muscle` *(left & right)* — also: Boxer's muscle
    - `Subclavius muscle` *(left & right)*
    - `Transversus thoracis muscle` *(left & right)*

## Fasciae

Whole system: `Fasciae`


### Cervical fasciae

- `Cervical fasciae` — also: Deep cervical fascia, Fasciae of neck, Cervical fascia
  - `Superficial investing cervical fascia` *(left & right)* — also: investing layer of cervical fascia

### Fasciae of abdomen

- `Fasciae of abdomen` — also: Abdominal fasciae, Fascia of abdomen
  - `Endo-abdominal fascia` — also: Parietal abdominal fascia, eep investing abdominal fascia
    - `Diaphragmatic fascia` — also: Inferior diaphragmic fascia
    - `Iliopsoas fascia` *(left & right)*
    - `Transversalis fascia`
  - `Investing abdominal fascia` *(left & right)* — also: Superficial investing abdominal fascia

### Fasciae of head

- `Fasciae of head` — also: Fascia of head
  - `Masseteric fascia` *(left & right)*
  - `Superficial layer of temporal fascia` *(left & right)*

### Fasciae of lower limb

- `Fasciae of lower limb` — also: Fascia of lower limb
  - `Anterior intermuscular septum of leg` *(left & right)* — also: Anterior crural intermusclar septum
  - `Crural fascia` *(left & right)* — also: Deep fascia of leg
  - `Fascia lata` *(left & right)*
  - `Flexor retinaculum of ankle` *(left & right)* — also: Lanciniate ligament
  - `Iliopectineal arch` *(left & right)* — also: Iliopectineal ligament
  - `Inferior extensor retinaculum of ankle` *(left & right)* — also: Cruciate crural ligament
  - `Inferior fibular retinaculum` *(left & right)* — also: Inferior peroneal retinaculum
  - `Lateral femoral intermuscular septum` *(left & right)*
  - `Medial femoral intermuscular septum` *(left & right)*
  - `Piriformis fascia` *(left & right)*
  - `Plantar aponeurosis` *(left & right)*
  - `Popliteal fascia` *(left & right)*
  - `Posterior intermuscular septum of leg` *(left & right)* — also: Posterior crural intermusclar septum
  - `Superficial transverse metatarsal ligament` *(left & right)*
  - `Superior extensor retinaculum of ankle` *(left & right)* — also: Transverse crural ligament
  - `Superior fibular retinaculum` *(left & right)* — also: Superior peroneal retinaculum
  - `Transverse intermuscular septum of leg` *(left & right)* — also: Transverse crural intermusclar septum

### Fasciae of thorax

- `Fasciae of thorax` — also: Fascia of thorax
  - `Clavipectoral fascia` *(left & right)*
  - `Pectoral fascia` *(left & right)*

### Fasciae of upper limb

- `Fasciae of upper limb` — also: Fascia of upper limb
  - `Antebrachial fascia` *(left & right)*
    - `Extensor retinaculum of wrist` *(left & right)* — also: Dorsal carpal ligament
  - `Brachial fascia` *(left & right)*
  - `Deltoid fascia` *(left & right)*
  - `Dorsal fascia of hand` *(left & right)*
  - `Flexor retinaculum of wrist` *(left & right)* — also: Transverse carpal ligament
  - `Lateral intermuscular septum of arm` *(left & right)*
  - `Medial intermuscular septum of arm` *(left & right)*
  - `Palmar aponeurosis` *(left & right)*
  - `Superficial transverse metacarpal ligament` *(left & right)*

### Thoracolumbar fascia

- `Thoracolumbar fascia` — also: Lumbodorsal fascia
  - `Anterior layer of thoracolumbar fascia` *(left & right)* — also: Deep layer of thoracolumbar fascia, Quadratus lumborum fascia
  - `Middle layer of thoracolumbar fascia` *(left & right)* — also: Intermediate layer of thoracolumbar fascia
  - `Posterior layer of thoracolumbar fascia` *(left & right)* — also: Superficial layer of thoracolumbar fascia

## Lymphatic system

Whole system: `Lymphoid organs` — also: Lymphoid system


### Primary lymphoid organs

- `Primary lymphoid organs`
  - `Thymus`
    - `Left lobe of thymus`
    - `Right lobe of thymus`

### Secondary lymphoid organs

- `Secondary lymphoid organs`
  - `Lymph nodes` — also: Lymphatic nodes
    - `Abdominal lymph nodes`
      - `Parietal abdominal lymph nodes`
        - `Inferior diaphragmatic nodes`
        - `Inferior epigastric nodes` *(left & right)*
        - `Left lumbar nodes`
          - `Lateral aortic nodes`
          - `Pre-aortic nodes` — also: Preaortic nodes
          - `Retro-aortic nodes` — also: Postaortic nodes
        - `Right lumbar nodes`
          - `Lateral caval nodes`
          - `Precaval nodes`
          - `Retrocaval nodes` — also: Postcaval nodes
      - `Visceral abdominal lymph nodes`
        - `Coeliac nodes` — also: Celiac nodes
        - `Hepatic nodes`
          - `Cystic node`
        - `Inferior mesenteric nodes`
          - `Left colic nodes`
          - `Sigmoid nodes`
        - `Intermediate lumbar nodes`
        - `Pancreatic nodes`
          - `Inferior pancreatic nodes`
          - `Superior pancreatic nodes`
        - `Pancreaticoduodenal nodes`
          - `Superior pancreaticoduodenal nodes`
        - `Pyloric nodes`
          - `(Retropyloric nodes)`
          - `(Subpyloric nodes)`
          - `(Suprapyloric node)`
        - `Right gastric nodes`
        - `Right gastro-omental nodes` — also: Right gastroomental nodes
        - `Splenic nodes` — also: Pancreaticosplenic nodes
        - `Superior mesenteric nodes`
          - `Appendicular nodes`
          - `Central superior mesenteric nodes`
          - `Ileocolic nodes`
          - `Juxta-intestinal mesenteric nodes` — also: Juxtaintestinal mesenteric nodes
          - `Middle colic nodes` — also: Mesocolic nodes
          - `Paracolic superior mesenteric nodes`
          - `Precaecal nodes` — also: Prececal nodes
          - `Retrocaecal nodes` — also: Retrocecal nodes
          - `Right colic nodes`
    - `Lymph nodes of head`
      - `Deep parotid nodes`
        - `Infra-auricular nodes` *(left & right)* — also: Infraauricular nodes
        - `Intraglandular parotid nodes` *(left & right)*
        - `Pre-auricular nodes` *(left & right)* — also: Preauricular nodes
      - `Facial nodes`
        - `Bucinator node` *(left & right)* — also: Buccinator node
        - `Mandibular node` *(left & right)*
        - `Nasolabial node` *(left & right)*
      - `Mastoid nodes` *(left & right)* — also: Postauricular nodes
      - `Occipital nodes` *(left & right)*
      - `Superficial parotid nodes` *(left & right)*
    - `Lymph nodes of lower limb`
      - `(Anterior tibial node)` *(left & right)*
      - `(Fibular node)` *(left & right)* — also: Peroneal node
      - `(Posterior tibial node)` *(left & right)*
      - `Inguinal nodes`
        - `Deep inguinal nodes`
          - `(Intermediate deep inguinal node)` *(left & right)*
          - `(Proximal deep inguinal node)` *(left & right)* — also: Node of Rosenmüller, Node of Rosenmuller, Node of Cloquet
        - `Superficial inguinal nodes`
          - `Inferior superficial inguinal nodes` *(left & right)*
          - `Superolateral superficial inguinal nodes` *(left & right)*
          - `Superomedial superficial inguinal nodes` *(left & right)*
      - `Popliteal nodes`
        - `Deep popliteal nodes` *(left & right)*
        - `Superficial popliteal nodes` *(left & right)*
    - `Lymph nodes of neck` — also: Cervical lymph nodes
      - `Anterior cervical nodes`
        - `Deep anterior cervical nodes`
          - `Paratracheal cervical nodes` — also: Paratracheal nodes of neck
          - `Pretracheal nodes` *(left & right)*
          - `Thyroid nodes` *(left & right)*
        - `Superficial anterior cervical nodes` — also: Anterior jugular nodes
      - `Lateral cervical nodes`
        - `Inferior deep lateral cervical nodes`
          - `Inferior deep lateral cervical nodes` *(left & right)*
            - `Anterior inferior jugular nodes` *(left & right)*
        - `Superficial lateral cervical nodes` *(left & right)*
        - `Superior deep lateral nodes`
          - `Jugulodigastric node` *(left & right)*
          - `Lateral superior jugular node` *(left & right)*
      - `Retropharyngeal nodes` *(left & right)*
      - `Submandibular nodes` *(left & right)*
      - `Submental nodes` *(left & right)*
      - `Supraclavicular nodes` *(left & right)*
    - `Lymph nodes of upper limb`
      - `Axillary nodes`
        - `Anterior axillary nodes` *(left & right)* — also: Pectoral nodes
        - `Apical axillary nodes` *(left & right)*
        - `Central axillary nodes` *(left & right)*
        - `Lateral axillary nodes` *(left & right)* — also: Humeral nodes
        - `Posterior axillary nodes` *(left & right)* — also: Subscapular nodes
      - `Brachial nodes` *(left & right)*
      - `Cubital nodes` *(left & right)*
        - `Supratrochlear nodes` *(left & right)*
      - `Infraclavicular nodes` *(left & right)* — also: Deltopectoral nodes
      - `Interpectoral nodes` *(left & right)*
    - `Pelvic lymph nodes`
      - `Parietal pelvic lymph nodes`
        - `Common iliac nodes`
          - `Intermediate common iliac nodes`
          - `Lateral common iliac nodes`
          - `Medial common iliac nodes`
          - `Median sacral nodes` — also: Promontorial nodes
          - `Subaortic nodes`
        - `External iliac nodes`
          - `(Intermediate lacunar node)` *(left & right)*
          - `(Lateral lacunar node)` *(left & right)*
          - `(Medial lacunar node)` *(left & right)*
          - `Intermediate external iliac nodes` *(left & right)*
          - `Medial external iliac nodes` *(left & right)*
          - `Obturator nodes` *(left & right)*
        - `Internal iliac nodes`
          - `Gluteal nodes`
            - `Inferior gluteal nodes` *(left & right)*
            - `Superior gluteal nodes` *(left & right)*
          - `Lateral sacral nodes` *(left & right)* — also: Sacral nodes
      - `Visceral pelvic lymph nodes`
        - `Pararectal nodes` — also: Middle anorectal nodes
        - `Paravesical nodes`
          - `Lateral vesical nodes` *(left & right)*
          - `Postvesical nodes` — also: Retrovesical nodes
          - `Prevesical nodes`
    - `Thoracic lymph nodes`
      - `Mediastinal lymph nodes`
        - `Brachiocephalic nodes`
        - `Inferior tracheobronchial nodes`
        - `Juxta-oesophageal nodes` — also: Juxtaoesophageal nodes
        - `Lateral pericardial nodes`
        - `Node of arch of azygos vein`
        - `Node of ligamentum arteriosum`
        - `Paratracheal thoracic nodes` — also: Paratracheal nodes of thorax
        - `Prepericardial nodes`
        - `Superior tracheobronchial nodes`
      - `Parietal thoracic lymph nodes`
        - `Intercostal nodes` *(left & right)*
        - `Parasternal nodes` *(left & right)*
        - `Prevertebral nodes`
        - `Superior diaphragmatic nodes`
      - `Pulmonary lymph nodes`
        - `Intrapulmonary nodes`
  - `Pharyngeal lymphoid ring` — also: Waldeyer's ring
    - `Palatine tonsil` *(left & right)*
  - `Spleen`

## Arteries (and the heart)

Whole system: `Arterial system` — also: Cardiovascular system


### Arteries of heart

- `Arteries of heart` — also: Cardiac arteries
  - `Left coronary artery`
    - `Anterior interventricular artery` — also: Anterior interventricular branch of left coronary artery, Left anterior descending artery, LAD
      - `Septal branches of anterior interventricular artery` — also: Interventricular septal branches of left coronary artery
    - `Circumflex artery of heart` — also: Circumflex branch of left coronary artery
  - `Right coronary artery`
    - `Right inferolateral branch of right coronary artery` — also: Right posterolateral branch of right coronary artery

### Heart

- `Heart` — also: Cardia
  - `Left atrium`
  - `Left ventricle`
    - `Inferior papillary muscle of left ventricle` — also: Posterior papillary muscle of left ventricle, Inferoseptal papillary muscle
  - `Right atrium`
  - `Right ventricle`
    - `Anterior papillary muscle of right ventricle`
    - `Inferior papillary muscle of right ventricle` — also: Posterior papillary muscle of right ventricle
    - `Septal papillary muscle of right ventricle` — also: Septal papillary muscle of the infundibulum, Medial papillary muscle, Conal papillary muscle, Muscle of Lancisi
  - `Valvular complex of heart`
    - `Right atrioventricular valve` — also: Tricuspid valve
      - `Inferior leaflet of right atrioventricular valve` — also: Posterior leaflet of right atrioventricular valve
      - `Posterior leaflet of left atrioventricular valve` — also: Mural leaflet of left atrioventricular valve, Posterior cusp of left atrioventricular valve
      - `Septal leaflet of right atrioventricular valve` — also: Septal cusp of right atrioventricular valve
    - `Root of aorta` — also: Aortic bulb
      - `Aortic valve` — also: Left ventriculoarterial junction
        - `Left coronary leaflet` — also: Left semilunar leaflet of aortic valve, Left semilunar cusp
        - `Non-coronary leaflet` — also: Noncoronary leaflet, Posterior semilunar leaflet of aortic valve, Nonadjacent leaflet of aortic valve, Posterior semilunar cusp, Noncoronary cusp
        - `Right coronary leaflet` — also: Right semilunar leaflet of aortic valve, Right semilunar cusp
    - `Root of pulmonary trunk`
      - `Pulmonary valve` — also: Valve of pulmonary trunk, Right ventriculoarterial junction
        - `Anterior semilunar leaflet of pulmonary valve` — also: Non-adjacent leaflet of pulmonary valve, Anterior semilunar cusp of pulmonary valve
        - `Left semilunar leaflet of pulmonary valve` — also: Left adjacent leaflet of pulmonary valve, Left semilunar cusp of pulmonary valve
        - `Right semilunar leaflet of pulmonary valve` — also: Right adjacent leaflet of pulmonary valve, Right semilunar cusp of pulmonary valve

### Pulmonary arteries

- `Pulmonary arteries`
  - `Left pulmonary artery`
    - `Inferior lobar artery of left lung`
      - `Anterior basal segmental artery of left lung`
      - `Lateral basal segmental artery of left lung`
      - `Medial basal segmental artery of left lung`
      - `Posterior basal segmental artery of left lung`
      - `Superior segmental artery of left lung`
    - `Superior lobar artery of left lung`
      - `Anterior segmental artery of left lung`
      - `Apical segmental artery of left lung`
      - `Inferior lingular artery of left lung` — also: Ramus inferior of lingular artery
      - `Posterior segmental artery of left lung`
      - `Superior lingular artery of left lung` — also: Ramus superior of lingular artery
  - `Pulmonary trunk`
    - `Bifurcation of pulmonary trunk`
  - `Right pulmonary artery`
    - `Inferior lobar artery of right lung`
      - `Anterior basal segmental artery of right lung`
      - `Lateral basal segmental artery of right lung`
      - `Medial basal segmental artery of right lung`
      - `Posterior basal segmental artery of right lung`
      - `Superior segmental artery of right lung`
    - `Middle lobar artery of right lung`
      - `Lateral segmental artery of right lung`
      - `Medial segmental artery of right lung`
    - `Superior lobar artery of right lung`
      - `Anterior segmental artery of right lung`
      - `Apical segmental artery of right lung`
      - `Posterior segmental artery of right lung`

### Systemic arteries

- `Systemic arteries`
  - `Aorta`
    - `Abdominal aorta`
      - `Coeliac trunk` — also: Celiac trunk, Haller's tripus
        - `Common hepatic artery`
          - `Gastroduodenal artery`
          - `Proper hepatic artery` — also: Hepatic artery proper
        - `Left gastric artery`
        - `Splenic artery`
      - `Inferior mesenteric artery`
        - `Left colic artery`
          - `Ascending branch of left colic artery` — also: Ascending artery
          - `Descending branch of left colic artery`
        - `Sigmoid arteries`
        - `Superior anorectal artery` — also: Superior rectal artery, Superior hemorrhoidal artery
      - `Inferior phrenic artery`
      - `Left renal artery`
        - `Anterior branch of renal artery` *(left)*
        - `Inferior suprarenal artery` *(left)*
        - `Intrarenal arteries of left kidney`
        - `Posterior branch of renal artery` *(left)*
      - `Left testicular artery`
      - `Lumbar arteries` *(left & right)*
      - `Right renal artery`
        - `Anterior branch of renal artery` *(right)*
        - `Inferior suprarenal artery` *(right)*
        - `Intrarenal arteries of right kidney`
        - `Posterior branch of renal artery` *(right)*
      - `Right testicular artery` *(right)*
      - `Superior mesenteric artery`
        - `Ileocolic artery`
          - `Appendicular artery`
          - `Colic branch of ileocolic artery`
          - `Ileal branch of ileocolic artery`
        - `Inferior pancreaticoduodenal artery`
          - `Anterior inferior pancreaticoduodenal artery` — also: Anterior branch of inferior pancreaticoduodenal artery
        - `Marginal artery` — also: Juxtacolic artery, Marginal arcade, Marginal artery of Drummond
        - `Middle colic artery`
        - `Right colic artery`
    - `Aortic arch` — also: Arch of aorta
      - `Brachiocephalic trunk` — also: Innominate artery
    - `Aortic bifurcation` — also: Bifurcation of aorta
      - `Common iliac artery` *(left & right)*
        - `External iliac artery` *(left & right)*
          - `Inferior epigastric artery` *(left & right)*
        - `Internal iliac artery` *(left & right)* — also: Hypogastric artery
          - `Anterior division of internal iliac artery` *(left & right)* — also: Anterior trunk of internal iliac artery
            - `Inferior gluteal artery` *(left & right)*
            - `Internal pudendal artery` *(left & right)*
              - `Deep artery of penis` *(left & right)*
              - `Dorsal artery of penis` *(left & right)*
            - `Obturator artery` *(left & right)*
          - `Posterior division of internal iliac artery` *(left & right)* — also: Posterior trunk of internal iliac artery
            - `Iliolumbar artery` *(left & right)*
              - `Iliacus branch of iliolumbar artery` *(left & right)*
              - `Lumbar branch of iliolumbar artery` *(left & right)*
              - `Spinal branch of iliolumbar artery` *(left & right)*
            - `Superior gluteal artery` *(left & right)*
      - `Median sacral artery`
    - `Ascending aorta`
    - `Thoracic aorta` — also: Descending thoracic aorta
      - `Posterior intercostal arteries` *(left & right)*
      - `Subcostal artery` *(left & right)*
      - `Superior phrenic arteries`
  - `Arteries of spinal cord`
    - `Anterior spinal artery` *(left & right)*
  - `Cerebral arterial circle` — also: Circle of Willis
    - `Anterior communicating artery`
    - `Posterior communicating artery` *(left & right)*
  - `Femoral artery` *(left & right)*
    - `Deep external pudendal artery` *(left & right)*
    - `Deep femoral artery` *(left & right)* — also: Deep artery of thigh
      - `Lateral circumflex femoral artery` *(left & right)*
        - `Descending branch of lateral circumflex femoral artery` *(left)*
      - `Medial circumflex femoral artery` *(left & right)*
      - `Perforating femoral arteries` *(left & right)*
    - `Popliteal artery` *(left & right)*
      - `Anterior tibial artery` *(left & right)*
        - `Dorsalis pedis artery` *(left & right)* — also: Dorsal artery of foot
          - `Arcuate artery` *(left & right)*
            - `Dorsal metatarsal arteries` *(left & right)*
              - `Dorsal digital arteries of foot` *(left & right)*
          - `Deep plantar artery` *(left & right)*
          - `Lateral tarsal artery` *(left & right)*
      - `Inferior lateral genicular artery` *(left & right)*
      - `Inferior medial genicular artery` *(left & right)*
      - `Middle genicular artery` *(left & right)*
      - `Patellar anastomosis` *(left & right)*
      - `Posterior tibial artery` *(left & right)*
        - `Calcaneal branches of posterior tibial artery` *(left & right)*
        - `Fibular artery` *(left & right)* — also: Peroneal artery
          - `Calcaneal branches of fibular artery` *(left & right)*
        - `Lateral plantar artery` *(left & right)*
          - `Plantar arch` *(left & right)* — also: Deep plantar arch
            - `Plantar metatarsal arteries` *(left & right)*
              - `Common plantar digital arteries` *(left & right)*
                - `Proper plantar digital arteries` *(left & right)* — also: Plantar digital arteries proper
              - `Perforating branches of plantar metatarsal arteries` *(left & right)*
        - `Medial plantar artery` *(left & right)*
          - `Superficial branch of medial plantar artery` *(left & right)*
      - `Superior lateral genicular artery` *(left & right)*
      - `Superior medial genicular artery` *(left & right)*
    - `Superficial epigastric artery` *(left & right)*
    - `Superficial external pudendal artery` *(left & right)*
  - `Left common carotid artery`
    - `External carotid artery` *(left)*
      - `Ascending pharyngeal artery` *(left)*
      - `Facial artery` *(left)* — also: External maxillary artery
        - `Angular artery` *(left)*
        - `Inferior labial artery` *(left)* — also: Inferior labial branch of facial artery
        - `Submental artery` *(left)*
        - `Superior labial artery` *(left)* — also: Superior labial branch of facial artery
      - `Maxillary artery` *(left)* — also: Internal maxillary artery
        - `Anterior deep temporal artery` *(left)*
        - `Artery of pterygoid canal` *(left)* — also: Vidian artery
          - `?x` *(left)*
        - `Buccal artery` *(left)*
        - `Descending palatine artery` *(left)*
          - `Greater palatine artery` *(left)*
        - `Inferior alveolar artery` *(left)*
          - `Mental branch of inferior alveolar artery` *(left)*
          - `Mylohyoid branch of inferior alveolar artery` *(left)*
        - `Infra-orbital artery` *(left)* — also: Infraorbital artery
        - `Middle meningeal artery` *(left)*
          - `Accessory branch of middle meningeal artery` *(left)* — also: Pterygomeningeal artery, Accessory meningeal artery
        - `Posterior deep temporal artery` *(left)*
        - `Posterior lateral nasal branches of sphenopalatine artery.` *(left)*
        - `Posterior septal branches of sphenopalatine artery` *(left)*
        - `Posterior superior alveolar artery` *(left)*
      - `Occipital artery` *(left)*
      - `Superficial temporal artery` *(left)*
        - `Frontal branch of superficial temporal artery` *(left)*
        - `Transverse facial artery` *(left)*
    - `Internal carotid artery` *(left)*
      - `Middle cerebral artery (M1-segment)` *(left)*
        - `Distal lateral striate branches` *(left)*
        - `Insular branches of middle cerebral artery (M2-segment)` *(left)*
          - `Anterior temporal branch` *(left)*
          - `Artery of precentral sulcus` *(left)*
          - `Middle cerebral artery (M3 segment)` *(left)*
            - `Artery of central sulcus` *(left)*
            - `Parietal branches of middle cerebral artery` *(left)*
            - `Postcentral arterial branch` *(left)*
            - `Posterior parietal artery` *(left)*
            - `Posterior temporal branch` *(left)*
            - `Temporal branches of middle cerebral artery` *(left)*
            - `Temporo-occipital branch` *(left)*
              - `Branch to angular gyrus` *(left)*
          - `Prefrontal artery` *(left)*
            - `Lateral frontobasal artery` *(left)*
        - `Proximal lateral striate branches` *(left)*
      - `Ophthalmic artery` *(left)*
        - `Anterior ethmoidal artery` *(left)*
          - `Anterior septal branches of anterior ethmoidal artery` *(left)*
        - `Central retinal artery` *(left)* — also: Zinn's artery
        - `Lacrimal artery` *(left)*
        - `Long posterior ciliary arteries` *(left)*
        - `Posterior ethmoidal artery` *(left)*
          - `Septal branches of posterior ethmoidal artery` *(left)*
        - `Short posterior ciliary arteries` *(left)*
        - `Supra-orbital artery` *(left)* — also: Supraorbital artery
        - `Supratrochlear artery` *(left)*
  - `Left subclavian artery`
    - `Axillary artery` *(left)*
      - `Anterior circumflex humeral artery` *(left)*
      - `Brachial artery` *(left)*
        - `Deep brachial artery` *(left)* — also: Profunda brachii artery, Deep artery of arm
          - `Middle collateral artery` *(left)* — also: Posterior branch of deep brachial artery
          - `Radial collateral artery` *(left)* — also: Anterior branch of deep brachial artery
        - `Inferior ulnar collateral artery` *(left)*
        - `Radial artery` *(left)*
          - `Deep palmar arch` *(left)*
            - `Palmar metacarpal arteries` *(left)*
          - `Dorsal carpal anastomosis` *(left)* — also: Dorsal carpal network
            - `Dorsal metacarpal arteries` *(left)*
              - `Dorsal digital arteries of hand` *(left)*
          - `Palmar carpal branch of radial artery` *(left)*
        - `Superior ulnar collateral artery` *(left)*
        - `Ulnar artery` *(left)*
          - `(Ulnar recurrent artery)` *(left)*
          - `Common interosseous artery` *(left)*
            - `Posterior interosseous artery` *(left)*
            - `Recurrent interosseous artery` *(left)*
          - `Dorsal carpal branch of ulnar artery` *(left)*
          - `Superficial palmar arch` *(left)*
            - `Common palmar digital arteries` *(left)*
              - `Proper palmar digital arteries` *(left)*
      - `Lateral thoracic artery` *(left)*
      - `Posterior circumflex humeral artery` *(left)*
      - `Subscapular artery` *(left)*
        - `Circumflex scapular artery` *(left)*
        - `Thoracodorsal artery` *(left)*
      - `Thoraco-acromial artery` *(left)* — also: Thoracoacromial artery
        - `Pectoral branches of thoraco-acromial artery` *(left)* — also: Pectoral branches of thoracoacromial artery
    - `Costocervical trunk` *(left)*
      - `Deep cervical artery` *(left)*
      - `Supreme intercostal artery` *(left)* — also: Highest intercostal artery
        - `First posterior intercostal artery` *(left)*
        - `Second posterior intercostal artery` *(left)*
    - `Internal thoracic artery` *(left)* — also: Internal mammary artery
      - `Musculophrenic artery` *(left)*
      - `Superior epigastric artery` *(left)*
    - `Thyrocervical trunk` *(left)*
      - `Inferior thyroid artery` *(left)*
      - `Suprascapular artery` *(left)* — also: Transverse scapular artery
      - `Transverse cervical artery` *(left)*
        - `(Deep branch of transverse cervical artery)` *(left)*
        - `(Superficial branch of transverse cervical artery)` *(left)*
    - `Vertebral artery` *(left)*
      - `Posterior inferior cerebellar artery` *(left)*
  - `Right common carotid artery`
    - `External carotid artery` *(right)*
      - `Ascending pharyngeal artery` *(right)*
      - `Facial artery` *(right)* — also: External maxillary artery
        - `Angular artery` *(right)*
        - `Inferior labial artery` *(right)* — also: Inferior labial branch of facial artery
        - `Submental artery` *(right)*
        - `Superior labial artery` *(right)* — also: Superior labial branch of facial artery
      - `Maxillary artery` *(right)* — also: Internal maxillary artery
        - `Anterior deep temporal artery` *(right)*
        - `Artery of pterygoid canal` *(right)* — also: Vidian artery
          - `?x` *(right)*
        - `Buccal artery` *(right)*
        - `Descending palatine artery` *(right)*
          - `Greater palatine artery` *(right)*
        - `Inferior alveolar artery` *(right)*
          - `Mental branch of inferior alveolar artery` *(right)*
          - `Mylohyoid branch of inferior alveolar artery` *(right)*
        - `Infra-orbital artery` *(right)* — also: Infraorbital artery
        - `Middle meningeal artery` *(right)*
          - `Accessory branch of middle meningeal artery` *(right)* — also: Pterygomeningeal artery, Accessory meningeal artery
        - `Posterior deep temporal artery` *(right)*
        - `Posterior lateral nasal branches of sphenopalatine artery.` *(right)*
        - `Posterior septal branches of sphenopalatine artery` *(right)*
        - `Posterior superior alveolar artery` *(right)*
      - `Occipital artery` *(right)*
      - `Superficial temporal artery` *(right)*
        - `Frontal branch of superficial temporal artery` *(right)*
        - `Transverse facial artery` *(right)*
    - `Internal carotid artery` *(right)*
      - `Anterior cerebral artery` *(left & right)*
        - `Callosomarginal artery` *(left & right)*
          - `Frontal branches of callosomarginal artery` *(left & right)*
        - `Orbitofrontal branches of anterior cerebral artery` *(left & right)* — also: Medial orbitofrontal arteries
        - `Pericallosal artery` *(left & right)*
      - `Middle cerebral artery (M1-segment)` *(right)*
        - `Distal lateral striate branches` *(right)*
        - `Insular branches of middle cerebral artery (M2)` *(right)* — also: Insular arteries
          - `Anterior temporal branch` *(right)*
          - `Artery of precentral sulcus` *(right)*
          - `Middle cerebral artery (M3-segment)` *(right)*
            - `Artery of central sulcus` *(right)*
            - `Parietal branches of middle cerebral artery` *(right)*
            - `Postcentral arterial branch` *(right)*
            - `Posterior parietal artery` *(right)*
            - `Posterior temporal branch` *(right)*
            - `Temporal branches of middle cerebral artery` *(right)*
            - `Temporo-occipital branch` *(right)*
              - `Branch to angular gyrus` *(right)*
          - `Prefrontal artery` *(right)*
            - `Lateral frontobasal artery` *(right)*
        - `Proximal lateral striate branches` *(right)*
      - `Ophthalmic artery` *(right)*
        - `Anterior ethmoidal artery` *(right)*
          - `Anterior septal branches of anterior ethmoidal artery` *(right)*
        - `Central retinal artery` *(right)* — also: Zinn's artery
        - `Lacrimal artery` *(right)*
        - `Long posterior ciliary arteries` *(right)*
        - `Posterior ethmoidal artery` *(right)*
          - `Septal branches of posterior ethmoidal artery` *(right)*
        - `Short posterior ciliary arteries` *(right)*
        - `Supra-orbital artery` *(right)* — also: Supraorbital artery
        - `Supratrochlear artery` *(right)*
  - `Right subclavian artery`
    - `Axillary artery` *(right)*
      - `Anterior circumflex humeral artery` *(right)*
      - `Brachial artery` *(right)*
        - `Deep brachial artery` *(right)* — also: Profunda brachii artery, Deep artery of arm
          - `Middle collateral artery` *(right)* — also: Posterior branch of deep brachial artery
          - `Radial collateral artery` *(right)* — also: Anterior branch of deep brachial artery
        - `Inferior ulnar collateral artery` *(right)*
        - `Radial artery` *(right)*
          - `Deep palmar arch` *(right)*
            - `Palmar metacarpal arteries` *(right)*
          - `Dorsal carpal anastomosis` *(right)* — also: Dorsal carpal network
            - `Dorsal metacarpal arteries` *(right)*
              - `Dorsal digital arteries of hand` *(right)*
          - `Palmar carpal branch of radial artery` *(right)*
        - `Superior ulnar collateral artery` *(right)*
        - `Ulnar artery` *(right)*
          - `(Ulnar recurrent artery)` *(right)*
          - `Common interosseous artery` *(right)*
            - `Posterior interosseous artery` *(right)*
            - `Recurrent interosseous artery` *(right)*
          - `Dorsal carpal branch of ulnar artery` *(right)*
          - `Superficial palmar arch` *(right)*
            - `Common palmar digital arteries` *(right)*
              - `Proper palmar digital arteries` *(right)*
      - `Lateral thoracic artery` *(right)*
      - `Posterior circumflex humeral artery` *(right)*
      - `Subscapular artery` *(right)*
        - `Circumflex scapular artery` *(right)*
        - `Thoracodorsal artery` *(right)*
      - `Thoraco-acromial artery` *(right)* — also: Thoracoacromial artery
        - `Pectoral branches of thoraco-acromial artery` *(right)* — also: Pectoral branches of thoracoacromial artery
    - `Costocervical trunk` *(right)*
      - `Deep cervical artery` *(right)*
      - `Supreme intercostal artery` *(right)* — also: Highest intercostal artery
        - `First posterior intercostal artery` *(right)*
        - `Second posterior intercostal artery` *(right)*
    - `Internal thoracic artery` *(right)* — also: Internal mammary artery
      - `Musculophrenic artery` *(right)*
      - `Superior epigastric artery` *(right)*
    - `Thyrocervical trunk` *(right)*
      - `Inferior thyroid artery` *(right)*
      - `Suprascapular artery` *(right)* — also: Transverse scapular artery
      - `Transverse cervical artery` *(right)*
        - `(Deep branch of transverse cervical artery)` *(right)*
        - `(Superficial branch of transverse cervical artery)` *(right)*
    - `Vertebral artery` *(right)*
      - `Basilar artery`
        - `Anterior inferior cerebellar artery` *(left & right)*
        - `Lateral pontine branches of basilar artery` *(left & right)* — also: Long circumferential pontine branches of basilar artery
        - `Medial pontine branches of basilar artery` *(left & right)* — also: Paramedian pontine branches of basilar artery
        - `Posterior cerebral artery` *(left & right)*
          - `Lateral occipital artery` *(left)*
          - `Medial occipital artery` *(left & right)*
            - `Lateral occipital artery` *(right)*
          - `Parieto-occipital artery` *(left & right)* — also: Parietooccipital artery
        - `Superior cerebellar artery` *(left & right)*
      - `Posterior inferior cerebellar artery` *(right)*

## Veins

Whole system: `Venous system` — also: Cardiovascular system


### Cardiac veins

- `Cardiac veins` — also: Veins of heart
  - `Coronary sinus`
    - `Great cardiac vein`
    - `Inferior vein of left ventricle` — also: Posterior vein of left ventricle
    - `Inferior vein of left ventricle (//Posterior '')`
    - `Middle cardiac vein` — also: Inferior interventricular vein, Posterior interventricular vein

### Pulmonary veins

- `Pulmonary veins`
  - `Left inferior pulmonary vein`
    - `Inferior basal vein of left lung`
    - `Superior basal vein of left lung`
    - `Superior vein of left lung`
  - `Left superior pulmonary vein`
    - `Anterior vein of left lung`
    - `Apicoposterior vein of left lung`
    - `Lingular vein of left lung`
      - `Inferior lingular vein of left lung`
      - `Superior lingular vein of left lung`
  - `Right inferior pulmonary vein`
    - `Inferior basal vein of right lung`
    - `Superior basal vein of right lung`
    - `Superior vein of right lung`
  - `Right superior pulmonary vein`
    - `Anterior vein of right lung`
    - `Apical vein of right lung`
    - `Lateral vein of right lung` — also: Lateral part of middle lobe vein
    - `Medial vein of right lung` — also: Medial part of middle lobe vein
    - `Posterior vein of right lung`

### Systemic veins

- `Systemic veins`
  - `Cranial veins`
    - `Dural venous sinuses`
      - `Inferior petrosal sinus` *(left & right)*
        - `Basilar venous plexus` — also: Basilar plexus
          - `Anterior intercavernous sinus`
          - `Posterior intercavernous sinus`
        - `Cavernous sinus` *(left & right)*
      - `Sigmoid sinus` *(left & right)*
        - `Transverse sinus` *(left & right)*
          - `Superior petrosal sinus` *(left & right)*
          - `Confluence of sinuses` — also: Torcular of Herophilus
            - `Inferior sagittal sinus`
            - `Occipital sinus`
            - `Straight sinus`
            - `Superior sagittal sinus`
    - `Orbital veins`
      - `Inferior ophthalmic vein` *(left & right)*
      - `Superior ophthalmic vein` *(left & right)*
  - `Femoral vein` *(left & right)*
    - `Deep femoral vein` *(left & right)* — also: Profunda femoris vein, Deep vein of thigh
      - `Perforating veins` *(left & right)*
    - `Great saphenous vein` *(left & right)* — also: Long saphenous vein
      - `External pudendal veins` *(left & right)*
        - `Superficial dorsal veins of penis`
      - `Superficial epigastric vein` *(left & right)*
    - `Lateral circumflex femoral veins` *(left & right)*
    - `Medial circumflex femoral veins` *(left & right)*
    - `Popliteal vein` *(left & right)*
      - `Anterior tibial veins` *(left & right)*
      - `Genicular veins` *(left & right)*
      - `Posterior tibial veins` *(left & right)*
        - `Fibular veins` *(left & right)* — also: Peroneal veins
        - `Lateral plantar veins` *(left & right)*
          - `Plantar venous arch` *(left & right)*
            - `Plantar metatarsal veins` *(left & right)*
              - `Plantar digital veins` *(left & right)*
        - `Medial plantar veins` *(left & right)*
      - `Small saphenous vein` *(left & right)* — also: Short saphenous vein
        - `Dorsal venous arch of foot` *(left & right)*
          - `Dorsal metatarsal veins` *(left & right)*
            - `Dorsal digital veins of foot` *(left & right)*
          - `Intercapitular veins of foot` *(left & right)*
  - `Hepatic portal vein`
    - `Tributaries of hepatic portal vein`
      - `Splenic vein` — also: Lienal vein
        - `Inferior mesenteric vein`
          - `Sigmoid veins`
        - `Left gastro-omental vein` — also: Left gastroomental vein
      - `Superior mesenteric vein`
        - `Middle colic vein`
        - `Right colic vein`
        - `Right gastro-omental vein` — also: Right gastroomental vein, Right gastroepiploic vein, Right gastro-epiploic vein
  - `Inferior vena cava (thoracic part)`
    - `Hepatic veins`
    - `Inferior vena cava (abdominal part)`
      - `Common iliac vein` *(left & right)*
        - `External iliac vein` *(left & right)*
          - `Inferior epigastric vein` *(left & right)*
        - `Iliolumbar vein` *(left & right)*
        - `Internal iliac vein` *(left & right)* — also: Hypogastric vein
          - `Inferior gluteal veins` *(left & right)*
          - `Internal pudendal vein` *(left & right)*
          - `Lateral sacral veins` *(left & right)*
          - `Superior gluteal veins` *(left & right)*
          - `Deep dorsal vein of penis`
      - `Left renal vein`
        - `Intrarenal veins of left kidney`
        - `Left testicular vein`
      - `Lumbar veins` *(left & right)*
      - `Right renal vein`
        - `Intrarenal veins of right kidney`
      - `Right testicular vein`
  - `Internal jugular vein` *(left & right)*
    - `Common facial vein` *(left & right)*
      - `Anterior division of retromandibular vein` *(left & right)*
        - `Retromandibular vein` *(left & right)* — also: Posterior facial vein
          - `Maxillary veins` *(left & right)*
          - `Superficial temporal veins` *(left & right)*
      - `Facial vein` *(left & right)* — also: Anterior facial vein
        - `Angular vein` *(left & right)*
        - `Inferior labial veins` *(left & right)*
        - `Submental vein` *(left & right)*
        - `Superior labial vein` *(left & right)*
    - `Lingual vein` *(left & right)*
    - `Superior thyroid vein` *(left & right)*
  - `Left subclavian vein`
    - `Axillary vein` *(left)*
      - `Anterior circumflex humeral vein` *(left)*
      - `Basilic vein` *(left)*
        - `Median cubital vein` *(left)* — also: Median vein of forearm
          - `Median antebrachial vein` *(left)*
      - `Brachial veins` *(left)*
        - `Radial veins` *(left)*
          - `Deep venous palmar arch` *(left)*
        - `Ulnar veins` *(left)*
          - `Superficial venous palmar arch` *(left)*
            - `Palmar digital veins` *(left)*
      - `Cephalic vein` *(left)*
        - `Dorsal venous network of hand` *(left)*
          - `Dorsal digital veins of hand` *(left)*
      - `Lateral thoracic vein` *(left)*
      - `Posterior circumflex humeral vein` *(left)*
      - `Subscapular vein` *(left)*
        - `Circumflex scapular vein` *(left)*
        - `Thoracodorsal vein` *(left)*
    - `External jugular vein` *(left)*
      - `Anterior jugular vein` *(left)*
      - `Posterior auricular vein` *(left)*
      - `Posterior division of retromandibular vein` *(left)*
      - `Suprascapular vein` *(left)* — also: Transverse scapular vein
  - `Right subclavian vein`
    - `Axillary vein` *(right)*
      - `Anterior circumflex humeral vein` *(right)*
      - `Basilic vein` *(right)*
        - `Median cubital vein` *(right)* — also: Median vein of forearm
          - `Median antebrachial vein` *(right)*
      - `Brachial veins` *(right)*
        - `Radial veins` *(right)*
          - `Deep venous palmar arch` *(right)*
        - `Ulnar veins` *(right)*
          - `Superficial venous palmar arch` *(right)*
            - `Palmar digital veins` *(right)*
      - `Cephalic vein` *(right)*
        - `Dorsal venous network of hand` *(right)*
          - `Dorsal digital veins of hand` *(right)*
      - `Lateral thoracic vein` *(right)*
      - `Posterior circumflex humeral vein` *(right)*
      - `Subscapular vein` *(right)*
        - `Circumflex scapular vein` *(right)*
        - `Thoracodorsal vein` *(right)*
    - `External jugular vein` *(right)*
      - `Anterior jugular vein` *(right)*
      - `Posterior auricular vein` *(right)*
      - `Posterior division of retromandibular vein` *(right)*
      - `Suprascapular vein` *(right)* — also: Transverse scapular vein
  - `Superior vena cava`
    - `Azygos vein`
      - `Accessory hemi-azygos vein` — also: Superior hemiazygos vein, Superior hemi-azygos vein
      - `Hemi-azygos vein` — also: Inferior hemiazygos vein, Inferior hemi-azygos vein
        - `Left ascending lumbar vein`
        - `Left subcostal vein`
      - `Right ascending lumbar vein`
      - `Right subcostal vein`
      - `Right superior intercostal vein`
      - `Right superior phrenic vein`
    - `Left brachiocephalic vein` — also: Left innominate vein
      - `Internal thoracic veins` *(left)*
        - `Musculophrenic veins` *(left)*
        - `Superior epigastric veins` *(left)*
      - `Left superior intercostal vein`
      - `Vertebral vein` *(left)*
        - `Occipital vein` *(left)*
    - `Right brachiocephalic vein` — also: Right innominate vein
      - `Internal thoracic veins` *(right)*
        - `Musculophrenic veins` *(right)*
        - `Superior epigastric veins` *(right)*
      - `Vertebral vein` *(right)*
        - `Occipital vein` *(right)*

## Nervous system

Whole system: `Nervous system` — also: Neurology


### Central nervous system

- `Central nervous system`
  - `Brain`
    - `Brainstem`
      - `Fourth ventricle`
      - `Medulla oblongata` *(left & right)* — also: Myelencephalon, Bulb
        - `Olive` *(left & right)*
        - `Pyramid of medulla oblongata` *(left & right)*
        - `Grey matter of medulla oblongata` — also: Gray matter of medulla oblongata
          - `Anterior cochlear nucleus` *(left & right)* — also: Ventral cochlear nucleus
          - `Inferior salivatory nucleus` *(left & right)*
          - `Nucleus ambiguus` *(left & right)*
          - `Nucleus of hypoglossal nerve` *(left & right)* — also: Hypoglossal nucleus, Nucleus of cranial nerve XII
          - `Nucleus of solitary tract` *(left & right)* — also: Solitary nucleus
          - `Posterior cochlear nucleus` *(left & right)* — also: Dorsal cochlear nucleus
          - `Posterior nucleus of vagus nerve` *(left & right)* — also: Dorsal nucleus of vagus nerve, Dorsal vagal nucleus
      - `Midbrain`
        - `Central mesencephalic structures` — also: Central stuctures of midbrain
          - `Aqueduct of midbrain` — also: Cerebral aqueduct
        - `Cerebral peduncle`
          - `Base of peduncle` *(left & right)*
          - `Tegmentum of midbrain` — also: Mesencephalic tegmentum
            - `Grey matter of tegmentum of midbrain` — also: Gray matter of tegmentum of midbrain
              - `Nucleus of trochlear nerve` *(left & right)* — also: Trochlear nucleus, Nucleus of cranial nerve IV
              - `Oculomotor complex`
                - `Accessory nucleus of oculomotor nerve` *(left & right)* — also: Accessory oculomotor nucleus, Edinger-Westphal nucleus
                - `Nucleus of oculomotor nerve` *(left & right)* — also: Oculomotor nucleus, Nucleus of cranial nerve III
              - `Red nucleus` *(left & right)*
        - `Interpeduncular fossa` *(left & right)*
        - `Midbrain` *(left & right)*
        - `Tectum of midbrain` — also: Tectal plate, Quadrigeminal plate
          - `Inferior colliculus` *(left & right)*
          - `Superior colliculus` *(left & right)*
      - `Pons` *(left & right)*
        - `Tegmentum of pons` — also: Pontine tegmentum
          - `Grey matter of pontine tegmentum` — also: Gray matter of pontine tegmentum
            - `Motor nucleus of facial nerve` *(left & right)* — also: Nucleus of facial nerve, Facial nucleus, Motor nucleus of cranial nerve VII
            - `Nucleus of abducens nerve` *(left & right)* — also: Abducens nucleus, Nucleus of cranial nerve VI
            - `Superior salivatory nucleus` *(left & right)*
            - `Vestibular nuclei` *(left & right)*
    - `Cerebellum`
      - `Anterior quadrangular lobule` *(left & right)*
      - `Biventral lobule` *(left & right)* — also: Lobule VIII of cerebellar hemisphere, Lobule H VIII
      - `Central lobule`
      - `Culmen`
      - `Declive` — also: Lobule VI of vermis
      - `Flocculus` *(left & right)* — also: Lobule X of cerebellar hemisphere, Lobule H X
      - `Folium of vermis` — also: Lobule VIIA of vermis
      - `Gracile lobule` *(left & right)* — also: Lobule VIIB of cerebellar hemisphere, Paramedian lobule, Lobule H VIIB
      - `Inferior semilunar lobule` *(left & right)* — also: Crus II of ansiform lobule
      - `Lingula of cerebellum` — also: Lobule I of vermis
      - `Nodule of vermis` — also: Lobule X of vermis
      - `Peduncle of flocculus` *(left & right)* — also: Tract of Loewy, Stalk of flocculus
      - `Posterior quadrangular lobule` *(left & right)* — also: Lobule VI of cerebellar hemisphere, Simple lobule, Lobulus VI hemisphaerii cerebelli, Lobulus simplex
      - `Pyramis of vermis` — also: Lobule VIII of vermis
      - `Superior cerebellar peduncle` *(left & right)*
      - `Superior semilunar lobule` *(left & right)* — also: Crus I of ansiform lobule
      - `Tonsil of cerebellum` *(left & right)* — also: Lobule IX of cerebellar hemisphere, Lobule H IX
      - `Tuber of vermis` — also: Lobule VIIB of vermis
      - `Uvula of vermis` — also: Lobule IX of vermis
      - `Wing of central lobule` *(left & right)*
    - `Cerebrum` — also: Forebrain
      - `Diencephalon`
        - `Habenula`
        - `Hypothalamus` — also: Floor of third ventricle
        - `Lateral geniculate body` *(left & right)*
        - `Mamillary body` *(left & right)* — also: Mammillary body
        - `Medial geniculate body` *(left & right)*
        - `Optic chiasm` *(left & right)* — also: Optic chiasma
        - `Optic tract` *(left & right)*
        - `Thalamus` *(left & right)* — also: Dorsal thalamus
        - `Third ventricle`
        - `White matter of diencephalon`
          - `Posterior commissure`
          - `Stria medullaris thalami` *(left & right)*
      - `Telencephalon`
        - `Basal forebrain`
          - `Amygdaloid body` *(left & right)* — also: Amygdaloid complex, Amygdala
          - `Septal nuclei` — also: Precommissural septum, Cholinergic cell group Ch1
        - `Corpus striatum` — also: Basal nuclei of telencephalon, Basal ganglia
          - `Dorsal striatum` — also: Neostriatum
            - `Caudate nucleus` *(left & right)*
            - `Putamen` *(left & right)*
          - `Globus pallidus` *(left & right)* — also: Dorsal pallidum
          - `Lentiform nucleus` *(left & right)* — also: Lenticular nucleus
        - `Frontal lobe`
          - `Inferior frontal sulcus` *(left & right)*
            - `Orbital part of  inferior frontal gyrus` *(right)*
          - `Lat_Fis-ant-Horizont` *(left & right)*
          - `Lat_Fis-ant-Vertical` *(left & right)*
          - `Middle frontal gyrus` *(left & right)*
          - `Olfactory sulcus` *(left & right)*
          - `Opercular part of inferior frontal gyrus` *(left & right)*
          - `Orbital gyri (Frontomarginal gyrus and sulcus*)` *(left & right)*
          - `Orbital gyri` *(left & right)*
          - `Orbital part of  inferior frontal gyrus` *(left)*
          - `Orbital sulci (H-shaped orbital sulci*)` *(left & right)*
          - `Orbital sulci (Lateral Orbital sulcus*)` *(left & right)*
          - `Paracentral gyrus and sulcus*` *(left & right)*
          - `Paracentral sulcus` *(left & right)*
          - `Precentral gyrus` *(left & right)*
          - `Precentral sulcus (inferior part)*` *(left & right)*
          - `Precentral sulcus (Superior part)*` *(left & right)*
          - `Straight gyrus (Gyrus rectus)` *(left & right)*
          - `Superior frontal gyrus` *(left & right)*
          - `Superior frontal sulcus` *(left & right)*
          - `Transverse frontopolar gyrus and sulcus*` *(left & right)*
          - `Triangular part of inferior frontal gyrus` *(left & right)*
        - `Insula` — also: Insular lobe
          - `Insula (Subcentral gyrus and ant. and post. sulci*)` *(left & right)*
        - `Interlobar sulci`
          - `' Posterior transverse collateral sulcus`
          - `Anterior occipital sulcus*` *(left & right)*
          - `Central sulcus` *(left & right)* — also: Fissure of Rolando
          - `Circular sulcus of insula` *(left & right)*
          - `Collateral sulcus` *(left & right)*
          - `Lat_Fis-post` *(left & right)*
          - `Parieto-occipital sulcus` *(left & right)* — also: Parietooccipital sulcus
          - `Subparietal sulcus` *(left & right)*
          - `Sulcus interm_prim-Jensen` *(left & right)*
        - `Lateral ventricle` *(left & right)*
        - `Limbic lobe`
          - `Cingulate gyrus (Posteroventral part*)` *(left & right)*
          - `Cingulate gyrus and sulcus (Middle anterior part)` *(left & right)*
          - `Cingulate gyrus and sulcus (Middle posterior part)` *(left & right)*
          - `Cingulate gyrus and sulcus (Posterior dorsal part)` *(left & right)*
          - `Cingulate sulcus (Marginal part*)` *(left & right)*
          - `Hippocampus` *(left & right)* — also: Hippocampal formation
        - `Occipital lobe`
          - `Calcarine sulcus` *(left & right)*
          - `Cuneus` *(left & right)*
          - `Inferior occipital gyrus and sulcus*` *(left & right)*
          - `Lateral occipital gyrus (Middle occipital gyrus*)` *(left & right)*
          - `Lingual gyrus` *(left & right)*
          - `Lunate sulcus` *(left & right)*
          - `Occipital pole` *(left & right)*
          - `Superior occipital gyri` *(left & right)*
          - `Transverse occipital sulcus` *(left & right)*
        - `Parietal lobe`
          - `Angular gyrus` *(left & right)*
          - `Intraparietal sulcus` *(left & right)*
          - `Postcentral gyrus` *(left & right)*
          - `Postcentral sulcus` *(left & right)*
          - `Precuneus` *(left & right)*
          - `Superior parietal lobule` *(left & right)*
          - `Supramarginal gyrus` *(left & right)*
        - `Temporal lobe`
          - `Inferior temporal gyrus` *(left & right)*
          - `Inferior temporal sulcus` *(left & right)*
          - `Lateral occipitotemporal gyrus` *(left & right)*
          - `Medial occipitotemporal gyrus (Parahippocampal*)` *(left & right)*
          - `Middle temporal gyrus` *(left & right)*
          - `Occipitotemporal sulcus (Lateral part*)` *(left & right)*
          - `Superior temporal gyrus (Lateral part)` *(left & right)*
          - `Superior temporal sulcus` *(left & right)*
          - `Temporal plane` *(left & right)*
          - `Temporal pole` *(left & right)*
          - `Transverse temporal gyri` *(left & right)* — also: Heschl’s transverse convolutions
        - `Walls of lateral ventricle`
          - `Septum pellucidum`
        - `White matter of telencephalon` *(left & right)*
          - `Commissural fibres of telencephalon` — also: Commissural fibers of telencephalon
            - `Anterior commissure`
            - `Corpus callosum`
            - `Hippocampal commissure` — also: Commissure of fornix, Lyre of David
          - `Projection fibres of telencephalon` — also: Projection fibers of telencephalon
            - `Fornix` *(left & right)*
          - `Stria terminalis` *(left & right)*
  - `Meninges`
    - `Dura` — also: Dura mater, Pachymeninx
      - `Cranial dura` — also: Cranial dura mater
        - `Falx cerebri` — also: Cerebral falx
        - `Tentorium cerebelli` *(left & right)* — also: Cerebellar tentorium
      - `Spinal dura` — also: Spinal dura mater
    - `Leptomeninges`
      - `Pia` — also: Pia mater
        - `Cranial pia` — also: Cranial pia mater
          - `Choroid plexus` *(left & right)*
  - `Spinal cord`
    - `Central structures of spinal cord` — also: Central cord structures
      - `Central canal`
      - `Central canal'`
    - `Grey matter of spinal cord` — also: Grey columns of spinal cord, Gray columns of spinal cord
      - `Anterior horn of spinal cord` — also: Ventral horn of spinal cord, Anterior gray column, Spinal laminae VIII and IX
        - `Anterior horn of spinal cord` — also: Ventral horn of spinal cord, Anterior gray column, Spinal laminae VIII and IX
        - `Nucleus of accessory nerve` *(left & right)* — also: Accessory nucleus
      - `Intermediate zone of spinal cord` — also: Intermediate gray column, Spinal laminae VII
        - `Intermediolateral nucleus`
        - `Intermediomedial nucleus`
        - `Lateral intermediate substance`
      - `Posterior horn of spinal cord` — also: Dorsal horn of spinal cord, Posterior gray column
        - `Nucleus proprius` — also: Spinal laminae III and IV
        - `Posterior horn of spinal cord` — also: Dorsal horn of spinal cord, Posterior gray column
        - `Spinal reticular process` — also: Spinal reticular formation
    - `White matter of spinal cord` — also: White columns of spinal cord
      - `Anterior funiculus` — also: Ventral funiculus, Ventral white column
        - `Anterior corticospinal tract` — also: Tract of Türck
        - `Anterior fasciculus proprius` — also: Ventral fasciculus proprius
        - `Lateral vestibulospinal tract`
        - `Medial reticulospinal tract` — also: Pontoreticulospinal tract
        - `Medial vestibulospinal tract`
        - `Tectospinal tract`
      - `Anterolateral fasciculus` — also: Anterolateral tracts, Ventrolateral fasciculus, Ventrolateral tracts, Anterolateral system
        - `Anterior spinothalamic tract` — also: Ventral spinothalamic tract, Tract of Dejerine
        - `Lateral spinothalamic tract` — also: Tract of Edinger
        - `Spinotectal tract` — also: Bundle of Held
      - `Lateral funiculus` — also: Lateral white column
        - `Anterior spinocerebellar tract` — also: Ventral spinocerebellar tract, Tract of Gowers
        - `Lateral corticospinal tract`
        - `Lateral fasciculus proprius`
        - `Lateral reticulospinal tract` — also: Bulboreticulospinal tract, Medullary reticulospinal tract
        - `Posterior spinocerebellar tract` — also: Dorsal spinocerebellar tract, Tract of Flechsig
        - `Rubrospinal tract` — also: Tract of von Monakow
      - `Posterior funiculus` — also: Dorsal funiculus, Dorsal white column
        - `Cuneate fasciculus` — also: Tract of Burdach
        - `Gracile fasciculus` — also: Tract of Goll
        - `Posterior fasciculus proprius` — also: Dorsal fasciculus proprius
      - `Posterolateral tract` — also: Dorsolateral tract, Lissauer’s tract, Zone of Lissauer
      - `White matter of spinal cord` — also: White columns of spinal cord

### Peripheral nervous system

- `Peripheral nervous system`
  - `Autonomic division of peripheral nervous system` — also: Autonomic nervous system
    - `Thoracolumbar part of autonomic division` — also: Sympathetic part of autonomic division, Thoracolumbar outflow, Sympathetic nervous system
      - `Sympathetic nerves` *(left & right)*
      - `Sympathetic trunk` *(left & right)* — also: Sympathetic chain
        - `Ganglia of sympathetic trunk` *(left & right)*
  - `Cranial nerves`
    - `Abducens nerve (VI)` *(left & right)* — also: Cranial nerve VI, Abducent nerve
    - `Facial nerve (VII)` *(left & right)* — also: Cranial nerve VII
      - `Chorda tympani` *(left & right)*
    - `Glossopharyngeal nerve (IX)` *(left & right)* — also: Cranial nerve IX
    - `Hypoglossal nerve (XII)` *(left & right)* — also: Cranial nerve XII
    - `Oculomotor nerve (III)` *(left & right)* — also: Cranial nerve III
    - `Olfactory nerve (I)` *(left & right)* — also: Cranial nerve I, Olfactory nerves, Olfactory fibre bundles, Olfactory fiber bundles
    - `Optic nerve (II)` *(left & right)* — also: Cranial nerve II
    - `Trigeminal nerve (V)` *(left & right)* — also: Cranial nerve V
      - `Maxillary nerve` *(left & right)* — also: Cranial nerve V2, Maxillary division of trigeminal nerve
        - `Meningeal branch of maxillary nerve` *(left & right)*
      - `Motor root of trigeminal nerve` *(left & right)*
      - `Ophthalmic nerve` *(left & right)* — also: Cranial nerve V1, Ophthalmic division of trigeminal nerve
      - `Sensory root of trigeminal nerve` *(left & right)*
      - `Mandibular nerve` — also: Cranial nerve V3, Mandibular division of trigeminal nerve
        - `Anterior division of mandibular nerve` *(left & right)*
          - `Buccal nerve` *(left & right)*
        - `Posterior division of mandibular nerve` *(left & right)*
          - `Inferior alveolar nerve` *(left & right)*
            - `Mental nerve` *(left & right)*
            - `Nerve to mylohyoid muscle` *(left & right)*
          - `Lingual nerve` *(left & right)*
    - `Trochlear nerve (IV)` *(left & right)* — also: Cranial nerve IV
    - `Vagus nerve (X)` *(left & right)* — also: Cranial nerve X, For the Anterior and Posterior vagal trunks
    - `Vestibulocochlear nerve (VIII)` *(left & right)* — also: Cranial nerve VIII
      - `Cochlear nerve` *(left)*
      - `Vestibular nerve` *(left & right)*
  - `Ganglia`
    - `Spinal ganglion` *(left & right)* — also: Sensory ganglion of spinal nerve, Dorsal root ganglion
  - `Roots of nerves`
    - `Cauda equina`
  - `Spinal nerves`
    - `Anterior root of spinal nerve` *(left & right)* — also: Motor root of spinal nerve, Ventral root of spinal nerve
    - `Brachial plexus`
      - `Branches of lateral cord of brachial plexus`
        - `Lateral pectoral nerve` *(left & right)*
        - `Musculocutaneous nerve` *(left & right)* — also: Nerve of Casserio
          - `Lateral antebrachial cutaneous nerve` *(left & right)* — also: Lateral cutaneous nerve of forearm
      - `Branches of medial cord of brachial plexus`
        - `Medial antebrachial cutaneous nerve` *(left & right)* — also: Medial cutaneous nerve of forearm
          - `Anterior branch of medial antebrachial cutaneous nerve` *(left & right)* — also: Anterior branch of medial cutaneous nerve of forearm
          - `Posterior branch of medial antebrachial cutaneous nerve` *(left & right)* — also: Posterior branch of medial cutaneous nerve of forearm
        - `Medial brachial cutaneous nerve` *(left & right)* — also: Medial cutaneous nerve of arm
        - `Medial pectoral nerve` *(left & right)*
        - `Ulnar nerve` *(left & right)*
          - `Deep branch of ulnar nerve` *(left & right)*
          - `Dorsal branch of ulnar nerve` *(left & right)*
            - `Dorsal digital branches of ulnar nerve` *(left & right)* — also: Ulnar dorsal digital nerves of hand, Dorsal digital nerves
          - `Muscular branches of ulnar nerve` *(left & right)*
          - `Palmar branch of ulnar nerve` *(left & right)* — also: Palmar cutaneous branch of ulnar nerve
          - `Superficial branch of ulnar nerve` *(left & right)*
            - `Common palmar digital branches of ulnar nerve` *(left & right)* — also: Common medial palmar digital nerves
              - `Proper palmar digital branches of ulnar nerve` *(left & right)* — also: Proper medial palmar digital nerves
      - `Branches of posterior cord of brachial plexus`
        - `Axillary nerve` *(left & right)*
          - `Muscular branches of axillary nerve` *(left & right)*
          - `Superior lateral brachial cutaneous nerve` *(left & right)* — also: Superior lateral cutaneous nerve of arm
        - `Inferior subscapular nerve` *(left & right)*
        - `Radial nerve` *(left & right)*
          - `Deep branch of radial nerve` *(left & right)*
            - `Posterior interosseous nerve of forearm` *(left & right)* — also: Posterior antebrachial interosseous nerve
          - `Inferior lateral brachial cutaneous nerve` *(left & right)* — also: Inferior lateral cutaneous nerve of arm
          - `Muscular branches of radial nerve` *(left & right)*
          - `Posterior antebrachial cutaneous nerve` *(left & right)* — also: Posterior cutaneous nerve of forearm
          - `Superficial branch of radial nerve` *(left & right)*
            - `Dorsal digital branches of radial nerve` *(left & right)* — also: Radial dorsal digital nerves of hand
        - `Superior subscapular nerve` *(left)*
        - `Thoracodorsal nerve` *(left & right)* — also: Middle subscapular nerve
      - `Infraclavicular part of brachial plexus`
        - `Posterior cord of brachial plexus` *(left & right)*
      - `Median nerve` *(left & right)*
        - `(Communicating branch of median nerve with ulnar nerve)` *(left & right)* — also: Martin-Gruber anastomosis
        - `Anterior interosseous nerve of forearm` *(left & right)* — also: Anterior antebrachial interosseous nerve
        - `Common palmar digital branches of median nerve` *(left & right)* — also: Common lateral palmar digital nerves
          - `Proper palmar digital branches of median nerve` *(left & right)* — also: Proper lateral palmar digital nerves
        - `Muscular branches of median nerve` *(left & right)*
        - `Palmar branch of median nerve` *(left & right)*
      - `Supraclavicular branches of brachial plexus`
        - `Dorsal scapular nerve` *(left & right)*
        - `Long thoracic nerve` *(left & right)* — also: Bell’s external respiratory nerve
        - `Subclavian nerve` *(left & right)* — also: Nerve to subclavius muscle
        - `Suprascapular nerve` *(left & right)*
      - `Supraclavicular part of brachial plexus`
        - `Inferior trunk of brachial plexus` *(left & right)* — also: Lower trunk
          - `Anterior division of inferior trunk of brachial plexus` *(left & right)*
          - `Posterior division of inferior trunk of brachial plexus` *(left & right)*
        - `Middle trunk of brachial plexus` *(left & right)*
          - `Anterior division of middle trunk of brachial plexus` *(left & right)*
          - `Posterior division of middle trunk of brachial plexus` *(left & right)*
        - `Roots of brachial plexus` *(left & right)*
        - `Superior trunk of brachial plexus` *(left & right)* — also: Upper trunk
          - `Anterior division of superior trunk of brachial plexus` *(left & right)*
          - `Posterior division of superior trunk of brachial plexus` *(left & right)*
    - `Cervical nerves` — also: Spinal nerves C1-C8
      - `Accessory nerve (XI)` *(left & right)* — also: Cranial nerve XI
    - `Lumbar nerves` — also: Spinal nerves L1-L5
      - `Anterior rami of lumbar nerves` — also: Ventral rami of lumbar nerves
        - `Ilio-inguinal nerve` *(left & right)* — also: Ilioinguinal nerve
        - `Iliohypogastric nerve` *(left & right)*
    - `Lumbosacral plexus`
      - `Lumbar plexus`
        - `Branches of anterior part of lumbar plexus`
          - `Genitofemoral nerve` *(left & right)*
            - `Femoral branch of genitofemoral nerve` *(left & right)*
            - `Genital branch of genitofemoral nerve` *(left & right)*
          - `Obturator nerve` *(left & right)*
            - `Anterior branch of obturator nerve` *(left & right)*
            - `Posterior branch of obturator nerve` *(left & right)*
        - `Branches of posterior part of lumbar plexus`
          - `Femoral nerve` *(left & right)*
            - `Anterior cutaneous branches of femoral nerve` *(left & right)* — also: Anterior femoral cutaneous nerves, Anterior cutaneous nerves of thigh
            - `Saphenous nerve` *(left & right)*
              - `Infrapatellar branch of saphenous nerve` *(left & right)*
              - `Medial crural cutaneous branches of saphenous nerve` *(left & right)* — also: Medial cutaneous nerve of leg, Medial crural cutaneous nerve
          - `Lateral femoral cutaneous nerve` *(left & right)* — also: Lateral cutaneous nerve of thigh
          - `Superior subscapular nerve` *(right)*
      - `Sacral plexus`
        - `Anterior root of posterior femoral cutaneous nerve` *(left & right)*
        - `Nerve to piriformis muscle` *(left & right)*
        - `Nerve to quadratus femoris muscle` *(left & right)*
        - `Posterior femoral cutaneous nerve` *(left & right)* — also: Posterior cutaneous nerve of thigh
        - `Posterior root of posterior femoral cutaneous nerve` *(left & right)*
        - `Pudendal nerve` *(left & right)*
        - `Sciatic nerve` *(left & right)*
          - `Common fibular nerve` *(left & right)* — also: Common peroneal nerve
            - `Deep fibular nerve` *(left & right)* — also: Deep peroneal nerve
              - `Dorsal digital branches of deep fibular nerve` *(left & right)* — also: Dorsal digital nerves of foot
              - `Muscular branches of deep fibular nerve` *(left & right)* — also: Muscular branches of deep peroneal nerve
            - `Superficial fibular nerve` *(left & right)* — also: Superficial peroneal nerve
              - `Dorsal digital branches of superficial fibular nerve` *(left & right)* — also: Dorsal digital nerves of foot
              - `Intermediate dorsal cutaneous nerve of foot` *(left & right)* — also: Intermediate cutaneous nerve of dorsum of foot
              - `Medial dorsal cutaneous nerve of foot` *(left & right)* — also: Medial cutaneous nerve of dorsum of foot
            - `Sural communicating branch of common fibular nerve` *(left & right)* — also: Sural communicating branch of common peroneal nerve
          - `Tibial nerve` *(left & right)*
            - `Lateral plantar nerve` *(left & right)*
              - `Common plantar digital branches of lateral plantar nerve` *(left & right)* — also: Common lateral plantar digital nerves
                - `Proper plantar digital branches of lateral plantar nerve` *(left & right)* — also: Proper lateral plantar digital nerves
            - `Medial plantar nerve` *(left & right)*
              - `Common plantar digital branches of medial plantar nerve` *(left)* — also: Common medial plantar digital nerves
                - `Proper plantar digital branches of medial plantar nerve` *(left & right)* — also: Proper medial plantar digital nerves
            - `Medial sural cutaneous nerve` *(left & right)* — also: Medial cutaneous nerve of calf
              - `Sural nerve` *(left & right)*
        - `Superior gluteal nerve` *(left & right)*
    - `Posterior root of spinal nerve` *(left & right)* — also: Sensory root of spinal nerve, Dorsal root of spinal nerve
    - `Thoracic nerves`
      - `Intercostal nerves` *(left & right)*

### Sense organs

- `Sense organs`
  - `Ear`
    - `External ear`
      - `Tympanic membrane` *(left & right)* — also: Rivinus’ membrane
    - `Internal ear`
      - `Cochlea` *(left & right)*
      - `Vestibule` *(left & right)*
    - `Middle ear`
      - `Auditory tube` *(left & right)* — also: Pharyngotympanic tube, Eustachian tube
  - `Eye`
    - `Accessory visual structures`
      - `Lacrimal apparatus`
        - `Lacrimal canaliculus` *(left & right)*
          - `Ampulla of lacrimal canaliculus` *(left & right)*
        - `Lacrimal gland` *(left & right)*
        - `Lacrimal sac` *(left & right)*
        - `Nasolacrimal duct` *(left & right)*
      - `Suspensory ligament of eyeball` *(left & right)* — also: Lockwood’s ligament
    - `Eyeball`
      - `Anterior segment of eyeball` *(left & right)*
        - `Anterior chamber of eyeball` *(left & right)*
        - `Zonular fibres` *(left & right)* — also: Zonular fibers
      - `Ciliary body-curve` *(right)*
      - `Cornea` *(left & right)*
      - `Equator of eyeball` *(right)*
      - `External axis of eyeball`
      - `Internal axis of eyeball` *(right)*
      - `Iris` *(left & right)*
      - `Lens` *(left & right)*
      - `Meridians of eyeball` *(right)*
      - `Posterior segment of eyeball` *(left & right)*
        - `Vitreous body` *(left & right)*
      - `Retina` *(left & right)*
      - `Sclera` *(left & right)*

## Organs (visceral systems)

Whole system: `Visceral systems`


### Abdominopelvic cavity

- `Abdominopelvic cavity`
  - `Peritoneal structures`
    - `Greater omentum`
    - `Lesser omentum`
    - `Mesocolon`
      - `Meso-appendix` — also: Mesoappendix

### Digestive system

- `Digestive system` — also: Alimentary system, Gastrointestinal system
  - `Digestive canal` — also: Alimentary canal
    - `Large intestine` — also: Large bowel
      - `Colon`
        - `Ascending colon` — also: Right colon
        - `Descending colon` — also: Left colon
        - `Sigmoid colon`
        - `Taeniae coli` — also: Teniae colli, Teniae coli
          - `Free taenia` — also: Free tenia
          - `Mesocolic taenia` — also: Mesocolic tenia
          - `Omental taenia` — also: Omental tenia
        - `Transverse colon`
        - `Vermiform appendix` — also: Appendix
    - `Oesophagus` — also: Esophagus
      - `Oesophagus-profile`
    - `Small intestine` — also: Small bowel
      - `Duodenum`
      - `Jejunum`
    - `Stomach`
      - `Mucosa of stomach` — also: Mucous membrane of stomach
  - `Extrahepatic bile ducts`
    - `Bile duct` — also: Common bile duct
  - `Gallbladder`
  - `Liver`
    - `Anterior lateral segment of liver (VI)` — also: Segment VI of liver
    - `Anterior medial segment of liver (V)` — also: Segment V of liver
    - `Left anterior lateral segment of liver (III)` — also: Segment III of liver
    - `Left medial segment of liver (IV)` — also: Segment IV of liver
    - `Left posterior lateral segment of liver (II)` — also: Segment II of liver
    - `Posterior lateral segment of liver (VII)` — also: Segment VII of liver
    - `Posterior medial segment of liver (VIII)` — also: Segment VIII of liver
    - `Posterior segment of liver (I)` — also: Segment I of liver
  - `Mouth`
    - `Gingiva` — also: Gum
    - `Salivary glands` — also: Glands of mouth
      - `Major salivary glands`
        - `(Accessory parotid gland)` *(left & right)* — also: Socia parotidis
        - `Parotid duct` *(left & right)* — also: Stensen's duct
        - `Parotid gland` *(left & right)*
        - `Sublingual gland` *(left & right)*
        - `Submandibular duct` *(left & right)* — also: Submaxillary duct, Wharton's duct
        - `Submandibular gland` *(left & right)*
    - `Soft palate`
    - `Tongue`
    - `Uvula of palate`
  - `Pancreas`
    - `Accessory pancreatic duct`
    - `Pancreatic duct` — also: Duct of Wirsung
  - `Pharynx`
    - `Laryngopharynx` — also: Hypopharynx
    - `Nasopharynx`
    - `Oropharynx`

### Endocrine glands

- `Endocrine glands`
  - `Hypophysis`
    - `Adenohypophysis`
    - `Neurohypophysis`
  - `Parathyroid glands`
    - `Inferior parathyroid gland` *(left & right)*
    - `Superior parathyroid gland` *(left & right)* — also: Parathyroid 4
  - `Pineal gland` — also: Pineal body
  - `Suprarenal gland` *(left & right)* — also: Adrenal gland
  - `Thyroid gland`

### Genital systems

- `Genital systems`
  - `Male genital system` — also: Male reproductive system
    - `Male external genitalia` — also: Male external genital organs
      - `Penis` — also: Dick, Cock
        - `Corpus cavernosum of penis`
        - `Corpus spongiosum of penis` — also: Corpus cavernosum urethrae
        - `Glans penis`
    - `Male internal genitalia` — also: Male internal genital organs
      - `Ductus deferens` *(left & right)* — also: Vas deferens
      - `Ejaculatory duct` *(left & right)*
      - `Epididymis` *(left & right)*
      - `Prostate` — also: Prostate gland
      - `Seminal gland` *(left & right)* — also: Seminal vesicle
      - `Testis` *(left & right)* — also: Testicle

### Respiratory system

- `Respiratory system`
  - `Larynx`
    - `Epiglottis`
  - `Lungs`
    - `Left lung`
      - `Inferior lobe of left lung`
      - `Superior lobe of left lung` — also: upper lobe of left lung
    - `Right lung`
      - `Inferior lobe of right lung`
      - `Middle lobe of right lung`
      - `Superior lobe of right lung` — also: Upper lobe of right lung
  - `Nose`
    - `Mucosa of nasal cavity` — also: Mucous membrane of nasal cavity, Nasal mucosa
  - `Tracheobronchial tree` — also: Bronchial tree
    - `Bronchi`
      - `Left main bronchus`
        - `Left inferior lobar bronchus`
          - `(Anteromedial basal segmental bronchus of left lung)`
          - `Anterior basal segmental bronchus of left lung (BVIII)` — also: Bronchus 8 of left lung, B8
          - `Lateral basal segmental bronchus of left lung (BIX)` — also: Bronchus 9 of left lung, B9
          - `Medial basal segmental bronchus of left lung (BVII)` — also: Bronchus 7 of left lung, B7
          - `Posterior basal segmental bronchus of left lung (BX)` — also: Bronchus 10 of left lung, B10
          - `Superior segmental bronchus of left lung (BVI)` — also: Bronchus 6 of left lung, B6
        - `Left superior lobar bronchus`
          - `Anterior segmental bronchus of left lung (BIII)` — also: Bronchus 3 of left lung, B3
          - `Apicoposterior segmental bronchus of left lung (BI+BII)` — also: Bronchus 1+2, B1+2
          - `Inferior lingular segmental bronchus of left lung (BV)` — also: Bronchus 5 of left lung, B5
          - `Superior lingular segmental bronchus of left lung (BIV)` — also: Bronchus 4 of left lung, B4
      - `Right main bronchus`
        - `Intermediate bronchus` *(right)* — also: Hyparterial bronchus
          - `Middle lobar bronchus` *(right)* — also: Right middle lobar bronchus
            - `Lateral segmental bronchus of right lung (BIV)` — also: Bronchus 4 of right lung, B4
            - `Medial segmental bronchus of right lung (BV)` — also: Bronchus 5 of right lung, B5
          - `Right inferior lobar bronchus`
            - `Anterior basal segmental bronchus of right lung (BVIII)` — also: Bronchus 5 of right lung, B8
            - `Lateral basal segmental bronchus of right lung (BIX)` — also: Bronchus 5 of right lung, B9
            - `Medial basal segmental bronchus of right lung (BVII)` — also: Bronchus 5 of right lung, B7
            - `Posterior basal segmental bronchus of right lung (BX)` — also: Bronchus 5 of right lung, B10
            - `Superior segmental bronchus of right lung (BVI)` — also: Bronchus 5 of right lung, B6
        - `Right superior lobar bronchus` — also: Eparterial bronchus
          - `Anterior segmental bronchus of right lung (BIII)` — also: Bronchus 3 of right lung, B3
          - `Apical segmental bronchus of right lung (BI)` — also: Bronchus 1 of right lung, B1
          - `Posterior segmental bronchus of right lung (BII)` — also: Bronchus 2 of right lung, B2
    - `Trachea`

### Thoracic cavity

- `Thoracic cavity`
  - `Pleura`

### Urinary system

- `Urinary system`
  - `Kidney` *(left & right)*
  - `Renal pelvis` *(left & right)* — also: Branching type
  - `Ureter` *(left & right)*
  - `Urethra`
  - `Urinary bladder`

## Body regions

Whole system: `Regions of human body`


### Hairs

- `Hairs`
  - `Eyelashes` *(left & right)*
  - `Hairs of eyebrow` *(left & right)*
  - `Hairs of head` — also: Scalp hair
  - `Pubic hairs`

### Regions of abdomen

- `Regions of abdomen` — also: Abdominal region
  - `Epigastric region` *(left & right)*
  - `Hypochondriac region` *(left & right)*
  - `Hypogastric region` *(left & right)* — also: Pubic region
  - `Inguinal region` *(left & right)* — also: Groin
  - `Lateral region of abdomen` *(left & right)* — also: Flank
  - `Umbilical region` *(left & right)*
    - `Umbilicus` *(left & right)*

### Regions of back

- `Regions of back` — also: Back region
  - `Infrascapular region` *(left & right)*
  - `Interscapular region` *(left & right)*
  - `Lumbar region` *(left & right)*
  - `Sacral region` *(left & right)*
  - `Scapular region` *(left & right)*
  - `Triangle of auscultation` *(left & right)* — also: Auscultatory triangle
  - `Vertebral region` *(left & right)*

### Regions of head

- `Regions of head` — also: Head region
  - `Auricular region`
    - `Anterior notch of auricle` *(left & right)*
    - `Antihelix` *(left & right)*
    - `Antitragus` *(left & right)*
    - `Apex of auricle` *(left & right)* — also: Tip of ear
    - `Auricular region` *(left & right)*
    - `Auricular tubercle` *(left & right)*
    - `Cavity of concha` *(left & right)*
    - `Concha of auricle` *(left & right)*
    - `Crura of antihelix` *(left & right)*
    - `Cymba conchae` *(left & right)*
    - `Eminentia conchae` *(left & right)*
    - `Eminentia fossae triangularis` *(left & right)*
    - `Eminentia scaphae` *(left & right)*
    - `Fossa antihelica` *(left & right)* — also: Antihelical fossa
    - `Helix` *(left & right)*
    - `Intertragic incisure` *(left & right)* — also: Intertragic notch
    - `Lobule of auricle` *(left & right)* — also: Lobe of ear
    - `Posterior auricular groove` *(left & right)*
    - `Scapha` *(left & right)*
    - `Tragus` *(left & right)*
    - `Triangular fossa` *(left & right)*
  - `Regions of epicranium` — also: Epicranial region, Neurocranial region, Epicranium, Calva
    - `Frontal region` *(left & right)*
      - `Eyebrow` *(left & right)*
    - `Mastoid region` *(left & right)*
    - `Occipital region` *(left & right)*
    - `Parietal region` *(left & right)*
    - `Temporal region` *(left & right)*
  - `Regions of face` — also: Facial region
    - `Buccal region` *(left & right)*
    - `Infra-orbital region` *(left & right)* — also: Infraorbital region
    - `Mental region` *(left & right)*
    - `Mentolabial sulcus` *(left & right)*
    - `Nasal region` *(left & right)*
    - `Nasolabial sulcus` *(left & right)*
    - `Oral region` *(left & right)*
      - `Angle of mouth` *(left & right)*
      - `Labial commissure` *(left & right)*
      - `Philtrum` *(left & right)*
      - `Tubercle of upper lip` *(left & right)* — also: Tubercle of superior lip
    - `Orbital region` *(left & right)*
    - `Parotideomasseteric region` *(left & right)* — also: Parotid region
    - `Zygomatic region` *(left & right)*

### Regions of lower limb

- `Regions of lower limb`
  - `Femoral region`
    - `Anterior region of thigh` — also: Anterior femoral region
      - `Anterior region of thigh` *(left & right)* — also: Anterior femoral region
        - `Femoral triangle` *(left & right)* — also: Scarpa's triangle
    - `Posterior region of thigh` *(left & right)* — also: Posterior femoral region
  - `Gluteal region`
    - `Gluteal region` *(left & right)*
      - `Gluteal fold` *(left & right)*
  - `Hip region` *(left & right)*
  - `Knee region`
    - `Anterior region of knee` *(left & right)* — also: Anterior knee region
    - `Posterior region of knee` — also: Posterior knee region
      - `Posterior region of knee` *(left & right)* — also: Posterior knee region
        - `Popliteal fossa` *(left & right)*
  - `Leg region`
    - `Anterior region of leg` *(left & right)* — also: Anterior leg region
    - `Posterior region of leg` *(left & right)* — also: Posterior leg region
  - `Regions of foot` — also: Foot region
    - `Dorsum of foot` *(left & right)* — also: Dorsal region of foot, Superior region of foot
    - `Heel region` *(left & right)*
    - `Lateral border of foot` *(left & right)* — also: Fibular border of foot, Peroneal border of foot
    - `Medial border of foot` *(left & right)* — also: Tibial border of foot
    - `Metatarsal region` *(left & right)*
      - `Hallucial eminence` *(left & right)* — also: Ball of foot
    - `Regions of digits of foot` — also: Regions of toes
      - `Dorsal surfaces of digits of foot` *(left & right)* — also: Dorsal surfaces of toes
        - `Nail plate (foot)` *(left & right)*
          - `Perionyx (foot)` *(left & right)*
      - `Plantar surfaces of digits of foot` *(left & right)* — also: Plantar surfaces of toes
    - `Sole` *(left & right)* — also: Plantar region, Inferior region of foot
      - `Distal transverse arch of foot` *(left & right)*
      - `Lateral part of longitudinal arch of foot` *(left & right)*
      - `Medial part of longitudinal arch of foot` *(left & right)*
      - `Proximal transverse arch of foot` *(left & right)*
  - `Talocrural region` — also: Ankle region
    - `Anterior region of ankle` *(left & right)* — also: Anterior talocrural region, Anterior ankle region
    - `Lateral malleolus` *(left & right)*
    - `Medial malleolus` *(left & right)*
    - `Posterior region of ankle` — also: Posterior talocrural region, Posterior ankle region
      - `Lateral retromalleolar region` *(left & right)*
      - `Medial retromalleolar region` *(left & right)*

### Regions of neck

- `Regions of neck` — also: Cervical region
  - `Anterior region of neck` — also: Anterior cervical region
    - `Infrahyoid region`
      - `Carotid triangle` *(left & right)*
      - `Muscular triangle` *(left & right)* — also: Omotracheal triangle
    - `Suprahyoid region`
      - `Submandibular triangle` *(left & right)* — also: Submandibular region
      - `Submental triangle` *(left & right)*
  - `Lateral region of neck` — also: Lateral cervical region, Posterior triangle of neck, Posterior cervical triangle
    - `Greater supraclavicular fossa` *(left & right)*
    - `Lateral region of neck` *(left & right)* — also: Lateral cervical region, Posterior triangle of neck, Posterior cervical triangle
  - `Posterior region of neck` *(left & right)* — also: Posterior cervical region
  - `Sternocleidomastoid region`
    - `Sternocleidomastoid region` *(left & right)*
      - `Lesser supraclavicular fossa` *(left & right)*

### Regions of perineum

- `Regions of perineum`
  - `Anal region` *(left & right)*
  - `Urogenital region` *(left & right)* — also: Urogenital triangle

### Regions of thorax

- `Regions of thorax` — also: Thoracic region
  - `Anterior region of thorax` — also: Anterior thoracic region, Anterior chest, Front of chest
    - `Deltopectoral triangle` *(left & right)* — also: Clavipectoral triangle, Mohrenheim's triangle
    - `Infraclavicular fossa` *(left & right)* — also: Mohrenheim's fossa
    - `Pectoral region` *(left & right)*
      - `Inframammary region` *(left & right)*
      - `Mammary region` *(left & right)*
    - `Presternal region` *(left & right)*
  - `Lateral region of thorax` *(left & right)* — also: Lateral thoracic region

### Regions of upper limb

- `Regions of upper limb`
  - `Antebrachial region` — also: Forearm region
    - `Anterior region of forearm` *(left & right)* — also: Anterior antebrachial region
    - `Lateral border of forearm` *(left & right)* — also: Radial border of forearm
    - `Medial border of forearm` *(left & right)* — also: Ulnar border of forearm
    - `Posterior region of forearm` *(left & right)* — also: Posterior antebrachial region
  - `Brachial region` — also: Arm region
    - `Anterior region of arm` *(left & right)* — also: Anterior brachial region
      - `Lateral bicipital groove` *(left & right)*
      - `Medial bicipital groove` *(left & right)*
    - `Posterior region of arm` *(left & right)* — also: Posterior brachial region
  - `Carpal region` — also: Wrist region
    - `Anterior region of wrist` *(left & right)* — also: Anterior carpal region
    - `Posterior region of wrist` *(left & right)* — also: Posterior carpal region
    - `Radial foveola` *(left & right)* — also: Anatomical snuffbox
  - `Cubital region` — also: Elbow region
    - `Anterior region of elbow` *(left & right)* — also: Anterior cubital region
      - `Cubital fossa` *(left & right)* — also: Antecubital fossa
    - `Posterior region of elbow` *(left & right)* — also: Posterior cubital region
  - `Deltoid region` *(left & right)*
  - `Regions of hand` — also: Hand region
    - `Dorsum of hand` *(left & right)* — also: Dorsal region of hand
    - `Palm` *(left & right)* — also: Palmar region
    - `Regions of digits of hand`
      - `Dorsal surfaces of digits of hand` *(left & right)*
        - `Nail plate` *(left & right)* — also: Body of nail
          - `Perionyx` *(left & right)*
      - `Palmar surfaces of digits of hand` *(left & right)*

## Reference lines, directions and movements

Whole system: `References and movements`


### Reference lines

- `Reference lines`
  - `Posterior axillary line` *(left & right)*
  - `Posterior median line`
  - `Parasternal line` *(left & right)*
  - `Paravertebral line` *(left & right)* — also: Vertebral line
  - `Mamillary line` *(left & right)* — also: Nipple line, Mammilary line
  - `Midclavicular line` *(left & right)*
  - `Anterior axillary line` *(left & right)*
  - `Anterior median line`
  - `Scapular line` *(left & right)*
  - `Sternal line` *(left & right)*

### Directions

- `Directions`
  - `Caudal`
  - `Occipital`
  - `Palmar`
  - `Plantar`
  - `Radial`
  - `Tibial`
  - `Ulnar`
  - `Ventral`
  - `Proximal`
  - `Fibular` — also: Peroneal
  - `Frontal`
  - `Cranial`
  - `Distal`
  - `Dorsal`

### Movements

- `Movements`
  - `Abduction`
  - `Adduction`
  - `Circumduction`
  - `Dorsiflexion` — also: Dorsoflexion
  - `Eversion`
  - `Inversion`
  - `Lateral rotation` — also: External rotation
  - `Medial rotation` — also: Internal rotation
  - `Plantar flexion` — also: Plantiflexion, Plantaflexion
  - `Pronation`
  - `Supination`

### Planes

- `Planes`
  - `Coronal planes` — also: Frontal planes
  - `Median plane` — also: Median sagittal plane, Midsagittal plane
  - `Paramedian plane` — also: Paramedian sagittal plane
  - `Transverse planes` — also: Horizontal planes, Axial planes
