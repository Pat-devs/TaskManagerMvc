# Taskmanager

*En liten TaskManager med følgende funksjonalitet:*

1. Se alle oppgaver.
2. Legge til en oppgave.
3. Markere en oppgave som fullført.
4. Avslutte programmet.

Funksjonalitet er hensiktsmessig liten, da hovedformålet er å vise hvordan:

* Separation of Concerns
* Single Responsibility Principle
* Interface Segregation
* Loose Coupling
* testbarhet

kommer fram gjennom faktiske designvalg.

## Litt om oppsettet og de 7 første commitsene
Disse notatene dokumenterer den delen av TaskManager-økten som var gjennomført fram til *Views/ViewGenerator.cs*.


### [1. Sette opp prosjektet](https://github.com/Pat-devs/TaskManagerMvc/commit/a8f0324b9ed8630d30d0d3aaedc3f109e385c197):
```bash
dotnet new sln -n TaskManagerMvc
dotnet new console -n App
dotnet new classlib -n TaskManager.Core
dotnet new xunit -n TaskManager.Tests
```

a. Legg prosjektene i solution:

```bash
dotnet sln add App/App.csproj
dotnet sln add TaskManager.Core/TaskManager.Core.csproj
dotnet sln add TaskManager.Tests/TaskManager.Tests.csproj
```

b. Legg til referanser:

```bash
dotnet add App/App.csproj reference TaskManager.Core/TaskManager.Core.csproj

dotnet add TaskManager.Tests/TaskManager.Tests.csproj reference TaskManager.Core/TaskManager.Core.csproj
```

c. Test at alt bygger:

```bash
dotnet build
```

Denne oppdelingen gjør at TaskManager.Core slipper å kjenne til App eller testprosjektet.

Det gir oss et nyttig designspørsmål som vi skal stille gjennom hele applikasjonen:

**Hvem trenger egentlig å vite om hvem?**

At domenelogikken senere kan testes direkte fra TaskManager.Tests helt uten å starte konsollprogrammet, er et direkte resultat av at vi har separert ansvarsområdene.

### [2. impement base structure without implementing it yet](https://github.com/Pat-devs/TaskManagerMvc/commit/530069ab9112ff269b13018796dc5c385c0dc1d6):

Vi fjernet default klassen Class1.cs i Taskmanager.Core mappen, og laget 3 mapper:
- Taskmanager.Core/Models
- Taskmanager.Core/Views
- Taskmanager.Core/Controllers

Deretter begynte vi med kontrakten **IUserTask**, og så implementerte vi **UserTask**.

Vi begynner med kontrakten (IUserTask) sånt at resten av applikasjonen vet hva kan forventes at en Task kan gjøre.

Den har en id, tittel, deadline, og den kan markeres som fullført.

Ved å gjøre *IsCompleted* -setteren private, beskytter vi objektets tilstand mot uønskede endringer utenfra.

Tilstands endring tvinges i stedet gjennom domeneoperasjonen task.MarkAsCompleted(), noe som sikrer at UserTask har full eierskap til sine egne forretningsregler.


**MarkAsCompleted** metoden kaster en NotImplementedException fordi vi ønsket først å etablere strukturen, men ikke late som oppførselen var ferdig.

Det gir oss et naturlig utgangspunkt for å skrive en test som beskriver forventet oppførsel.

### [3. implement a UserTask test](https://github.com/Pat-devs/TaskManagerMvc/commit/aae6e58ad341767534a8277b5244c773e7fdc108):

Vi erstattet standardtesten med UserTaskTests og skrev tester for modellen.

Testene følger et vanlig mønster:

* Arrange -> bygg opp situasjonen
* Act     -> utfør operasjonen
* Assert  -> kontroller resultatet

Dette gjør testen lettere å lese som en beskrivelse av forventet oppførsel.

På dette tidspunktet kaster MarkAsCompleted() fortsatt NotImplementedException.

Dermed har vi en forventning før implementasjonen er ferdig:

**RED**

Testen forteller oss helt konkret hvilken oppførsel som mangler.

Vi kan teste UserTask isolert fordi modellen er separert fra brukergrensesnitt og programflyt.

### [4. make the first test pass](https://github.com/Pat-devs/TaskManagerMvc/commit/06bcc82370893e6abe3962885c58293ee97e1a64):

Vi gjorde den minste nødvendige endringen for å få testen grønn:

Før endringen:

*Test -> MarkAsCompleted() -> NotImplementedException*

Etter endringen:

*Test -> MarkAsCompleted() -> IsCompleted = true*

Dette viser en enkel TDD-tankegang:

