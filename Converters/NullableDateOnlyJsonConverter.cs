using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace bdt_evm_app.Converters;

// Tolera string vacío / espacios como null al deserializar DateOnly?.
// Motivo: el frontend manda "" para fechas que no se completan (p.ej. la
// fecha de aprobación en un cambio en estado "propuesto", que se define en
// un 2do paso). System.Text.Json nativo lanza al recibir "" y ASP.NET lo
// traduce en un 400 opaco. Acá "" => null, y se aceptan tanto "yyyy-MM-dd"
// como un datetime ISO completo ("2026-06-26T00:00:00Z").
public class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var raw = reader.GetString();
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        if (DateOnly.TryParseExact(raw, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        // Fallback: el cliente mandó un datetime ISO completo.
        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTime))
            return DateOnly.FromDateTime(dateTime);

        throw new JsonException($"Fecha inválida: '{raw}'. Formato esperado '{Format}'.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString(Format, CultureInfo.InvariantCulture));
        else
            writer.WriteNullValue();
    }
}
