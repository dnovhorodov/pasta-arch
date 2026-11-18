namespace PastaFit.Core.Domain;

public sealed record Class(ClassId Id, string Name, int Capacity);

public sealed record ClassAvailability(Guid Id, string Name, int Capacity, int Available);