Red -> Green -> Refactor

Vi trenger ikke overdesigne løsningen før vi har en konkret forventning.

**Hvem skal vite hvordan en oppgave blir fullført?**

UserTask.

Controlleren skal senere kunne bestemme at en oppgave skal fullføres, men selve tilstandsendringen tilhører modellen.

Dette er Single Responsibility Principle i praksis.

UserTask har ansvar for en enkel oppgave og dens egen tilstand.

### [5. implement Interface for TaskContext](https://github.com/Pat-devs/TaskManagerMvc/commit/e772da1d9aef2577410c7fbd335a1bf68ffff647):

En UserTask har kun som formål å representere en unik oppgave. Å i tillegg gi den ansvar for å administrere hele oppgavesamlingen ville brutt med Single Responsibility Principle (SRP).

For å opprettholde et tydelig skille mellom enheten og samlingen, overlater vi håndteringen av oppgavesamlingen til ITaskContext.

Andre komponenter trenger å vite at Model kan:

legge til en oppgave, returnere oppgavene, finne en oppgave på ID, markere en oppgave som fullført, fortelle hvor mange oppgaver den har.

De trenger ikke vite *hvordan* oppgavene lagres internt.

Oppgavesamlingen er en implementasjonsdetalj i TaskContext, ikke en del av ansvaret til Controller eller View.

Controlleren er dermed fri for domenedetaljer. Å generere unik ID, opprette instansen og håndtere intern lagring er interne detaljer i TaskContext modellen.

### [6. implement TaskContext](https://github.com/Pat-devs/TaskManagerMvc/commit/c83d2b6110bb1aa848f7ba5d60556e28801bc0ab):

Vi har dermed en tydelig ansvarsdeling:

* TaskContext -> håndterer samlingen
* UserTask    -> håndterer en oppgave

### [7. implement view generator](https://github.com/Pat-devs/TaskManagerMvc/commit/421e43cea5005cab4536a7cf28e308926c757499):


Modellen kan nå representere og håndtere oppgaver, men den skal ikke være ansvarlig for Console.

En UserTask skal ikke bry seg om den vises:

- i terminalen,
- på en nettside,
- i en desktop-app,
- i en mobilapp.

Derfor introduserer vi et View-lag som håndterer kommunikasjon med brukeren.

**Interface Segregation**

IViewGenerator er en egen kontrakt for brukerkommunikasjon.

ITaskContext er en annen kontrakt for oppgavesamlingen.

Vi kunne laget ett stort interface med alt mulig, men da ville komponentene blitt tvunget til å kjenne operasjoner de ikke trenger.

## Etter livekoding sessionen:

Vi har nå tre tydelige ansvar i Model/View-delen:

1. UserTask
  -> tilstanden og oppførselen til en oppgave

2. TaskContext
  -> samlingen av oppgaver og operasjoner på samlingen

3. ViewGenerator
  -> kommunikasjon med Console-brukeren

Single Responsibility betyr ikke *en metode per klasse*. Det betyr at metodene i en klasse bør høre til det samme sammenhengende ansvarsområdet.

Det neste arkitektoniske problemet er derfor:

- Model skal ikke lese Console.
- View skal ikke bestemme programflyten.

Hvem bestemmer hva som skal skje når brukeren velger et menyvalg?

# Resten av TaskManager
*(Selvstudium : fullfør TaskManager)*

Livekodingen stoppet etter at `ViewGenerator` var implementert.

Vi har dermed allerede:

* en modell som representerer én oppgave,
* en modell som håndterer samlingen av oppgaver,
* tester av modell-laget,
* et View som håndterer kommunikasjon med Console.

Det som fortsatt mangler er delen som binder disse komponentene sammen.

Vi skal fullføre applikasjonen gjennom flere små commits.

Poenget er ikke bare å komme fram til fungerende kode. Underveis skal vi kunne se hvorfor arkitekturen endrer seg.

Vi fortsetter med følgende commits:

8. `fix view generator`
9. `implement task controller`
10. `wire application together`
11. `refactor controller to abstractions`
12. `add task controller interface`

Etter hver commit bør du kjøre:

```bash
dotnet build
dotnet test
```

Når applikasjonen er koblet sammen skal vi også bruke:

```bash
dotnet run --project App
```

---

# 8. fix view generator

Før vi begynner på Controller bør vi rydde opp noen småting fra livekodingen.

Dette er også et viktig poeng:

**En commit trenger ikke alltid introdusere ny funksjonalitet.**

Noen ganger handler en commit om å gjøre eksisterende kode konsistent før vi bygger videre på den.

