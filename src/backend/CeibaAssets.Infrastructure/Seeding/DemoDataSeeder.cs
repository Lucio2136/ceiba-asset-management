using CeibaAssets.Domain.Assets;
using CeibaAssets.Domain.Assignments;
using CeibaAssets.Domain.Audit;
using CeibaAssets.Domain.Locations;
using CeibaAssets.Domain.People;
using CeibaAssets.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CeibaAssets.Infrastructure.Seeding;

/// <summary>
/// Datos de demostración exclusivamente ficticios: edificios, pisos, ubicaciones, activos,
/// responsables y asignaciones. Es idempotente: no hace nada si ya existen edificios.
/// </summary>
public static class DemoDataSeeder
{
    private static readonly (string Code, string Name, int Levels, string Prefix)[] BuildingDefinitions =
    [
        ("TORRE-A", "CEIBA Torre A", 8, "A"),
        ("TORRE-B", "CEIBA Torre B", 10, "B"),
        ("ECOMMERCE", "CEIBA Ecommerce", 1, "EC"),
    ];

    private static readonly (string Number, string Name, string Email, string Department)[] People =
    [
        ("E-1001", "Ana Beltrán Ruiz", "ana.beltran@ceiba.example", "Tecnologías de la Información"),
        ("E-1002", "Bruno Salazar Ortiz", "bruno.salazar@ceiba.example", "Finanzas"),
        ("E-1003", "Carla Mendoza Vega", "carla.mendoza@ceiba.example", "Operaciones"),
        ("E-1004", "Diego Fuentes Lara", "diego.fuentes@ceiba.example", "Recursos Humanos"),
        ("E-1005", "Elena Quiroz Peña", "elena.quiroz@ceiba.example", "Comercio electrónico"),
    ];

    // Tipo, nombre, fabricante, modelo, serie, código de ubicación y responsable (índice en People) si está asignado.
    private static readonly (AssetType Type, string Name, string Maker, string Model, string Serial, string Location, int? Person)[] Assets =
    [
        (AssetType.Computer, "PC escritorio TI-01", "Dell", "OptiPlex 7010", "FICT-PC-0001", "A-02-OPEN", 0),
        (AssetType.Computer, "PC escritorio TI-02", "Dell", "OptiPlex 7010", "FICT-PC-0002", "A-02-OPEN", null),
        (AssetType.Computer, "PC escritorio FIN-01", "HP", "EliteDesk 800", "FICT-PC-0003", "A-05-OPEN", 1),
        (AssetType.Computer, "PC escritorio ECO-01", "Lenovo", "ThinkCentre M70", "FICT-PC-0004", "EC-01-OPEN", null),
        (AssetType.Laptop, "Laptop Ana", "Lenovo", "ThinkPad T14", "FICT-LT-0001", "A-02-OPEN", 0),
        (AssetType.Laptop, "Laptop Bruno", "Dell", "Latitude 5440", "FICT-LT-0002", "A-05-OPEN", 1),
        (AssetType.Laptop, "Laptop Carla", "HP", "ProBook 450", "FICT-LT-0003", "B-04-OPEN", 2),
        (AssetType.Laptop, "Laptop de préstamo 1", "Lenovo", "ThinkPad E14", "FICT-LT-0004", "B-01-MEET", null),
        (AssetType.Laptop, "Laptop de préstamo 2", "Lenovo", "ThinkPad E14", "FICT-LT-0005", "B-01-MEET", null),
        (AssetType.Monitor, "Monitor 24\" TI-01", "Dell", "P2422H", "FICT-MN-0001", "A-02-OPEN", 0),
        (AssetType.Monitor, "Monitor 24\" FIN-01", "Dell", "P2422H", "FICT-MN-0002", "A-05-OPEN", null),
        (AssetType.Monitor, "Monitor 27\" Sala A-03", "LG", "27UL500", "FICT-MN-0003", "A-03-MEET", null),
        (AssetType.Webcam, "Webcam sala de juntas B-02", "Logitech", "C920", "FICT-WC-0001", "B-02-MEET", null),
        (AssetType.Webcam, "Webcam Carla", "Logitech", "C270", "FICT-WC-0002", "B-04-OPEN", 2),
        (AssetType.Peripheral, "Teclado inalámbrico 1", "Logitech", "K400", "FICT-DUP-001", "A-02-OPEN", null),
        (AssetType.Peripheral, "Mouse inalámbrico 1", "Logitech", "M185", "FICT-DUP-001", "A-02-OPEN", null),
        (AssetType.Peripheral, "Estación de acoplamiento B-04", "Dell", "WD19S", "FICT-PR-0003", "B-04-OPEN", null),
        (AssetType.CiscoPhone, "Teléfono IP recepción A", "Cisco", "CP-8845", "FICT-CP-0001", "A-01-OPEN", null),
        (AssetType.CiscoPhone, "Teléfono IP Dirección B-10", "Cisco", "CP-8851", "FICT-CP-0002", "B-10-MEET", 3),
        (AssetType.CiscoPhone, "Teléfono IP sala B-02", "Cisco", "CP-7841", "FICT-CP-0003", "B-02-MEET", null),
        (AssetType.MobileDevice, "Smartphone Diego", "Samsung", "Galaxy A54", "FICT-MB-0001", "B-10-OPEN", 3),
        (AssetType.MobileDevice, "Tablet inventario Ecommerce", "Samsung", "Galaxy Tab A8", "FICT-MB-0002", "EC-01-MEET", 4),
        (AssetType.MobileDevice, "Smartphone de reserva", "Motorola", "Moto G73", "FICT-MB-0003", "A-01-OPEN", null),
    ];

