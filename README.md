# GeneticAlgorithmSimulation

Symulacja algorytmu genetycznego w Unity, w której populacja wilków ewoluuje, aby efektywniej polować na owce. 

---

## Opis projektu

Projekt implementuje algorytm genetyczny w środowisku 3D stworzonym w Unity. Wilki (agenci) konkurują ze sobą, polują na owce i rozmnażają się — przekazując swoje geny potomstwu. Przez kolejne epoki populacja ewoluuje w kierunku lepszych łowców.

---

## Architektura

```
GeneticAlgorithm        # Główna pętla algorytmu — zarządza populacją, epokami i krzyżowaniem
├── GeneticAgent        # Komponent agenta — przechowuje geny i fitness
│   └── Gene            # Pojedynczy gen (nazwa, min, max, wartość bieżąca)
├── Wolf                # Logika wilka — ruch, ataki, kolor, fitness
│   └── AIAgent         # Bazowa klasa agenta — ruch po gridzie, pozycja
└── Sheep               # Cel polowania — usuwana po schwytaniu
```

---

## Algorytm genetyczny

### Geny

Każdy wilk posiada zestaw genów (`List<Gene>`), gdzie każdy gen ma:
- `Name` — nazwa genu (np. `"Movement Range"`, `"View Range"`, `"Frequency - aggressive"`)
- `Min` / `Max` — dopuszczalny zakres wartości
- `CurrentValue` — aktualna wartość genu

Kolor każdego wilka jest obliczany na podstawie jego genów — wilki o podobnym DNA mają podobne ubarwienie.

### Ocena przystosowania (Fitness)

Fitness wilka odpowiada liczbie zgromadzonych punktów pożywienia (`foodNum`):

- **+50** za schwytanie owcy
- **−n** za każdą przebytą komórkę (koszt ruchu)
- Wilki z `fitness <= 0` są usuwane z populacji na koniec rundy

### Selekcja

```
Losowe przemieszanie → sortowanie malejące po fitness
→ odrzucenie dolnej połowy populacji
→ rodzice = lepsza połowa
```

### Krzyżowanie

Z każdej pary rodziców powstają **dwa potomki**:
- Geny potomka = pierwsza połowa genów rodzica 1 + druga połowa genów rodzica 2
- Energia potomka = średnia z procenta energii przekazanego przez każdego z rodziców
- Potomek jest umieszczany na losowej wolnej pozycji w gridzie

### Mutacja

Losowa zmiana od 0 do 2 genów:
- Zmiana wartości genu o maksymalnie ±10% zakresu
- Prawdopodobieństwo mutacji agenta: konfigurowalne (`mutationChance`, domyślnie 10%)

---

## Środowisko

- Akcja rozgrywa się na siatce (`GridManager`) zarządzającej pozycjami wilków i owiec
- Każda epoka składa się z `n` rund akcji (`numberActions`)
- W każdej rundzie wilki poruszają się i atakują — kolejność ruchu zależna od genu `"Movement Order"`
- Owce są respawnowane na początku każdej epoki do minimalnej liczebności

---

## Konfiguracja (SerializeField)

| Parametr | Domyślnie | Opis |
|---|---|---|
| `populationSize` | 100 | Liczebność początkowej populacji wilków |
| `epochsNum` | 10 | Liczba epok |
| `sheepInitPopulationSize` | 10 | Minimalna liczba owiec na początku epoki |
| `mutationChance` | 0.1 | Szansa na mutację agenta (0–1) |
| `actionTime` | 5s | Czas trwania jednej akcji |
| `numberActions` | 5 | Liczba akcji na epokę |

---

## Struktura plików

```
Assets/
├── Scripts/
│   ├── GeneticAlgorithm.cs   # Główna pętla GA
│   ├── GeneticAgent.cs       # Komponent agenta / geny
│   ├── Gene.cs               # Model genu
│   ├── Wolf.cs               # Logika wilka
│   ├── AIAgent.cs            # Bazowa klasa agenta
│   └── Sheep.cs              # Logika owcy
```

---

## Wymagania

- Unity 2022.3+
- Brak zewnętrznych zależności — wyłącznie standardowe Unity API