Start med:

```bash
dotnet build
```

På dette tidspunktet vil vi oppdage et problem mellom `IViewGenerator` og `ViewGenerator`.

Interfacet sier:

```csharp
void DisplayTask(
    IEnumerable<IUserTask> tasks,
    string header);
```

Mens implementasjonen heter:

```csharp
public void DisplayTasks(
    IEnumerable<IUserTask> tasks,
    string header)
```

Et interface er en kontrakt.

Hvis en klasse sier:

```csharp
public class ViewGenerator : IViewGenerator
```

sier den samtidig:

> Jeg lover å implementere kontrakten som `IViewGenerator` beskriver.

Navn og signatur må derfor stemme.

Siden metoden viser **flere** oppgaver, bruker vi navnet:

```csharp
DisplayTasks
```

Oppdater `Views/IViewGenerator.cs`:

```csharp
using TaskManager.Core.Models;

namespace TaskManager.Core.Views;

public interface IViewGenerator
{
    void DisplayMainMenu();

    void DisplayTasks(
        IEnumerable<IUserTask> tasks,
        string header);

    void DisplayMessage(string message);

    string GetInput(string prompt);

    int GetIntInput(string prompt);

    DateTime GetDateInput(string prompt);

    void WaitForKey();
}
```

Vi rydder også opp noen småting i `ViewGenerator`.

Fjern unødvendige `using`-direktiver slik at toppen av filen bare trenger:

```csharp
using TaskManager.Core.Models;

namespace TaskManager.Core.Views;
```

Kontroller at `DisplayTasks` skriver mellomrom mellom status og tittel:

```csharp
Console.WriteLine(
    $"{task.Id}. [{status}] " +
    $"{task.Title} " +
    $"(frist: {task.DueDate:dd.MM.yyyy})");
```

`DisplayMessage` skal bruke parameteren den mottar.

Endre:

```csharp
Console.WriteLine("message");
```

til:

```csharp
Console.WriteLine(message);
```

Metoden skal dermed være:

```csharp
public void DisplayMessage(string message)
{
    Console.WriteLine();
    Console.WriteLine(message);
}
```

Rett også teksten i `WaitForKey`:

```csharp
public void WaitForKey()
{
    Console.WriteLine();
    Console.WriteLine("Trykk en tast for å fortsette...");
    Console.ReadKey(true);
}
```

Kjør:

```bash
dotnet build
dotnet test
```

Begge skal nå være grønne.

Commit:

```bash
git add .
git commit -m "fix view generator"
```

### Hva lærte vi?

Dette var ikke en arkitektonisk endring.

Vi gjorde kontrakten og implementasjonen konsistente før andre komponenter begynner å avhenge av dem.

Det er mye enklere å bygge Controller når vi vet at både Model og View allerede har stabile kontrakter.

---

# 9. implement task controller

Vi har nå:

```text
Model
 ├── UserTask
 └── TaskContext

View
 └── ViewGenerator
```

Men ingen av disse komponentene skal kontrollere hele programflyten.

`TaskContext` skal ikke spørre brukeren hvilket menyvalg hen ønsker.

`ViewGenerator` skal ikke bestemme at menyvalg `"3"` betyr at en oppgave skal fullføres.

Vi trenger derfor en Controller.

Controllerens ansvar er å koordinere de andre komponentene.

En nyttig måte å tenke på Controller er som en dirigent:

```text
View           Model
  \             /
   \           /
    Controller
```

Dirigenten spiller ikke alle instrumentene selv.

Den bestemmer hvem som skal gjøre hva, og i hvilken rekkefølge.

---

## Opprett Controllers-mappen dersom den ikke finnes

```text
TaskManager.Core/
└── Controllers/
```

Opprett:

```text
TaskManager.Core/Controllers/TaskController.cs
```

I første omgang skal vi **med vilje bruke konkrete typer**.

