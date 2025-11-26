namespace Costealo.API.Models;

/// <summary>
/// Representa un error de validación durante la importación de Excel
/// </summary>
public record ImportValidationError(
    int Row,           // Número de fila en Excel (1-indexed, sin contar header)
    string Column,     // Nombre de la columna (ej: "Producto", "Precio")
    string Value,      // El valor inválido
    string Error       // Mensaje de error específico
);