    public static async Task SeedAsync(CeibaDbContext db, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        if (await db.Buildings.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var locationsByCode = new Dictionary<string, Location>(StringComparer.Ordinal);

        foreach (var (code, name, levels, prefix) in BuildingDefinitions)
        {
            var building = Building.Create(code, name, levels);
            db.Buildings.Add(building);

            for (var level = 1; level <= levels; level++)
            {
                var floor = building.AddFloor(level, levels == 1 ? "Nivel único" : $"Piso {level}");
                var floorKey = $"{prefix}-{level:00}";

                AddLocation(floor, locationsByCode, $"{floorKey}-OPEN", "Área abierta", $"Área de trabajo abierta del piso {level}.");
                AddLocation(floor, locationsByCode, $"{floorKey}-MEET", "Sala de juntas", $"Sala de juntas del piso {level}.");

                if (level == 1 && levels > 1)
                {
                    AddLocation(floor, locationsByCode, $"{floorKey}-LOBBY", "Recepción", "Recepción y control de acceso.");
                }
            }
        }

        var people = People
            .Select(p => ResponsiblePerson.Create(p.Number, p.Name, p.Email, p.Department))
            .ToList();
        db.ResponsiblePeople.AddRange(people);

        var assetTagNumber = 0;
        foreach (var definition in Assets)
        {
            assetTagNumber++;
            var location = locationsByCode[definition.Location];
            var asset = Asset.Create(
                definition.Type,
                definition.Name,
                definition.Maker,
                definition.Model,
                definition.Serial,
                "Activo ficticio de demostración.",
                location.Id,
                BuildIdentifiers(definition.Type, assetTagNumber),
                now.AddDays(-90));
            db.Assets.Add(asset);

            db.AssetMovements.Add(AssetMovement.Create(
                asset.Id,
                null,
                location.Id,
                now.AddDays(-90),
                null,
                "Ubicación inicial (datos de demostración)."));

            if (definition.Person is { } personIndex)
            {
                var assignedAt = now.AddDays(-30);
                asset.MarkAssigned(assignedAt);
                db.AssetAssignments.Add(AssetAssignment.Create(asset.Id, people[personIndex].Id, assignedAt, null, "Asignación inicial."));
            }
        }

        AddDemoHistory(db, people, now);

        db.AuditEvents.Add(AuditEvent.Create(
            now,
            null,
            "Sistema",
            "system.seeded",
            "System",
            "demo-data",
            "Se cargaron los datos ficticios de demostración."));

        await db.SaveChangesAsync(cancellationToken);
    }

    private static void AddLocation(
        Floor floor,
        Dictionary<string, Location> locationsByCode,
        string code,
        string name,
        string description)
    {
        var location = floor.AddLocation(code, name, description);
        locationsByCode[location.Code] = location;
    }

    private static List<AssetIdentifierInput> BuildIdentifiers(AssetType type, int number)
    {
        var identifiers = new List<AssetIdentifierInput>
        {
            new(AssetIdentifierKind.AssetTag, $"CEIBA-AT-{number:0000}", true),
        };

        switch (type)
        {
            case AssetType.Computer or AssetType.Laptop:
                identifiers.Add(new(AssetIdentifierKind.HostName, $"CEIBA-{(type == AssetType.Laptop ? "LT" : "PC")}-{number:000}", false));
                identifiers.Add(new(AssetIdentifierKind.MacAddress, $"02:CE:1B:A0:{number / 256:X2}:{number % 256:X2}", true));
                break;
            case AssetType.CiscoPhone:
                identifiers.Add(new(AssetIdentifierKind.PhoneExtension, $"5{number:000}", false));
                break;
            case AssetType.MobileDevice:
                identifiers.Add(new(AssetIdentifierKind.Imei, $"35{number:0000000000000}", true));
                break;
            default:
                break;
        }

        return identifiers;
    }

    /// <summary>
    /// Agrega una asignación ya devuelta para que exista historial en la demostración.
    /// </summary>
    private static void AddDemoHistory(CeibaDbContext db, List<ResponsiblePerson> people, DateTime now)
    {
        var asset = db.Assets.Local.First(a => a.Name == "Laptop de préstamo 1");
        var assignedAt = now.AddDays(-60);
        var returnedAt = now.AddDays(-45);

        asset.MarkAssigned(assignedAt);
        var assignment = AssetAssignment.Create(asset.Id, people[4].Id, assignedAt, null, "Préstamo temporal.");
        assignment.Return(returnedAt, "Entregada en buen estado.");
        asset.MarkReturned(returnedAt);
        db.AssetAssignments.Add(assignment);

        var maintenance = db.Assets.Local.First(a => a.Name == "PC escritorio ECO-01");
        maintenance.SendToMaintenance(now.AddDays(-2));

        var retired = db.Assets.Local.First(a => a.Name == "Smartphone de reserva");
        retired.Retire(now.AddDays(-10));
    }
}