```csharp
using TaskManager.Core.Models;
using TaskManager.Core.Views;

namespace TaskManager.Core.Controllers;

public class TaskController
{
    private readonly TaskContext _context;
    private readonly ViewGenerator _view;

    public TaskController(
        TaskContext context,
        ViewGenerator view)
    {
        _context = context;
        _view = view;
    }

    public void Run()
    {
        var running = true;

        while (running)
        {
            _view.DisplayMainMenu();

            var choice = _view.GetInput(
                "Velg et alternativ: ");

            switch (choice)
            {
                case "1":
                    ViewAllTasks();
                    break;

                case "2":
                    AddTask();
                    break;

                case "3":
                    CompleteTask();
                    break;

                case "0":
                    running = false;
                    break;

                default:
                    _view.DisplayMessage(
                        "Ugyldig valg.");

                    _view.WaitForKey();
                    break;
            }
        }

        _view.DisplayMessage(
            "Programmet avsluttes.");
    }

    private void ViewAllTasks()
    {
        var tasks = _context.GetAllTasks();

        _view.DisplayTasks(
            tasks,
            "Alle oppgaver");

        _view.WaitForKey();
    }

    private void AddTask()
    {
        var title =
            _view.GetInput("Tittel: ");

        var description =
            _view.GetInput("Beskrivelse: ");

        var dueDate =
            _view.GetDateInput("Frist: ");

        try
        {
            var task = _context.AddTask(
                title,
                description,
                dueDate);

            _view.DisplayMessage(
                $"Oppgave {task.Id} ble opprettet.");
        }
        catch (ArgumentException exception)
        {
            _view.DisplayMessage(
                exception.Message);
        }

        _view.WaitForKey();
    }

    private void CompleteTask()
    {
        var tasks = _context.GetAllTasks();

        _view.DisplayTasks(
            tasks,
            "Alle oppgaver");

        if (tasks.Count == 0)
        {
            _view.WaitForKey();
            return;
        }

        var id = _view.GetIntInput(
            "ID på oppgaven: ");

        var success =
            _context.CompleteTask(id);

        if (success)
        {
            _view.DisplayMessage(
                "Oppgaven ble markert som fullført.");
        }
        else
        {
            _view.DisplayMessage(
                "Fant ingen oppgave med den ID-en.");
        }

        _view.WaitForKey();
    }
}
```

Kjør:

```bash
dotnet build
dotnet test
```

Commit:

```bash
git add .
git commit -m "implement task controller"
```

---

## Studer `Run()`

Dette er første gang vi faktisk kan se hele MVC-flyten i kode.

```text
User
  |
  v
View
  |
  v
Controller
  |
  v
Model
  |
  v
Controller
  |
  v
View
```

Brukeren kommuniserer med `ViewGenerator`.

Controller mottar resultatet og bestemmer hvilken operasjon som skal utføres.

Hvis data skal hentes eller endres, går Controller til Model.

Hvis noe skal presenteres til brukeren, går Controller til View.

---

## Hvem gjør egentlig hva?

Når brukeren velger:

```text
3. Marker oppgave som fullført
```

gjør Controller dette:

```csharp
var success =
    _context.CompleteTask(id);
```

Men Controller inneholder ikke:

```csharp
task.IsCompleted = true;
```

Det er viktig.

Controller bestemmer:

> Denne operasjonen skal utføres.

Model bestemmer:

> Slik endres dataene når operasjonen utføres.

Selve tilstandsendringen finnes fortsatt i:

```csharp
UserTask.MarkAsCompleted()
```

Dermed har vi fortsatt tydelig Separation of Concerns.

---

## Legg merke til valideringen

I `AddTask()` spør Controller View om data:

```csharp
var title =
    _view.GetInput("Tittel: ");
```

Men Controller bestemmer ikke om en tom tittel er gyldig.

Det avgjøres av `UserTask`.

```csharp
if (string.IsNullOrWhiteSpace(title))
{
    throw new ArgumentException(
        "En oppgave må ha en tittel.",
        nameof(title));
}
```

Hvorfor?

Fordi regelen:

> En oppgave må ha en tittel.

er en regel om hva en gyldig `UserTask` er.

Den skal gjelde uansett om oppgaven kommer fra:

* Console,
* en webapplikasjon,
* en test,
* et API,
* en desktop-applikasjon.

Derfor hører regelen hjemme i Model.

Controller håndterer bare resultatet:

```csharp
catch (ArgumentException exception)
{
    _view.DisplayMessage(
        exception.Message);
}
```

---

# 10. wire application together

Vi har nå alle de tre MVC-delene:

```text
Model
View
Controller
```

Men objektene eksisterer fortsatt ikke når applikasjonen starter.

Noen må opprette dem.

Åpne:

```text
App/Program.cs
```

Standardinnholdet kan fjernes.

Legg inn:

```csharp
using TaskManager.Core.Controllers;
using TaskManager.Core.Models;
using TaskManager.Core.Views;

var context =
    new TaskContext();

var view =
    new ViewGenerator();

var controller =
    new TaskController(
        context,
        view);

controller.Run();
```

Kjør først:

```bash
dotnet build
```

Deretter:

```bash
dotnet test
```

Til slutt:

```bash
dotnet run --project App
```

Commit:

```bash
git add .
git commit -m "wire application together"
```

---

## Hva er `Program.cs` sitt ansvar?

Legg merke til hvor lite kode som nå finnes her.

`Program.cs`:

* oppretter Model,
* oppretter View,
* oppretter Controller,
* kobler objektene sammen,
* starter Controller.

Det håndterer ikke oppgaver.

Det viser ikke menyer.

Det bestemmer ikke hva menyvalg `"3"` betyr.

Dette stedet kalles ofte en:

**Composition Root**

Det er stedet hvor vi bygger objektgrafen vår og bestemmer hvilke konkrete implementasjoner applikasjonen skal bruke.

```text
Program.cs
    |
    +--> TaskContext
    |
    +--> ViewGenerator
    |
    +--> TaskController
```

---

# Test applikasjonen manuelt

Start:

```bash
dotnet run --project App
```

## 1. Vis oppgaver

Velg:

```text
1
```

Siden vi ikke har lagt til noe ennå, forventer vi:

```text
Ingen oppgaver.
```

---

## 2. Legg til en oppgave

Velg:

```text
2
```

Eksempel:

```text
Tittel:
Lære MVC

Beskrivelse:
Bygge TaskManager

Frist:
10.10.2030
```

Applikasjonen bør bekrefte at oppgaven ble opprettet.

---

## 3. Vis alle oppgaver

Velg:

```text
1
```

Du bør nå se noe tilsvarende:

```text
1. [ÅPEN] Lære MVC (frist: 10.10.2030)
```

---

## 4. Marker oppgaven som fullført

Velg:

```text
3
```

Skriv:

```text
1
```

Vis listen på nytt.

Resultatet bør nå være:

```text
1. [FERDIG] Lære MVC (frist: 10.10.2030)
```

---

## 5. Test business-regelen

Forsøk å legge til en oppgave uten tittel.

Applikasjonen skal ikke opprette oppgaven.

Dette er samme regel som allerede er testet i Model-laget.

Legg merke til at Controller ikke inneholder regelen.

Den bare håndterer feilen som kommer fra Model.

---

## 6. Avslutt

Velg:

```text
0
```

Programmet skal avsluttes.

---

# Nå fungerer applikasjonen – men designet er ikke ferdig

På dette tidspunktet har vi en fungerende applikasjon.

Det betyr likevel ikke at arkitekturen er så fleksibel som den kan være.

Åpne `TaskController`.

Se på disse feltene:

```csharp
private readonly TaskContext _context;
private readonly ViewGenerator _view;
```

Og konstruktøren:

```csharp
public TaskController(
    TaskContext context,
    ViewGenerator view)
```

Still spørsmålet vi startet hele prosjektet med:

**Hvem trenger egentlig å vite om hvem?**

Controller trenger å kunne:

* hente oppgaver,
* legge til oppgaver,
* fullføre oppgaver,
* vise informasjon,
* hente input.

Men trenger Controller egentlig å vite at modellimplementasjonen heter:

```csharp
TaskContext
```

og at View-implementasjonen heter:

```csharp
ViewGenerator
```

Nei.

Det eneste Controller trenger er operasjonene som kontraktene tilbyr.

Dette leder oss til neste commit.

---

# 11. refactor controller to abstractions

Vi opprettet interfaces tidligere:

```text
ITaskContext
IViewGenerator
```

Til nå har Controller likevel vært koblet direkte til de konkrete implementasjonene:

```text
TaskController
     |
     +--> TaskContext
     |
     +--> ViewGenerator
```

Dette kalles sterkere kobling enn nødvendig.

Vi skal nå gjøre Controller avhengig av kontraktene i stedet.

Endre:

```csharp
private readonly TaskContext _context;
private readonly ViewGenerator _view;
```

til:

```csharp
private readonly ITaskContext _context;
private readonly IViewGenerator _view;
```

Endre konstruktøren fra:

```csharp
public TaskController(
    TaskContext context,
    ViewGenerator view)
```

til:

```csharp
public TaskController(
    ITaskContext context,
    IViewGenerator view)
{
    _context = context;
    _view = view;
}
```

Resten av `TaskController` trenger ingen endringer.

Det er selve poenget.

Kjør:

```bash
dotnet build
dotnet test
dotnet run --project App
```

Commit:

```bash
git add .
git commit -m "refactor controller to abstractions"
```

---

# Hva har vi vunnet?

Før:

```text
TaskController
     |
     +--> TaskContext
     |
     +--> ViewGenerator
```

Etter:

```text
TaskController
     |
     +--> ITaskContext
     |
     +--> IViewGenerator
```

De konkrete klassene implementerer kontraktene:

```text
             ITaskContext
                  ^
                  |
             TaskContext
```

og:

```text
            IViewGenerator
                  ^
                  |
            ViewGenerator
```

Dette er **Loose Coupling**.

Controller kjenner hva komponentene kan gjøre, men trenger ikke kjenne implementasjonsdetaljene deres.

---

## Tenk framover

I dag lagrer `TaskContext` alle oppgavene i minnet:

```csharp
private readonly List<IUserTask> _tasks = new();
```

Senere kunne vi laget:

```csharp
public class DatabaseTaskContext : ITaskContext
{
    // Database implementation
}
```

Controller trenger da ikke nødvendigvis endres.

Den bruker fortsatt:

```csharp
ITaskContext
```

Det eneste stedet hvor vi trenger å velge implementasjon er stedet hvor objektene kobles sammen:

```text
Program.cs
```

Det er en viktig konsekvens av loose coupling.

---

# Dependency Injection

Vi gjør allerede en enkel form for **Dependency Injection**.

`TaskController` oppretter ikke sine egne avhengigheter:

```csharp
_context = new TaskContext();
_view = new ViewGenerator();
```

I stedet mottar den dem gjennom konstruktøren:

```csharp
public TaskController(
    ITaskContext context,
    IViewGenerator view)
```

Det betyr at Controller ikke bestemmer hvilken konkret Model eller View som skal brukes.

De blir gitt til Controller utenfra.

Dette kalles:

**Constructor Injection**

---

# Hvorfor er dette bra for testing?

Tenk deg at vi senere ønsker å teste Controller.

En ekte `ViewGenerator` bruker:

```csharp
Console.ReadLine()
Console.WriteLine()
Console.ReadKey()
```

Det er upraktisk i en automatisk unit test.

Men Controller krever ikke lenger:

```csharp
ViewGenerator
```

Den krever:

```csharp
IViewGenerator
```

Dermed kunne en test gitt Controller en annen implementasjon:

```text
FakeViewGenerator
```

som ikke bruker Console i det hele tatt.

På samme måte kunne vi erstattet `TaskContext` med en testimplementasjon av:

```csharp
ITaskContext
```

Dette er en av grunnene til at loose coupling og testbarhet ofte henger tett sammen.

---

# 12. add task controller interface

Vi har nå egne kontrakter for:

```text
IUserTask
ITaskContext
IViewGenerator
```

Controlleren mangler fortsatt en tilsvarende kontrakt.

Opprett:

```text
TaskManager.Core/Controllers/ITaskController.cs
```

med:

```csharp
namespace TaskManager.Core.Controllers;

public interface ITaskController
{
    void Run();
}
```

Oppdater deretter `TaskController`.

Endre:

```csharp
public class TaskController
```

til:

```csharp
public class TaskController : ITaskController
```

Resten av Controller trenger ingen endringer.

---

## Oppdater Composition Root

Til slutt kan `Program.cs` uttrykke alle avhengighetene gjennom kontraktene:

```csharp
using TaskManager.Core.Controllers;
using TaskManager.Core.Models;
using TaskManager.Core.Views;

ITaskContext context =
    new TaskContext();

IViewGenerator view =
    new ViewGenerator();

ITaskController controller =
    new TaskController(
        context,
        view);

controller.Run();
```

Kjør hele verifikasjonen:

```bash
dotnet build
dotnet test
dotnet run --project App
```

Commit:

```bash
git add .
git commit -m "add task controller interface"
```

---

# Interface Segregation

Vi har nå fire interfaces:

```text
IUserTask
ITaskContext
IViewGenerator
ITaskController
```

Hvorfor ikke bare lage ett stort interface?

For eksempel:

```csharp
public interface IApplication
{
    void AddTask();

    void CompleteTask();

    void DisplayMainMenu();

    void DisplayTasks();

    string GetInput();

    IUserTask GetTask();

    void Run();
}
```

Problemet er at komponentene da må forholde seg til operasjoner de ikke trenger.

`ViewGenerator` trenger ikke kjenne til:

```csharp
CompleteTask()
```

`TaskContext` trenger ikke kjenne til:

```csharp
DisplayMainMenu()
```

`UserTask` trenger ikke kjenne til:

```csharp
Run()
```

Derfor deler vi kontraktene etter sammenhengende ansvar.

Dette er **Interface Segregation Principle**.

Det betyr ikke:

> Ett interface skal bare ha én metode.

Det betyr:

> En komponent skal ikke tvinges til å avhenge av operasjoner den ikke trenger.

---

# Den ferdige strukturen

Prosjektet skal nå omtrent se slik ut:

```text
TaskManagerMvc/
│
├── App/
│   └── Program.cs
│
├── TaskManager.Core/
│   │
│   ├── Models/
│   │   ├── IUserTask.cs
│   │   ├── UserTask.cs
│   │   ├── ITaskContext.cs
│   │   └── TaskContext.cs
│   │
│   ├── Views/
│   │   ├── IViewGenerator.cs
│   │   └── ViewGenerator.cs
│   │
│   └── Controllers/
│       ├── ITaskController.cs
│       └── TaskController.cs
│
└── TaskManager.Tests/
    ├── UserTaskTests.cs
    └── TaskContextTests.cs
```

---

# Se på hele MVC-flyten igjen

Når brukeren velger å fullføre en oppgave skjer omtrent dette:

```text
User
  |
  | velger menyvalg 3
  v
ViewGenerator
  |
  | returnerer input
  v
TaskController
  |
  | CompleteTask(id)
  v
ITaskContext
  |
  v
TaskContext
  |
  | MarkAsCompleted()
  v
UserTask
```

Når resultatet skal vises går flyten motsatt vei:

```text
UserTask / TaskContext
        |
        v
TaskController
        |
        v
IViewGenerator
        |
        v
ViewGenerator
        |
        v
      User
```

Ingen av komponentene trenger å gjøre hele jobben.

Det er hensikten.

---

# Oppsummering av ansvarsområdene

## `UserTask`

Representerer én oppgave.

Har ansvar for:

```text
tilstand + regler for én oppgave
```

For eksempel:

```csharp
MarkAsCompleted()
```

og regelen om at en oppgave må ha en tittel.

---

## `TaskContext`

Representerer og administrerer samlingen av oppgaver.

Har ansvar for:

```text
AddTask
GetAllTasks
GetTaskById
CompleteTask
```

Controller trenger ikke vite:

* hvordan ID genereres,
* hvilken collection som brukes,
* hvordan oppgaven finnes,
* hvordan oppgaven markeres som fullført.

---

## `ViewGenerator`

Har ansvar for kommunikasjon med Console-brukeren.

For eksempel:

```text
vise meny
vise oppgaver
vise meldinger
hente tekst
hente tall
hente dato
vente på tast
```

View bestemmer ikke hvilken Model-operasjon et menyvalg skal føre til.

---

## `TaskController`

Har ansvar for programflyten.

Controller bestemmer:

```text
Når skal Model brukes?
Hvilken Model-operasjon skal brukes?
Hva skal View vise etterpå?
```

Controller eier ikke domenereglene og eier ikke Console-kommunikasjonen.

---

## `Program.cs`

Er Composition Root.

Det oppretter objektene og kobler dem sammen.

Det inneholder ikke business logic.

---

# Designprinsippene vi ønsket å demonstrere

## Separation of Concerns

Vi har separert forskjellige typer problemer:

```text
Model       -> data og regler
View        -> input og output
Controller  -> programflyt
```

---

## Single Responsibility Principle

Klassene har sammenhengende ansvarsområder:

```text
UserTask       -> én oppgave
TaskContext    -> oppgavesamlingen
ViewGenerator  -> Console-kommunikasjon
TaskController -> programflyt
```

SRP betyr ikke én metode per klasse.

Det betyr at en klasse bør ha ett sammenhengende ansvar.

---

## Interface Segregation

Vi bruker små, relevante kontrakter:

```text
IUserTask
ITaskContext
IViewGenerator
ITaskController
```

Komponentene trenger ikke forholde seg til operasjoner de aldri bruker.

---

## Loose Coupling

Controller avhenger av:

```text
ITaskContext
IViewGenerator
```

i stedet for direkte av:

```text
TaskContext
ViewGenerator
```

Dermed kan implementasjoner lettere byttes.

---

## Testbarhet

`UserTask` og `TaskContext` kan testes direkte uten å:

* starte Console-applikasjonen,
* skrive brukerinput,
* navigere en meny,
* starte hele systemet.

Interfaces gjør også senere testing av andre komponenter enklere fordi konkrete avhengigheter kan erstattes.

---

# Kontroller forståelsen din

Ikke se på svarene med en gang.

## Scenario 1

Vi ønsker å endre hvordan oppgavelisten ser ut i terminalen.

Hvilken komponent bør primært endres?

<details>
<summary>Svar</summary>

`ViewGenerator`.

Dette handler om presentasjon.

</details>

---

## Scenario 2

En oppgave skal ikke kunne opprettes uten tittel.

Hvor bør regelen ligge?

<details>
<summary>Svar</summary>

Model, nærmere bestemt `UserTask`.

Dette er en regel om hva en gyldig oppgave er.

</details>

---

## Scenario 3

Menyvalg `3` skal utføre en annen operasjon.

Hvor forventer vi primært å gjøre endringen?

<details>
<summary>Svar</summary>

`TaskController`.

Dette handler om programflyt.

</details>

---

## Scenario 4

Oppgavene skal lagres i en database i stedet for i en `List`.

Hvilken kontrakt er spesielt viktig?

<details>
<summary>Svar</summary>

`ITaskContext`.

Vi kan lage en ny implementasjon av kontrakten uten at Controller nødvendigvis trenger å endres.

</details>

---

## Scenario 5

Vi bytter fra Console til et annet brukergrensesnitt.

Hvilken kontrakt gjør det mulig å introdusere en annen View-implementasjon?

<details>
<summary>Svar</summary>

`IViewGenerator`.

Controller trenger bare kontrakten, ikke nødvendigvis `ViewGenerator`.

</details>

---

# Et viktig spørsmål når du leser koden

Når du går gjennom prosjektet igjen, prøv å ikke bare spørre:

> Hva gjør denne metoden?

Spør også:

> Hvorfor ligger denne metoden akkurat i denne komponenten?

Eksempel:

```csharp
MarkAsCompleted()
```

Hvorfor ligger den i `UserTask`?

Fordi den endrer tilstanden til én oppgave.

---

```csharp
GetIntInput()
```

Hvorfor ligger den i `ViewGenerator`?

Fordi den håndterer input fra brukergrensesnittet.

---

```csharp
CompleteTask()
```

i Controller:

Hvorfor ligger denne flyten i `TaskController`?

Fordi Controller bestemmer at en Model-operasjon skal utføres basert på brukerens handling.

---

# Sluttresultatet

Applikasjonen vår er svært liten.

Det er med vilje.

Vi kunne skrevet hele programmet i:

```text
Program.cs
```

Det ville kanskje fungert.

Men hensikten med denne øvelsen er ikke å demonstrere hvor få filer vi klarer å bruke.

Hensikten er å vise hvordan kode kan organiseres når forskjellige typer ansvar begynner å oppstå.

Den viktigste læringen er derfor ikke:

```text
MVC = tre mapper
```

men:

```text
MODEL
Data + regler

VIEW
Input + output

CONTROLLER
Programflyt
```

sammen med:

```text
Separation of Concerns
-> del forskjellige problemer

Single Responsibility Principle
-> ett sammenhengende ansvar

Interface Segregation
-> små og relevante kontrakter

Loose Coupling
-> avhengighet til kontrakter fremfor konkrete implementasjoner

Testbarhet
-> komponenter kan testes mer isolert
```

---

# Frivillig videre arbeid

Når hovedapplikasjonen fungerer kan du prøve én eller flere av disse uten å følge ferdig kode først.

## Oppgave A – `DeleteTask`

Utvid `ITaskContext` med:

```csharp
bool DeleteTask(int id);
```

Implementer operasjonen i `TaskContext`.

Legg deretter til støtte for sletting i:

```text
ViewGenerator
TaskController
```

Spør deg selv underveis:

> Hvilke komponenter må faktisk endres, og hvorfor?

---

## Oppgave B – vis bare åpne oppgaver

Legg til en Model-operasjon:

```csharp
public List<IUserTask> GetPendingTasks()
{
    return _tasks
        .Where(task => !task.IsCompleted)
        .ToList();
}
```

Hvorfor filtrerer vi her og ikke i View?

Fordi spørsmålet:

> Hvilke oppgaver er fortsatt åpne?

handler om dataene.

Hvordan de åpne oppgavene presenteres er View sitt ansvar.

---

## Oppgave C – skriv én test til

Skriv testen:

```csharp
[Fact]
public void GetTaskById_ReturnsCorrectTask()
{
    var context = new TaskContext();

    var first = context.AddTask(
        "First",
        string.Empty,
        new DateTime(2030, 1, 1));

    var second = context.AddTask(
        "Second",
        string.Empty,
        new DateTime(2030, 1, 2));

    var result =
        context.GetTaskById(second.Id);

    Assert.Equal(second.Id, result?.Id);
}
```

Kjør:

```bash
dotnet test
```

Prøv deretter å forklare hvorfor denne testen kan kjøre helt uten Console, View eller Controller.

Det er ikke bare fordi vi bruker xUnit.

Det er fordi designet vårt lar Model testes isolert.
